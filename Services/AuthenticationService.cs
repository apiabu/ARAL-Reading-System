using ARALReadingSystem.Data;
using Microsoft.Data.Sqlite;
using System;

namespace ARALReadingSystem.Services
{
    public class AuthenticationService
    {
        // =========================================================
        // LOGIN
        // =========================================================

        public bool Login(
            string username,
            string password,
            out string teacherName)
        {
            teacherName =
                string.Empty;


            // =====================================================
            // VALIDATION
            // =====================================================

            if (string.IsNullOrWhiteSpace(
                    username) ||
                string.IsNullOrWhiteSpace(
                    password))
            {
                return false;
            }


            string cleanUsername =
                username.Trim();


            // =====================================================
            // DATABASE
            // =====================================================

            using SqliteConnection connection =
                DatabaseService.GetConnection();

            connection.Open();


            using SqliteCommand command =
                connection.CreateCommand();


            // =====================================================
            // FIGURE 18 TEACHER TABLE
            // =====================================================
            //
            // TEACHER
            //
            // Teacher_ID
            // Username
            // Password
            // First_Name
            // Last_Name
            //
            // No PasswordHash.
            // No PasswordSalt.
            // No FullName database column.
            // =====================================================

            command.CommandText = @"
                SELECT
                    Teacher_ID,
                    Username,
                    Password,
                    First_Name,
                    Last_Name

                FROM TEACHER

                WHERE Username = $username

                LIMIT 1;
            ";


            command.Parameters.AddWithValue(
                "$username",
                cleanUsername);


            using SqliteDataReader reader =
                command.ExecuteReader();


            // =====================================================
            // USERNAME NOT FOUND
            // =====================================================

            if (!reader.Read())
            {
                return false;
            }


            // =====================================================
            // READ TEACHER
            // =====================================================

            string storedPassword =
                reader.IsDBNull(2)
                    ? string.Empty
                    : reader.GetString(2);


            string firstName =
                reader.IsDBNull(3)
                    ? string.Empty
                    : reader.GetString(3)
                        .Trim();


            string lastName =
                reader.IsDBNull(4)
                    ? string.Empty
                    : reader.GetString(4)
                        .Trim();


            // =====================================================
            // PASSWORD
            // =====================================================

            bool valid =
                string.Equals(
                    storedPassword,
                    password,
                    StringComparison.Ordinal);


            if (!valid)
            {
                return false;
            }


            // =====================================================
            // TEACHER DISPLAY NAME
            // =====================================================

            teacherName =
                BuildTeacherName(
                    firstName,
                    lastName);


            return true;
        }


        // =========================================================
        // BUILD TEACHER NAME
        // =========================================================

        private static string BuildTeacherName(
            string firstName,
            string lastName)
        {
            string fullName =
                $"{firstName} {lastName}"
                    .Trim();


            if (!string.IsNullOrWhiteSpace(
                    fullName))
            {
                return fullName;
            }


            return "ARAL Teacher";
        }
    }
}