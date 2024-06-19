using System.ComponentModel.DataAnnotations;
using SpotMate.Application.ValidationAttributes;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Requests;

public class EditUserDto
{
    [MinLength(1)]
    [MaxLength(1000)]
    [Required]
    public required string FullName { get; init; }
    
    [Required]
    [MaxLength(10)]
    public required IEnumerable<Guid> Interests { get; init; } 
    
    [Birthday]
    [Required]
    public required DateTime Birthday     
    {
        get => _birthday;
        set => _birthday = DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
    [Required]
    public required Gender Gender { get; init; }
    
    public UserStatus? UserStatus { get; init; }
    
    private DateTime _birthday;
}