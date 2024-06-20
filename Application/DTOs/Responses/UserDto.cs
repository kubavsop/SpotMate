using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.Extensions.Options;
using SpotMate.Application.Mapping;
using SpotMate.Application.Options;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class UserDto: UserShortDto
{
    [Required]
    public required string Email { get; init; }
    [Required]
    public bool IsInvisible { get; init; }
    public DateTime? Birthday { get; init; }
    public Gender? Gender { get; init; }
    [Required]
    public bool IsInterestBasedLocationSharable { get; init; }
    [Required]
    public required IEnumerable<InterestDto> Interests { get; init; }
}