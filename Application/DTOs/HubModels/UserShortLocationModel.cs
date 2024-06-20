using SpotMate.Application.DTOs.Responses;

namespace SpotMate.Application.DTOs.HubModels;

public sealed class UserShortLocationModel: UserShortDto
{
    public required CoordinatesModel Coordinate { get; set; }
}