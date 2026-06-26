using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        var solutionPattern = rubric.SolutionPattern.Replace("{StudentID}", studentId);

        if (!Regex.IsMatch(slnName, solutionPattern))
        {
            violations.Add($"Tên file solution '{slnName}' không đúng cấu trúc bắt buộc '{solutionPattern}'.");
        }

        // Parse file .sln để xem các project C# được đăng ký
        var referencedProjects = ParseSolutionProjects(slnFile);

        // 2. Kiểm tra các Project bắt buộc
        foreach (var reqProj in rubric.RequiredProjects)
        {
            var targetPattern = reqProj.Pattern.Replace("{StudentID}", studentId);
            var isMatched = referencedProjects.Any(p => Regex.IsMatch(p.ProjectName, targetPattern));

            if (reqProj.MustExist && !isMatched)
            {
                violations.Add($"Không tìm thấy project bắt buộc khớp với mẫu '{targetPattern}' đăng ký trong solution.");
            }
            else if (isMatched)
            {
                // Kiểm tra xem file project (.csproj) và thư mục của nó có tồn tại thật trên đĩa không
                var matchedProj = referencedProjects.First(p => Regex.IsMatch(p.ProjectName, targetPattern));
                var fullPath = Path.GetFullPath(Path.Combine(workspacePath, matchedProj.RelativePath));
                if (!File.Exists(fullPath))
                {
                    violations.Add($"File project '{matchedProj.ProjectName}' được đăng ký trong solution nhưng không tồn tại trên ổ đĩa tại: {matchedProj.RelativePath}");
                }
            }
        }

        // 3. Kiểm tra các file khác (ví dụ Postman JSON script)
        foreach (var reqFile in rubric.RequiredFiles)
        {
            var targetPattern = reqFile.Pattern.Replace("{StudentID}", studentId);
            var allFiles = Directory.GetFiles(workspacePath, "*", SearchOption.AllDirectories)
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

        // 1. Kiểm tra appsettings.json
        var appsettingsFiles = Directory.GetFiles(workspacePath, "appsettings.json", SearchOption.AllDirectories);
        if (appsettingsFiles.Length == 0)
        {
            // Tạm thời chỉ ghi log cảnh báo hoặc vi phạm tùy đề
            violations.Add("Thiếu file cấu hình appsettings.json.");
        }

        // 2. Quét code C# bằng Roslyn
        var csFiles = Directory.GetFiles(workspacePath, "*.cs", SearchOption.AllDirectories);
        foreach (var file in csFiles)
        {
            // Bỏ qua các file sinh tự động như Migrations hoặc thư mục obj/bin
            var relativePath = Path.GetRelativePath(workspacePath, file);
            var segments = relativePath.Split(Path.DirectorySeparatorChar);

            if (segments.Contains("bin") || 
                segments.Contains("obj") ||
                file.Contains(".Designer.cs") || 
                file.Contains("AssemblyInfo.cs"))
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

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            base.VisitInvocationExpression(node);

            var methodName = node.Expression switch
            {
                MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
                IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
                _ => null
            };

            // Quét các hàm cấu hình kết nối DB
            if (methodName == "UseSqlServer" || methodName == "UseNpgsql" || methodName == "UseSqlite")
            {
                foreach (var arg in node.ArgumentList.Arguments)
                {
                    // Nếu tham số truyền vào hàm là 1 string literal (chuỗi cứng)
                    if (arg.Expression is LiteralExpressionSyntax literal && 
                        literal.Kind() == SyntaxKind.StringLiteralExpression)
                    {
                        var val = literal.Token.ValueText;
                        if (!string.IsNullOrWhiteSpace(val) && (val.Contains("Server=") || val.Contains("Host=") || val.Contains("Database=") || val.Contains("DataSource=")))
                        {
                            Violations.Add($"File {_fileName}: Phát hiện hardcode Connection String trong hàm gọi '{methodName}(\"{val}\")'.");
                        }
                    }
                }
            }
        }

        public override void VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            base.VisitLiteralExpression(node);

            // Quét thêm các khai báo biến string có chứa cấu trúc của connection string
            if (node.Kind() == SyntaxKind.StringLiteralExpression)
            {
                var val = node.Token.ValueText;
                if (!string.IsNullOrWhiteSpace(val) && 
                    val.Length > 20 && 
                    (val.Contains("Server=") || val.Contains("Host=") || val.Contains("Database=")) && 
                    (val.Contains("User Id=") || val.Contains("Password=") || val.Contains("pwd=") || val.Contains("uid=")))
                {
                    Violations.Add($"File {_fileName}: Phát hiện chuỗi hằng số nghi vấn là Hardcoded Connection String: \"{val}\"");
                }
            }
        }
    }
}
