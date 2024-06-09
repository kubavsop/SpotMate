using System.ComponentModel.DataAnnotations;
using SpotMate.Application.ValidationAttributes;

namespace SpotMate.Application.DTOs.Requests;

public sealed class UploadAvatarDto
{
    [Required]
    [AllowedFileExtensions]
    [FileMaxSize(1024 * 1024 * 10)]
    public required IFormFile Avatar { get; init; }
}