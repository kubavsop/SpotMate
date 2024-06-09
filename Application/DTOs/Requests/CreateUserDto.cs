using System.ComponentModel.DataAnnotations;
using SpotMate.Application.ValidationAttributes;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Requests;

public sealed class CreateUserDto
{
    [Required]
    [MinLength(1)]
    [MaxLength(1000)]
    public required string UserName { get; init; }

    [Required]
    [EmailAddress]
    [MaxLength(1000)]
    public required string Email { get; init; }

    [Required]
    [MinLength(5)]
    [MaxLength(100)]
    public required string Password { get; init; }
}