using SpotMate.Domain.Entities;

namespace SpotMate.Application.Services;

public interface IJwtProvider
{
    public string Generate(SpotMateUser user, Guid tokenId, out DateTime expireAt);
}