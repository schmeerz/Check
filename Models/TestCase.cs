using System.ComponentModel.DataAnnotations;

namespace CheckYourMind.Models;

public class TestCase
{
    public int Id { get; set; }
    
    public int CaseId { get; set; }
    
    [Display(Name = "Кейс")]
    public Case? Case { get; set; }
    
    [Required(ErrorMessage = "Входные данные обязательны")]
    [Display(Name = "Входные данные")]
    public string Input { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Ожидаемый результат обязателен")]
    [Display(Name = "Ожидаемый результат")]
    public string ExpectedOutput { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "Описание тестового случая обязательно")]
    [Display(Name = "Описание")]
    public string Description { get; set; } = string.Empty;
} 