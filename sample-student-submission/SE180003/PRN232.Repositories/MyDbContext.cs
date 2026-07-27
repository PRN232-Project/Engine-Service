
using Microsoft.EntityFrameworkCore;
namespace PRN232.Repositories {
    public class MyDbContext : DbContext {
        public DbSet<Product> Products { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
            // [HARDCODE_CONNECTION_STRING_PLACEHOLDER]
        }
    }
    
    public class ProductRepo {
        private static readonly System.Collections.Generic.List<Product> _db = new();
        public System.Collections.Generic.List<Product> GetAll() => _db;
        public Product Add(Product p) { _db.Add(p); return p; }
    }
}
