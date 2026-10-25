using ARALReadingSystem.Data;
using ARALReadingSystem.Models;
using Microsoft.Data.Sqlite;
using System;

namespace ARALReadingSystem.Services
{
    public static class CurrentPupilService
    {
        private const string CurrentPupilKey =
            "CurrentPupilId";


        // =====================================================
        // INITIALIZE
        // =====================================================

        public static void Initialize()
        {
            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS AppState
                (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                );
            ";


            command.ExecuteNonQuery();
        }


        // =====================================================
        // SET CURRENT PUPIL
        // =====================================================

        public static void SetCurrentPupil(
            int pupilId)
        {
            Initialize();


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            // Make sure pupil still exists.
            using (SqliteCommand check =
                   connection.CreateCommand())
            {
                check.CommandText = @"
                    SELECT COUNT(*)
                    FROM PUPIL
                    WHERE Pupil_ID = $id;
                ";


                check.Parameters.AddWithValue(
                    "$id",
                    pupilId);


                long count =
                    Convert.ToInt64(
                        check.ExecuteScalar()
                        ?? 0L);


                if (count == 0)
                {
                    return;
                }
            }


            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                INSERT INTO AppState
                (
                    Key,
                    Value
                )
                VALUES
                (
                    $key,
                    $value
                )

                ON CONFLICT(Key)
                DO UPDATE SET
                    Value = excluded.Value;
            ";


            command.Parameters.AddWithValue(
                "$key",
                CurrentPupilKey);


            command.Parameters.AddWithValue(
                "$value",
                pupilId.ToString());


            command.ExecuteNonQuery();
        }


        // =====================================================
        // GET CURRENT PUPIL ID
        // =====================================================

        public static int? GetCurrentPupilId()
        {
            Initialize();


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                SELECT Value
                FROM AppState
                WHERE Key = $key
                LIMIT 1;
            ";


            command.Parameters.AddWithValue(
                "$key",
                CurrentPupilKey);


            object? value =
                command.ExecuteScalar();


            if (value == null ||
                value == DBNull.Value)
            {
                return null;
            }


            if (int.TryParse(
                    value.ToString(),
                    out int pupilId))
            {
                return pupilId;
            }


            return null;
        }


        // =====================================================
        // GET CURRENT PUPIL
        // =====================================================

        public static Pupil? GetCurrentPupil()
        {
            int? pupilId =
                GetCurrentPupilId();


            if (!pupilId.HasValue)
            {
                return null;
            }


            PupilService pupilService =
                new PupilService();


            Pupil? pupil =
                pupilService.GetPupilById(
                    pupilId.Value);


            /*
             * The pupil may have been deleted.
             */
            if (pupil == null)
            {
                ClearCurrentPupil();

                return null;
            }


            return pupil;
        }


        // =====================================================
        // ENSURE CURRENT PUPIL
        // =====================================================

        public static Pupil? GetOrSelectCurrentPupil()
        {
            /*
             * First try the pupil explicitly selected earlier.
             */
            Pupil? current =
                GetCurrentPupil();


            if (current != null)
            {
                return current;
            }


            /*
             * If there has never been a selection, choose the
             * first registered pupil.
             *
             * IMPORTANT:
             * We do NOT choose whoever has the most progress.
             */
            PupilService pupilService =
                new PupilService();


            var pupils =
                pupilService.GetAllPupils();


            if (pupils.Count == 0)
            {
                return null;
            }


            Pupil firstPupil =
                pupils[0];


            SetCurrentPupil(
                firstPupil.Id);


            return firstPupil;
        }


        // =====================================================
        // CLEAR CURRENT PUPIL
        // =====================================================

        public static void ClearCurrentPupil()
        {
            Initialize();


            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();


            command.CommandText = @"
                DELETE FROM AppState
                WHERE Key = $key;
            ";


            command.Parameters.AddWithValue(
                "$key",
                CurrentPupilKey);


            command.ExecuteNonQuery();
        }
    }
}