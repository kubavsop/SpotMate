using SpotMate.Application.DTOs.HubModels;

namespace SpotMate.Application.Hubs;

public interface ILocationHub
{
    Task ReceiveFriendsLocation(IEnumerable<UserLocationModel> friends);
    Task ReceiveFriendLocationChanged(UserLocationModel friend);
    Task ReceiveAddedFriend(UserLocationModel friend);
    Task ReceiveDeletedFriendId(Guid userId);
}