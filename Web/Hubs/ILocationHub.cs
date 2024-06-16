using SpotMate.Web.Hubs.Models;

namespace SpotMate.Web.Hubs;

public interface ILocationHub
{
    public Task GetFriendsLocation(IEnumerable<UserLocationModel> users);
}