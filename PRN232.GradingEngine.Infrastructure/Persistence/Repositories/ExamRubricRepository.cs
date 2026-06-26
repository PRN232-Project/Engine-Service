using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRN232.Domain.Entities;
using PRN232.GradingEngine.Application.Interfaces;

namespace PRN232.GradingEngine.Infrastructure.Persistence.Repositories;

public class ExamRubricRepository : IExamRubricRepository
{
    private readonly GradingDbContext _context;

    public ExamRubricRepository(GradingDbContext context)
    {
        _context = context;
    }

    public async Task<ExamRubric?> GetByIdAsync(Guid id)
    {
        return await _context.ExamRubrics
            .Include(r => r.RequiredProjects)
            .Include(r => r.RequiredFiles)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ExamRubric?> GetByExamCodeAsync(string examCode)
    {
        return await _context.ExamRubrics
            .Include(r => r.RequiredProjects)
            .Include(r => r.RequiredFiles)
            .FirstOrDefaultAsync(r => r.ExamCode == examCode);
    }

    public async Task AddAsync(ExamRubric rubric)
    {
        await _context.ExamRubrics.AddAsync(rubric);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
