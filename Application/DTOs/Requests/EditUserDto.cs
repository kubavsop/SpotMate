using System.ComponentModel.DataAnnotations;
using SpotMate.Application.ValidationAttributes;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Requests;

public class EditUserDto
{
    [MinLength(1)]
    [MaxLength(1000)]
    public string? FullName { get; init; }
    
    [Required]
    [MaxLength(10)]
    public required IEnumerable<Guid> Interests { get; init; } 
    
    [Birthday]
    public DateTime? Birthday     
    {
        get => _birthday;
        set => _birthday = value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value;
    }
    
    public Gender? Gender { get; init; }
    
    public UserStatus? UserStatus { get; init; }
    
    private DateTime? _birthday;
}