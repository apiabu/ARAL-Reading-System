using ARALReadingSystem.Data;
using ARALReadingSystem.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace ARALReadingSystem.Services
{
    public class ProgressCheckInService
    {
        // =========================================================
        // CHECK-IN MILESTONES
        // =========================================================

        public int GetRequiredSession(
            int checkInNumber)
        {
            return checkInNumber switch
            {
                1 => 9,
                2 => 18,
                3 => 27,
                4 => 36,

                _ => throw new ArgumentOutOfRangeException(
                    nameof(checkInNumber),
                    "Check-in number must be from 1 to 4.")
            };
        }


        // =========================================================
        // GET ONE CHECK-IN
        // =========================================================

        public ProgressCheckIn? GetCheckIn(
            int pupilId,
            int checkInNumber)
        {
            ValidatePupilId(
                pupilId);

            ValidateCheckInNumber(
                checkInNumber);


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                SELECT
                    Record_ID,
                    Pupil_ID,
                    Teacher_ID,
                    Checkin_Period,
                    COALESCE(Score_Titik, 0),
                    COALESCE(Score_Pantig, 0),
                    COALESCE(Score_Salita, 0),
                    COALESCE(Score_Parirala, 0),
                    COALESCE(Time_Taken, 0),
                    COALESCE(Remark, ''),
                    COALESCE(Date_Assessed, '')

                FROM CHECKIN_RECORD

                WHERE Pupil_ID = $pupilId
                  AND Checkin_Period = $checkInNumber

                ORDER BY Record_ID DESC

                LIMIT 1;
            ";


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);

            command.Parameters.AddWithValue(
                "$checkInNumber",
                checkInNumber);


            using SqliteDataReader reader =
                command.ExecuteReader();


            if (!reader.Read())
            {
                return null;
            }


            return ReadCheckIn(
                reader);
        }


        // =========================================================
        // GET ALL CHECK-INS FOR ONE PUPIL
        // =========================================================

        public List<ProgressCheckIn> GetCheckInsForPupil(
            int pupilId)
        {
            ValidatePupilId(
                pupilId);


            List<ProgressCheckIn> results =
                new List<ProgressCheckIn>();


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                SELECT
                    Record_ID,
                    Pupil_ID,
                    Teacher_ID,
                    Checkin_Period,
                    COALESCE(Score_Titik, 0),
                    COALESCE(Score_Pantig, 0),
                    COALESCE(Score_Salita, 0),
                    COALESCE(Score_Parirala, 0),
                    COALESCE(Time_Taken, 0),
                    COALESCE(Remark, ''),
                    COALESCE(Date_Assessed, '')

                FROM CHECKIN_RECORD

                WHERE Pupil_ID = $pupilId

                ORDER BY
                    Checkin_Period,
                    Record_ID;
            ";


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);


            using SqliteDataReader reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                results.Add(
                    ReadCheckIn(
                        reader));
            }


            return results;
        }


        // =========================================================
        // GET ALL COMPLETED CHECK-INS
        // =========================================================

        public List<ProgressCheckIn> GetAllCheckIns()
        {
            List<ProgressCheckIn> results =
                new List<ProgressCheckIn>();


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                SELECT
                    Record_ID,
                    Pupil_ID,
                    Teacher_ID,
                    Checkin_Period,
                    COALESCE(Score_Titik, 0),
                    COALESCE(Score_Pantig, 0),
                    COALESCE(Score_Salita, 0),
                    COALESCE(Score_Parirala, 0),
                    COALESCE(Time_Taken, 0),
                    COALESCE(Remark, ''),
                    COALESCE(Date_Assessed, '')

                FROM CHECKIN_RECORD

                ORDER BY
                    Pupil_ID,
                    Checkin_Period,
                    Record_ID;
            ";


            using SqliteDataReader reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                results.Add(
                    ReadCheckIn(
                        reader));
            }


            return results;
        }


        // =========================================================
        // COUNT COMPLETED CHECK-INS
        // =========================================================

        public int GetCompletedCheckInCount(
            int pupilId)
        {
            ValidatePupilId(
                pupilId);


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                SELECT COUNT(
                    DISTINCT Checkin_Period)

                FROM CHECKIN_RECORD

                WHERE Pupil_ID = $pupilId;
            ";


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);


            object? result =
                command.ExecuteScalar();


            return Convert.ToInt32(
                result ?? 0);
        }


        // =========================================================
        // CHECK IF RESULT EXISTS
        // =========================================================

        public bool HasCheckIn(
            int pupilId,
            int checkInNumber)
        {
            ValidatePupilId(
                pupilId);

            ValidateCheckInNumber(
                checkInNumber);


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                SELECT COUNT(*)

                FROM CHECKIN_RECORD

                WHERE Pupil_ID = $pupilId
                  AND Checkin_Period = $checkInNumber;
            ";


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);

            command.Parameters.AddWithValue(
                "$checkInNumber",
                checkInNumber);


            object? result =
                command.ExecuteScalar();


            return Convert.ToInt32(
                result ?? 0) > 0;
        }


        // =========================================================
        // SAVE CHECK-IN
        // =========================================================
        //
        // IMPORTANT:
        //
        // The assessment page still uses the individual 1 / 0
        // values while the teacher is recording a NEW assessment.
        //
        // Before saving, those temporary UI values are converted
        // into the four Figure 18 aggregate scores:
        //
        // Score_Titik
        // Score_Pantig
        // Score_Salita
        // Score_Parirala
        //
        // Individual item values are NEVER written to SQLite.
        // =========================================================

        public void SaveCheckIn(
            ProgressCheckIn checkIn)
        {
            if (checkIn == null)
            {
                throw new ArgumentNullException(
                    nameof(checkIn));
            }


            ValidatePupilId(
                checkIn.PupilId);

            ValidateCheckInNumber(
                checkIn.CheckInNumber);

            ValidateBinaryScores(
                checkIn);


            // =====================================================
            // CALCULATE FIGURE 18 CATEGORY SCORES
            // =====================================================

            int scoreTitik =
                checkIn.CurrentTitikScore;

            int scorePantig =
                checkIn.CurrentPantigScore;

            int scoreSalita =
                checkIn.CurrentSalitaScore;

            int scoreParirala =
                checkIn.CurrentPariralaScore;


            ValidateCategoryScores(
                scoreTitik,
                scorePantig,
                scoreSalita,
                scoreParirala);


            // =====================================================
            // TIME TAKEN
            // =====================================================

            if (checkIn.DurationSeconds < 0)
            {
                checkIn.DurationSeconds =
                    0;
            }


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            // =====================================================
            // TEACHER_ID
            // =====================================================
            //
            // Figure 18 links CHECKIN_RECORD to TEACHER.
            //
            // The pupil already belongs to one teacher, so use
            // PUPIL.Teacher_ID for this check-in record.
            // =====================================================

            int teacherId =
                GetTeacherIdForPupil(
                    connection,
                    checkIn.PupilId);


            string assessedDate =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss");


            using SqliteTransaction transaction =
                connection.BeginTransaction();


            try
            {
                int existingRecordId =
                    GetExistingRecordId(
                        connection,
                        transaction,
                        checkIn.PupilId,
                        checkIn.CheckInNumber);


                if (existingRecordId > 0)
                {
                    UpdateCheckIn(
                        connection,
                        transaction,
                        existingRecordId,
                        teacherId,
                        checkIn,
                        scoreTitik,
                        scorePantig,
                        scoreSalita,
                        scoreParirala,
                        assessedDate);


                    checkIn.Id =
                        existingRecordId;
                }
                else
                {
                    checkIn.Id =
                        InsertCheckIn(
                            connection,
                            transaction,
                            teacherId,
                            checkIn,
                            scoreTitik,
                            scorePantig,
                            scoreSalita,
                            scoreParirala,
                            assessedDate);
                }


                transaction.Commit();


                // =================================================
                // UPDATE MODEL WITH THE ACTUAL SAVED VALUES
                // =================================================

                checkIn.TeacherId =
                    teacherId;

                checkIn.ScoreTitik =
                    scoreTitik;

                checkIn.ScorePantig =
                    scorePantig;

                checkIn.ScoreSalita =
                    scoreSalita;

                checkIn.ScoreParirala =
                    scoreParirala;


                checkIn.Score =
                    scoreTitik +
                    scorePantig +
                    scoreSalita +
                    scoreParirala;


                checkIn.Completed =
                    true;


                checkIn.CompletedAt =
                    assessedDate;
            }
            catch
            {
                transaction.Rollback();

                throw;
            }
        }


        // =========================================================
        // INSERT CHECK-IN
        // =========================================================

        private static int InsertCheckIn(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int teacherId,
            ProgressCheckIn checkIn,
            int scoreTitik,
            int scorePantig,
            int scoreSalita,
            int scoreParirala,
            string assessedDate)
        {
            using SqliteCommand command =
                connection.CreateCommand();


            command.Transaction =
                transaction;


            command.CommandText = @"
                INSERT INTO CHECKIN_RECORD
                (
                    Pupil_ID,
                    Teacher_ID,
                    Checkin_Period,
                    Score_Titik,
                    Score_Pantig,
                    Score_Salita,
                    Score_Parirala,
                    Time_Taken,
                    Remark,
                    Date_Assessed
                )
                VALUES
                (
                    $pupilId,
                    $teacherId,
                    $checkInPeriod,
                    $scoreTitik,
                    $scorePantig,
                    $scoreSalita,
                    $scoreParirala,
                    $timeTaken,
                    $remark,
                    $dateAssessed
                );

                SELECT last_insert_rowid();
            ";


            AddSaveParameters(
                command,
                teacherId,
                checkIn,
                scoreTitik,
                scorePantig,
                scoreSalita,
                scoreParirala,
                assessedDate);


            object? result =
                command.ExecuteScalar();


            return Convert.ToInt32(
                result);
        }


        // =========================================================
        // UPDATE CHECK-IN
        // =========================================================

        private static void UpdateCheckIn(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int recordId,
            int teacherId,
            ProgressCheckIn checkIn,
            int scoreTitik,
            int scorePantig,
            int scoreSalita,
            int scoreParirala,
            string assessedDate)
        {
            using SqliteCommand command =
                connection.CreateCommand();


            command.Transaction =
                transaction;


            command.CommandText = @"
                UPDATE CHECKIN_RECORD

                SET
                    Teacher_ID = $teacherId,
                    Score_Titik = $scoreTitik,
                    Score_Pantig = $scorePantig,
                    Score_Salita = $scoreSalita,
                    Score_Parirala = $scoreParirala,
                    Time_Taken = $timeTaken,
                    Remark = $remark,
                    Date_Assessed = $dateAssessed

                WHERE Record_ID = $recordId;
            ";


            command.Parameters.AddWithValue(
                "$recordId",
                recordId);


            AddSaveParameters(
                command,
                teacherId,
                checkIn,
                scoreTitik,
                scorePantig,
                scoreSalita,
                scoreParirala,
                assessedDate);


            command.ExecuteNonQuery();
        }


        // =========================================================
        // SAVE PARAMETERS
        // =========================================================

        private static void AddSaveParameters(
            SqliteCommand command,
            int teacherId,
            ProgressCheckIn checkIn,
            int scoreTitik,
            int scorePantig,
            int scoreSalita,
            int scoreParirala,
            string assessedDate)
        {
            command.Parameters.AddWithValue(
                "$pupilId",
                checkIn.PupilId);

            command.Parameters.AddWithValue(
                "$teacherId",
                teacherId);

            command.Parameters.AddWithValue(
                "$checkInPeriod",
                checkIn.CheckInNumber);

            command.Parameters.AddWithValue(
                "$scoreTitik",
                scoreTitik);

            command.Parameters.AddWithValue(
                "$scorePantig",
                scorePantig);

            command.Parameters.AddWithValue(
                "$scoreSalita",
                scoreSalita);

            command.Parameters.AddWithValue(
                "$scoreParirala",
                scoreParirala);

            command.Parameters.AddWithValue(
                "$timeTaken",
                checkIn.DurationSeconds);

            command.Parameters.AddWithValue(
                "$remark",
                checkIn.TeacherNotes?.Trim() ?? "");

            command.Parameters.AddWithValue(
                "$dateAssessed",
                assessedDate);
        }


        // =========================================================
        // GET EXISTING RECORD
        // =========================================================

        private static int GetExistingRecordId(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int pupilId,
            int checkInNumber)
        {
            using SqliteCommand command =
                connection.CreateCommand();


            command.Transaction =
                transaction;


            command.CommandText = @"
                SELECT Record_ID

                FROM CHECKIN_RECORD

                WHERE Pupil_ID = $pupilId
                  AND Checkin_Period = $checkInNumber

                ORDER BY Record_ID DESC

                LIMIT 1;
            ";


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);

            command.Parameters.AddWithValue(
                "$checkInNumber",
                checkInNumber);


            object? result =
                command.ExecuteScalar();


            if (result == null ||
                result == DBNull.Value)
            {
                return 0;
            }


            return Convert.ToInt32(
                result);
        }


        // =========================================================
        // GET TEACHER FOR PUPIL
        // =========================================================

        private static int GetTeacherIdForPupil(
            SqliteConnection connection,
            int pupilId)
        {
            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                SELECT Teacher_ID

                FROM PUPIL

                WHERE Pupil_ID = $pupilId

                LIMIT 1;
            ";


            command.Parameters.AddWithValue(
                "$pupilId",
                pupilId);


            object? result =
                command.ExecuteScalar();


            if (result == null ||
                result == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "Hindi makita ang teacher na naka-assign " +
                    "sa pupil na ito.");
            }


            int teacherId =
                Convert.ToInt32(
                    result);


            if (teacherId <= 0)
            {
                throw new InvalidOperationException(
                    "Walang valid na teacher na naka-assign " +
                    "sa pupil na ito.");
            }


            return teacherId;
        }


        // =========================================================
        // READ DATABASE ROW
        // =========================================================
        //
        // IMPORTANT:
        //
        // We read the four stored aggregate scores directly.
        //
        // We DO NOT populate Titik1...Parirala3 because Figure 18
        // does not store those individual historical answers.
        // =========================================================

        private static ProgressCheckIn ReadCheckIn(
            SqliteDataReader reader)
        {
            int scoreTitik =
                reader.GetInt32(4);

            int scorePantig =
                reader.GetInt32(5);

            int scoreSalita =
                reader.GetInt32(6);

            int scoreParirala =
                reader.GetInt32(7);


            ValidateStoredCategoryScores(
                scoreTitik,
                scorePantig,
                scoreSalita,
                scoreParirala);


            ProgressCheckIn result =
                new ProgressCheckIn
                {
                    Id =
                        reader.GetInt32(0),

                    PupilId =
                        reader.GetInt32(1),

                    TeacherId =
                        reader.GetInt32(2),

                    CheckInNumber =
                        reader.GetInt32(3),

                    ScoreTitik =
                        scoreTitik,

                    ScorePantig =
                        scorePantig,

                    ScoreSalita =
                        scoreSalita,

                    ScoreParirala =
                        scoreParirala,

                    Score =
                        scoreTitik +
                        scorePantig +
                        scoreSalita +
                        scoreParirala,

                    Completed =
                        true,

                    DurationSeconds =
                        reader.GetInt32(8),

                    TeacherNotes =
                        reader.IsDBNull(9)
                            ? ""
                            : reader.GetString(9),

                    CompletedAt =
                        reader.IsDBNull(10)
                            ? ""
                            : reader.GetString(10)
                };


            return result;
        }


        // =========================================================
        // VALIDATE PUPIL ID
        // =========================================================

        private static void ValidatePupilId(
            int pupilId)
        {
            if (pupilId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pupilId),
                    "A valid pupil ID is required.");
            }
        }


        // =========================================================
        // VALIDATE CHECK-IN NUMBER
        // =========================================================

        private static void ValidateCheckInNumber(
            int checkInNumber)
        {
            if (checkInNumber < 1 ||
                checkInNumber > 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(checkInNumber),
                    "Check-in number must be from 1 to 4.");
            }
        }


        // =========================================================
        // VALIDATE CATEGORY SCORES
        // =========================================================

        private static void ValidateCategoryScores(
            int scoreTitik,
            int scorePantig,
            int scoreSalita,
            int scoreParirala)
        {
            ValidateCategoryScore(
                scoreTitik,
                6,
                nameof(scoreTitik));

            ValidateCategoryScore(
                scorePantig,
                4,
                nameof(scorePantig));

            ValidateCategoryScore(
                scoreSalita,
                4,
                nameof(scoreSalita));

            ValidateCategoryScore(
                scoreParirala,
                3,
                nameof(scoreParirala));
        }


        private static void ValidateStoredCategoryScores(
            int scoreTitik,
            int scorePantig,
            int scoreSalita,
            int scoreParirala)
        {
            ValidateCategoryScores(
                scoreTitik,
                scorePantig,
                scoreSalita,
                scoreParirala);
        }


        private static void ValidateCategoryScore(
            int value,
            int maximum,
            string propertyName)
        {
            if (value < 0 ||
                value > maximum)
            {
                throw new InvalidOperationException(
                    $"{propertyName} must be from 0 to {maximum}.");
            }
        }


        // =========================================================
        // VALIDATE BINARY UI ITEM SCORES
        // =========================================================
        //
        // These values exist only in the current assessment UI.
        // They are NOT database columns.
        // =========================================================

        private static void ValidateBinaryScores(
            ProgressCheckIn checkIn)
        {
            ValidateBinary(
                checkIn.Titik1,
                nameof(checkIn.Titik1));

            ValidateBinary(
                checkIn.Titik2,
                nameof(checkIn.Titik2));

            ValidateBinary(
                checkIn.Titik3,
                nameof(checkIn.Titik3));

            ValidateBinary(
                checkIn.Titik4,
                nameof(checkIn.Titik4));

            ValidateBinary(
                checkIn.Titik5,
                nameof(checkIn.Titik5));

            ValidateBinary(
                checkIn.Titik6,
                nameof(checkIn.Titik6));


            ValidateBinary(
                checkIn.Pantig1,
                nameof(checkIn.Pantig1));

            ValidateBinary(
                checkIn.Pantig2,
                nameof(checkIn.Pantig2));

            ValidateBinary(
                checkIn.Pantig3,
                nameof(checkIn.Pantig3));

            ValidateBinary(
                checkIn.Pantig4,
                nameof(checkIn.Pantig4));


            ValidateBinary(
                checkIn.Salita1,
                nameof(checkIn.Salita1));

            ValidateBinary(
                checkIn.Salita2,
                nameof(checkIn.Salita2));

            ValidateBinary(
                checkIn.Salita3,
                nameof(checkIn.Salita3));

            ValidateBinary(
                checkIn.Salita4,
                nameof(checkIn.Salita4));


            ValidateBinary(
                checkIn.Parirala1,
                nameof(checkIn.Parirala1));

            ValidateBinary(
                checkIn.Parirala2,
                nameof(checkIn.Parirala2));

            ValidateBinary(
                checkIn.Parirala3,
                nameof(checkIn.Parirala3));
        }


        // =========================================================
        // VALIDATE ONE BINARY UI SCORE
        // =========================================================

        private static void ValidateBinary(
            int value,
            string propertyName)
        {
            if (value != 0 &&
                value != 1)
            {
                throw new ArgumentException(
                    $"{propertyName} must be 0 or 1.");
            }
        }
    }
}