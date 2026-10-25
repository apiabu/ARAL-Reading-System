using System;

namespace ARALReadingSystem.Models
{
    public class Pupil
    {
        // =========================================================
        // FIGURE 18 PUPIL DATA
        // =========================================================

        // PUPIL.Pupil_ID
        public int PupilId { get; set; }


        // PUPIL.Teacher_ID
        public int TeacherId { get; set; }


        // PUPIL.First_Name
        public string FirstName { get; set; } =
            "";


        // PUPIL.Last_Name
        public string LastName { get; set; } =
            "";


        // PUPIL.Grade_Level
        public string GradeLevel { get; set; } =
            "";


        // PUPIL.Gender
        public string Gender { get; set; } =
            "";


        // PUPIL.Birthdate
        public DateTime? Birthdate { get; set; }


        // =========================================================
        // COMPATIBILITY ALIASES
        // =========================================================
        //
        // These are NOT database columns.
        //
        // They only allow existing application pages to continue
        // using their previous property names.
        // =========================================================

        public int Id
        {
            get =>
                PupilId;

            set =>
                PupilId =
                    value;
        }


        public string Sex
        {
            get =>
                Gender;

            set =>
                Gender =
                    value ?? "";
        }


        // =========================================================
        // COMPUTED AGE
        // =========================================================
        //
        // Age is NOT stored in PUPIL.
        //
        // It is calculated only from Figure 18 Birthdate when some
        // existing application code needs it.
        // =========================================================

        public int? Age
        {
            get
            {
                if (!Birthdate.HasValue)
                {
                    return null;
                }


                DateTime today =
                    DateTime.Today;


                DateTime birthday =
                    Birthdate.Value.Date;


                if (birthday > today)
                {
                    return null;
                }


                int age =
                    today.Year -
                    birthday.Year;


                if (birthday >
                    today.AddYears(-age))
                {
                    age--;
                }


                return age;
            }
        }


        // =========================================================
        // APPLICATION-ONLY SESSION STATE
        // =========================================================
        //
        // These are NOT PUPIL database columns.
        //
        // PupilService supplies these values for the current
        // application/session flow.
        // =========================================================

        public int CurrentSession { get; set; } =
            1;


        public int LastCompletedSession { get; set; } =
            0;


        public int CompletedCheckIns { get; set; } =
            0;


        // =========================================================
        // DISPLAY PROPERTIES
        // =========================================================
        //
        // Computed only.
        // No additional database columns.
        // =========================================================

        public string FullName
        {
            get
            {
                string lastName =
                    LastName?.Trim() ?? "";


                string firstName =
                    FirstName?.Trim() ?? "";


                if (string.IsNullOrWhiteSpace(
                        lastName))
                {
                    return firstName;
                }


                if (string.IsNullOrWhiteSpace(
                        firstName))
                {
                    return lastName;
                }


                return
                    $"{lastName}, {firstName}";
            }
        }


        public string BirthdateText
        {
            get
            {
                if (!Birthdate.HasValue)
                {
                    return "—";
                }


                return Birthdate.Value
                    .ToString(
                        "yyyy-MM-dd");
            }
        }


        public string CheckInText
        {
            get
            {
                return
                    $"{CompletedCheckIns} / 4";
            }
        }


        public string ProgressText
        {
            get
            {
                return
                    $"{LastCompletedSession} / 36 sessions";
            }
        }


        public int ProgressPercent
        {
            get
            {
                int completed =
                    Math.Clamp(
                        LastCompletedSession,
                        0,
                        36);


                return
                    (int)Math.Round(
                        completed /
                        36.0 *
                        100);
            }
        }
    }
}