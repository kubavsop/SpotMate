using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.Requests;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.Exceptions;
using SpotMate.Application.OperationResult;
using SpotMate.Application.Options;
using SpotMate.Domain.Entities;

namespace SpotMate.Application.Services.Impl;

public class StepService : IStepService
{
    private readonly IApplicationDbContext _context;
    private readonly BaseUrlOptions _baseUrlOptions;

    public StepService(IApplicationDbContext context, IOptions<BaseUrlOptions> baseUrlOptions)
    {
        _context = context;
        _baseUrlOptions = baseUrlOptions.Value;
    }

    public async Task<Result> CreateDailyStepAsync(CreateDailyStepDto dto, Guid userId)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var dailyStep = await _context.DailySteps.FirstOrDefaultAsync(s => s.UserId == userId && s.Date == currentDate);

        if (dailyStep != null)
        {
            dailyStep.StepCount = dto.StepCount;
            await _context.SaveChangesAsync();
            return Result.Success();
        }

        dailyStep = new DailyStep
        {
            UserId = userId,
            StepCount = dto.StepCount,
            Date = currentDate
        };

        await _context.DailySteps.AddAsync(dailyStep);
        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result<IEnumerable<StepDto>>> GetDailyRatingAsync(Guid userId)
    {
        var fromDate = DateOnly.FromDateTime(DateTime.UtcNow);
        return await GetRatingAsync(fromDate, userId);
    }

    public async Task<Result<IEnumerable<StepDto>>> GetWeeklyRatingAsync(Guid userId)
    {
        var currentDate = DateTime.UtcNow;
        var daysSinceMonday = currentDate.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)currentDate.DayOfWeek - 1;
        var fromDate = DateOnly.FromDateTime(currentDate.AddDays(-daysSinceMonday));
        return await GetRatingAsync(fromDate, userId);
    }

    public async Task<Result<IEnumerable<StepDto>>> GetMonthlyRatingAsync(Guid userId)
    {
        var fromDate = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        return await GetRatingAsync(fromDate, userId);
    }
    private async Task<Result<IEnumerable<StepDto>>> GetRatingAsync(DateOnly fromDate, Guid userId)
    {
        var user = await _context.Users
            .AsNoTracking()
            .AsSplitQuery()
            .Include(u => u.DailySteps)
            .Include(u => u.Friends)
            .ThenInclude(f => f.Friend)
            .ThenInclude(f => f.DailySteps)
            .FirstOrDefaultAsync(u => u.Id == userId);
        
        if (user == null)
        {
            return new NotFoundException(nameof(SpotMateUser), userId);
        }

        var rating = user.Friends.Select(f => new StepDto
        {
            Id = f.FriendId,
            UserName = f.Friend.UserName,
            Avatar = f.Friend.AvatarFileName != null ? $"{_baseUrlOptions.Url}{f.Friend.AvatarFileName}" : null,
            FullName = f.Friend.FullName,
            UserStatus = f.Friend.UserStatus,
            LastOnline = f.Friend.LastOnline,
            StepCount = f.Friend.DailySteps.Where(d => d.Date >= fromDate).Sum(ds => ds.StepCount)
        }).ToList();
        
        rating.Add(new StepDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Avatar = user.AvatarFileName != null ? $"{_baseUrlOptions.Url}{user.AvatarFileName}" : null,
            FullName = user.FullName,
            UserStatus = user.UserStatus,
            LastOnline = user.LastOnline,
            StepCount = user.DailySteps.Where(d => d.Date >= fromDate).Sum(ds => ds.StepCount)
        });

        return rating.OrderByDescending(r => r.StepCount).ToList();
    }
}