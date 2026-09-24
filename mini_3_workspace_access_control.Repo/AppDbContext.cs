using Microsoft.EntityFrameworkCore;

namespace mini_3_workspace_access_control.Repo;

public class AppDbContext : DbContext
{
    public  AppDbContext(DbContextOptions options) : base(options)
    {}
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        
        //modelBuilder.SeedData();
    }
}