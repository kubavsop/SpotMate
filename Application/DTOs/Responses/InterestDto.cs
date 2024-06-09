using System.ComponentModel.DataAnnotations;
using SpotMate.Application.Mapping;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class InterestDto: IMapFrom<Interest>
{
    [Required]
    public Guid Id { get; init; }
    
    [Required]
    public required InterestType Type { get; init; }
}