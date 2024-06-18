using System.ComponentModel.DataAnnotations;
using SpotMate.Application.Mapping;
using SpotMate.Domain.Entities;

namespace SpotMate.Application.DTOs.Responses;

public sealed class MessageDto
{
    [Required]
    public required Guid Id { get; init; }
    [Required]
    public required DateTime CreateTime { get; init; }
    [Required]
    public required string Text { get; init; }
    [Required]
    public required UserShortDto User { get; init; }
    [Required]
    public required bool IsMine { get; init; }
}