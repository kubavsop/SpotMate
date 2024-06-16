using System.ComponentModel.DataAnnotations;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public class UserFullDto: UserShortDto
{
    [Required]
    public required string Email { get; init; }

    public DateTime? Birthday { get; init; }
    public Gender? Gender { get; init; }
    [Required]
    public required IEnumerable<InterestDto> Interests { get; init; }
}