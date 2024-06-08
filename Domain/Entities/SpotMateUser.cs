using Microsoft.AspNetCore.Identity;
using SpotMate.Domain.Entities.Base;
using SpotMate.Domain.Enums;

namespace SpotMate.Domain.Entities;

public sealed class SpotMateUser: IdentityUser<Guid>, IBaseEntity
{
    public DateTime CreateTime { get; set; }
    public DateTime? ModifiedTime { get; set; }
    public override required string Email { get; set; }
    public override required string UserName { get; set; }
    public string? FullName { get;  set; }
    public DateTime? Birthday { get; set; }
    public Gender? Gender { get; set; }

    public List<RefreshToken> RefreshTokens { get; } = [];
    public List<Interest> Interests { get; } = [];
}