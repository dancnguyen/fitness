namespace Fitness.Models
{
  public class Storage
  {
    public List<Workout> WorkoutHistory { get; set; } = new();
    public Workout CurrentWorkout { get; set; } = new();
    public Workout PreviousWorkout { get; set; } = new();
  }
}
