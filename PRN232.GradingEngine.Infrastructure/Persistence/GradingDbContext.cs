using Microsoft.EntityFrameworkCore;
using PRN232.Domain.Entities;

namespace PRN232.GradingEngine.Infrastructure.Persistence;

public class GradingDbContext : DbContext
{
    public GradingDbContext(DbContextOptions<GradingDbContext> options) : base(options)
    {
    }

    public DbSet<ExamRubric> ExamRubrics { get; set; }
    public DbSet<RequiredProject> RequiredProjects { get; set; }
    public DbSet<RequiredFile> RequiredFiles { get; set; }
    public DbSet<SubmissionAggregate> Submissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("grading");

        // Map ExamRubric and configure relational 1-N constraints
        modelBuilder.Entity<ExamRubric>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExamCode).IsRequired();
            entity.Property(e => e.SolutionPattern).IsRequired();

            entity.HasMany(e => e.RequiredProjects)
                  .WithOne(p => p.ExamRubric)
                  .HasForeignKey(p => p.ExamRubricId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.RequiredFiles)
                  .WithOne(f => f.ExamRubric)
                  .HasForeignKey(f => f.ExamRubricId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RequiredProject>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Pattern).IsRequired();
        });

        modelBuilder.Entity<RequiredFile>(entity =>
        {
            entity.HasKey(f => f.Id);
            entity.Property(f => f.Pattern).IsRequired();
        });

        // Map SubmissionAggregate lists to Postgres text[] array columns
        modelBuilder.Entity<SubmissionAggregate>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.StudentId).IsRequired();
            entity.Property(s => s.NamingViolations)
                .HasColumnType("text[]")
                .IsRequired();
            entity.Property(s => s.BuildErrors)
                .HasColumnType("text[]")
                .IsRequired();
            entity.Property(s => s.TestSectionResults)
                .HasColumnType("jsonb")
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                    v => DeserializeTestSectionResults(v)
                );
        });
    }

    private static System.Collections.Generic.List<PRN232.Domain.ValueObjects.TestSectionResult> DeserializeTestSectionResults(string v)
    {
        if (string.IsNullOrWhiteSpace(v))
            return new System.Collections.Generic.List<PRN232.Domain.ValueObjects.TestSectionResult>();
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.List<PRN232.Domain.ValueObjects.TestSectionResult>>(v, (System.Text.Json.JsonSerializerOptions)null) 
                ?? new System.Collections.Generic.List<PRN232.Domain.ValueObjects.TestSectionResult>();
        }
        catch
        {
            return new System.Collections.Generic.List<PRN232.Domain.ValueObjects.TestSectionResult>();
        }
    }
}
