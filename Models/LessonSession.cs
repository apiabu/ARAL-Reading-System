using System.Collections.Generic;

namespace ARALReadingSystem.Models
{
    public class LessonSession
    {
        public int SessionNumber { get; set; }

        public string Title { get; set; } = "";

        public string ShortTitle { get; set; } = "";

        public string Description { get; set; } = "";

        public bool IsCheckIn { get; set; }

        public int? CheckInNumber { get; set; }

        public List<LessonStep> Steps { get; set; }
            = new List<LessonStep>();

        public int TotalSteps
        {
            get
            {
                return Steps.Count;
            }
        }

        public string DisplayTitle
        {
            get
            {
                return $"Sesyon {SessionNumber}: {Title}";
            }
        }
    }
}