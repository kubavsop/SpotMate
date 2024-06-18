namespace SpotMate.Application.DTOs.HubModels;

public class SendMessageModel
{
    public required Guid ChatId { get; init; }
    public required string Text { get; init; }
}