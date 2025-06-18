using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CheckYourMind.Data;
using CheckYourMind.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Text;

namespace CheckYourMind.Pages.Cases;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(ApplicationDbContext context, ILogger<DetailsModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Case? Case { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Введите код на Go")]
    public string GoCode { get; set; } = string.Empty;

    public string? ExecutionResult { get; set; }
    public List<TestResult>? TestResults { get; set; }

    public class TestResult
    {
        public string TestName { get; set; } = string.Empty;
        public string Input { get; set; } = string.Empty;
        public string ExpectedOutput { get; set; } = string.Empty;
        public string ActualOutput { get; set; } = string.Empty;
        public bool Passed { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            _logger.LogWarning("ID кейса не указан");
            return NotFound();
        }

        _logger.LogInformation("Загрузка кейса с ID: {Id}", id);

        Case = await _context.Cases
            .Include(c => c.TestCases)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (Case == null)
        {
            _logger.LogWarning("Кейс с ID {Id} не найден", id);
            return NotFound();
        }

        _logger.LogInformation("Кейс загружен: {Title}, Тестов: {TestCount}, Описание: {Description}", 
            Case.Title, 
            Case.TestCases?.Count ?? 0,
            Case.Description);

        if (Case.TestCases != null)
        {
            foreach (var test in Case.TestCases)
            {
                _logger.LogInformation("Тест: {Id}, Вход: {Input}, Ожидаемый вывод: {Expected}", 
                    test.Id, test.Input, test.ExpectedOutput);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            _logger.LogWarning("ID кейса не указан при POST-запросе");
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Модель невалидна");
            return Page();
        }

        // Загружаем кейс
        Case = await _context.Cases
            .Include(c => c.TestCases)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (Case == null)
        {
            _logger.LogWarning("Кейс с ID {Id} не найден при POST-запросе", id);
            return NotFound();
        }

        try
        {
            // Создаем временный файл для Go кода
            var tempFile = Path.GetTempFileName() + ".go";
            await System.IO.File.WriteAllTextAsync(tempFile, GoCode);

            // Получаем тестовые случаи
            var testCases = await _context.TestCases
                .Where(tc => tc.CaseId == Case.Id)
                .ToListAsync();

            _logger.LogInformation("Загружено тестовых случаев: {Count}", testCases.Count);

            var results = new List<TestResult>();

            // Запускаем Go код для каждого тестового случая
            foreach (var testCase in testCases)
            {
                _logger.LogInformation("Запуск теста: {Input} -> {Expected}", 
                    testCase.Input, 
                    testCase.ExpectedOutput);

                var startInfo = new ProcessStartInfo
                {
                    FileName = "go",
                    Arguments = $"run {tempFile} \"{testCase.Input}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = Process.Start(startInfo);
                if (process == null)
                {
                    throw new Exception("Не удалось запустить процесс Go");
                }

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                var actualOutput = ExtractArrayOutput(output);
                var passed = actualOutput == testCase.ExpectedOutput;

                _logger.LogInformation("Результат теста: {Passed}, Получено: {Actual}", 
                    passed, 
                    actualOutput);

                results.Add(new TestResult
                {
                    TestName = $"Тест {testCase.Id}",
                    Input = testCase.Input,
                    ExpectedOutput = testCase.ExpectedOutput,
                    ActualOutput = actualOutput,
                    Passed = passed
                });
            }

            TestResults = results;

            // Формируем общий результат
            var allTestsPassed = TestResults.All(t => t.Passed);
            ExecutionResult = allTestsPassed
                ? "Все тесты пройдены успешно!"
                : $"Провалено тестов: {TestResults.Count(t => !t.Passed)} из {TestResults.Count}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выполнении Go кода");
            ExecutionResult = $"Ошибка выполнения: {ex.Message}";
        }

        return Page();
    }

    private string ExtractArrayOutput(string output)
    {
        // Удаляем "Sorted: " из начала строки
        var cleanOutput = output.Trim().Replace("Sorted: ", "");
        
        // Удаляем квадратные скобки
        cleanOutput = cleanOutput.Trim('[', ']');
        
        // Разбиваем на числа и собираем обратно с правильными пробелами
        var numbers = cleanOutput.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", numbers);
    }
} 