using SpotMate.Application.DTOs.Responses;

namespace SpotMate.Application.DTOs.HubModels;

public sealed class UserLocationModel: UserShortDto
{
    public required CoordinatesModel Coordinate { get; init; }
    public Guid? ChatId { get; set; }
}