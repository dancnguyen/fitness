namespace Fitness.Models
{
  public class Workout
  {
    public string SessionType { get; set; } = string.Empty;
    public List<Exercise> Exercises { get; set; } = new();
  }
}
