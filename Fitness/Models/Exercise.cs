using System.ComponentModel.DataAnnotations;

namespace Fitness.Models
{
  public class Exercise
  {
    public enum WeightType { LBS, KGS };

    [MinLength(1, ErrorMessage = "Name cannot be empty")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000, ErrorMessage = "Weight must be between 1 and 1000")]
    public int? Weight { get; set; }

    public WeightType Measurement { get; set; } = WeightType.LBS;

    [Required]
    [Range(1, 100, ErrorMessage = "Minimum Reps must be between 1 and 100")]
    public int? MinReps { get; set; }

    [Required]
    [Range(1, 100, ErrorMessage = "Maximum Reps must be between 1 and 100")]
    [CustomValidation(typeof(Exercise), nameof(ValidateMaxReps))]
    public int? MaxReps { get; set; }

    [Range(1, 10, ErrorMessage = "Sets must be between 1 and 10")]
    public int Sets { get; set; } = 3;

    public int CompletedSets { get; set; }

    public string FormatWeight()
    {
      return $"{Weight} {Measurement.ToString().ToLower()}";
    }

    public string FormatReps()
    {
      if (MinReps == MaxReps)
        return MinReps.ToString() ?? string.Empty;
      return $"{MinReps}-{MaxReps}";
    }

    public static ValidationResult ValidateMaxReps(int maxReps, ValidationContext context)
    {
      Exercise exercise = (Exercise)context.ObjectInstance;
      if (maxReps < exercise.MinReps)
      {
        return new ValidationResult(
            "Maximum Reps cannot be less than Minimum Reps",
            new[] { nameof(MaxReps) }
        );
      }
      return ValidationResult.Success!;
    }
  }
}
