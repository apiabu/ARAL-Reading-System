using ARALReadingSystem.Models;
using ARALReadingSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ARALReadingSystem.Views
{
    public partial class CheckInPrintPreviewWindow : Window
    {
        // =========================================================
        // DATA / SERVICES
        // =========================================================

        private readonly Pupil _pupil;

        private readonly ProgressCheckInService _checkInService =
            new ProgressCheckInService();

        private List<ProgressCheckIn> _results =
            new List<ProgressCheckIn>();


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public CheckInPrintPreviewWindow(
            Pupil pupil)
        {
            InitializeComponent();

            _pupil =
                pupil ??
                throw new ArgumentNullException(
                    nameof(pupil));

            Loaded +=
                CheckInPrintPreviewWindow_Loaded;
        }


        // =========================================================
        // LOAD
        // =========================================================

        private void CheckInPrintPreviewWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -=
                CheckInPrintPreviewWindow_Loaded;

            LoadPreview();
        }


        // =========================================================
        // LOAD PREVIEW
        // =========================================================

        private void LoadPreview()
        {
            PreviewDateText.Text =
                DateTime.Now.ToString(
                    "MM/dd/yy, h:mm tt");


            PupilNameText.Text =
                $"Scorecard: {GetDisplayName(_pupil)}";


            string gradeLevel =
                string.IsNullOrWhiteSpace(
                    _pupil.GradeLevel)
                    ? "—"
                    : _pupil.GradeLevel;


            string gender =
                string.IsNullOrWhiteSpace(
                    _pupil.Gender)
                    ? "—"
                    : _pupil.Gender;


            string birthdate =
                FormatPupilBirthdate(
                    _pupil.Birthdate);


            PupilInfoText.Text =
                $"{gradeLevel} ARAL cohort · " +
                $"Kasarian: {gender}";


            if (!string.IsNullOrWhiteSpace(
                    birthdate))
            {
                PupilInfoText.Text +=
                    $" · Kapanganakan: {birthdate}";
            }


            /*
             * Figure 18 stores Teacher_ID.
             *
             * The current screen does not yet contain the
             * logged-in Teacher object, so we do not invent
             * a teacher's first/last name here.
             */

            TeacherNameText.Text =
                "ARAL Teacher";


            TeacherRoleText.Text =
                "ARAL Remedial — Grade 1 & 2";


            _results =
                _checkInService
                    .GetCheckInsForPupil(
                        _pupil.Id)
                    .ToList();


            BuildReport();
        }


        // =========================================================
        // REPORT
        // =========================================================

        private void BuildReport()
        {
            ReportPanel.Children.Clear();


            for (int number = 1;
                 number <= 4;
                 number++)
            {
                ProgressCheckIn? result =
                    _results.FirstOrDefault(
                        item =>
                            item.CheckInNumber ==
                            number);


                if (result != null &&
                    result.Completed)
                {
                    ReportPanel.Children.Add(
                        CreateCompletedCheckInCard(
                            result));
                }
                else
                {
                    ReportPanel.Children.Add(
                        CreateEmptyCheckInCard(
                            number));
                }
            }


            /*
             * Current preview uses one visual A4 page.
             * The actual FlowDocument can paginate
             * automatically during printing.
             */

            PageNumberText.Text =
                "1/1";


            SheetCountText.Text =
                "1 sheet of paper";
        }


        // =========================================================
        // COMPLETED CHECK-IN CARD
        // =========================================================

        private Border CreateCompletedCheckInCard(
            ProgressCheckIn checkIn)
        {
            Border card =
                CreateBaseCard();


            StackPanel root =
                new StackPanel();


            card.Child =
                root;


            // -----------------------------------------------------
            // HEADER
            // -----------------------------------------------------

            Grid header =
                new Grid();


            header.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });


            header.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        GridLength.Auto
                });


            // -----------------------------------------------------
            // LEFT HEADER
            // -----------------------------------------------------

            StackPanel leftHeader =
                new StackPanel();


            leftHeader.Children.Add(
                new TextBlock
                {
                    Text =
                        $"Check-in #{checkIn.CheckInNumber}",

                    FontSize =
                        16,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            29,
                            48,
                            67)
                });


            int requiredSession =
                _checkInService.GetRequiredSession(
                    checkIn.CheckInNumber);


            leftHeader.Children.Add(
                new TextBlock
                {
                    Text =
                        $"Matapos ang Sesyon {requiredSession}",

                    Margin =
                        new Thickness(
                            0,
                            3,
                            0,
                            0),

                    FontSize =
                        9,

                    Foreground =
                        Brush(
                            97,
                            119,
                            138)
                });


            Grid.SetColumn(
                leftHeader,
                0);


            header.Children.Add(
                leftHeader);


            // -----------------------------------------------------
            // RIGHT HEADER
            // -----------------------------------------------------

            StackPanel rightHeader =
                new StackPanel
                {
                    HorizontalAlignment =
                        HorizontalAlignment.Right
                };


            StackPanel scoreLine =
                new StackPanel
                {
                    Orientation =
                        Orientation.Horizontal,

                    HorizontalAlignment =
                        HorizontalAlignment.Right
                };


            scoreLine.Children.Add(
                new TextBlock
                {
                    Text =
                        checkIn.TotalScore.ToString(),

                    FontSize =
                        18,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            0,
                            106,
                            179)
                });


            scoreLine.Children.Add(
                new TextBlock
                {
                    Text =
                        $" /{ProgressCheckIn.TotalItems} tama",

                    Margin =
                        new Thickness(
                            3,
                            4,
                            0,
                            0),

                    FontSize =
                        9,

                    Foreground =
                        Brush(
                            91,
                            110,
                            126)
                });


            rightHeader.Children.Add(
                scoreLine);


            string details =
                BuildDurationDateText(
                    checkIn);


            rightHeader.Children.Add(
                new TextBlock
                {
                    Text =
                        details,

                    Margin =
                        new Thickness(
                            0,
                            4,
                            0,
                            0),

                    FontSize =
                        8.5,

                    Foreground =
                        Brush(
                            63,
                            79,
                            92),

                    HorizontalAlignment =
                        HorizontalAlignment.Right
                });


            Grid.SetColumn(
                rightHeader,
                1);


            header.Children.Add(
                rightHeader);


            root.Children.Add(
                header);


            // =====================================================
            // FIGURE 18 CATEGORY SCORES
            // =====================================================
            //
            // CHECKIN_RECORD permanently stores:
            //
            // Score_Titik
            // Score_Pantig
            // Score_Salita
            // Score_Parirala
            //
            // We therefore display the stored category totals
            // instead of reconstructed individual ✓ / ✕ answers.
            // =====================================================


            root.Children.Add(
                CreateCategoryScore(
                    "Titik",
                    checkIn.TitikScore,
                    6));


            root.Children.Add(
                CreateCategoryScore(
                    "Pantig",
                    checkIn.PantigScore,
                    4));


            root.Children.Add(
                CreateCategoryScore(
                    "Salita",
                    checkIn.SalitaScore,
                    4));


            root.Children.Add(
                CreateCategoryScore(
                    "Parirala",
                    checkIn.PariralaScore,
                    3));


            // -----------------------------------------------------
            // REMARK / PUNA NG GURO
            // -----------------------------------------------------

            StackPanel notes =
                new StackPanel
                {
                    Margin =
                        new Thickness(
                            12,
                            12,
                            12,
                            0)
                };


            notes.Children.Add(
                new TextBlock
                {
                    Text =
                        "PUNA NG GURO",

                    FontSize =
                        7.5,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            105,
                            112,
                            118)
                });


            notes.Children.Add(
                new TextBlock
                {
                    Text =
                        string.IsNullOrWhiteSpace(
                            checkIn.Remark)
                            ? "Walang puna ng guro."
                            : checkIn.Remark,

                    Margin =
                        new Thickness(
                            0,
                            5,
                            0,
                            0),

                    FontSize =
                        9,

                    TextWrapping =
                        TextWrapping.Wrap,

                    Foreground =
                        Brush(
                            42,
                            54,
                            63)
                });


            root.Children.Add(
                notes);


            return card;
        }


        // =========================================================
        // CATEGORY SCORE
        // =========================================================

        private FrameworkElement CreateCategoryScore(
            string title,
            int score,
            int maximumScore)
        {
            Border section =
                new Border
                {
                    Margin =
                        new Thickness(
                            12,
                            12,
                            12,
                            0),

                    Padding =
                        new Thickness(
                            12,
                            9,
                            12,
                            9),

                    Background =
                        Brush(
                            248,
                            251,
                            253),

                    BorderBrush =
                        Brush(
                            226,
                            234,
                            239),

                    BorderThickness =
                        new Thickness(1),

                    CornerRadius =
                        new CornerRadius(9)
                };


            Grid row =
                new Grid();


            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });


            row.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        GridLength.Auto
                });


            TextBlock titleText =
                new TextBlock
                {
                    Text =
                        title,

                    FontSize =
                        10,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            29,
                            48,
                            67)
                };


            row.Children.Add(
                titleText);


            TextBlock scoreText =
                new TextBlock
                {
                    Text =
                        $"{score} / {maximumScore}",

                    FontSize =
                        10,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            0,
                            106,
                            179)
                };


            Grid.SetColumn(
                scoreText,
                1);


            row.Children.Add(
                scoreText);


            section.Child =
                row;


            return section;
        }


        // =========================================================
        // EMPTY CHECK-IN CARD
        // =========================================================

        private Border CreateEmptyCheckInCard(
            int checkInNumber)
        {
            Border card =
                CreateBaseCard();


            card.Padding =
                new Thickness(
                    20,
                    18,
                    20,
                    18);


            Grid grid =
                new Grid();


            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });


            grid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        GridLength.Auto
                });


            StackPanel left =
                new StackPanel();


            left.Children.Add(
                new TextBlock
                {
                    Text =
                        $"Check-in #{checkInNumber}",

                    FontSize =
                        15,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            29,
                            48,
                            67)
                });


            int requiredSession =
                _checkInService.GetRequiredSession(
                    checkInNumber);


            left.Children.Add(
                new TextBlock
                {
                    Text =
                        $"Matapos ang Sesyon {requiredSession}",

                    Margin =
                        new Thickness(
                            0,
                            4,
                            0,
                            0),

                    FontSize =
                        8.5,

                    Foreground =
                        Brush(
                            97,
                            119,
                            138)
                });


            grid.Children.Add(
                left);


            TextBlock status =
                new TextBlock
                {
                    Text =
                        "Wala pang talaan",

                    FontSize =
                        8.5,

                    FontWeight =
                        FontWeights.SemiBold,

                    Foreground =
                        Brush(
                            188,
                            120,
                            0),

                    VerticalAlignment =
                        VerticalAlignment.Center
                };


            Grid.SetColumn(
                status,
                1);


            grid.Children.Add(
                status);


            card.Child =
                grid;


            return card;
        }


        // =========================================================
        // BASE CARD
        // =========================================================

        private static Border CreateBaseCard()
        {
            return new Border
            {
                Margin =
                    new Thickness(
                        0,
                        0,
                        0,
                        18),

                Padding =
                    new Thickness(
                        20,
                        18,
                        20,
                        18),

                Background =
                    Brushes.White,

                BorderBrush =
                    Brush(
                        226,
                        217,
                        205),

                BorderThickness =
                    new Thickness(1),

                CornerRadius =
                    new CornerRadius(18)
            };
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


                printService.PrintDocument(
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


        // =========================================================
        // CANCEL
        // =========================================================

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            Close();
        }


        // =========================================================
        // DURATION / DATE
        // =========================================================

        private static string BuildDurationDateText(
            ProgressCheckIn checkIn)
        {
            string duration =
                FormatDuration(
                    checkIn.TimeTaken);


            string date =
                FormatDate(
                    checkIn.DateAssessed);


            if (!string.IsNullOrWhiteSpace(
                    duration) &&
                !string.IsNullOrWhiteSpace(
                    date))
            {
                return
                    $"Oras: {duration} · {date}";
            }


            if (!string.IsNullOrWhiteSpace(
                    duration))
            {
                return
                    $"Oras: {duration}";
            }


            return date;
        }


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


        private static string FormatDate(
            string assessedAt)
        {
            if (string.IsNullOrWhiteSpace(
                    assessedAt))
            {
                return "";
            }


            if (DateTime.TryParse(
                    assessedAt,
                    out DateTime date))
            {
                return
                    date.ToString(
                        "yyyy-MM-dd");
            }


            return assessedAt;
        }


        // =========================================================
        // PUPIL BIRTHDATE
        // =========================================================

        private static string FormatPupilBirthdate(
            DateTime? birthdate)
        {
            if (!birthdate.HasValue)
            {
                return "";
            }


            return birthdate.Value.ToString(
                "yyyy-MM-dd");
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
        // COLOR HELPER
        // =========================================================

        private static SolidColorBrush Brush(
            byte red,
            byte green,
            byte blue)
        {
            return new SolidColorBrush(
                Color.FromRgb(
                    red,
                    green,
                    blue));
        }
    }
}