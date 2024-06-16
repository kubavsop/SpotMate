using SpotMate.Application.DTOs.Responses;

namespace SpotMate.Application.DTOs.HubModels;

public sealed class UserLocationModel: UserShortDto
{
    public required CoordinatesModel Coordinates { get; init; }
}