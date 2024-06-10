using SpotMate.Application.DTOs.Base;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Requests;

public sealed class RequestSearchParameters: BaseSearchParameters
{
    public string? UserName { get; init; }
    public RequestStatus? RequestStatus { get; init; }
}