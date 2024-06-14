using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SpotMate.Application.Mapping;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class FriendDto: IMapFrom<SpotMateUser>
{
    [Required]
    public Guid Id { get; init; }
    [Required]
    public required string UserName { get; init; }
    [Required]
    public required string Email { get; init; }
    [Required]
    public bool IsLocationFrozen { get; set; }
    public string? Avatar { get; set; }
    public string? FullName { get;  init; }
    public UserStatus? UserStatus { get; init; }
    public DateTime? Birthday { get; init; }
    public Gender? Gender { get; init; }
    [Required]
    public required IEnumerable<InterestDto> Interests { get; init; }
    public void Mapping(Profile profile)
    {
        profile.CreateMap<SpotMateUser, FriendDto>()
            .ForMember(dest => dest.Avatar,
                opt => 
                    opt.MapFrom(src => src.AvatarFileName != null ? $"http://89.111.175.47:8080/static/{src.AvatarFileName}" : null));
    }
}