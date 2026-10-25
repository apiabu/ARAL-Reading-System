using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace ARALReadingSystem.Data
{
    public static class DatabaseService
    {
        // =========================================================
        // DATABASE LOCATION
        // =========================================================

        private static readonly string DatabaseFolder =
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "ARALReadingSystem");

        private static readonly string DatabasePath =
            Path.Combine(
                DatabaseFolder,
                "aral.db");


        // =========================================================
        // CONNECTION
        // =========================================================

        public static SqliteConnection GetConnection()
        {
            EnsureDatabaseFolderExists();

            SqliteConnectionStringBuilder builder =
                new SqliteConnectionStringBuilder
                {
                    DataSource = DatabasePath,
                    ForeignKeys = true
                };

            return new SqliteConnection(
                builder.ToString());
        }


        // =========================================================
        // INITIALIZE DATABASE
        // =========================================================

        public static void InitializeDatabase()
        {
            try
            {
                EnsureDatabaseFolderExists();

                MigrateOldDatabaseIfNeeded();

                using SqliteConnection connection =
                    GetConnection();

                connection.Open();

                /*
                 * If the database already follows Figure 18 exactly,
                 * do not rebuild it.
                 */
                if (!IsFigure18Schema(connection))
                {
                    /*
                     * Only perform the legacy conversion when old
                     * application tables actually exist.
                     */
                    if (HasLegacyTables(connection))
                    {
                        MigrateLegacyDatabase(
                            connection);
                    }
                }

                CreateFigure18Tables(
                    connection);

                CreateDefaultTeacherIfNeeded(
                    connection);

                ValidateForeignKeys(
                    connection);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Failed to initialize ARAL database.\n\n" +
                    $"Database location:\n{DatabasePath}\n\n" +
                    ex.Message,
                    ex);
            }
        }


        // =========================================================
        // FIGURE 18 TABLES
        // =========================================================

        private static void CreateFigure18Tables(
            SqliteConnection connection)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS TEACHER
                (
                    Teacher_ID INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    Username TEXT
                        NOT NULL
                        UNIQUE,

                    Password TEXT
                        NOT NULL,

                    First_Name TEXT
                        NOT NULL,

                    Last_Name TEXT
                        NOT NULL
                );


                CREATE TABLE IF NOT EXISTS PUPIL
                (
                    Pupil_ID INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    Teacher_ID INTEGER
                        NOT NULL,

                    First_Name TEXT
                        NOT NULL,

                    Last_Name TEXT
                        NOT NULL,

                    Grade_Level TEXT,

                    Gender TEXT,

                    Birthdate TEXT,

                    FOREIGN KEY (Teacher_ID)
                        REFERENCES TEACHER(Teacher_ID)
                        ON UPDATE CASCADE
                        ON DELETE RESTRICT
                );


                CREATE TABLE IF NOT EXISTS CHECKIN_RECORD
                (
                    Record_ID INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    Pupil_ID INTEGER
                        NOT NULL,

                    Teacher_ID INTEGER
                        NOT NULL,

                    Checkin_Period INTEGER
                        NOT NULL,

                    Score_Titik INTEGER,

                    Score_Pantig INTEGER,

                    Score_Salita INTEGER,

                    Score_Parirala INTEGER,

                    Time_Taken INTEGER,

                    Remark TEXT,

                    Date_Assessed TEXT,

                    FOREIGN KEY (Pupil_ID)
                        REFERENCES PUPIL(Pupil_ID)
                        ON UPDATE CASCADE
                        ON DELETE CASCADE,

                    FOREIGN KEY (Teacher_ID)
                        REFERENCES TEACHER(Teacher_ID)
                        ON UPDATE CASCADE
                        ON DELETE RESTRICT
                );
            ";

            command.ExecuteNonQuery();
        }


        // =========================================================
        // EXACT FIGURE 18 SCHEMA CHECK
        // =========================================================

        private static bool IsFigure18Schema(
            SqliteConnection connection)
        {
            if (!TableExists(
                    connection,
                    "TEACHER") ||
                !TableExists(
                    connection,
                    "PUPIL") ||
                !TableExists(
                    connection,
                    "CHECKIN_RECORD"))
            {
                return false;
            }

            string[] teacherColumns =
            {
                "Teacher_ID",
                "Username",
                "Password",
                "First_Name",
                "Last_Name"
            };

            string[] pupilColumns =
            {
                "Pupil_ID",
                "Teacher_ID",
                "First_Name",
                "Last_Name",
                "Grade_Level",
                "Gender",
                "Birthdate"
            };

            string[] checkInColumns =
            {
                "Record_ID",
                "Pupil_ID",
                "Teacher_ID",
                "Checkin_Period",
                "Score_Titik",
                "Score_Pantig",
                "Score_Salita",
                "Score_Parirala",
                "Time_Taken",
                "Remark",
                "Date_Assessed"
            };

            return TableHasExactlyColumns(
                       connection,
                       "TEACHER",
                       teacherColumns) &&
                   TableHasExactlyColumns(
                       connection,
                       "PUPIL",
                       pupilColumns) &&
                   TableHasExactlyColumns(
                       connection,
                       "CHECKIN_RECORD",
                       checkInColumns);
        }


        private static bool TableHasExactlyColumns(
            SqliteConnection connection,
            string tableName,
            string[] expectedColumns)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText =
                $"PRAGMA table_info([{tableName}]);";

            using SqliteDataReader reader =
                command.ExecuteReader();

            int index = 0;

            while (reader.Read())
            {
                if (index >=
                    expectedColumns.Length)
                {
                    return false;
                }

                string actualColumn =
                    reader.GetString(1);

                if (!string.Equals(
                        actualColumn,
                        expectedColumns[index],
                        StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                index++;
            }

            return index ==
                   expectedColumns.Length;
        }


        // =========================================================
        // LEGACY TABLE CHECK
        // =========================================================

        private static bool HasLegacyTables(
            SqliteConnection connection)
        {
            return
                TableExists(
                    connection,
                    "Teachers") ||
                TableExists(
                    connection,
                    "Pupils") ||
                TableExists(
                    connection,
                    "ReadingSessions") ||
                TableExists(
                    connection,
                    "ProgressCheckIns");
        }


        // =========================================================
        // LEGACY MIGRATION
        // =========================================================

        private static void MigrateLegacyDatabase(
            SqliteConnection connection)
        {
            bool hasTeachers =
                TableExists(
                    connection,
                    "Teachers");

            bool hasPupils =
                TableExists(
                    connection,
                    "Pupils");

            bool hasCheckIns =
                TableExists(
                    connection,
                    "ProgressCheckIns");


            /*
             * SQLite does not allow PRAGMA foreign_keys to be
             * meaningfully changed while a transaction is active.
             * Disable it BEFORE beginning the migration transaction.
             */
            SetForeignKeys(
                connection,
                false);


            using SqliteTransaction transaction =
                connection.BeginTransaction();

            try
            {
                DropTemporaryMigrationTables(
                    connection,
                    transaction);

                CreateTemporaryFigure18Tables(
                    connection,
                    transaction);


                if (hasTeachers)
                {
                    MigrateTeachers(
                        connection,
                        transaction);
                }


                EnsureTemporaryTeacher(
                    connection,
                    transaction);


                if (hasPupils)
                {
                    MigratePupils(
                        connection,
                        transaction);
                }


                if (hasCheckIns)
                {
                    MigrateCheckIns(
                        connection,
                        transaction);
                }


                ReplaceLegacyTables(
                    connection,
                    transaction);


                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();

                throw;
            }
            finally
            {
                SetForeignKeys(
                    connection,
                    true);
            }


            ValidateForeignKeys(
                connection);
        }


        // =========================================================
        // TEMP TABLES
        // =========================================================

        private static void DropTemporaryMigrationTables(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.Transaction =
                transaction;

            command.CommandText = @"
                DROP TABLE IF EXISTS CHECKIN_RECORD_NEW;
                DROP TABLE IF EXISTS PUPIL_NEW;
                DROP TABLE IF EXISTS TEACHER_NEW;
            ";

            command.ExecuteNonQuery();
        }


        private static void CreateTemporaryFigure18Tables(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.Transaction =
                transaction;

            command.CommandText = @"
                CREATE TABLE TEACHER_NEW
                (
                    Teacher_ID INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    Username TEXT
                        NOT NULL
                        UNIQUE,

                    Password TEXT
                        NOT NULL,

                    First_Name TEXT
                        NOT NULL,

                    Last_Name TEXT
                        NOT NULL
                );


                CREATE TABLE PUPIL_NEW
                (
                    Pupil_ID INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    Teacher_ID INTEGER
                        NOT NULL,

                    First_Name TEXT
                        NOT NULL,

                    Last_Name TEXT
                        NOT NULL,

                    Grade_Level TEXT,

                    Gender TEXT,

                    Birthdate TEXT
                );


                CREATE TABLE CHECKIN_RECORD_NEW
                (
                    Record_ID INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    Pupil_ID INTEGER
                        NOT NULL,

                    Teacher_ID INTEGER
                        NOT NULL,

                    Checkin_Period INTEGER
                        NOT NULL,

                    Score_Titik INTEGER,

                    Score_Pantig INTEGER,

                    Score_Salita INTEGER,

                    Score_Parirala INTEGER,

                    Time_Taken INTEGER,

                    Remark TEXT,

                    Date_Assessed TEXT
                );
            ";

            command.ExecuteNonQuery();
        }


        // =========================================================
        // MIGRATE TEACHERS
        // =========================================================

        private static void MigrateTeachers(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            bool hasId =
                ColumnExists(
                    connection,
                    transaction,
                    "Teachers",
                    "Id");

            bool hasUsername =
                ColumnExists(
                    connection,
                    transaction,
                    "Teachers",
                    "Username");

            bool hasFullName =
                ColumnExists(
                    connection,
                    transaction,
                    "Teachers",
                    "FullName");


            if (!hasUsername)
            {
                return;
            }


            string idExpression =
                hasId
                    ? "Id"
                    : "NULL";


            string fullNameExpression =
                hasFullName
                    ? "COALESCE(FullName, '')"
                    : "''";


            /*
             * The legacy database stored PasswordHash /
             * PasswordSalt. Those values are NOT plaintext
             * passwords and must not be copied into the Figure 18
             * Password field.
             *
             * For the existing default teacher account, preserve
             * the application's known default password instead.
             */

            using SqliteCommand select =
                connection.CreateCommand();

            select.Transaction =
                transaction;

            select.CommandText = $@"
                SELECT
                    {idExpression},
                    Username,
                    {fullNameExpression}
                FROM Teachers;
            ";


            using SqliteDataReader reader =
                select.ExecuteReader();

            while (reader.Read())
            {
                int? teacherId =
                    reader.IsDBNull(0)
                        ? null
                        : Convert.ToInt32(
                            reader.GetValue(0));

                string username =
                    reader.IsDBNull(1)
                        ? ""
                        : reader.GetString(1)
                            .Trim();

                string fullName =
                    reader.IsDBNull(2)
                        ? ""
                        : reader.GetString(2)
                            .Trim();


                if (string.IsNullOrWhiteSpace(
                        username))
                {
                    continue;
                }


                SplitTeacherName(
                    fullName,
                    out string firstName,
                    out string lastName);


                /*
                 * The original application created the default
                 * teacher as:
                 *
                 * Username: teacher
                 * Password: Teacher123!
                 *
                 * Because the old database stores only a hash and
                 * salt, the original plaintext cannot be recovered.
                 */

                string password =
                    string.Equals(
                        username,
                        "teacher",
                        StringComparison.OrdinalIgnoreCase)
                        ? "Teacher123!"
                        : "Teacher123!";


                using SqliteCommand insert =
                    connection.CreateCommand();

                insert.Transaction =
                    transaction;

                insert.CommandText = @"
                    INSERT OR IGNORE INTO TEACHER_NEW
                    (
                        Teacher_ID,
                        Username,
                        Password,
                        First_Name,
                        Last_Name
                    )
                    VALUES
                    (
                        $teacherId,
                        $username,
                        $password,
                        $firstName,
                        $lastName
                    );
                ";


                insert.Parameters.AddWithValue(
                    "$teacherId",
                    teacherId.HasValue
                        ? teacherId.Value
                        : DBNull.Value);


                insert.Parameters.AddWithValue(
                    "$username",
                    username);


                insert.Parameters.AddWithValue(
                    "$password",
                    password);


                insert.Parameters.AddWithValue(
                    "$firstName",
                    firstName);


                insert.Parameters.AddWithValue(
                    "$lastName",
                    lastName);


                insert.ExecuteNonQuery();
            }
        }


        // =========================================================
        // TEMPORARY TEACHER
        // =========================================================

        private static void EnsureTemporaryTeacher(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            using SqliteCommand count =
                connection.CreateCommand();

            count.Transaction =
                transaction;

            count.CommandText =
                "SELECT COUNT(*) FROM TEACHER_NEW;";


            long teacherCount =
                Convert.ToInt64(
                    count.ExecuteScalar());


            if (teacherCount > 0)
            {
                return;
            }


            using SqliteCommand insert =
                connection.CreateCommand();

            insert.Transaction =
                transaction;

            insert.CommandText = @"
                INSERT INTO TEACHER_NEW
                (
                    Username,
                    Password,
                    First_Name,
                    Last_Name
                )
                VALUES
                (
                    'teacher',
                    'Teacher123!',
                    'ARAL',
                    'Teacher'
                );
            ";

            insert.ExecuteNonQuery();
        }


        // =========================================================
        // MIGRATE PUPILS
        // =========================================================

        private static void MigratePupils(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            bool hasId =
                ColumnExists(
                    connection,
                    transaction,
                    "Pupils",
                    "Id");

            bool hasFirstName =
                ColumnExists(
                    connection,
                    transaction,
                    "Pupils",
                    "FirstName");

            bool hasLastName =
                ColumnExists(
                    connection,
                    transaction,
                    "Pupils",
                    "LastName");

            bool hasGradeLevel =
                ColumnExists(
                    connection,
                    transaction,
                    "Pupils",
                    "GradeLevel");

            bool hasSex =
                ColumnExists(
                    connection,
                    transaction,
                    "Pupils",
                    "Sex");


            if (!hasFirstName ||
                !hasLastName)
            {
                return;
            }


            string idExpression =
                hasId
                    ? "Id"
                    : "NULL";


            string gradeExpression =
                hasGradeLevel
                    ? "GradeLevel"
                    : "NULL";


            string genderExpression =
                hasSex
                    ? "Sex"
                    : "NULL";


            int teacherId =
                GetFirstTemporaryTeacherId(
                    connection,
                    transaction);


            using SqliteCommand select =
                connection.CreateCommand();

            select.Transaction =
                transaction;

            select.CommandText = $@"
                SELECT
                    {idExpression},
                    FirstName,
                    LastName,
                    {gradeExpression},
                    {genderExpression}
                FROM Pupils;
            ";


            using SqliteDataReader reader =
                select.ExecuteReader();

            while (reader.Read())
            {
                int? pupilId =
                    reader.IsDBNull(0)
                        ? null
                        : Convert.ToInt32(
                            reader.GetValue(0));


                string firstName =
                    reader.IsDBNull(1)
                        ? ""
                        : reader.GetString(1)
                            .Trim();


                string lastName =
                    reader.IsDBNull(2)
                        ? ""
                        : reader.GetString(2)
                            .Trim();


                object gradeLevel =
                    reader.IsDBNull(3)
                        ? DBNull.Value
                        : reader.GetString(3);


                object gender =
                    reader.IsDBNull(4)
                        ? DBNull.Value
                        : reader.GetString(4);


                /*
                 * IMPORTANT:
                 *
                 * The old Pupils table stored Age, not Birthdate.
                 * An exact birthdate cannot be recovered from Age.
                 *
                 * Therefore Birthdate is migrated as NULL.
                 * We do NOT invent January 1 or another date.
                 */


                using SqliteCommand insert =
                    connection.CreateCommand();

                insert.Transaction =
                    transaction;

                insert.CommandText = @"
                    INSERT OR IGNORE INTO PUPIL_NEW
                    (
                        Pupil_ID,
                        Teacher_ID,
                        First_Name,
                        Last_Name,
                        Grade_Level,
                        Gender,
                        Birthdate
                    )
                    VALUES
                    (
                        $pupilId,
                        $teacherId,
                        $firstName,
                        $lastName,
                        $gradeLevel,
                        $gender,
                        NULL
                    );
                ";


                insert.Parameters.AddWithValue(
                    "$pupilId",
                    pupilId.HasValue
                        ? pupilId.Value
                        : DBNull.Value);


                insert.Parameters.AddWithValue(
                    "$teacherId",
                    teacherId);


                insert.Parameters.AddWithValue(
                    "$firstName",
                    firstName);


                insert.Parameters.AddWithValue(
                    "$lastName",
                    lastName);


                insert.Parameters.AddWithValue(
                    "$gradeLevel",
                    gradeLevel);


                insert.Parameters.AddWithValue(
                    "$gender",
                    gender);


                insert.ExecuteNonQuery();
            }
        }


        // =========================================================
        // MIGRATE CHECK-INS
        // =========================================================

        private static void MigrateCheckIns(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            if (!ColumnExists(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "PupilId") ||
                !ColumnExists(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "CheckInNumber"))
            {
                return;
            }


            bool hasId =
                ColumnExists(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "Id");

            bool hasCompleted =
                ColumnExists(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "Completed");

            bool hasCompletedAt =
                ColumnExists(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "CompletedAt");

            bool hasDuration =
                ColumnExists(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "DurationSeconds");

            bool hasTeacherNotes =
                ColumnExists(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "TeacherNotes");


            string idExpression =
                hasId
                    ? "Id"
                    : "NULL";


            string completedExpression =
                hasCompleted
                    ? "Completed"
                    : "1";


            string completedAtExpression =
                hasCompletedAt
                    ? "CompletedAt"
                    : "NULL";


            string durationExpression =
                hasDuration
                    ? "DurationSeconds"
                    : "NULL";


            string notesExpression =
                hasTeacherNotes
                    ? "TeacherNotes"
                    : "NULL";


            string titikExpression =
                BuildScoreExpression(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "Titik",
                    6);


            string pantigExpression =
                BuildScoreExpression(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "Pantig",
                    4);


            string salitaExpression =
                BuildScoreExpression(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "Salita",
                    4);


            /*
             * There are THREE Parirala items.
             */
            string pariralaExpression =
                BuildScoreExpression(
                    connection,
                    transaction,
                    "ProgressCheckIns",
                    "Parirala",
                    3);


            using SqliteCommand select =
                connection.CreateCommand();

            select.Transaction =
                transaction;

            select.CommandText = $@"
                SELECT
                    {idExpression},
                    PupilId,
                    CheckInNumber,
                    {titikExpression},
                    {pantigExpression},
                    {salitaExpression},
                    {pariralaExpression},
                    {durationExpression},
                    {notesExpression},
                    {completedAtExpression},
                    {completedExpression}
                FROM ProgressCheckIns;
            ";


            using SqliteDataReader reader =
                select.ExecuteReader();

            while (reader.Read())
            {
                bool completed =
                    !reader.IsDBNull(10) &&
                    Convert.ToInt32(
                        reader.GetValue(10)) == 1;


                if (!completed)
                {
                    continue;
                }


                int? recordId =
                    reader.IsDBNull(0)
                        ? null
                        : Convert.ToInt32(
                            reader.GetValue(0));


                int pupilId =
                    Convert.ToInt32(
                        reader.GetValue(1));


                int checkinPeriod =
                    Convert.ToInt32(
                        reader.GetValue(2));


                int scoreTitik =
                    reader.IsDBNull(3)
                        ? 0
                        : Convert.ToInt32(
                            reader.GetValue(3));


                int scorePantig =
                    reader.IsDBNull(4)
                        ? 0
                        : Convert.ToInt32(
                            reader.GetValue(4));


                int scoreSalita =
                    reader.IsDBNull(5)
                        ? 0
                        : Convert.ToInt32(
                            reader.GetValue(5));


                int scoreParirala =
                    reader.IsDBNull(6)
                        ? 0
                        : Convert.ToInt32(
                            reader.GetValue(6));


                object timeTaken =
                    reader.IsDBNull(7)
                        ? DBNull.Value
                        : Convert.ToInt32(
                            reader.GetValue(7));


                object remark =
                    reader.IsDBNull(8)
                        ? DBNull.Value
                        : reader.GetString(8);


                object dateAssessed =
                    reader.IsDBNull(9) ||
                    string.IsNullOrWhiteSpace(
                        Convert.ToString(
                            reader.GetValue(9)))
                        ? DBNull.Value
                        : Convert.ToString(
                            reader.GetValue(9))!;


                int teacherId =
                    GetTeacherIdForTemporaryPupil(
                        connection,
                        transaction,
                        pupilId);


                if (teacherId <= 0)
                {
                    continue;
                }


                using SqliteCommand insert =
                    connection.CreateCommand();

                insert.Transaction =
                    transaction;

                insert.CommandText = @"
                    INSERT OR IGNORE INTO CHECKIN_RECORD_NEW
                    (
                        Record_ID,
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
                    SELECT
                        $recordId,
                        $pupilId,
                        $teacherId,
                        $checkinPeriod,
                        $scoreTitik,
                        $scorePantig,
                        $scoreSalita,
                        $scoreParirala,
                        $timeTaken,
                        $remark,
                        $dateAssessed
                    WHERE EXISTS
                    (
                        SELECT 1
                        FROM PUPIL_NEW
                        WHERE Pupil_ID =
                              $pupilId
                    );
                ";


                insert.Parameters.AddWithValue(
                    "$recordId",
                    recordId.HasValue
                        ? recordId.Value
                        : DBNull.Value);


                insert.Parameters.AddWithValue(
                    "$pupilId",
                    pupilId);


                insert.Parameters.AddWithValue(
                    "$teacherId",
                    teacherId);


                insert.Parameters.AddWithValue(
                    "$checkinPeriod",
                    checkinPeriod);


                insert.Parameters.AddWithValue(
                    "$scoreTitik",
                    scoreTitik);


                insert.Parameters.AddWithValue(
                    "$scorePantig",
                    scorePantig);


                insert.Parameters.AddWithValue(
                    "$scoreSalita",
                    scoreSalita);


                insert.Parameters.AddWithValue(
                    "$scoreParirala",
                    scoreParirala);


                insert.Parameters.AddWithValue(
                    "$timeTaken",
                    timeTaken);


                insert.Parameters.AddWithValue(
                    "$remark",
                    remark);


                insert.Parameters.AddWithValue(
                    "$dateAssessed",
                    dateAssessed);


                insert.ExecuteNonQuery();
            }
        }


        // =========================================================
        // REPLACE OLD TABLES
        // =========================================================

        private static void ReplaceLegacyTables(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.Transaction =
                transaction;

            command.CommandText = @"
                DROP TABLE IF EXISTS ReadingSessions;
                DROP TABLE IF EXISTS ProgressCheckIns;
                DROP TABLE IF EXISTS Pupils;
                DROP TABLE IF EXISTS Teachers;

                DROP TABLE IF EXISTS CHECKIN_RECORD;
                DROP TABLE IF EXISTS PUPIL;
                DROP TABLE IF EXISTS TEACHER;


                ALTER TABLE TEACHER_NEW
                    RENAME TO TEACHER;

                ALTER TABLE PUPIL_NEW
                    RENAME TO PUPIL;

                ALTER TABLE CHECKIN_RECORD_NEW
                    RENAME TO CHECKIN_RECORD;
            ";

            command.ExecuteNonQuery();


            /*
             * Temporary tables intentionally have no FK clauses.
             * SQLite cannot add FK constraints with ALTER COLUMN.
             *
             * Rebuild the final PUPIL and CHECKIN_RECORD tables
             * once so the final schema has the proper relationships.
             */

            RebuildFinalForeignKeyTables(
                connection,
                transaction);
        }


        // =========================================================
        // FINAL FOREIGN KEY REBUILD
        // =========================================================

        private static void RebuildFinalForeignKeyTables(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.Transaction =
                transaction;

            command.CommandText = @"
                ALTER TABLE PUPIL
                    RENAME TO PUPIL_DATA;

                ALTER TABLE CHECKIN_RECORD
                    RENAME TO CHECKIN_RECORD_DATA;


                CREATE TABLE PUPIL
                (
                    Pupil_ID INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    Teacher_ID INTEGER
                        NOT NULL,

                    First_Name TEXT
                        NOT NULL,

                    Last_Name TEXT
                        NOT NULL,

                    Grade_Level TEXT,

                    Gender TEXT,

                    Birthdate TEXT,

                    FOREIGN KEY (Teacher_ID)
                        REFERENCES TEACHER(Teacher_ID)
                        ON UPDATE CASCADE
                        ON DELETE RESTRICT
                );


                INSERT INTO PUPIL
                (
                    Pupil_ID,
                    Teacher_ID,
                    First_Name,
                    Last_Name,
                    Grade_Level,
                    Gender,
                    Birthdate
                )
                SELECT
                    Pupil_ID,
                    Teacher_ID,
                    First_Name,
                    Last_Name,
                    Grade_Level,
                    Gender,
                    Birthdate
                FROM PUPIL_DATA;


                CREATE TABLE CHECKIN_RECORD
                (
                    Record_ID INTEGER
                        PRIMARY KEY AUTOINCREMENT,

                    Pupil_ID INTEGER
                        NOT NULL,

                    Teacher_ID INTEGER
                        NOT NULL,

                    Checkin_Period INTEGER
                        NOT NULL,

                    Score_Titik INTEGER,

                    Score_Pantig INTEGER,

                    Score_Salita INTEGER,

                    Score_Parirala INTEGER,

                    Time_Taken INTEGER,

                    Remark TEXT,

                    Date_Assessed TEXT,

                    FOREIGN KEY (Pupil_ID)
                        REFERENCES PUPIL(Pupil_ID)
                        ON UPDATE CASCADE
                        ON DELETE CASCADE,

                    FOREIGN KEY (Teacher_ID)
                        REFERENCES TEACHER(Teacher_ID)
                        ON UPDATE CASCADE
                        ON DELETE RESTRICT
                );


                INSERT INTO CHECKIN_RECORD
                (
                    Record_ID,
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
                SELECT
                    Record_ID,
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
                FROM CHECKIN_RECORD_DATA;


                DROP TABLE CHECKIN_RECORD_DATA;
                DROP TABLE PUPIL_DATA;
            ";

            command.ExecuteNonQuery();
        }


        // =========================================================
        // SCORE EXPRESSION
        // =========================================================

        private static string BuildScoreExpression(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string tableName,
            string prefix,
            int maximumItems)
        {
            string expression =
                "0";


            for (int index = 1;
                 index <= maximumItems;
                 index++)
            {
                string columnName =
                    $"{prefix}{index}";


                if (!ColumnExists(
                        connection,
                        transaction,
                        tableName,
                        columnName))
                {
                    continue;
                }


                expression +=
                    $" + COALESCE([{columnName}], 0)";
            }


            return expression;
        }


        // =========================================================
        // TEACHER LOOKUPS
        // =========================================================

        private static int GetFirstTemporaryTeacherId(
            SqliteConnection connection,
            SqliteTransaction transaction)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.Transaction =
                transaction;

            command.CommandText = @"
                SELECT Teacher_ID
                FROM TEACHER_NEW
                ORDER BY Teacher_ID
                LIMIT 1;
            ";


            object? result =
                command.ExecuteScalar();


            if (result == null ||
                result == DBNull.Value)
            {
                throw new InvalidOperationException(
                    "No teacher is available during database migration.");
            }


            return Convert.ToInt32(
                result);
        }


        private static int GetTeacherIdForTemporaryPupil(
            SqliteConnection connection,
            SqliteTransaction transaction,
            int pupilId)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.Transaction =
                transaction;

            command.CommandText = @"
                SELECT Teacher_ID
                FROM PUPIL_NEW
                WHERE Pupil_ID =
                      $pupilId
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
                return 0;
            }


            return Convert.ToInt32(
                result);
        }


        // =========================================================
        // DEFAULT TEACHER
        // =========================================================

        private static void CreateDefaultTeacherIfNeeded(
            SqliteConnection connection)
        {
            using SqliteCommand count =
                connection.CreateCommand();

            count.CommandText =
                "SELECT COUNT(*) FROM TEACHER;";


            long teacherCount =
                Convert.ToInt64(
                    count.ExecuteScalar());


            if (teacherCount > 0)
            {
                return;
            }


            using SqliteCommand insert =
                connection.CreateCommand();

            insert.CommandText = @"
                INSERT INTO TEACHER
                (
                    Username,
                    Password,
                    First_Name,
                    Last_Name
                )
                VALUES
                (
                    'teacher',
                    'Teacher123!',
                    'ARAL',
                    'Teacher'
                );
            ";


            insert.ExecuteNonQuery();
        }


        // =========================================================
        // FOREIGN KEYS
        // =========================================================

        private static void SetForeignKeys(
            SqliteConnection connection,
            bool enabled)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText =
                enabled
                    ? "PRAGMA foreign_keys = ON;"
                    : "PRAGMA foreign_keys = OFF;";

            command.ExecuteNonQuery();
        }


        private static void ValidateForeignKeys(
            SqliteConnection connection)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText =
                "PRAGMA foreign_key_check;";


            using SqliteDataReader reader =
                command.ExecuteReader();


            if (reader.Read())
            {
                string table =
                    reader.IsDBNull(0)
                        ? "Unknown"
                        : reader.GetString(0);

                throw new InvalidOperationException(
                    "Database foreign-key validation failed. " +
                    $"Problem table: {table}.");
            }
        }


        // =========================================================
        // TABLE EXISTS
        // =========================================================

        private static bool TableExists(
            SqliteConnection connection,
            string tableName)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.CommandText = @"
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table'
                  AND lower(name) =
                      lower($tableName);
            ";


            command.Parameters.AddWithValue(
                "$tableName",
                tableName);


            return Convert.ToInt64(
                       command.ExecuteScalar()) > 0;
        }


        // =========================================================
        // COLUMN EXISTS
        // =========================================================

        private static bool ColumnExists(
            SqliteConnection connection,
            SqliteTransaction transaction,
            string tableName,
            string columnName)
        {
            using SqliteCommand command =
                connection.CreateCommand();

            command.Transaction =
                transaction;

            command.CommandText =
                $"PRAGMA table_info([{tableName}]);";


            using SqliteDataReader reader =
                command.ExecuteReader();


            while (reader.Read())
            {
                string existingColumn =
                    reader.GetString(1);


                if (string.Equals(
                        existingColumn,
                        columnName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }


            return false;
        }


        // =========================================================
        // TEACHER NAME CONVERSION
        // =========================================================

        private static void SplitTeacherName(
            string fullName,
            out string firstName,
            out string lastName)
        {
            string cleaned =
                fullName?.Trim() ?? "";


            if (string.IsNullOrWhiteSpace(
                    cleaned))
            {
                firstName =
                    "ARAL";

                lastName =
                    "Teacher";

                return;
            }


            string[] parts =
                cleaned.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);


            if (parts.Length == 1)
            {
                firstName =
                    parts[0];

                lastName =
                    "Teacher";

                return;
            }


            lastName =
                parts[^1];


            firstName =
                string.Join(
                    " ",
                    parts,
                    0,
                    parts.Length - 1);
        }


        // =========================================================
        // DATABASE DIRECTORY
        // =========================================================

        private static void EnsureDatabaseFolderExists()
        {
            if (!Directory.Exists(
                    DatabaseFolder))
            {
                Directory.CreateDirectory(
                    DatabaseFolder);
            }
        }


        // =========================================================
        // OLD BUILD-FOLDER DATABASE
        // =========================================================

        private static void MigrateOldDatabaseIfNeeded()
        {
            string oldDatabasePath =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "aral.db");


            if (File.Exists(
                    DatabasePath))
            {
                return;
            }


            if (!File.Exists(
                    oldDatabasePath))
            {
                return;
            }


            string oldFullPath =
                Path.GetFullPath(
                    oldDatabasePath);


            string newFullPath =
                Path.GetFullPath(
                    DatabasePath);


            if (string.Equals(
                    oldFullPath,
                    newFullPath,
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }


            File.Copy(
                oldDatabasePath,
                DatabasePath,
                overwrite: false);
        }
    }
}