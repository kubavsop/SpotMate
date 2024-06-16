using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SpotMate.Application.DTOs.HubModels;
using SpotMate.Application.Mapping;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public class UserShortDto: IMapFrom<SpotMateUser>
{
    private const string BaseUrl = "http://89.111.175.47:8080/static/";
    [Required]
    public Guid Id { get; init; }
    [Required]
    public required string UserName { get; init; }
    public string? Avatar { get; set; }
    
    public string? FullName { get;  set; }
    
    public UserStatus? UserStatus { get; set; }
    
    public DateTime? LastOnline { get; set; }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<SpotMateUser, UserShortDto>()
            .ForMember(dest => dest.Avatar,
                opt => 
                    opt.MapFrom(src => src.AvatarFileName != null ? $"{BaseUrl}{src.AvatarFileName}" : null));
        
        profile.CreateMap<SpotMateUser, UserFullDto>()
            .ForMember(dest => dest.Avatar,
                opt => 
                    opt.MapFrom(src => src.AvatarFileName != null ? $"{BaseUrl}{src.AvatarFileName}" : null));
        
        profile.CreateMap<SpotMateUser, UserDto>()
            .ForMember(dest => dest.Avatar,
                opt => 
                    opt.MapFrom(src => src.AvatarFileName != null ? $"{BaseUrl}{src.AvatarFileName}" : null));
        
        profile.CreateMap<SpotMateUser, FriendDto>()
            .ForMember(dest => dest.Avatar,
                opt => 
                    opt.MapFrom(src => src.AvatarFileName != null ? $"{BaseUrl}{src.AvatarFileName}" : null));
        
        profile.CreateMap<SpotMateUser, UserLocationModel>()
            .ForMember(dest => dest.Avatar,
                opt => 
                    opt.MapFrom(src => src.AvatarFileName != null ? $"{BaseUrl}{src.AvatarFileName}" : null))
            .ForMember(dest => dest.Coordinates,
            opt => 
                opt.MapFrom(src => new CoordinatesModel
                {
                    Longitude = src.Longitude,
                    Latitude = src.Latitude
                }));
    }
}