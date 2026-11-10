using Microsoft.EntityFrameworkCore;
using QuizApp.Models;

namespace QuizApp.Data;

/// <summary>
/// Контекст EF Core: представляет сессию работы с базой данных и набор сущностей.
/// </summary>
public class AppDbContext : DbContext
{
    private const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=QuizAppDb;Integrated Security=true;TrustServerCertificate=True;";

    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(ConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<QuizQuestion>(entity =>
        {
            entity.ToTable("QuizQuestions", table =>
                table.HasCheckConstraint(
                    "CK_QuizQuestions_Difficulty",
                    "[Difficulty] IN (N'Легко', N'Средне', N'Сложно')"));

            entity.HasKey(q => q.QuestionId);
            entity.Property(q => q.Text)
                .HasColumnName("QuestionText")
                .HasMaxLength(1000)
                .IsRequired();
            entity.Property(q => q.Difficulty)
                .HasMaxLength(20)
                .IsRequired();
        });
    }
}
