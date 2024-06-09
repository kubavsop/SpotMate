using System.ComponentModel;
using SpotMate.Application.DTOs.Base;

namespace SpotMate.Application.DTOs.Requests;

public class UserSearchParameters: BaseSearchParameters
{
    public string? UserName { get; init; }
    public IEnumerable<Guid>?  Interests { get; init; }
    [DefaultValue(false)]
    public bool IsInterestMatch { get; init; } = false;
}