using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Responses;

public sealed class NonFriendDto
{
    [Required]
    public required UserShortDto UserShort { get; set; }
    
    [Required]
    public required bool HasFriendRequest { get; set; }
}