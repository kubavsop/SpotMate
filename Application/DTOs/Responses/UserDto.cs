using System.ComponentModel.DataAnnotations;
using AutoMapper;
using Microsoft.Extensions.Options;
using SpotMate.Application.Mapping;
using SpotMate.Application.Options;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class UserDto: IMapFrom<SpotMateUser>
{
    [Required]
    public required string UserName { get; init; }
    [Required]
    public required string Email { get; init; }
    public string? Avatar { get; set; }
    public string? FullName { get;  init; }
    public DateTime? Birthday { get; init; }
    public Gender? Gender { get; init; }
    [Required]
    public required IEnumerable<InterestDto> Interests { get; init; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<SpotMateUser, UserDto>()
            .ForMember(dest => dest.Avatar,
                opt => 
                    opt.MapFrom(src => src.AvatarFileName != null ? $"http://localhost:5064/static/{src.AvatarFileName}" : null));
    }
}