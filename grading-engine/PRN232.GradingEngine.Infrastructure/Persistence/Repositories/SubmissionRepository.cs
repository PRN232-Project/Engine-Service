using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PRN232.Domain.Entities;
using PRN232.GradingEngine.Application.Interfaces;

namespace PRN232.GradingEngine.Infrastructure.Persistence.Repositories;

public class SubmissionRepository : ISubmissionRepository
{
    private readonly GradingDbContext _context;

    public SubmissionRepository(GradingDbContext context)
    {
        _context = context;
    }

    public async Task<SubmissionAggregate?> GetByIdAsync(Guid id)
    {
        return await _context.Submissions.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task AddAsync(SubmissionAggregate submission)
    {
        await _context.Submissions.AddAsync(submission);
    }

    public Task UpdateAsync(SubmissionAggregate submission)
    {
        _context.Submissions.Update(submission);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
