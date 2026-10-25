using System;

namespace ARALReadingSystem.Models
{
    public class ProgressCheckIn
    {
        // =========================================================
        // FIGURE 18 DATABASE IDENTITY
        // =========================================================

        // CHECKIN_RECORD.Record_ID
        public int Id { get; set; }

        // CHECKIN_RECORD.Pupil_ID
        public int PupilId { get; set; }

        // CHECKIN_RECORD.Teacher_ID
        public int TeacherId { get; set; }

        // CHECKIN_RECORD.Checkin_Period
        public int CheckInNumber { get; set; }


        // =========================================================
        // FIGURE 18 DATABASE CATEGORY SCORES
        // =========================================================
        //
        // These four properties represent the actual values stored
        // in CHECKIN_RECORD.
        //
        // Score_Titik
        // Score_Pantig
        // Score_Salita
        // Score_Parirala
        //
        // They must NOT be reconstructed from historical item data.
        // =========================================================

        public int ScoreTitik { get; set; }

        public int ScorePantig { get; set; }

        public int ScoreSalita { get; set; }

        public int ScoreParirala { get; set; }


        // =========================================================
        // UI-ONLY ITEM SCORES
        // =========================================================
        //
        // These values are used only while the teacher is answering
        // the current assessment checklist.
        //
        // Figure 18 does NOT store these individual item values.
        // =========================================================


        // =========================================================
        // TITIK - 6 ITEMS
        // =========================================================

        public int Titik1 { get; set; }

        public int Titik2 { get; set; }

        public int Titik3 { get; set; }

        public int Titik4 { get; set; }

        public int Titik5 { get; set; }

        public int Titik6 { get; set; }


        // =========================================================
        // PANTIG - 4 ITEMS
        // =========================================================

        public int Pantig1 { get; set; }

        public int Pantig2 { get; set; }

        public int Pantig3 { get; set; }

        public int Pantig4 { get; set; }


        // =========================================================
        // SALITA - 4 ITEMS
        // =========================================================

        public int Salita1 { get; set; }

        public int Salita2 { get; set; }

        public int Salita3 { get; set; }

        public int Salita4 { get; set; }


        // =========================================================
        // PARIRALA - 3 ITEMS
        // =========================================================

        public int Parirala1 { get; set; }

        public int Parirala2 { get; set; }

        public int Parirala3 { get; set; }


        // =========================================================
        // CURRENT UI ITEM TOTALS
        // =========================================================
        //
        // These are calculated only from the current assessment
        // screen's temporary item values.
        //
        // They are useful before SaveCheckIn().
        // =========================================================

        public int CurrentTitikScore =>
            Titik1 +
            Titik2 +
            Titik3 +
            Titik4 +
            Titik5 +
            Titik6;


        public int CurrentPantigScore =>
            Pantig1 +
            Pantig2 +
            Pantig3 +
            Pantig4;


        public int CurrentSalitaScore =>
            Salita1 +
            Salita2 +
            Salita3 +
            Salita4;


        public int CurrentPariralaScore =>
            Parirala1 +
            Parirala2 +
            Parirala3;


        // =========================================================
        // CATEGORY SCORE COMPATIBILITY
        // =========================================================
        //
        // Existing pages use TitikScore, PantigScore, SalitaScore
        // and PariralaScore.
        //
        // For a completed/saved record, return the Figure 18
        // database aggregate values.
        //
        // For a new assessment that has not yet been saved, return
        // the temporary UI item totals.
        // =========================================================

        public int TitikScore =>
            Completed
                ? ScoreTitik
                : CurrentTitikScore;


        public int PantigScore =>
            Completed
                ? ScorePantig
                : CurrentPantigScore;


        public int SalitaScore =>
            Completed
                ? ScoreSalita
                : CurrentSalitaScore;


        public int PariralaScore =>
            Completed
                ? ScoreParirala
                : CurrentPariralaScore;


        // =========================================================
        // TOTAL SCORE
        // =========================================================
        //
        // Figure 18 does not contain Total_Score.
        // This value is calculated only.
        // =========================================================

        public int TotalScore =>
            ScoreTitik +
            ScorePantig +
            ScoreSalita +
            ScoreParirala;


        public const int TotalItems =
            17;


        // =========================================================
        // COMPATIBILITY SCORE
        // =========================================================
        //
        // Application-only property.
        // NOT a Figure 18 database column.
        // =========================================================

        public double Score { get; set; }


        // =========================================================
        // COMPLETION STATE
        // =========================================================
        //
        // Application-only.
        //
        // If a CHECKIN_RECORD exists, Completed is true.
        // =========================================================

        public bool Completed { get; set; }


        // =========================================================
        // FIGURE 18: DATE_ASSESSED
        // =========================================================

        public string CompletedAt { get; set; } =
            "";


