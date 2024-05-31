using System.ComponentModel.DataAnnotations;
using SpotMate.Application.ValidationAttributes;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Requests;

public sealed class CreateUserDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public required string UserName { get; set; }
    
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public required string FullName { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(1000)]
    public required string Email { get; set; }

    [Required]
    [MinLength(5)]
    [MaxLength(100)]
    public required string Password { get; set; }
    
    [Birthday]
    [Required]
    public DateTime Birthday     
    {
        get => _birthday;
        set => _birthday = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
    
    [Required]
    public Gender Gender { get; set; }
    
    private DateTime _birthday;
}