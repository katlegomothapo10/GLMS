using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using GLMS.Models;

namespace GLMS.Data
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Use SQL Server for migrations
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=GLMSDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}