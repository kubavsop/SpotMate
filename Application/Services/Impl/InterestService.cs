using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SpotMate.Application.Context;
using SpotMate.Application.DTOs.Responses;
using SpotMate.Application.OperationResult;

namespace SpotMate.Application.Services.Impl;

public class InterestService: IInterestService
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    
    public InterestService(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<IEnumerable<InterestDto>>>  GetInterestsAsync()
    {
        var interests = await _context.Interests
            .OrderBy(i => i.CreateTime)
            .AsNoTracking()
            .ToListAsync();

        return _mapper.Map<List<InterestDto>>(interests);
    }
}