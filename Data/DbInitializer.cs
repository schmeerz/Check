using CheckYourMind.Models;
using System.Text.Json;

namespace CheckYourMind.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        context.Database.EnsureCreated();

        // Проверяем, есть ли уже тестовый кейс
        if (context.Cases.Any())
        {
            return; // База данных уже заполнена
        }

        // Создаем тестового пользователя, если его нет
        var testUser = context.Users.FirstOrDefault(u => u.Name == "admin");
        if (testUser == null)
        {
            testUser = new User
            {
                Name = "admin",
                Password = "admin123" // В реальном приложении пароль должен быть хэширован
            };
            context.Users.Add(testUser);
            context.SaveChanges();
        }

        
        context.SaveChanges();
    }
} 