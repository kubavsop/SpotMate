using System.ComponentModel.DataAnnotations;

namespace SpotMate.Application.DTOs.Requests;

public sealed class CreateDailyStepDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public int StepCount { get; set; }
}