        public string DateAssessed
        {
            get =>
                CompletedAt;

            set =>
                CompletedAt =
                    value ?? "";
        }


        // =========================================================
        // FIGURE 18: TIME_TAKEN
        // =========================================================

        public int DurationSeconds { get; set; }


        public int TimeTaken
        {
            get =>
                DurationSeconds;

            set =>
                DurationSeconds =
                    Math.Max(
                        0,
                        value);
        }


        // =========================================================
        // FIGURE 18: REMARK
        // =========================================================

        public string TeacherNotes { get; set; } =
            "";


        public string Remark
        {
            get =>
                TeacherNotes;

            set =>
                TeacherNotes =
                    value ?? "";
        }


        // =========================================================
        // SCORE DISPLAY
        // =========================================================

        public string ScoreText
        {
            get
            {
                if (!Completed)
                {
                    return
                        $"— / {TotalItems} tama";
                }


                return
                    $"{TotalScore} / " +
                    $"{TotalItems} tama";
            }
        }


        // =========================================================
        // STATUS DISPLAY
        // =========================================================

        public string StatusText
        {
            get
            {
                return Completed
                    ? "Naitala"
                    : "Wala pa";
            }
        }


        public string ActionText
        {
            get
            {
                return Completed
                    ? "Baguhin"
                    : "Magtala";
            }
        }


        // =========================================================
        // DURATION
        // =========================================================

        public int DurationMinutes =>
            DurationSeconds > 0
                ? DurationSeconds / 60
                : 0;


        public int DurationRemainingSeconds =>
            DurationSeconds > 0
                ? DurationSeconds % 60
                : 0;


        public string DurationText
        {
            get
            {
                if (DurationSeconds <= 0)
                {
                    return "";
                }


                TimeSpan duration =
                    TimeSpan.FromSeconds(
                        DurationSeconds);


                if (duration.TotalHours >= 1)
                {
                    return
                        $"{(int)duration.TotalHours}h " +
                        $"{duration.Minutes}m " +
                        $"{duration.Seconds}s";
                }


                if (duration.Minutes > 0)
                {
                    return
                        $"{duration.Minutes}m " +
                        $"{duration.Seconds}s";
                }


                return
                    $"{duration.Seconds}s";
            }
        }


        // =========================================================
        // DATE ASSESSED DISPLAY
        // =========================================================

        public string CompletedDateText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(
                        CompletedAt))
                {
                    return "";
                }


                if (DateTime.TryParse(
                        CompletedAt,
                        out DateTime assessedDate))
                {
                    return
                        assessedDate.ToString(
                            "yyyy-MM-dd");
                }


                return CompletedAt;
            }
        }


        // =========================================================
        // RESULT DETAIL
        // =========================================================

        public string DetailText
        {
            get
            {
                if (!Completed)
                {
                    return
                        "Handa nang talaan ang scorecard.";
                }


                if (!string.IsNullOrWhiteSpace(
                        DurationText) &&
                    !string.IsNullOrWhiteSpace(
                        CompletedDateText))
                {
                    return
                        $"Oras: {DurationText} · " +
                        $"{CompletedDateText}";
                }


                if (!string.IsNullOrWhiteSpace(
                        DurationText))
                {
                    return
                        $"Oras: {DurationText}";
                }


                if (!string.IsNullOrWhiteSpace(
                        CompletedDateText))
                {
                    return
                        CompletedDateText;
                }


                return
                    "Naitala na ang scorecard.";
            }
        }


        // =========================================================
        // REMARK DISPLAY
        // =========================================================

        public bool HasTeacherNotes =>
            !string.IsNullOrWhiteSpace(
                TeacherNotes);


        public string TeacherNotesDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(
                        TeacherNotes))
                {
                    return
                        "Walang puna ng guro.";
                }


                return TeacherNotes;
            }
        }


        public bool HasRemark =>
            !string.IsNullOrWhiteSpace(
                Remark);


        public string RemarkDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(
                        Remark))
                {
                    return
                        "Walang puna ng guro.";
                }


                return Remark;
            }
        }


        // =========================================================
        // COMPLETION HELPERS
        // =========================================================

        public bool HasResult =>
            Completed;


        public string CheckInTitle =>
            $"Check-in #{CheckInNumber}";


        // =========================================================
        // CURRENT APPLICATION MILESTONE
        // =========================================================
        //
        // Application/session-flow only.
        // NOT stored in CHECKIN_RECORD.
        // =========================================================

        public int RequiredSession
        {
            get
            {
                return CheckInNumber switch
                {
                    1 => 9,
                    2 => 18,
                    3 => 27,
                    4 => 36,
                    _ => 0
                };
            }
        }


        public string RequiredSessionText
        {
            get
            {
                if (RequiredSession <= 0)
                {
                    return "";
                }


                return
                    $"Matapos ang Sesyon " +
                    $"{RequiredSession}";
            }
        }
    }
}