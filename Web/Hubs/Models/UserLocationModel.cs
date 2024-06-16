using SpotMate.Application.DTOs.Responses;

namespace SpotMate.Web.Hubs.Models;

public sealed class UserLocationModel: UserShortDto
{
    public required CoordinatesModel Coordinates { get; init; }
}