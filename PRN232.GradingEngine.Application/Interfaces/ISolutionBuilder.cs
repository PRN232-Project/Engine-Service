using System.Collections.Generic;
using System.Threading.Tasks;

namespace PRN232.GradingEngine.Application.Interfaces;

public interface ISolutionBuilder
{
    /// <summary>
    /// Builds the .NET solution, returning whether it succeeded and any compile/build errors.
    /// </summary>
    Task<(bool Success, List<string> BuildErrors)> BuildSolutionAsync(string solutionPath);
}
