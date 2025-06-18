using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using CheckYourMind.Data;
using CheckYourMind.Models;
using Microsoft.EntityFrameworkCore;

namespace CheckYourMind.Pages;

public class RegisterModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public RegisterModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public required InputModel Input { get; set; }

    public class InputModel
    {
        [Required(ErrorMessage = "Введите имя пользователя")]
        [Display(Name = "Имя пользователя")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Введите пароль")]
        [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public required string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают.")]
        public required string ConfirmPassword { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var userExists = await _context.Users
                .Where(u => u.Name == Input.Name)
                .AnyAsync();

            if (userExists)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким именем уже существует.");
                return Page();
            }

            var user = new User
            {
                Name = Input.Name,
                Password = Input.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login");
        }

        return Page();
    }
} 