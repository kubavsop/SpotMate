using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Domain.Entities;

namespace SpotMate.Persistence;

public sealed class ApplicationDbContext: IdentityDbContext<SpotMateUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    public DbSet<RefreshToken> RefreshTokens { get; init; }
    
    public ApplicationDbContext(DbContextOptions options) : base(options) { }
}