using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using PRN232.Domain.Entities;
using PRN232.GradingEngine.Application.Interfaces;

namespace PRN232.GradingEngine.Infrastructure.StaticAnalysis;

public class RoslynStructureAnalyzer : IStaticCodeAnalyzer
{
    public Task<List<string>> CheckNamingConventionsAsync(string workspacePath, string studentId, ExamRubric rubric)
    {
        var violations = new List<string>();

        if (!Directory.Exists(workspacePath))
        {
            violations.Add($"Thư mục bài nộp không tồn tại: {workspacePath}");
            return Task.FromResult(violations);
        }

        // 1. Kiểm tra Solution file (.sln)
        var slnFiles = Directory.GetFiles(workspacePath, "*.sln", SearchOption.TopDirectoryOnly);
        if (slnFiles.Length == 0)
        {
            violations.Add("Không tìm thấy file Solution (.sln) ở thư mục gốc.");
            return Task.FromResult(violations);
        }

        var slnFile = slnFiles[0];
        var slnName = Path.GetFileNameWithoutExtension(slnFile);
        var solutionPattern = ResolveStudentPattern(rubric.SolutionPattern, studentId);

        if (!Regex.IsMatch(slnName, solutionPattern))
        {
            violations.Add($"Tên file solution '{slnName}' không đúng cấu trúc bắt buộc '{solutionPattern}'.");
        }

        // Parse file .sln để xem các project C# được đăng ký
        var referencedProjects = ParseSolutionProjects(slnFile);

        // 2. Kiểm tra các Project bắt buộc
        foreach (var reqProj in rubric.RequiredProjects)
        {
            var targetPattern = ResolveStudentPattern(reqProj.Pattern, studentId);
            var isMatched = referencedProjects.Any(p => IsProjectNameMatched(p.ProjectName, targetPattern));

            if (reqProj.MustExist && !isMatched)
            {
                violations.Add($"Không tìm thấy project bắt buộc khớp với mẫu '{targetPattern}' đăng ký trong solution.");
            }
            else if (isMatched)
            {
                // Kiểm tra xem file project (.csproj) và thư mục của nó có tồn tại thật trên đĩa không
                var matchedProj = referencedProjects.First(p => IsProjectNameMatched(p.ProjectName, targetPattern));
                var fullPath = Path.GetFullPath(Path.Combine(workspacePath, matchedProj.RelativePath));
                if (!File.Exists(fullPath))
                {
                    violations.Add($"File project '{matchedProj.ProjectName}' được đăng ký trong solution nhưng không tồn tại trên ổ đĩa tại: {matchedProj.RelativePath}");
                }
            }
        }

        // 3. Kiểm tra các file bắt buộc theo rubric (bao gồm appsettings nếu rubric yêu cầu)
        foreach (var reqFile in rubric.RequiredFiles)
        {
            var targetPattern = ResolveStudentPattern(reqFile.Pattern, studentId);
            var allFiles = Directory.GetFiles(workspacePath, "*", SearchOption.AllDirectories)
                .Where(f => !IsGeneratedOrBuildArtifact(workspacePath, f))
                .Select(Path.GetFileName)
                .ToList();

            var fileFound = allFiles.Any(f => f != null && Regex.IsMatch(f, targetPattern));
            if (reqFile.MustExist && !fileFound)
            {
                violations.Add($"Không tìm thấy file bắt buộc khớp với mẫu '{targetPattern}' trong bài nộp.");
            }
        }

        return Task.FromResult(violations);
    }

    public async Task<List<string>> CheckHardcodedConnectionStringAsync(string workspacePath)
    {
        var violations = new List<string>();

        if (!Directory.Exists(workspacePath))
            return violations;

        // 1. F-14: Kiểm tra appsettings.json tồn tại và có key ConnectionStrings hợp lệ
        var appsettingsFiles = Directory.GetFiles(workspacePath, "appsettings.json", SearchOption.AllDirectories);
        if (appsettingsFiles.Length == 0)
        {
            violations.Add("Thiếu file cấu hình appsettings.json.");
        }
        else
        {
            foreach (var appsettingsPath in appsettingsFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(appsettingsPath);
                    using var doc = JsonDocument.Parse(json);
                    if (!doc.RootElement.TryGetProperty("ConnectionStrings", out var connectionStringsNode) ||
                        connectionStringsNode.ValueKind != JsonValueKind.Object ||
                        !connectionStringsNode.EnumerateObject().Any())
                    {
                        violations.Add($"File {Path.GetFileName(appsettingsPath)} thiếu key 'ConnectionStrings' hoặc không có giá trị hợp lệ.");
                    }
                }
                catch (JsonException ex)
                {
                    violations.Add($"File {Path.GetFileName(appsettingsPath)} không phải JSON hợp lệ: {ex.Message}");
                }
                catch (Exception ex)
                {
                    violations.Add($"Không thể đọc file {Path.GetFileName(appsettingsPath)}: {ex.Message}");
                }
            }
        }

