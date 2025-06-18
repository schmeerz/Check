using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CheckYourMind.Data;
using CheckYourMind.Models;
using System.ComponentModel.DataAnnotations;

namespace CheckYourMind.Pages.Account;

public class LoginModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public LoginModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Name == Input.Name && u.Password == Input.Password);

            if (user != null)
            {
                // В реальном приложении здесь нужно создать сессию/куки
                return RedirectToPage("/Index");
            }
            
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        return Page();
    }
} 