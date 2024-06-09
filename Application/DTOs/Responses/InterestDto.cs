using SpotMate.Application.Mapping;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class InterestDto: IMapFrom<Interest>
{
    public Guid Id { get; init; }
    public required InterestType Type { get; init; }
}