using Microsoft.EntityFrameworkCore;
using SpotMate.Domain.Entities;

namespace SpotMate.Application.Context;

public interface IApplicationDbContext
{
    DbSet<SpotMateUser> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}