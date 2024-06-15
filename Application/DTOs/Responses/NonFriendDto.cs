using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Responses;

public sealed class NonFriendDto: UserShortDto
{
    [Required]
    public required bool HasFriendRequest { get; set; }
}