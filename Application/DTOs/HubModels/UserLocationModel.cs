using SpotMate.Application.DTOs.Responses;

namespace SpotMate.Application.DTOs.HubModels;

public sealed class UserLocationModel: UserShortDto
{
    public required CoordinatesModel Coordinate { get; set; }
    public ChatShortDto? Chat { get; set; }
}