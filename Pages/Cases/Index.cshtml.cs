using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CheckYourMind.Data;
using CheckYourMind.Models;
using Microsoft.Extensions.Logging;

namespace CheckYourMind.Pages.Cases;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApplicationDbContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public IList<Case> Cases { get; set; } = new List<Case>();

    public async Task OnGetAsync()
    {
        try
        {
            // Проверяем, есть ли кейсы в базе
            if (!await _context.Cases.AnyAsync())
            {
                // Получаем или создаем тестового пользователя
                var admin = await _context.Users.FirstOrDefaultAsync(u => u.Name == "admin");
                if (admin == null)
                {
                    admin = new User { Name = "admin", Password = "admin123" };
                    _context.Users.Add(admin);
                    await _context.SaveChangesAsync();
                }

                // Создаем тестовый кейс
                var testCase = new Case
                {
                    Title = "Поиск подстроки в строке",
                    Description = "Реализуйте алгоритм поиска подстроки в строке без использования встроенных методов поиска.\n\n" +
                                "Требования:\n" +
                                "1. Не использовать String.Contains, String.IndexOf и подобные методы\n" +
                                "2. Реализовать алгоритм Кнута-Морриса-Пратта (KMP)\n" +
                                "3. Временная сложность должна быть O(n + m), где n - длина строки, m - длина подстроки\n\n" +
                                "Пример:\n" +
                                "Input: s = \"hello world\", pattern = \"world\"\n" +
                                "Output: 6 (индекс начала подстроки)",
                    Difficulty = 4,
                    Category = "Алгоритмы",
                    AuthorId = admin.Id,
                    CreatedAt = DateTime.UtcNow,
                    TestCases = new List<TestCase>
                    {
                        new TestCase
                        {
                            Input = "s = \"hello world\", pattern = \"world\"",
                            ExpectedOutput = "6",
                            Description = "Базовый случай поиска подстроки"
                        },
                        new TestCase
                        {
                            Input = "s = \"mississippi\", pattern = \"issi\"",
                            ExpectedOutput = "1",
                            Description = "Поиск подстроки с повторяющимися символами"
                        },
                        new TestCase
                        {
                            Input = "s = \"aaaaa\", pattern = \"aaa\"",
                            ExpectedOutput = "0",
                            Description = "Поиск подстроки в строке с повторяющимися символами"
                        },
                        new TestCase
                        {
                            Input = "s = \"hello world\", pattern = \"xyz\"",
                            ExpectedOutput = "-1",
                            Description = "Подстрока не найдена"
                        }
                    }
                };

                _context.Cases.Add(testCase);
                await _context.SaveChangesAsync();
            }

            Cases = await _context.Cases
                .Include(c => c.Author)
                .Include(c => c.TestCases)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            _logger.LogInformation($"Found {Cases.Count} cases in the database");
            foreach (var caseItem in Cases)
            {
                _logger.LogInformation($"Case: {caseItem.Title}, Difficulty: {caseItem.Difficulty}, Tests: {caseItem.TestCases.Count}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading cases");
        }
    }
} 