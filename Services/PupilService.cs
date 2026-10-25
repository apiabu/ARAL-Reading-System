using ARALReadingSystem.Data;
using ARALReadingSystem.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace ARALReadingSystem.Services
{
    public class PupilService
    {
        // =========================================================
        // SESSION PROGRESS STORAGE
        // =========================================================
        //
        // Figure 18 contains only:
        //
        // TEACHER
        // PUPIL
        // CHECKIN_RECORD
        //
        // LastCompletedSession / CurrentSession are therefore
        // application state and are NOT stored in SQLite.
        // =========================================================

        private static readonly string ProgressFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ARALReadingSystem",
                "SessionProgress");


        // =========================================================
        // GET ALL PUPILS
        // =========================================================

        public List<Pupil> GetAllPupils()
        {
            List<Pupil> pupils =
                new List<Pupil>();


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                SELECT
                    Pupil_ID,
                    Teacher_ID,
                    First_Name,
                    Last_Name,
                    Grade_Level,
                    Gender,
                    Birthdate,

                    (
                        SELECT COUNT(*)
                        FROM CHECKIN_RECORD c
                        WHERE c.Pupil_ID =
                              PUPIL.Pupil_ID
                    ) AS CompletedCheckIns

                FROM PUPIL

                ORDER BY
                    Last_Name,
                    First_Name;
            ";


            using SqliteDataReader reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                Pupil pupil =
                    ReadPupil(
                        reader);

                ApplySessionProgress(
                    pupil);

                pupils.Add(
                    pupil);
            }


            return pupils;
        }


        // =========================================================
        // GET PUPILS BY TEACHER
        // =========================================================

        public List<Pupil> GetPupilsByTeacher(
            int teacherId)
        {
            List<Pupil> pupils =
                new List<Pupil>();


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                SELECT
                    Pupil_ID,
                    Teacher_ID,
                    First_Name,
                    Last_Name,
                    Grade_Level,
                    Gender,
                    Birthdate,

                    (
                        SELECT COUNT(*)
                        FROM CHECKIN_RECORD c
                        WHERE c.Pupil_ID =
                              PUPIL.Pupil_ID
                    ) AS CompletedCheckIns

                FROM PUPIL

                WHERE Teacher_ID =
                      $teacherId

                ORDER BY
                    Last_Name,
                    First_Name;
            ";


            command.Parameters.AddWithValue(
                "$teacherId",
                teacherId);


            using SqliteDataReader reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                Pupil pupil =
                    ReadPupil(
                        reader);

                ApplySessionProgress(
                    pupil);

                pupils.Add(
                    pupil);
            }


            return pupils;
        }


        // =========================================================
        // GET PUPIL BY ID
        // =========================================================

        public Pupil? GetPupilById(
            int pupilId)
        {
            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                SELECT
                    Pupil_ID,
                    Teacher_ID,
                    First_Name,
                    Last_Name,
                    Grade_Level,
                    Gender,
                    Birthdate,

                    (
                        SELECT COUNT(*)
                        FROM CHECKIN_RECORD c
                        WHERE c.Pupil_ID =
                              PUPIL.Pupil_ID
                    ) AS CompletedCheckIns

                FROM PUPIL

                WHERE Pupil_ID =
                      $pupilId

                LIMIT 1;
            ";


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);


            using SqliteDataReader reader =
                command.ExecuteReader();


            if (!reader.Read())
            {
                return null;
            }


            Pupil pupil =
                ReadPupil(
                    reader);

            ApplySessionProgress(
                pupil);

            return pupil;
        }


        // =========================================================
        // READ PUPIL
        // =========================================================

        private static Pupil ReadPupil(
            SqliteDataReader reader)
        {
            string birthdateText =
                reader.IsDBNull(6)
                    ? ""
                    : reader.GetString(6);


            DateTime? birthdate =
                ParseBirthdate(
                    birthdateText);


            return new Pupil
            {
                PupilId =
                    reader.GetInt32(0),

                TeacherId =
                    reader.GetInt32(1),

                FirstName =
                    reader.IsDBNull(2)
                        ? ""
                        : reader.GetString(2),

                LastName =
                    reader.IsDBNull(3)
                        ? ""
                        : reader.GetString(3),

                GradeLevel =
                    reader.IsDBNull(4)
                        ? ""
                        : reader.GetString(4),

                Gender =
                    reader.IsDBNull(5)
                        ? ""
                        : reader.GetString(5),

                Birthdate =
                    birthdate,

                CompletedCheckIns =
                    reader.IsDBNull(7)
                        ? 0
                        : Convert.ToInt32(
                            reader.GetInt64(7))
            };
        }


        // =========================================================
        // PUPIL COUNT
        // =========================================================

        public int GetPupilCount()
        {
            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText =
                "SELECT COUNT(*) FROM PUPIL;";


            return Convert.ToInt32(
                command.ExecuteScalar());
        }


        // =========================================================
        // PUPIL COUNT BY TEACHER
        // =========================================================

        public int GetPupilCountByTeacher(
            int teacherId)
        {
            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                SELECT COUNT(*)
                FROM PUPIL
                WHERE Teacher_ID =
                      $teacherId;
            ";


            command.Parameters.AddWithValue(
                "$teacherId",
                teacherId);


            return Convert.ToInt32(
                command.ExecuteScalar());
        }


        // =========================================================
        // ADD PUPIL
        // =========================================================

        public int AddPupil(
            string firstName,
            string lastName,
            string gradeLevel,
            string gender,
            DateTime birthdate)
        {
            int teacherId =
                GetDefaultTeacherId();


            return AddPupil(
                teacherId,
                firstName,
                lastName,
                gradeLevel,
                gender,
                birthdate);
        }


        public int AddPupil(
            int teacherId,
            string firstName,
            string lastName,
            string gradeLevel,
            string gender,
            DateTime birthdate)
        {
            ValidatePupil(
                firstName,
                lastName,
                birthdate);


            ValidateTeacherExists(
                teacherId);


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                INSERT INTO PUPIL
                (
                    Teacher_ID,
                    First_Name,
                    Last_Name,
                    Grade_Level,
                    Gender,
                    Birthdate
                )
                VALUES
                (
                    $teacherId,
                    $firstName,
                    $lastName,
                    $gradeLevel,
                    $gender,
                    $birthdate
                );

                SELECT last_insert_rowid();
            ";


            command.Parameters.AddWithValue(
                "$teacherId",
                teacherId);


            command.Parameters.AddWithValue(
                "$firstName",
                firstName.Trim());


            command.Parameters.AddWithValue(
                "$lastName",
                lastName.Trim());


            command.Parameters.AddWithValue(
                "$gradeLevel",
                ToDatabaseValue(
                    gradeLevel));


            command.Parameters.AddWithValue(
                "$gender",
                ToDatabaseValue(
                    gender));


            command.Parameters.AddWithValue(
                "$birthdate",
                birthdate.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture));


            object? result =
                command.ExecuteScalar();


            if (result == null ||
                result == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "Hindi nalikha ang pupil record.");
            }


            int pupilId =
                Convert.ToInt32(
                    Convert.ToInt64(
                        result));


            SaveLastCompletedSession(
                pupilId,
                0);


            return pupilId;
        }


        // =========================================================
        // UPDATE PUPIL
        // =========================================================

        public void UpdatePupilName(
            int pupilId,
            string firstName,
            string lastName)
        {
            ValidatePupilName(
                firstName,
                lastName);

            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();

            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                UPDATE PUPIL
                SET
                    First_Name = $firstName,
                    Last_Name = $lastName
                WHERE Pupil_ID = $pupilId;
            ";

            command.Parameters.AddWithValue(
                "$firstName",
                firstName.Trim());

            command.Parameters.AddWithValue(
                "$lastName",
                lastName.Trim());

            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);

            int affectedRows =
                command.ExecuteNonQuery();

            if (affectedRows == 0)
            {
                throw new InvalidOperationException(
                    "Hindi makita ang pupil record na ia-update.");
            }
        }

        public void UpdatePupil(
            int pupilId,
            string firstName,
            string lastName,
            string gradeLevel,
            string gender,
            DateTime birthdate)
        {
            ValidatePupil(
                firstName,
                lastName,
                birthdate);


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                UPDATE PUPIL

                SET
                    First_Name =
                        $firstName,

                    Last_Name =
                        $lastName,

                    Grade_Level =
                        $gradeLevel,

                    Gender =
                        $gender,

                    Birthdate =
                        $birthdate

                WHERE Pupil_ID =
                      $pupilId;
            ";


            command.Parameters.AddWithValue(
                "$firstName",
                firstName.Trim());


            command.Parameters.AddWithValue(
                "$lastName",
                lastName.Trim());


            command.Parameters.AddWithValue(
                "$gradeLevel",
                ToDatabaseValue(
                    gradeLevel));


            command.Parameters.AddWithValue(
                "$gender",
                ToDatabaseValue(
                    gender));


            command.Parameters.AddWithValue(
                "$birthdate",
                birthdate.ToString(
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture));


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);


            int affectedRows =
                command.ExecuteNonQuery();


            if (affectedRows == 0)
            {
                throw new InvalidOperationException(
                    "Hindi makita ang pupil record na ia-update.");
            }
        }


        // =========================================================
        // UPDATE PUPIL - NULLABLE BIRTHDATE
        // =========================================================
        //
        // Useful for migrated pupils whose exact historical
        // birthdate was not available in the old database.
        // =========================================================

        public void UpdatePupil(
            int pupilId,
            string firstName,
            string lastName,
            string gradeLevel,
            string gender,
            DateTime? birthdate)
        {
            ValidatePupilName(
                firstName,
                lastName);


            if (birthdate.HasValue &&
                birthdate.Value.Date >
                DateTime.Today)
            {
                throw new ArgumentException(
                    "Hindi maaaring nasa hinaharap ang birthdate.");
            }


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                UPDATE PUPIL

                SET
                    First_Name =
                        $firstName,

                    Last_Name =
                        $lastName,

                    Grade_Level =
                        $gradeLevel,

                    Gender =
                        $gender,

                    Birthdate =
                        $birthdate

                WHERE Pupil_ID =
                      $pupilId;
            ";


            command.Parameters.AddWithValue(
                "$firstName",
                firstName.Trim());


            command.Parameters.AddWithValue(
                "$lastName",
                lastName.Trim());


            command.Parameters.AddWithValue(
                "$gradeLevel",
                ToDatabaseValue(
                    gradeLevel));


            command.Parameters.AddWithValue(
                "$gender",
                ToDatabaseValue(
                    gender));


            command.Parameters.AddWithValue(
                "$birthdate",
                birthdate.HasValue
                    ? birthdate.Value.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture)
                    : DBNull.Value);


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);


            int affectedRows =
                command.ExecuteNonQuery();


            if (affectedRows == 0)
            {
                throw new InvalidOperationException(
                    "Hindi makita ang pupil record na ia-update.");
            }
        }


        // =========================================================
        // DELETE PUPIL
        // =========================================================

        public void DeletePupil(
            int pupilId)
        {
            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            /*
             * CHECKIN_RECORD uses:
             *
             * FOREIGN KEY (Pupil_ID)
             * REFERENCES PUPIL(Pupil_ID)
             * ON DELETE CASCADE
             *
             * Therefore deleting the PUPIL automatically deletes
             * its CHECKIN_RECORD rows.
             */

            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                DELETE FROM PUPIL
                WHERE Pupil_ID =
                      $pupilId;
            ";


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);


            int affectedRows =
                command.ExecuteNonQuery();


            if (affectedRows == 0)
            {
                throw new InvalidOperationException(
                    "Hindi makita ang pupil record na buburahin.");
            }


            DeleteSessionProgress(
                pupilId);
        }


        // =========================================================
        // COMPLETE SESSION
        // =========================================================

        public void CompleteSession(
            int pupilId,
            int sessionNumber)
        {
            if (sessionNumber < 1 ||
                sessionNumber > 36)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sessionNumber),
                    "Session number must be from 1 to 36.");
            }


            Pupil? pupil =
                GetPupilById(
                    pupilId);


            if (pupil == null)
            {
                throw new InvalidOperationException(
                    "Hindi makita ang pupil record.");
            }


            int lastCompleted =
                pupil.LastCompletedSession;


            /*
             * A previously completed session may be reopened.
             * Doing so does not move progress backwards.
             */
            if (sessionNumber <=
                lastCompleted)
            {
                return;
            }


            int expectedSession =
                lastCompleted + 1;


            if (sessionNumber !=
                expectedSession)
            {
                throw new InvalidOperationException(
                    $"Hindi pa maaaring tapusin ang Sesyon " +
                    $"{sessionNumber}. " +
                    $"Ang susunod na sesyon ay Sesyon " +
                    $"{expectedSession}.");
            }


            SaveLastCompletedSession(
                pupilId,
                sessionNumber);
        }


        // =========================================================
        // GET LAST COMPLETED SESSION
        // =========================================================

        public int GetLastCompletedSession(
            int pupilId)
        {
            return GetStoredLastCompletedSession(
                pupilId);
        }


        // =========================================================
        // APPLY SESSION PROGRESS
        // =========================================================

        private static void ApplySessionProgress(
            Pupil pupil)
        {
            int lastCompleted =
                GetStoredLastCompletedSession(
                    pupil.PupilId);


            pupil.LastCompletedSession =
                lastCompleted;


            pupil.CurrentSession =
                lastCompleted >= 36
                    ? 36
                    : lastCompleted + 1;
        }


        // =========================================================
        // READ SESSION PROGRESS
        // =========================================================

        private static int GetStoredLastCompletedSession(
            int pupilId)
        {
            string path =
                GetProgressFilePath(
                    pupilId);


            if (!File.Exists(
                    path))
            {
                return 0;
            }


            try
            {
                string text =
                    File.ReadAllText(
                            path)
                        .Trim();


                if (!int.TryParse(
                        text,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int session))
                {
                    return 0;
                }


                return Math.Clamp(
                    session,
                    0,
                    36);
            }
            catch
            {
                return 0;
            }
        }


        // =========================================================
        // SAVE SESSION PROGRESS
        // =========================================================

        private static void SaveLastCompletedSession(
            int pupilId,
            int sessionNumber)
        {
            EnsureProgressFolder();


            string path =
                GetProgressFilePath(
                    pupilId);


            int safeSession =
                Math.Clamp(
                    sessionNumber,
                    0,
                    36);


            File.WriteAllText(
                path,
                safeSession.ToString(
                    CultureInfo.InvariantCulture));
        }


        // =========================================================
        // DELETE SESSION PROGRESS
        // =========================================================

        private static void DeleteSessionProgress(
            int pupilId)
        {
            string path =
                GetProgressFilePath(
                    pupilId);


            if (!File.Exists(
                    path))
            {
                return;
            }


            try
            {
                File.Delete(
                    path);
            }
            catch
            {
                /*
                 * SQLite deletion has already succeeded.
                 *
                 * Failure to remove auxiliary session state must
                 * not recreate or alter the pupil database record.
                 */
            }
        }


        // =========================================================
        // SESSION PROGRESS PATH
        // =========================================================

        private static string GetProgressFilePath(
            int pupilId)
        {
            return Path.Combine(
                ProgressFolder,
                $"pupil_{pupilId}.progress");
        }


        private static void EnsureProgressFolder()
        {
            if (!Directory.Exists(
                    ProgressFolder))
            {
                Directory.CreateDirectory(
                    ProgressFolder);
            }
        }


        // =========================================================
        // DEFAULT TEACHER
        // =========================================================

        public int GetDefaultTeacherId()
        {
            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                SELECT Teacher_ID
                FROM TEACHER
                ORDER BY Teacher_ID
                LIMIT 1;
            ";


            object? result =
                command.ExecuteScalar();


            if (result == null ||
                result == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "Walang teacher record sa database.");
            }


            return Convert.ToInt32(
                Convert.ToInt64(
                    result));
        }


        // =========================================================
        // VALIDATE TEACHER
        // =========================================================

        private static void ValidateTeacherExists(
            int teacherId)
        {
            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                SELECT COUNT(*)
                FROM TEACHER
                WHERE Teacher_ID =
                      $teacherId;
            ";


            command.Parameters.AddWithValue(
                "$teacherId",
                teacherId);


            long count =
                Convert.ToInt64(
                    command.ExecuteScalar());


            if (count == 0)
            {
                throw new ArgumentException(
                    "Hindi makita ang teacher record.",
                    nameof(teacherId));
            }
        }


        // =========================================================
        // VALIDATION
        // =========================================================

        private static void ValidatePupil(
            string firstName,
            string lastName,
            DateTime birthdate)
        {
            ValidatePupilName(
                firstName,
                lastName);


            if (birthdate.Date >
                DateTime.Today)
            {
                throw new ArgumentException(
                    "Hindi maaaring nasa hinaharap ang birthdate.");
            }
        }


        private static void ValidatePupilName(
            string firstName,
            string lastName)
        {
            if (string.IsNullOrWhiteSpace(
                    firstName))
            {
                throw new ArgumentException(
                    "Kailangan ang pangalan ng mag-aaral.");
            }


            if (string.IsNullOrWhiteSpace(
                    lastName))
            {
                throw new ArgumentException(
                    "Kailangan ang apelyido ng mag-aaral.");
            }
        }


        // =========================================================
        // DATABASE VALUE
        // =========================================================

        private static object ToDatabaseValue(
            string? value)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return DBNull.Value;
            }


            return value.Trim();
        }


        // =========================================================
        // BIRTHDATE PARSER
        // =========================================================

        private static DateTime? ParseBirthdate(
            string value)
        {
            if (string.IsNullOrWhiteSpace(
                    value))
            {
                return null;
            }


            if (DateTime.TryParseExact(
                    value,
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime exactDate))
            {
                return exactDate;
            }


            if (DateTime.TryParse(
                    value,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime date))
            {
                return date;
            }


            return null;
        }
    }
}