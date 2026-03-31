using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    
    public DbSet<User> Users { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.HasIndex(u => u.Email)
                  .IsUnique();              

            entity.Property(u => u.Email)
                  .IsRequired()
                  .HasMaxLength(254);

            entity.Property(u => u.DisplayName)
                  .HasMaxLength(100);
        });

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(t => t.Id);

            entity.Property(t => t.Title)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(t => t.Details)
                  .HasMaxLength(1000);

            entity.Property(t => t.Priority)
                  .HasConversion<string>()   
                  .IsRequired();

    
            entity.HasOne(t => t.User)
                  .WithMany(u => u.Todos)
                  .HasForeignKey(t => t.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}