using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PRN232.GradingEngine.Application.Interfaces;

namespace PRN232.GradingEngine.Infrastructure.TestExecution;

public class DotnetSolutionBuilder : ISolutionBuilder
{
    public async Task<(bool Success, List<string> BuildErrors)> BuildSolutionAsync(string solutionPath)
    {
        var buildErrors = new List<string>();

        if (!File.Exists(solutionPath))
        {
            buildErrors.Add($"Không tìm thấy file solution để build tại: {solutionPath}");
            return (false, buildErrors);
        }

        var directory = Path.GetDirectoryName(solutionPath) ?? "";

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"build \"{solutionPath}\" -c Debug --no-incremental",
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        try
        {
            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // Đọc song song stdout để lấy thông tin build
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            var output = outputTask.Result;
            var error = errorTask.Result;

            if (process.ExitCode != 0)
            {
                // Parse stdout/stderr để lấy chi tiết lỗi compiler (thường dòng có dạng: file.cs(dong,cot): error CSxxxx: Mo ta)
                var lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var errorRegex = new Regex(@":\s*error\s+CS\d+:", RegexOptions.IgnoreCase);

                foreach (var line in lines)
                {
                    if (errorRegex.IsMatch(line) || line.Contains("Build FAILED") || line.Contains("error :"))
                    {
                        buildErrors.Add(line.Trim());
                    }
                }

                // Nếu không lọc được lỗi nào bằng Regex nhưng exit code vẫn khác 0
                if (buildErrors.Count == 0)
                {
                    buildErrors.Add("dotnet build failed with exit code " + process.ExitCode);
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        buildErrors.Add(error.Trim());
                    }
                }

                return (false, buildErrors);
            }

            return (true, buildErrors);
        }
        catch (Exception ex)
        {
            buildErrors.Add($"Lỗi hệ thống khi gọi dotnet build: {ex.Message}");
            return (false, buildErrors);
        }
    }
}
