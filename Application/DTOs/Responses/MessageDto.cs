using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Responses;

public sealed class MessageDto
{
    [Required]
    public required Guid ChatId { get; init; }
    [Required]
    public required Guid Id { get; init; }
    [Required]
    public required DateTime CreateTime { get; init; }
    [Required]
    public required string Text { get; init; }
    [Required]
    public required UserMessageModel User { get; init; }
    [Required]
    public bool IsMine { get; set; }
    
    [Required]
    public required bool IsUnread { get; init; }
}