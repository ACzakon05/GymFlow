namespace GymFlow.Models
{
    public class ProgressStatistics
    {
        public int TotalWorkouts { get; set; }
        public int TotalExercises { get; set; }
        public decimal MaxWeight { get; set; }
        public double AvgReps { get; set; }
        public List<ExerciseStatistic> ExerciseStats { get; set; } = new();
    }

    public class ExerciseStatistic
    {
        public int ExerciseId { get; set; }
        public string ExerciseName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int EntryCount { get; set; }
        public decimal MaxWeight { get; set; }
        public double AvgReps { get; set; }
    }
}
