using SpotMate.Application.DTOs.HubModels;

namespace SpotMate.Web.Hubs;

public interface ILocationHub
{
    Task ReceiveFriendsLocation(IEnumerable<UserLocationModel> friends);
    Task ReceiveFriendLocationChanged(UserLocationModel friend);
}