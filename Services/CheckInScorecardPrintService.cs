using ARALReadingSystem.Models;
using ARALReadingSystem.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace ARALReadingSystem.Services
{
    public class CheckInScorecardPrintService
    {
        private readonly ProgressCheckInService _checkInService =
            new ProgressCheckInService();


        private const double A4Width =
            793.7;

        private const double A4Height =
            1122.5;


        // =========================================================
        // OPEN PREVIEW
        // =========================================================

        public void PrintPupilScorecard(
            Pupil pupil)
        {
            if (pupil == null)
            {
                throw new ArgumentNullException(
                    nameof(pupil));
            }


            CheckInPrintPreviewWindow preview =
                new CheckInPrintPreviewWindow(
                    pupil);


            Window? owner =
                Application.Current?.MainWindow;


            if (owner != null &&
                owner != preview)
            {
                preview.Owner =
                    owner;
            }


            preview.ShowDialog();
        }


        // =========================================================
        // ACTUAL PRINT
        // =========================================================

        public void PrintDocument(
            Pupil pupil)
        {
            if (pupil == null)
            {
                throw new ArgumentNullException(
                    nameof(pupil));
            }


            PrintDialog printDialog =
                new PrintDialog();


            bool? accepted =
                printDialog.ShowDialog();


            if (accepted != true)
            {
                return;
            }


            FlowDocument document =
                BuildPrintDocument(
                    pupil);


            document.PageWidth =
                A4Width;

            document.PageHeight =
                A4Height;

            document.PagePadding =
                new Thickness(
                    50,
                    38,
                    50,
                    38);

            document.ColumnWidth =
                double.PositiveInfinity;


            IDocumentPaginatorSource source =
                document;


            DocumentPaginator paginator =
                source.DocumentPaginator;


            paginator.PageSize =
                new Size(
                    A4Width,
                    A4Height);


            printDialog.PrintDocument(
                paginator,
                $"Scorecard - {GetDisplayName(pupil)}");
        }


        // =========================================================
        // BUILD PRINT DOCUMENT
        // =========================================================

        private FlowDocument BuildPrintDocument(
            Pupil pupil)
        {
            FlowDocument document =
                new FlowDocument
                {
                    FontFamily =
                        new FontFamily(
                            "Segoe UI"),

                    FontSize =
                        10,

                    Foreground =
                        Brush(
                            29,
                            48,
                            67)
                };


            AddHeader(
                document,
                pupil);


            List<ProgressCheckIn> results =
                _checkInService
                    .GetCheckInsForPupil(
                        pupil.Id)
                    .ToList();


            for (int number = 1;
                 number <= 4;
                 number++)
            {
                ProgressCheckIn? checkIn =
                    results.FirstOrDefault(
                        item =>
                            item.CheckInNumber ==
                            number);


                if (checkIn != null &&
                    checkIn.Completed)
                {
                    AddCompletedCheckIn(
                        document,
                        checkIn);
                }
                else
                {
                    AddEmptyCheckIn(
                        document,
                        number);
                }
            }


            AddFooter(
                document);


            return document;
        }


        // =========================================================
        // HEADER
        // =========================================================

        private void AddHeader(
            FlowDocument document,
            Pupil pupil)
        {
            // -----------------------------------------------------
            // PRINT META
            // -----------------------------------------------------

            Table topMeta =
                new Table
                {
                    CellSpacing = 0,

                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            12)
                };


            topMeta.Columns.Add(
                new TableColumn());

            topMeta.Columns.Add(
                new TableColumn());

            topMeta.Columns.Add(
                new TableColumn());


            TableRowGroup metaGroup =
                new TableRowGroup();


            TableRow metaRow =
                new TableRow();


            metaRow.Cells.Add(
                CreateTextCell(
                    DateTime.Now.ToString(
                        "MM/dd/yy, h:mm tt"),
                    TextAlignment.Left,
                    7.5,
                    FontWeights.Normal));


            metaRow.Cells.Add(
                CreateTextCell(
                    "Internal Preview",
                    TextAlignment.Center,
                    7.5,
                    FontWeights.SemiBold));


            metaRow.Cells.Add(
                CreateTextCell(
                    "",
                    TextAlignment.Right,
                    7.5,
                    FontWeights.Normal));


            metaGroup.Rows.Add(
                metaRow);


            topMeta.RowGroups.Add(
                metaGroup);


            document.Blocks.Add(
                topMeta);


            // -----------------------------------------------------
            // PUPIL / TEACHER HEADER
            // -----------------------------------------------------

            Table header =
                new Table
                {
                    CellSpacing = 0,

                    Margin =
                        new Thickness(
                            18,
                            0,
                            18,
                            10)
                };


            header.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            2,
                            GridUnitType.Star)
                });


            header.Columns.Add(
                new TableColumn
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });


            TableRowGroup headerGroup =
                new TableRowGroup();


            TableRow row =
                new TableRow();


            // -----------------------------------------------------
            // PUPIL
            // -----------------------------------------------------

            TableCell pupilCell =
                new TableCell();


            Paragraph title =
                new Paragraph
                {
                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            4),

                    FontSize =
                        19,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            29,
                            48,
                            67)
                };


            title.Inlines.Add(
                $"Scorecard: {GetDisplayName(pupil)}");


            pupilCell.Blocks.Add(
                title);


            string gradeLevel =
                string.IsNullOrWhiteSpace(
                    pupil.GradeLevel)
                    ? "—"
                    : pupil.GradeLevel;


            string gender =
                string.IsNullOrWhiteSpace(
                    pupil.Gender)
                    ? "—"
                    : pupil.Gender;


            string birthdate =
                FormatPupilBirthdate(
                    pupil.Birthdate);


            Paragraph pupilInfo =
                new Paragraph
                {
                    Margin =
                        new Thickness(0),

                    FontSize =
                        8.5,

                    Foreground =
                        Brush(
                            97,
                            119,
                            138)
                };


            pupilInfo.Inlines.Add(
                $"{gradeLevel} ARAL cohort · " +
                $"Kasarian: {gender}");


            if (!string.IsNullOrWhiteSpace(
                    birthdate))
            {
                pupilInfo.Inlines.Add(
                    $" · Kapanganakan: {birthdate}");
            }


            pupilCell.Blocks.Add(
                pupilInfo);


            // -----------------------------------------------------
            // TEACHER LABEL
            // -----------------------------------------------------

            TableCell teacherCell =
                new TableCell();


            Paragraph teacher =
                new Paragraph
                {
                    Margin =
                        new Thickness(
                            0,
                            1,
                            0,
                            3),

                    TextAlignment =
                        TextAlignment.Right,

                    FontSize =
                        9,

                    FontWeight =
                        FontWeights.SemiBold
                };


            teacher.Inlines.Add(
                "ARAL Teacher");


            teacherCell.Blocks.Add(
                teacher);


            Paragraph role =
                new Paragraph
                {
                    Margin =
                        new Thickness(0),

                    TextAlignment =
                        TextAlignment.Right,

                    FontSize =
                        7.5,

                    Foreground =
                        Brush(
                            135,
                            151,
                            166)
                };


            role.Inlines.Add(
                "ARAL Remedial — Grade 1 & 2");


            teacherCell.Blocks.Add(
                role);


            row.Cells.Add(
                pupilCell);

            row.Cells.Add(
                teacherCell);


            headerGroup.Rows.Add(
                row);


            header.RowGroups.Add(
                headerGroup);


            document.Blocks.Add(
                header);


            // -----------------------------------------------------
            // NAVIGATION LABELS
            // -----------------------------------------------------

            Paragraph nav =
                new Paragraph
                {
                    Margin =
                        new Thickness(
                            18,
                            0,
                            18,
                            12),

                    FontSize =
                        7.5,

                    Foreground =
                        Brush(
                            49,
                            71,
                            90)
                };


            nav.Inlines.Add(
                "▦  Talaan ng Klase     ");

            nav.Inlines.Add(
                "♙  Mga Mag-aaral     ");

            nav.Inlines.Add(
                "▣  Mga Sesyon     ");

            nav.Inlines.Add(
                "▣  Progress Check-in");


            document.Blocks.Add(
                nav);


            Border divider =
                new Border
                {
                    Height =
                        1,

                    Background =
                        Brush(
                            229,
                            220,
                            209),

                    Margin =
                        new Thickness(
                            0,
                            0,
                            0,
                            16)
                };


            document.Blocks.Add(
                new BlockUIContainer(
                    divider));
        }


        // =========================================================
        // COMPLETED CHECK-IN
        // =========================================================

        private void AddCompletedCheckIn(
            FlowDocument document,
            ProgressCheckIn checkIn)
        {
            Border card =
                CreateCard();


            StackPanel root =
                new StackPanel();


            card.Child =
                root;


            // -----------------------------------------------------
            // CHECK-IN HEADER
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


            StackPanel left =
                new StackPanel();


            left.Children.Add(
                new TextBlock
                {
                    Text =
                        $"Check-in #{checkIn.CheckInNumber}",

                    FontSize =
                        14,

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


            left.Children.Add(
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
                        7.5,

                    Foreground =
                        Brush(
                            97,
                            119,
                            138)
                });


            header.Children.Add(
                left);


            StackPanel right =
                new StackPanel
                {
                    HorizontalAlignment =
                        HorizontalAlignment.Right
                };


            right.Children.Add(
                new TextBlock
                {
                    Text =
                        $"{checkIn.TotalScore} / " +
                        $"{ProgressCheckIn.TotalItems} tama",

                    FontSize =
                        13,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            0,
                            106,
                            179),

                    HorizontalAlignment =
                        HorizontalAlignment.Right
                });


            right.Children.Add(
                new TextBlock
                {
                    Text =
                        BuildDurationDateText(
                            checkIn),

                    Margin =
                        new Thickness(
                            0,
                            3,
                            0,
                            0),

                    FontSize =
                        7.5,

                    Foreground =
                        Brush(
                            63,
                            79,
                            92),

                    HorizontalAlignment =
                        HorizontalAlignment.Right
                });


            Grid.SetColumn(
                right,
                1);


            header.Children.Add(
                right);


            root.Children.Add(
                header);


            // -----------------------------------------------------
            // FIGURE 18 CATEGORY SCORES
            // -----------------------------------------------------
            //
            // Individual historical answers are intentionally
            // NOT printed here.
            //
            // CHECKIN_RECORD permanently stores only:
            //
            // Score_Titik
            // Score_Pantig
            // Score_Salita
            // Score_Parirala
            //
            // -----------------------------------------------------

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
            // REMARK
            // -----------------------------------------------------

            StackPanel notes =
                new StackPanel
                {
                    Margin =
                        new Thickness(
                            10,
                            12,
                            10,
                            0)
                };


            notes.Children.Add(
                new TextBlock
                {
                    Text =
                        "PUNA NG GURO",

                    FontSize =
                        6.5,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            110,
                            116,
                            121)
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
                            4,
                            0,
                            0),

                    FontSize =
                        8,

                    TextWrapping =
                        TextWrapping.Wrap
                });


            root.Children.Add(
                notes);


            document.Blocks.Add(
                new BlockUIContainer(
                    card)
                {
                    Margin =
                        new Thickness(
                            18,
                            0,
                            18,
                            13)
                });
        }


        // =========================================================
        // EMPTY CHECK-IN
        // =========================================================

        private void AddEmptyCheckIn(
            FlowDocument document,
            int number)
        {
            Border card =
                CreateCard();


            card.Padding =
                new Thickness(
                    18,
                    15,
                    18,
                    15);


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
                        $"Check-in #{number}",

                    FontSize =
                        13,

                    FontWeight =
                        FontWeights.Bold
                });


            int requiredSession =
                _checkInService.GetRequiredSession(
                    number);


            left.Children.Add(
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
                        7.5,

                    Foreground =
                        Brush(
                            97,
                            119,
                            138)
                });


            grid.Children.Add(
                left);


            TextBlock empty =
                new TextBlock
                {
                    Text =
                        "Wala pang talaan",

                    FontSize =
                        7.5,

                    FontWeight =
                        FontWeights.SemiBold,

                    Foreground =
                        Brush(
                            190,
                            120,
                            0),

                    VerticalAlignment =
                        VerticalAlignment.Center
                };


            Grid.SetColumn(
                empty,
                1);


            grid.Children.Add(
                empty);


            card.Child =
                grid;


            document.Blocks.Add(
                new BlockUIContainer(
                    card)
                {
                    Margin =
                        new Thickness(
                            18,
                            0,
                            18,
                            12)
                });
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
                            10,
                            10,
                            10,
                            0),

                    Padding =
                        new Thickness(
                            11,
                            8,
                            11,
                            8),

                    CornerRadius =
                        new CornerRadius(8),

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
                        new Thickness(1)
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


            TextBlock categoryText =
                new TextBlock
                {
                    Text =
                        title,

                    FontSize =
                        8.5,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        Brush(
                            29,
                            48,
                            67)
                };


            row.Children.Add(
                categoryText);


            TextBlock scoreText =
                new TextBlock
                {
                    Text =
                        $"{score} / {maximumScore}",

                    FontSize =
                        8.5,

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
        // CARD
        // =========================================================

        private static Border CreateCard()
        {
            return new Border
            {
                Padding =
                    new Thickness(
                        18,
                        15,
                        18,
                        15),

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
                    new CornerRadius(16)
            };
        }


        // =========================================================
        // FOOTER
        // =========================================================

        private void AddFooter(
            FlowDocument document)
        {
            Paragraph footer =
                new Paragraph
                {
                    Margin =
                        new Thickness(
                            0,
                            10,
                            0,
                            0),

                    FontSize =
                        6.5,

                    Foreground =
                        Brush(
                            92,
                            104,
                            113)
                };


            footer.Inlines.Add(
                "Ninorte-Samarnon Reading Intervention");


            document.Blocks.Add(
                footer);
        }


        // =========================================================
        // TABLE CELL
        // =========================================================

        private static TableCell CreateTextCell(
            string text,
            TextAlignment alignment,
            double size,
            FontWeight weight)
        {
            Paragraph paragraph =
                new Paragraph
                {
                    Margin =
                        new Thickness(0),

                    TextAlignment =
                        alignment,

                    FontSize =
                        size,

                    FontWeight =
                        weight
                };


            paragraph.Inlines.Add(
                text);


            return new TableCell(
                paragraph);
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
            int seconds)
        {
            if (seconds <= 0)
            {
                return "";
            }


            TimeSpan duration =
                TimeSpan.FromSeconds(
                    seconds);


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