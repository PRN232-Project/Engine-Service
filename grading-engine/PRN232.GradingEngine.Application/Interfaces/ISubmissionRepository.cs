using System;
using System.Threading.Tasks;
using PRN232.Domain.Entities;

namespace PRN232.GradingEngine.Application.Interfaces;

public interface ISubmissionRepository
{
    Task<SubmissionAggregate?> GetByIdAsync(Guid id);
    Task AddAsync(SubmissionAggregate submission);
    Task UpdateAsync(SubmissionAggregate submission);
    Task SaveChangesAsync();
}
