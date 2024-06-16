using SpotMate.Application.DTOs.HubModels;

namespace SpotMate.Web.Hubs;

public interface ILocationHub
{
    Task ReceiveFriendsLocationAsync(IEnumerable<UserLocationModel> friends);
    Task ReceiveFriendLocationChangedAsync(UserLocationModel friend);
    Task ReceiveAddedFriendAsync(UserLocationModel friend);
    Task ReceiveDeletedFriendIdAsync(Guid userId);
}