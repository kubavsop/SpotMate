using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Responses;

public sealed class NonFriendDto: UserShortDto
{
    public bool? HasMyFriendRequest { get; set; }
}