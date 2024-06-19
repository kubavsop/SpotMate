using System.ComponentModel.DataAnnotations;
using SpotMate.Domain.Enums;

namespace SpotMate.Application.DTOs.Responses;

public sealed class UserMessageModel
{
    [Required]
    public Guid Id { get; init; }
    [Required]
    public required string UserName { get; init; }
    public string? Avatar { get; set; }
}