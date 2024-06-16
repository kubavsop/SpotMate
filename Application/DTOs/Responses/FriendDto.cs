using System.ComponentModel.DataAnnotations;
using AutoMapper;
using SpotMate.Application.Mapping;
using SpotMate.Domain.Entities;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class FriendDto: UserFullDto
{
    [Required]
    public bool IsLocationFrozen { get; set; }
}