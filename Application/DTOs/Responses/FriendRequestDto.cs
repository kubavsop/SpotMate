using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class FriendRequestDto
{
    public Guid Id { get; init; }
    public required UserShortDto User { get; init; }
    
    public required RequestStatus RequestStatus { get; init; }
}