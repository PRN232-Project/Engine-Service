using System.Collections.Generic;
using System.Threading.Tasks;
using PRN232.Domain.Entities;

namespace PRN232.GradingEngine.Application.Interfaces;

public interface IStaticCodeAnalyzer
{
    /// <summary>
    /// Checks the solution and project structure against naming rules, return list of violations.
    /// </summary>
    Task<List<string>> CheckNamingConventionsAsync(string workspacePath, string studentId, ExamRubric rubric);

    /// <summary>
    /// Checks for hardcoded connection strings inside code (DbContext, program configuration).
    /// </summary>
    Task<List<string>> CheckHardcodedConnectionStringAsync(string workspacePath);
}
