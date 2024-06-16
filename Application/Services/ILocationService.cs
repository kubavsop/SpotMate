using SpotMate.Application.DTOs.HubModels;

namespace SpotMate.Application.Services;

public interface ILocationService
{
    Task<IEnumerable<UserLocationModel>> HandleOnConnectedAsync(Guid userId);
    Task HandleOnDisconnectedAsync(Guid userId);
    Task<IEnumerable<Guid>> GetFriendsToNotifyAsync(Guid userId);
    Task UpdateUserLocationAsync(Guid userId, CoordinatesModel coordinatesModel);
}