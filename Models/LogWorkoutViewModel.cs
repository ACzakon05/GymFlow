namespace GymFlow.Models
{
    public class LogWorkoutViewModel
    {
        public int WorkoutId { get; set; }
    }

    public class WorkoutExerciseLogData
    {
        public int Sets { get; set; }
        public int Reps { get; set; }
        public decimal Weight { get; set; }
        public bool IsCompleted { get; set; }
        public string? Notes { get; set; }
        public string? Resistance { get; set; }
    }
}
