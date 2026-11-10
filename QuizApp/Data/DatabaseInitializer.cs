using Microsoft.EntityFrameworkCore;

namespace QuizApp.Data;

/// <summary>Создаёт базу и таблицы по модели Code First при первом запуске.</summary>
public static class DatabaseInitializer
{
    public static void Initialize()
    {
        using var db = new AppDbContext();
        db.Database.EnsureCreated();
    }
}
