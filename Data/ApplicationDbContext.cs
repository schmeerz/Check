using Microsoft.EntityFrameworkCore;
using CheckYourMind.Models;

namespace CheckYourMind.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Case> Cases { get; set; }
    public DbSet<TestCase> TestCases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка связи между User и Case
        modelBuilder.Entity<Case>()
            .HasOne(c => c.Author)
            .WithMany(u => u.CreatedCases)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Настройка связи между Case и TestCase
        modelBuilder.Entity<TestCase>()
            .HasOne(t => t.Case)
            .WithMany(c => c.TestCases)
            .HasForeignKey(t => t.CaseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 