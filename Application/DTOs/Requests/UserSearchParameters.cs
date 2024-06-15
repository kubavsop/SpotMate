using System.ComponentModel;

namespace SpotMate.Application.DTOs.Requests;

public class UserSearchParameters: UserShortSearchParameters
{
    public IEnumerable<Guid>?  Interests { get; init; }
    
    [DefaultValue(false)]
    public bool IsInterestMatch { get; init; } = false;
}