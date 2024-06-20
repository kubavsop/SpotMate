using SpotMate.Application.DTOs.HubModels;

namespace SpotMate.Application.Hubs;

public interface ILocationHub
{
    Task ReceiveFriendLocationChanged(UserLocationModel friend);
    Task ReceiveAddedFriend(UserLocationModel friend);
    Task ReceiveDeletedFriendId(Guid userId);
    Task ReceiveUserOfSimilarInterestsLocationChanged(UserLocationModel user);
    Task ReceiveAddedUserOfSimilarInterests(UserLocationModel user);
    Task ReceiveDeletedUserOfSimilarInterestsId(Guid userId);
}


