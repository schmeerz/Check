using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheckYourMind.Data;
using CheckYourMind.Models;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace CheckYourMind.Pages.Cases;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CreateModel> _logger;

    public CreateModel(ApplicationDbContext context, ILogger<CreateModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public Case Case { get; set; } = new();

    public IActionResult OnGet()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Попытка создания кейса неавторизованным пользователем");
            return RedirectToPage("/Login");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        _logger.LogInformation("Начало обработки POST-запроса для создания кейса");

        if (!User.Identity?.IsAuthenticated ?? true)
        {
            _logger.LogWarning("Попытка создания кейса неавторизованным пользователем");
            return RedirectToPage("/Login");
        }

        // Инициализируем коллекцию тестовых случаев, если она null
        Case.TestCases ??= new List<TestCase>();

        // Устанавливаем связь между кейсом и тестовыми случаями
        foreach (var testCase in Case.TestCases)
        {
            testCase.CaseId = Case.Id;
            testCase.Case = Case;
        }

        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            _logger.LogWarning("Невалидная модель при создании кейса: {Errors}", errors);
            return Page();
        }

        if (Case.TestCases == null || !Case.TestCases.Any())
        {
            _logger.LogWarning("Отсутствуют тестовые случаи");
            ModelState.AddModelError("", "Добавьте хотя бы один тестовый случай");
            return Page();
        }

        try
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (userId == 0)
            {
                _logger.LogError("Не удалось получить ID пользователя");
                return RedirectToPage("/Login");
            }

            _logger.LogInformation("Получен ID пользователя: {UserId}", userId);

            // Проверяем, существует ли пользователь
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogError("Пользователь с ID {UserId} не найден", userId);
                return RedirectToPage("/Login");
            }

            _logger.LogInformation("Пользователь найден: {UserName}", user.Name);

            Case.AuthorId = userId;
            Case.Author = user;
            Case.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation("Подготовлен кейс: Title={Title}, Category={Category}, Difficulty={Difficulty}, TestCases={TestCount}",
                Case.Title, Case.Category, Case.Difficulty, Case.TestCases.Count);

            // Проверяем уникальность названия кейса
            if (await _context.Cases.AnyAsync(c => c.Title == Case.Title))
            {
                _logger.LogWarning("Попытка создания кейса с существующим названием: {Title}", Case.Title);
                ModelState.AddModelError("Case.Title", "Кейс с таким названием уже существует");
                return Page();
            }

            // Проверяем валидность тестовых случаев
            foreach (var testCase in Case.TestCases)
            {
                if (string.IsNullOrWhiteSpace(testCase.Input) || 
                    string.IsNullOrWhiteSpace(testCase.ExpectedOutput) ||
                    string.IsNullOrWhiteSpace(testCase.Description))
                {
                    _logger.LogWarning("Невалидный тестовый случай: Input={Input}, ExpectedOutput={ExpectedOutput}, Description={Description}",
                        testCase.Input, testCase.ExpectedOutput, testCase.Description);
                    ModelState.AddModelError("", "Все поля тестового случая должны быть заполнены");
                    return Page();
                }
            }

            _logger.LogInformation("Начало сохранения кейса в базу данных");
            
            // Сначала сохраняем кейс
            _context.Cases.Add(Case);
            await _context.SaveChangesAsync();
            
            // Затем обновляем CaseId для всех тестовых случаев
            foreach (var testCase in Case.TestCases)
            {
                testCase.CaseId = Case.Id;
            }
            
            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Кейс успешно сохранен в базу данных. ID: {Id}", Case.Id);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка при сохранении кейса в базу данных");
                ModelState.AddModelError("", "Ошибка при сохранении кейса в базу данных. Пожалуйста, попробуйте позже.");
                return Page();
            }

            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Непредвиденная ошибка при создании кейса");
            ModelState.AddModelError("", "Произошла ошибка при создании кейса. Пожалуйста, попробуйте позже.");
            return Page();
        }
    }
} 