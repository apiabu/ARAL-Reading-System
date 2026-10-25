using System.Collections.Generic;

namespace ARALReadingSystem.Models
{
    public enum LessonStepType
    {
        // General / Sessions 1-4
        Introduction,
        Drawing,
        TextInput,
        Rules,
        ChoiceActivity,

        // Session 3
        HandColoring,
        HomeReinforcement,

        // Existing / future lesson types
        Sound,
        Reading,
        Practice,
        Review,
        CheckIn,

        // Session 5
        AlphabetDisplay,
        LetterMatching,
        VowelActivity,
        ConsonantActivity,
        MissingLetterActivity,

        // Sessions 6+
        LetterWriting,
        LetterColoring,
        BeginningSoundActivity,

        // Session 8
        LetterCaseActivity,
        BeginningLetterActivity,
        WordChoiceActivity
    }

    public class LessonChoice
    {
        public string Text { get; set; } = "";
        public bool CorrectAnswer { get; set; }
    }

    public class LessonStep
    {
        public int StepNumber { get; set; }
        public string SectionName { get; set; } = "";
        public string Title { get; set; } = "";
        public string Instruction { get; set; } = "";
        public string Prompt { get; set; } = "";
        public string Placeholder { get; set; } = "";
        public string VoiceoverText { get; set; } = "";
        public LessonStepType StepType { get; set; }
        public bool IsRequired { get; set; } = true;

        public List<string> Items { get; set; } =
            new List<string>();

        public List<LessonChoice> Choices { get; set; } =
            new List<LessonChoice>();
    }
}