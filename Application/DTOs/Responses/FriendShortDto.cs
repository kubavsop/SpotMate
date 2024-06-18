namespace SpotMate.Application.DTOs.Responses;

public sealed class FriendShortDto: UserShortDto
{
    public Guid? ChatId { get; set; }
}