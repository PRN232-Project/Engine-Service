using Microsoft.EntityFrameworkCore;

namespace PRN231_SU25_SE182004.api;

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // --- CASE A: BÀI LÀM HỢP LỆ (Không hardcode connection string) ---
        // Connection string được cấu hình qua appsettings.json và inject vào Program.cs
        if (!optionsBuilder.IsConfigured)
        {
             // Code hợp lệ: không làm gì hoặc đọc từ cấu hình.
        }

        // --- CASE B: BÀI LÀM VI PHẠM ---
        // optionsBuilder.UseSqlServer("Host=db.supabase.co;Database=SU25LeopardDB;User Id=postgres;Password=Thinhtran2412;");
    }
}
