using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PRN232.Domain.ValueObjects;
using PRN232.GradingEngine.Application.Interfaces;

namespace PRN232.GradingEngine.Infrastructure.TestExecution;

public class DynamicApiTestRunner : ITestRunner
{
    public async Task<TestSectionResult> RunApiTestsAsync(string solutionPath, string sectionName, decimal maxScore, List<ApiTestCase> testCases, string? apiProjectPath = null)
    {
        var executionLog = new StringBuilder();
        var failedTests = new List<string>();
        int passedCount = 0;
        int totalCount = testCases.Count;

        if (totalCount == 0)
        {
            return new TestSectionResult(sectionName, maxScore, 0, 0, "Không có test case nào cấu hình.", failedTests);
        }

        // Giải phóng cổng 5000 và các tiến trình khóa tệp trước khi chạy API
        KillPortAndProjectsProcesses(solutionPath);

        // 1. Tìm hoặc sử dụng project API được chỉ định
        string? apiProject = null;
        if (!string.IsNullOrEmpty(apiProjectPath))
        {
            var fullPath = Path.Combine(solutionPath, apiProjectPath);
            if (File.Exists(fullPath))
            {
                apiProject = fullPath;
            }
            else
            {
                // Fallback nếu truyền nhầm file tương đối nhưng ghi sẵn tuyệt đối
                if (File.Exists(apiProjectPath))
                {
                    apiProject = apiProjectPath;
                }
            }
        }

        if (string.IsNullOrEmpty(apiProject))
        {
            apiProject = FindApiProject(solutionPath);
        }

        if (string.IsNullOrEmpty(apiProject))
        {
            var errMsg = "Không tìm thấy project .csproj của API sinh viên để khởi chạy.";
            executionLog.AppendLine(errMsg);
            failedTests.Add("System Error: API project not found");
            return new TestSectionResult(sectionName, maxScore, 0, 1, executionLog.ToString(), failedTests);
        }

        executionLog.AppendLine($"--> Tìm thấy API Project: {Path.GetFileName(apiProject)}");
        executionLog.AppendLine($"--> Khởi chạy dự án API ở cổng http://localhost:5000 ...");

        // 2. Chạy dự án API ở background
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{apiProject}\" --urls \"http://localhost:5000\"",
            WorkingDirectory = Path.GetDirectoryName(apiProject),
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process? apiProcess = null;
        try
        {
            apiProcess = Process.Start(startInfo);
            if (apiProcess == null)
            {
                throw new Exception("Không thể khởi động tiến trình dotnet run.");
            }

            // 3. Đợi API khởi động (Thử kết nối tối đa 25 lần, mỗi lần cách nhau 400ms = 10s)
            using var client = new HttpClient();
            bool isStarted = false;
            for (int i = 0; i < 25; i++)
            {
                try
                {
                    // Thử gửi request rỗng
                    await client.GetAsync("http://localhost:5000");
                    isStarted = true;
                    break;
                }
                catch
                {
                    await Task.Delay(400);
                }
            }

            if (!isStarted)
            {
                executionLog.AppendLine("Lỗi: Quá thời gian khởi động API (Timeout 10s). API của sinh viên có thể bị lỗi cú pháp hoặc cổng 5000 bị chiếm dụng.");
                failedTests.Add("Startup Error: API failed to start within 10 seconds.");
                return new TestSectionResult(sectionName, maxScore, 0, 1, executionLog.ToString(), failedTests);
            }

            executionLog.AppendLine("--> API đã chạy thành công! Bắt đầu gửi HTTP requests...");

            // Khởi tạo danh sách biến động (Variables Context)
            var variables = new Dictionary<string, string>();

            // 4. Chạy từng Test Case
            foreach (var tc in testCases)
            {
                var tcPassed = true;
                var tcLog = new StringBuilder();

                // Thế biến cho URL
                var evaluatedUrlPath = ReplaceVariables(tc.UrlPath, variables);
                tcLog.AppendLine($"\n[RUNNING] TestCase: {tc.Name} | {tc.Method} http://localhost:5000{evaluatedUrlPath}");

                try
                {
                    using var requestMessage = new HttpRequestMessage(new HttpMethod(tc.Method), $"http://localhost:5000{evaluatedUrlPath}");
                    
                    // Gán custom headers và thế biến cho values
                    if (tc.Headers != null)
                    {
                        foreach (var header in tc.Headers)
                        {
                            var headerName = ReplaceVariables(header.Name, variables);
                            var headerValue = ReplaceVariables(header.Value, variables);
                            if (!string.IsNullOrWhiteSpace(headerName))
                            {
                                requestMessage.Headers.TryAddWithoutValidation(headerName, headerValue);
                                tcLog.AppendLine($"Request Header: {headerName} = {headerValue}");
                            }
                        }
                    }

                    // Thế biến cho Body
                    var evaluatedBody = ReplaceVariables(tc.RequestBody, variables);
                    if (!string.IsNullOrWhiteSpace(evaluatedBody) && (tc.Method == "POST" || tc.Method == "PUT"))
                    {
                        requestMessage.Content = new StringContent(evaluatedBody, Encoding.UTF8, "application/json");
                        tcLog.AppendLine($"Request Body: {evaluatedBody}");
                    }

                    var response = await client.SendAsync(requestMessage);
                    var statusCode = (int)response.StatusCode;
                    var responseBody = await response.Content.ReadAsStringAsync();

                    tcLog.AppendLine($"Response Status: {statusCode} {response.StatusCode}");
                    if (!string.IsNullOrEmpty(responseBody))
                    {
                        tcLog.AppendLine($"Response Body: {responseBody}");
                    }

                    // Check Status Code
                    if (statusCode != tc.ExpectedStatusCode)
                    {
                        tcPassed = false;
                        var err = $"Mã HTTP trả về không khớp. Kỳ vọng: {tc.ExpectedStatusCode}, Thực tế: {statusCode}";
                        tcLog.AppendLine($"[FAILED] {err}");
                        failedTests.Add($"{tc.Name}: {err}");
                    }
                    else
                    {
                        // Check Json Assertions (Nếu status code khớp và có cấu hình Assertions)
                        if (tc.JsonAssertions != null && tc.JsonAssertions.Count > 0 && !string.IsNullOrWhiteSpace(responseBody))
                        {
                            try
                            {
                                var jsonToken = JToken.Parse(responseBody);
                                foreach (var assert in tc.JsonAssertions)
                                {
                                    var token = jsonToken.SelectToken(assert.JsonPath);
                                    var actualValue = token?.ToString();

                                    tcLog.AppendLine($"Asserting Path '{assert.JsonPath}': Actual='{actualValue}', Expected='{assert.ExpectedValue}' (Op: {assert.Operator})");

                                    var assertOk = false;
                                    switch (assert.Operator.ToLower())
                                    {
                                        case "equal":
                                            assertOk = string.Equals(actualValue, assert.ExpectedValue, StringComparison.OrdinalIgnoreCase);
                                            break;
                                        case "notnull":
                                            assertOk = token != null && token.Type != JTokenType.Null;
                                            break;
                                        case "contains":
                                            assertOk = actualValue != null && actualValue.Contains(assert.ExpectedValue, StringComparison.OrdinalIgnoreCase);
                                            break;
                                        case "empty":
                                            assertOk = token == null || !token.Any();
                                            break;
                                        default:
                                            assertOk = false;
                                            break;
                                    }

                                    if (!assertOk)
                                    {
                                        tcPassed = false;
                                        var err = $"So khớp dữ liệu thất bại tại '{assert.JsonPath}'. Thực tế: '{actualValue}' (Kỳ vọng {assert.Operator}: '{assert.ExpectedValue}')";
                                        tcLog.AppendLine($"[FAILED] {err}");
                                        failedTests.Add($"{tc.Name}: {err}");
                                        break; // Dừng check các assert tiếp theo của test case này
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                tcPassed = false;
                                var err = $"Lỗi khi parse/query dữ liệu JSON: {ex.Message}";
                                tcLog.AppendLine($"[FAILED] {err}");
                                failedTests.Add($"{tc.Name}: {err}");
                            }
                        }

                        // Trích xuất biến động từ Response
                        if (tcPassed && tc.ExtractVariables != null && tc.ExtractVariables.Count > 0 && !string.IsNullOrWhiteSpace(responseBody))
                        {
                            try
                            {
                                var jsonToken = JToken.Parse(responseBody);
                                foreach (var ext in tc.ExtractVariables)
                                {
                                    if (!string.IsNullOrWhiteSpace(ext.JsonPath) && !string.IsNullOrWhiteSpace(ext.VariableName))
                                    {
                                        var token = jsonToken.SelectToken(ext.JsonPath);
                                        var val = token?.ToString();
                                        if (val != null)
                                        {
                                            variables[ext.VariableName] = val;
                                            tcLog.AppendLine($"[EXTRACTED] Đã lưu biến '{ext.VariableName}' = '{val}'");
                                        }
                                        else
                                        {
                                            tcLog.AppendLine($"[WARNING] Trích xuất thất bại tại JsonPath '{ext.JsonPath}'. Không tìm thấy dữ liệu.");
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                tcLog.AppendLine($"[WARNING] Lỗi trích xuất biến: {ex.Message}");
                            }
                        }
                    }

                    if (tcPassed)
                    {
                        tcLog.AppendLine("[SUCCESS] Pass!");
                        passedCount++;
                    }
                }
                catch (Exception ex)
                {
                    tcPassed = false;
                    var err = $"Lỗi kết nối khi gửi Request: {ex.Message}";
                    tcLog.AppendLine($"[FAILED] {err}");
                    failedTests.Add($"{tc.Name}: {err}");
                }

                executionLog.Append(tcLog.ToString());
            }
        }
        catch (Exception ex)
        {
            var errMsg = $"Lỗi nghiêm trọng trong hệ thống Test Runner: {ex.Message}";
            executionLog.AppendLine(errMsg);
            failedTests.Add("System Exception in Runner");
        }
        finally
        {
            // 5. Tắt tiến trình API chạy ngầm
            if (apiProcess != null && !apiProcess.HasExited)
            {
                executionLog.AppendLine("--> Đang tắt tiến trình API sinh viên...");
                try
                {
                    apiProcess.Kill(true);
                }
                catch { }
            }
        }

        executionLog.AppendLine($"\n==== TỔNG HỢP KẾT QUẢ: Pass {passedCount}/{totalCount} ====");
        return new TestSectionResult(sectionName, maxScore, passedCount, totalCount, executionLog.ToString(), failedTests);
    }

    private string? FindApiProject(string workspacePath)
    {
        var csprojFiles = Directory.GetFiles(workspacePath, "*.csproj", SearchOption.AllDirectories);
        if (csprojFiles.Length == 0) return null;

        // Ưu tiên project có tên chứa Api/API hoặc Web
        var apiProject = csprojFiles.FirstOrDefault(f => f.Contains("Api", StringComparison.OrdinalIgnoreCase) || f.Contains("Web", StringComparison.OrdinalIgnoreCase));
        if (apiProject != null) return apiProject;

        return csprojFiles.First();
    }

    private string ReplaceVariables(string? text, Dictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        var result = text;
        foreach (var kv in variables)
        {
            result = result.Replace($"{{{{{kv.Key}}}}}", kv.Value);
        }

        // Thay thế các biến động hệ thống (system dynamic variables) tương tự Postman
        if (result.Contains("{{$guid}}"))
        {
            result = result.Replace("{{$guid}}", Guid.NewGuid().ToString());
        }
        if (result.Contains("{{$timestamp}}"))
        {
            result = result.Replace("{{$timestamp}}", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        }
        if (result.Contains("{{$randomInt}}"))
        {
            var rnd = new Random();
            result = result.Replace("{{$randomInt}}", rnd.Next(1000, 9999).ToString());
        }

        return result;
    }

    private void KillPortAndProjectsProcesses(string solutionPath)
    {
        // 1. Kill các tiến trình trùng tên với các project .csproj trong thư mục solution
        try
        {
            var solutionDir = Path.GetDirectoryName(solutionPath);
            if (!string.IsNullOrEmpty(solutionDir) && Directory.Exists(solutionDir))
            {
                var csprojFiles = Directory.GetFiles(solutionDir, "*.csproj", SearchOption.AllDirectories);
                foreach (var csproj in csprojFiles)
                {
                    var procName = Path.GetFileNameWithoutExtension(csproj);
                    if (string.Equals(procName, "dotnet", StringComparison.OrdinalIgnoreCase)) continue;

                    foreach (var proc in Process.GetProcessesByName(procName))
                    {
                        try
                        {
                            proc.Kill(true);
                        }
                        catch { }
                    }
                }
            }
        }
        catch { }

        // 2. Kill bất kỳ tiến trình nào đang lắng nghe cổng 5000 (để giải phóng cổng và tệp tin)
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = "/c netstat -ano | findstr :5000",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var process = Process.Start(startInfo);
            if (process != null)
            {
                var output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                var lines = output.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    if (line.Contains("LISTENING"))
                    {
                        var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length > 0)
                        {
                            var pidStr = parts[parts.Length - 1];
                            if (int.TryParse(pidStr, out int pid))
                            {
                                try
                                {
                                    var proc = Process.GetProcessById(pid);
                                    if (proc.Id != Process.GetCurrentProcess().Id)
                                    {
                                        proc.Kill(true);
                                    }
                                }
                                catch { }
                            }
                        }
                    }
                }
            }
        }
        catch { }
    }
}
