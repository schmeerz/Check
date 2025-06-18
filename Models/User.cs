using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CheckYourMind.Models;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
    
    // Связь с кейсами, созданными пользователем
    public ICollection<Case> CreatedCases { get; set; } = new List<Case>();
} 