using System.Collections.Generic;
using System.Threading.Tasks;
using PRN232.Domain.Entities;

namespace PRN232.GradingEngine.Application.Interfaces;

public interface IExamRubricRepository
{
    Task<ExamRubric?> GetByIdAsync(Guid id);
    Task<ExamRubric?> GetByExamCodeAsync(string examCode);
    Task<List<ExamRubric>> GetAllAsync();
    Task AddAsync(ExamRubric rubric);
    Task SaveChangesAsync();
}
