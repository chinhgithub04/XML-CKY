using BTCK_CNXML.Models;
using Microsoft.EntityFrameworkCore;

namespace BTCK_CNXML.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<LoaiHoa> LoaiHoas { get; set; }
        public DbSet<Hoa> Hoas { get; set; }
        public DbSet<DatHoa> DatHoas { get; set; }
        public DbSet<Image> Images { get; set; }

    }
}
