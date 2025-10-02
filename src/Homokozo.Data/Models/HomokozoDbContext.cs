using Microsoft.EntityFrameworkCore;

public class HomokozoDbContext(DbContextOptions<HomokozoDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    // {
    //     optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=homokozo_db;Username=user;Password=password");
    // }
}
