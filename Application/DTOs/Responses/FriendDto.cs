using System.ComponentModel.DataAnnotations;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class FriendDto: UserShortDto
{
    [Required]
    public bool IsLocationFrozen { get; set; }
    
    [Required]
    public required string Email { get; init; }
    
    public DateTime? Birthday { get; init; }
    public Gender? Gender { get; init; }
    
    public ChatShortDto? Chat { get; set; }
    
    [Required]
    public required IEnumerable<InterestDto> Interests { get; init; }
}