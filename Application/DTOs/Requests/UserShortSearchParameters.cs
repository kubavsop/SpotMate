using SpotMate.Application.DTOs.Base;

namespace SpotMate.Application.DTOs.Requests;

public class UserShortSearchParameters: BaseSearchParameters
{
    public string? UserName { get; init; }
}