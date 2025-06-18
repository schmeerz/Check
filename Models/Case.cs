using System.ComponentModel.DataAnnotations;

namespace CheckYourMind.Models;

public class Case
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "Название кейса обязательно")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Название должно быть от 3 до 100 символов")]
    [Display(Name = "Название")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Описание кейса обязательно")]
    [MinLength(50, ErrorMessage = "Описание должно содержать минимум 50 символов")]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Укажите сложность кейса")]
    [Range(1, 5, ErrorMessage = "Сложность должна быть от 1 до 5")]
    [Display(Name = "Сложность")]
    public int Difficulty { get; set; }
    
    [Required(ErrorMessage = "Выберите категорию кейса")]
    [Display(Name = "Категория")]
    public string Category { get; set; } = string.Empty;
    
    [Display(Name = "Дата создания")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [Display(Name = "Автор")]
    public int AuthorId { get; set; }
    
    [Display(Name = "Автор")]
    public User? Author { get; set; }
    
    [Display(Name = "Тестовые случаи")]
    public ICollection<TestCase> TestCases { get; set; } = new List<TestCase>();
} 