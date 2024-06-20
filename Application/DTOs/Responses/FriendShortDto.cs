namespace SpotMate.Application.DTOs.Responses;

public sealed class FriendShortDto: UserShortDto
{
    public ChatShortDto? Chat { get; set; }
}