        // 2. F-15: Quét Roslyn để phát hiện hardcode connection string trong OnConfiguring
        var csFiles = Directory.GetFiles(workspacePath, "*.cs", SearchOption.AllDirectories);
        foreach (var file in csFiles)
        {
            if (IsGeneratedOrBuildArtifact(workspacePath, file))
            {
                continue;
            }

            try
            {
                var code = await File.ReadAllTextAsync(file);
                var tree = CSharpSyntaxTree.ParseText(code);
                var root = await tree.GetRootAsync();

                var walker = new ConnectionStringWalker(Path.GetFileName(file));
                walker.Visit(root);
                violations.AddRange(walker.Violations);
            }
            catch (Exception ex)
            {
                violations.Add($"Lỗi khi phân tích file {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        return violations;
    }

    private static bool IsGeneratedOrBuildArtifact(string workspacePath, string filePath)
    {
        var relativePath = Path.GetRelativePath(workspacePath, filePath);
        var segments = relativePath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return segments.Contains("bin", StringComparer.OrdinalIgnoreCase)
               || segments.Contains("obj", StringComparer.OrdinalIgnoreCase)
               || segments.Contains("Migrations", StringComparer.OrdinalIgnoreCase)
               || filePath.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase)
               || filePath.EndsWith("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveStudentPattern(string patternTemplate, string studentId)
    {
        var normalizedStudentId = (studentId ?? string.Empty).Trim();
        var studentIdWithoutPrefix = normalizedStudentId.StartsWith("SE", StringComparison.OrdinalIgnoreCase)
            ? normalizedStudentId[2..]
            : normalizedStudentId;

        // Hỗ trợ dữ liệu rubric cũ từng viết dạng "..._SE{StudentID}".
        // Nếu studentId đã có tiền tố SE thì tránh tạo lỗi nhân đôi SESE....
        if (patternTemplate.Contains("_SE{StudentID}", StringComparison.OrdinalIgnoreCase))
        {
            return patternTemplate.Replace("{StudentID}", studentIdWithoutPrefix, StringComparison.OrdinalIgnoreCase);
        }

        return patternTemplate.Replace("{StudentID}", normalizedStudentId, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsProjectNameMatched(string projectName, string targetPattern)
    {
        // Tên project không phân biệt hoa/thường (Windows/dev thực tế thường không strict case)
        // Nên normalize về lower trước khi match để tránh lỗi âm điểm sai (.API vs .api).
        var normalizedProjectName = (projectName ?? string.Empty).ToLowerInvariant();
        var normalizedPattern = (targetPattern ?? string.Empty).ToLowerInvariant();
        return Regex.IsMatch(normalizedProjectName, normalizedPattern);
    }

    private List<SolutionProjectInfo> ParseSolutionProjects(string slnPath)
    {
        var projects = new List<SolutionProjectInfo>();
        var lines = File.ReadAllLines(slnPath);
        
        // Dòng khai báo project trong file .sln thường có dạng:
        // Project("{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}") = "PRN231_SU25_SE182004.api", "PRN231_SU25_SE182004.api\PRN231_SU25_SE182004.api.csproj", "{GUID}"
        var regex = new Regex(@"Project\(""\{[A-Z0-9\-]+\}""\)\s*=\s*""([^""]+)"",\s*""([^""]+)"",\s*""\{[A-Z0-9\-]+\}""", RegexOptions.IgnoreCase);

        foreach (var line in lines)
        {
            var match = regex.Match(line);
            if (match.Success)
            {
                var name = match.Groups[1].Value;
                var relPath = match.Groups[2].Value;

                // Chuẩn hóa đường dẫn tương ứng với OS (Windows \ vs Unix /)
                relPath = relPath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);

                projects.Add(new SolutionProjectInfo(name, relPath));
            }
        }

        return projects;
    }

    private record SolutionProjectInfo(string ProjectName, string RelativePath);

    private class ConnectionStringWalker : CSharpSyntaxWalker
    {
        private readonly string _fileName;
        public List<string> Violations { get; } = new();

        public ConnectionStringWalker(string fileName)
        {
            _fileName = fileName;
        }

        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            if (!string.Equals(node.Identifier.ValueText, "OnConfiguring", StringComparison.Ordinal))
            {
                base.VisitMethodDeclaration(node);
                return;
            }

            foreach (var invocation in node.DescendantNodes().OfType<InvocationExpressionSyntax>())
            {
                var methodName = invocation.Expression switch
                {
                    MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
                    IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                    _ => null
                };

                if (methodName is not ("UseSqlServer" or "UseNpgsql" or "UseSqlite"))
                    continue;

                foreach (var arg in invocation.ArgumentList.Arguments)
                {
                    if (arg.Expression is LiteralExpressionSyntax literal &&
                        literal.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        var val = literal.Token.ValueText;
                        if (!string.IsNullOrWhiteSpace(val) && IsConnectionStringLike(val))
                        {
                            Violations.Add($"File {_fileName}: Phát hiện hardcode Connection String trong hàm gọi '{methodName}(\"{val}\")'.");
                        }
                    }
                }
            }

            base.VisitMethodDeclaration(node);
        }

        private static bool IsConnectionStringLike(string value)
        {
            var lowered = value.ToLowerInvariant();
            return (lowered.Contains("server=") || lowered.Contains("host=") || lowered.Contains("database=") || lowered.Contains("datasource="))
                   && (lowered.Contains("user id=") || lowered.Contains("uid=") || lowered.Contains("password=") || lowered.Contains("pwd="));
        }
    }
}
