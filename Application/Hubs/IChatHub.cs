using SpotMate.Application.DTOs.Responses;

namespace SpotMate.Application.Hubs;

public interface IChatHub
{
    public Task ReceiveMessage(MessageDto messageDto);
    public Task ReceiveReadMessage(Guid chatId, Guid messageId);
    public Task ReceiveTyping(Guid chatId);
}