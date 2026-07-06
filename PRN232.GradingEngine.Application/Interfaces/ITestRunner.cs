using System.Collections.Generic;
using System.Threading.Tasks;
using PRN232.Domain.ValueObjects;

namespace PRN232.GradingEngine.Application.Interfaces;

public interface ITestRunner
{
    Task<TestSectionResult> RunApiTestsAsync(string solutionPath, string sectionName, decimal maxScore, List<ApiTestCase> testCases, string? apiProjectPath = null);
}
