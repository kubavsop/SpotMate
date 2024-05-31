using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Requests;

public sealed class RefreshDto
{
    [Required]
    public required string AccessToken { get; set; }
}