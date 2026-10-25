using ARALReadingSystem.Models;
using ARALReadingSystem.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ARALReadingSystem.Views
{
    public partial class CheckInScorecardPage : Page
    {
        // =========================================================
        // DATA
        // =========================================================

        private readonly Pupil _pupil;

        private readonly ProgressCheckIn _checkIn;

        private readonly ProgressCheckInService _checkInService =
            new ProgressCheckInService();


        // =========================================================
        // EVENTS
        // =========================================================

        public event Action? BackRequested;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public CheckInScorecardPage(
            Pupil pupil,
            ProgressCheckIn checkIn)
        {
            InitializeComponent();


            _pupil =
                pupil ??
                throw new ArgumentNullException(
                    nameof(pupil));


            _checkIn =
                checkIn ??
                throw new ArgumentNullException(
                    nameof(checkIn));


            Loaded +=
                CheckInScorecardPage_Loaded;
        }


        // =========================================================
        // LOAD
        // =========================================================

        private void CheckInScorecardPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -=
                CheckInScorecardPage_Loaded;


            LoadScorecard();
        }


        // =========================================================
        // LOAD SCORECARD
        // =========================================================

        private void LoadScorecard()
        {
            // -----------------------------------------------------
            // HEADER
            // -----------------------------------------------------

            PageTitleText.Text =
                $"Scorecard: {GetDisplayName(_pupil)}";


            string gradeLevel =
                string.IsNullOrWhiteSpace(
                    _pupil.GradeLevel)
                    ? "—"
                    : _pupil.GradeLevel;


            /*
             * GuardianName was removed here.
             *
             * Figure 18 PUPIL contains only:
             *
             * Pupil_ID
             * Teacher_ID
             * First_Name
             * Last_Name
             * Grade_Level
             * Gender
             * Birthdate
             */

            PageSubtitleText.Text =
                $"{gradeLevel} ARAL cohort";


            // -----------------------------------------------------
            // CHECK-IN
            // -----------------------------------------------------

            CheckInTitleText.Text =
                $"Check-in #{_checkIn.CheckInNumber}";


            int requiredSession =
                _checkInService.GetRequiredSession(
                    _checkIn.CheckInNumber);


            MilestoneText.Text =
                $"Matapos ang Sesyon {requiredSession}";


            // -----------------------------------------------------
            // TOTAL SCORE
            // -----------------------------------------------------

            OverallScoreText.Text =
                _checkIn.TotalScore.ToString();


            TitikScoreText.Text =
                $"{_checkIn.TitikScore} / 6";


            PantigScoreText.Text =
                $"{_checkIn.PantigScore} / 4";


            SalitaScoreText.Text =
                $"{_checkIn.SalitaScore} / 4";


            PariralaScoreText.Text =
                $"{_checkIn.PariralaScore} / 3";


            // -----------------------------------------------------
            // TIME / DATE
            // -----------------------------------------------------

            string duration =
                FormatDuration(
                    _checkIn.DurationSeconds);


            string date =
                FormatCompletedDate(
                    _checkIn.CompletedAt);


            if (!string.IsNullOrWhiteSpace(
                    duration) &&
                !string.IsNullOrWhiteSpace(
                    date))
            {
                DurationDateText.Text =
                    $"Oras: {duration} · {date}";
            }
            else if (!string.IsNullOrWhiteSpace(
                         duration))
            {
                DurationDateText.Text =
                    $"Oras: {duration}";
            }
            else if (!string.IsNullOrWhiteSpace(
                         date))
            {
                DurationDateText.Text =
                    date;
            }
            else
            {
                DurationDateText.Text =
                    "";
            }


            // -----------------------------------------------------
            // CATEGORY RESULT DISPLAY
            // -----------------------------------------------------
            //
            // CHECKIN_RECORD stores category totals only.
            //
            // Therefore the permanent scorecard displays the
            // category totals rather than claiming that historical
            // individual item answers are still stored.
            //
            // -----------------------------------------------------


            // -----------------------------------------------------
            // TITIK
            // -----------------------------------------------------

            TitikResultsPanel.Children.Clear();


            AddCategorySummaryChip(
                TitikResultsPanel,
                "Titik",
                _checkIn.TitikScore,
                6);


            // -----------------------------------------------------
            // PANTIG
            // -----------------------------------------------------

            PantigResultsPanel.Children.Clear();


            AddCategorySummaryChip(
                PantigResultsPanel,
                "Pantig",
                _checkIn.PantigScore,
                4);


            // -----------------------------------------------------
            // SALITA
            // -----------------------------------------------------

            SalitaResultsPanel.Children.Clear();


            AddCategorySummaryChip(
                SalitaResultsPanel,
                "Salita",
                _checkIn.SalitaScore,
                4);


            // -----------------------------------------------------
            // PARIRALA
            // -----------------------------------------------------

            PariralaResultsPanel.Children.Clear();


            AddCategorySummaryChip(
                PariralaResultsPanel,
                "Parirala",
                _checkIn.PariralaScore,
                3);


            // -----------------------------------------------------
            // REMARK / TEACHER NOTES
            // -----------------------------------------------------

            TeacherNotesText.Text =
                string.IsNullOrWhiteSpace(
                    _checkIn.TeacherNotes)
                    ? "Walang puna ng guro."
                    : _checkIn.TeacherNotes;
        }


        // =========================================================
        // CATEGORY SUMMARY CHIP
        // =========================================================

        private static void AddCategorySummaryChip(
            WrapPanel panel,
            string category,
            int score,
            int maximumScore)
        {
            Border chip =
                new Border
                {
                    CornerRadius =
                        new CornerRadius(11),

                    Padding =
                        new Thickness(
                            14,
                            9,
                            14,
                            9),

                    Margin =
                        new Thickness(
                            0,
                            0,
                            8,
                            8),

                    Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                239,
                                248,
                                254)),

                    BorderBrush =
                        new SolidColorBrush(
                            Color.FromRgb(
                                202,
                                224,
                                238)),

                    BorderThickness =
                        new Thickness(1)
                };


            StackPanel content =
                new StackPanel
                {
                    Orientation =
                        Orientation.Horizontal
                };


            TextBlock categoryText =
                new TextBlock
                {
                    Text =
                        $"{category}: ",

                    FontSize =
                        14,

                    FontWeight =
                        FontWeights.SemiBold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                29,
                                48,
                                67))
                };


            TextBlock scoreText =
                new TextBlock
                {
                    Text =
                        $"{score} / {maximumScore}",

                    FontSize =
                        14,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                8,
                                119,
                                190))
                };


            content.Children.Add(
                categoryText);


            content.Children.Add(
                scoreText);


            chip.Child =
                content;


            panel.Children.Add(
                chip);
        }


        // =========================================================
        // DISPLAY NAME
        // =========================================================

        private static string GetDisplayName(
            Pupil pupil)
        {
            string name =
                $"{pupil.FirstName} " +
                $"{pupil.LastName}";


            name =
                name.Trim();


            return string.IsNullOrWhiteSpace(
                    name)
                ? pupil.FullName
                : name;
        }


        // =========================================================
        // FORMAT DURATION
        // =========================================================

        private static string FormatDuration(
            int totalSeconds)
        {
            if (totalSeconds <= 0)
            {
                return "";
            }


            TimeSpan duration =
                TimeSpan.FromSeconds(
                    totalSeconds);


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


        // =========================================================
        // FORMAT ASSESSED DATE
        // =========================================================

        private static string FormatCompletedDate(
            string completedAt)
        {
            if (string.IsNullOrWhiteSpace(
                    completedAt))
            {
                return "";
            }


            if (DateTime.TryParse(
                    completedAt,
                    out DateTime date))
            {
                return
                    date.ToString(
                        "yyyy-MM-dd");
            }


            return completedAt;
        }


        // =========================================================
        // BACK
        // =========================================================

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }


        // =========================================================
        // PRINT
        // =========================================================

        private void PrintButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                CheckInScorecardPrintService printService =
                    new CheckInScorecardPrintService();


                printService.PrintPupilScorecard(
                    _pupil);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Hindi ma-print ang scorecard.\n\n" +
                    ex.Message,
                    "Print Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}