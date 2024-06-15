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
    
    public string? FullName { get;  set; }
    
    public UserStatus? UserStatus { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<SpotMateUser, UserShortDto>()
            .ForMember(dest => dest.Avatar,
                opt => 
                    opt.MapFrom(src => src.AvatarFileName != null ? $"http://89.111.175.47:8080/static/{src.AvatarFileName}" : null));
    }
}