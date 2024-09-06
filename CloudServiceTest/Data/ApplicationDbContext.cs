using CloudServiceTest.Models;
using CloudServiceTest.Models.Database;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CloudServiceTest.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // 在这里添加其他 DbSet<T>，用于其他实体
        public DbSet<FileRecord> FileRecords { get; set; }
    }
}
