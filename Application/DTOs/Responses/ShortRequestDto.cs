using System.ComponentModel.DataAnnotations;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class ShortRequestDto
{
    [Required]
    public bool HasMyFriendRequest { get; init; }
    
    [Required]
    public RequestStatus RequestStatus { get; init; }
}