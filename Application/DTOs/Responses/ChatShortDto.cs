using System.ComponentModel.DataAnnotations;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class ChatShortDto
{
    [Required]
    public required Guid Id { get; init; }
    
    [Required]
    public required string Title { get; init; }
    
    public string? Avatar { get; set; }
    
    public UserStatus? UserStatus { get; set; }
    
    public DateTime? LastOnline { get; set; }
}