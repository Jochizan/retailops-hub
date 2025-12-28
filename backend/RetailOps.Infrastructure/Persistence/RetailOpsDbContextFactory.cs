using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RetailOps.Infrastructure.Persistence;

namespace RetailOps.Infrastructure.Persistence
{
    public class RetailOpsDbContextFactory : IDesignTimeDbContextFactory<RetailOpsDbContext>
    {
            public RetailOpsDbContext CreateDbContext(string[] args)
            {
                var optionsBuilder = new DbContextOptionsBuilder<RetailOpsDbContext>();
                optionsBuilder.UseSqlServer("Server=.;Database=RetailOpsDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=true");
        
                return new RetailOpsDbContext(optionsBuilder.Options);
            }    }
}
