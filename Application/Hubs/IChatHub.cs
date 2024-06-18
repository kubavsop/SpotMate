using SpotMate.Application.DTOs.Responses;

namespace SpotMate.Application.Hubs;

public interface IChatHub
{
    public Task ReceiveMessage(MessageDto messageDto);
}