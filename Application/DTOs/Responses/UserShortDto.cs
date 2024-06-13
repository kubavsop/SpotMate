using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SpotMate.Application.Mapping;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class UserShortDto: IMapFrom<SpotMateUser>
{
    [Required]
    public Guid Id { get; init; }
    [Required]
    public required string UserName { get; init; }
    public string? Avatar { get; set; }
    
    [Required]
    public required IEnumerable<InterestDto> Interests { get; init; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<SpotMateUser, UserShortDto>()
            .ForMember(dest => dest.Avatar,
                opt => 
                    opt.MapFrom(src => src.AvatarFileName != null ? $"http://localhost:5064/static/{src.AvatarFileName}" : null));
    }
}