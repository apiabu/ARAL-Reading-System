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
    public partial class ProgressCheckInPage : Page
    {
        private readonly PupilService _pupilService =
            new PupilService();

        private readonly ProgressCheckInService _checkInService =
            new ProgressCheckInService();

        private List<Pupil> _pupils =
            new List<Pupil>();

        private int _selectedCheckInNumber = 1;

        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public ProgressCheckInPage()
        {
            InitializeComponent();

            Loaded +=
                ProgressCheckInPage_Loaded;
        }

        // =========================================================
        // PAGE LOADED
        // =========================================================

        private void ProgressCheckInPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadPage();
        }

        // =========================================================
        // LOAD PAGE
        // =========================================================

        private void LoadPage()
        {
            try
            {
                _pupils =
                    _pupilService.GetAllPupils();

                EnsureSelectedCheckIn();

                UpdateCheckInSelectors();

                LoadPupilCards();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri ma-load an Progress Check-in.\n\n" +
                    ex.Message,
                    "Progress Check-in",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // SELECTED CHECK-IN
        // =========================================================

        private void EnsureSelectedCheckIn()
        {
            if (IsCheckInAvailableForAnyPupil(
                    _selectedCheckInNumber))
            {
                return;
            }

            for (int checkInNumber = 1;
                 checkInNumber <= 4;
                 checkInNumber++)
            {
                if (IsCheckInAvailableForAnyPupil(
                        checkInNumber))
                {
                    _selectedCheckInNumber =
                        checkInNumber;

                    return;
                }
            }

            // If nobody has reached a milestone yet,
            // display Check-in #1 as the first milestone.
            _selectedCheckInNumber = 1;
        }

        // =========================================================
        // CHECK-IN AVAILABILITY
        // =========================================================

        private bool IsCheckInAvailableForAnyPupil(
            int checkInNumber)
        {
            int milestone =
                _checkInService.GetRequiredSession(
                    checkInNumber);

            return _pupils.Any(
                pupil =>
                    pupil.LastCompletedSession >=
                    milestone);
        }

        private bool IsCheckInAvailableForPupil(
            Pupil pupil,
            int checkInNumber)
        {
            int milestone =
                _checkInService.GetRequiredSession(
                    checkInNumber);

            return
                pupil.LastCompletedSession >=
                milestone;
        }

        // =========================================================
        // UPDATE SELECTORS
        // =========================================================

        private void UpdateCheckInSelectors()
        {
            UpdateSelector(
                CheckIn1Button,
                CheckIn1IconBorder,
                CheckIn1Icon,
                1);

            UpdateSelector(
                CheckIn2Button,
                CheckIn2IconBorder,
                CheckIn2Icon,
                2);

            UpdateSelector(
                CheckIn3Button,
                CheckIn3IconBorder,
                CheckIn3Icon,
                3);

            UpdateSelector(
                CheckIn4Button,
                CheckIn4IconBorder,
                CheckIn4Icon,
                4);
        }

        private void UpdateSelector(
            Button button,
            Border iconBorder,
            TextBlock icon,
            int checkInNumber)
        {
            bool available =
                IsCheckInAvailableForAnyPupil(
                    checkInNumber);

            bool selected =
                checkInNumber ==
                _selectedCheckInNumber;

            button.IsEnabled =
                available;

            // -----------------------------------------------------
            // LOCKED
            // -----------------------------------------------------

            if (!available)
            {
                button.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            248,
                            250,
                            251));

                button.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            225,
                            232,
                            237));

                button.BorderThickness =
                    new Thickness(1);

                iconBorder.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            241,
                            244,
                            246));

                icon.Text =
                    "🔒";

                icon.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            126,
                            144,
                            157));

                return;
            }

            // -----------------------------------------------------
            // AVAILABLE
            // -----------------------------------------------------

            icon.Text =
                "✓";

            iconBorder.Background =
                new SolidColorBrush(
                    Color.FromRgb(
                        233,
                        248,
                        240));

            icon.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        30,
                        173,
                        111));

            // -----------------------------------------------------
            // SELECTED
            // -----------------------------------------------------

            if (selected)
            {
                button.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            239,
                            248,
                            254));

                button.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            8,
                            119,
                            190));

                button.BorderThickness =
                    new Thickness(2);
            }
            else
            {
                button.Background =
                    Brushes.White;

                button.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            220,
                            230,
                            237));

                button.BorderThickness =
                    new Thickness(1);
            }
        }

        // =========================================================
        // SELECTOR CLICK
        // =========================================================

        private void CheckInSelector_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (!button.IsEnabled)
                return;

            if (!int.TryParse(
                    button.Tag?.ToString(),
                    out int checkInNumber))
            {
                return;
            }

            if (checkInNumber < 1 ||
                checkInNumber > 4)
            {
                return;
            }

            _selectedCheckInNumber =
                checkInNumber;

            UpdateCheckInSelectors();

            LoadPupilCards();
        }

        // =========================================================
        // LOAD PUPIL CARDS
        // =========================================================

        private void LoadPupilCards()
        {
            try
            {
                int milestone =
                    _checkInService.GetRequiredSession(
                        _selectedCheckInNumber);

                SelectedCheckInTitleText.Text =
                    $"Check-in #{_selectedCheckInNumber}";

                SelectedCheckInSubtitleText.Text =
                    $"Mga mag-aaral na nakatapos na ng " +
                    $"Sesyon {milestone}.";

                List<Pupil> eligiblePupils =
                    _pupils
                        .Where(
                            pupil =>
                                IsCheckInAvailableForPupil(
                                    pupil,
                                    _selectedCheckInNumber))
                        .OrderBy(
                            pupil =>
                                pupil.LastName)
                        .ThenBy(
                            pupil =>
                                pupil.FirstName)
                        .ToList();

                List<CheckInPupilCard> cards =
                    new List<CheckInPupilCard>();

                foreach (Pupil pupil in eligiblePupils)
                {
                    ProgressCheckIn? result =
                        _checkInService.GetCheckIn(
                            pupil.Id,
                            _selectedCheckInNumber);

                    CheckInPupilCard card =
                        CreateCard(
                            pupil,
                            result,
                            _selectedCheckInNumber);

                    cards.Add(
                        card);
                }

                PupilSummaryText.Text =
                    $"{cards.Count} puwede / " +
                    $"{_pupils.Count} rehistrado";

                // -------------------------------------------------
                // EMPTY STATE
                // -------------------------------------------------

                if (cards.Count == 0)
                {
                    PupilCardsControl.ItemsSource =
                        null;

                    PupilCardsControl.Visibility =
                        Visibility.Collapsed;

                    EmptyStateBorder.Visibility =
                        Visibility.Visible;

                    if (_pupils.Count == 0)
                    {
                        EmptyStateTitleText.Text =
                            "Wala pang mag-aaral";

                        EmptyStateMessageText.Text =
                            "Magdagdag muna ng mag-aaral upang " +
                            "magamit ang Progress Check-in.";
                    }
                    else
                    {
                        EmptyStateTitleText.Text =
                            $"Naka-lock pa ang Check-in " +
                            $"#{_selectedCheckInNumber}";

                        EmptyStateMessageText.Text =
                            $"Kailangang matapos muna ang " +
                            $"Sesyon {milestone} bago magtala " +
                            $"ng score.";
                    }

                    return;
                }

                // -------------------------------------------------
                // SHOW CARDS
                // -------------------------------------------------

                EmptyStateBorder.Visibility =
                    Visibility.Collapsed;

                PupilCardsControl.Visibility =
                    Visibility.Visible;

                PupilCardsControl.ItemsSource =
                    cards;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri ma-load an mga mag-aaral para " +
                    "han check-in.\n\n" +
                    ex.Message,
                    "Progress Check-in",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // CREATE CARD
        // =========================================================

        private static CheckInPupilCard CreateCard(
            Pupil pupil,
            ProgressCheckIn? result,
            int checkInNumber)
        {
            bool completed =
                result != null &&
                result.Completed;

            CheckInPupilCard card =
                new CheckInPupilCard
                {
                    PupilId =
                        pupil.Id,

                    CheckInNumber =
                        checkInNumber,

                    FullName =
                        pupil.FullName,

                    GradeLevel =
                        string.IsNullOrWhiteSpace(
                            pupil.GradeLevel)
                            ? "—"
                            : pupil.GradeLevel,

                    HasResult =
                        completed
                };

            if (completed &&
                result != null)
            {
                card.StatusText =
                    "Naitala";

                card.StatusBackground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            231,
                            247,
                            238));

                card.StatusForeground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            31,
                            139,
                            91));

                card.ScoreText =
                    $"{result.TotalScore} / 17 tama";

                card.DetailText =
                    BuildResultDetail(
                        result);

                card.ActionText =
                    "Baguhin";
            }
            else
            {
                card.StatusText =
                    "Wala pa";

                card.StatusBackground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            255,
                            244,
                            220));

                card.StatusForeground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            183,
                            116,
                            0));

                card.ScoreText =
                    "— / 17 tama";

                card.DetailText =
                    "Handa nang talaan ang scorecard.";

                card.ActionText =
                    "Magtala";
            }

            return card;
        }

        // =========================================================
        // RESULT DETAIL
        // =========================================================

        private static string BuildResultDetail(
            ProgressCheckIn result)
        {
            string dateText =
                "";

            if (DateTime.TryParse(
                    result.CompletedAt,
                    out DateTime completedDate))
            {
                dateText =
                    completedDate.ToString(
                        "yyyy-MM-dd");
            }
            else if (!string.IsNullOrWhiteSpace(
                         result.CompletedAt))
            {
                dateText =
                    result.CompletedAt;
            }

            string durationText =
                FormatDuration(
                    result.DurationSeconds);

            if (!string.IsNullOrWhiteSpace(
                    durationText) &&
                !string.IsNullOrWhiteSpace(
                    dateText))
            {
                return
                    $"Oras: {durationText} · {dateText}";
            }

            if (!string.IsNullOrWhiteSpace(
                    dateText))
            {
                return dateText;
            }

            return
                "Naitala na ang scorecard.";
        }

        // =========================================================
        // FORMAT DURATION
        // =========================================================

        private static string FormatDuration(
            int seconds)
        {
            if (seconds <= 0)
                return "";

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

        // =========================================================
        // MAGTALA / BAGUHIN
        // =========================================================

      private void RecordScoreButton_Click(
    object sender,
    RoutedEventArgs e)
{
    if (sender is not Button button)
        return;

    if (button.Tag is not CheckInPupilCard card)
        return;

    try
    {
        Pupil? pupil =
            _pupilService.GetPupilById(
                card.PupilId);

        if (pupil == null)
        {
            MessageBox.Show(
                "Hindi makita ang pupil record.",
                "Progress Check-in",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }

        if (!IsCheckInAvailableForPupil(
                pupil,
                card.CheckInNumber))
        {
            int milestone =
                _checkInService.GetRequiredSession(
                    card.CheckInNumber);

            MessageBox.Show(
                $"Kailangang matapos muna ni " +
                $"{pupil.FullName} ang Sesyon " +
                $"{milestone}.",
                "Progress Check-in",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }

        CheckInRecordPage recordPage =
            new CheckInRecordPage(
                pupil,
                card.CheckInNumber);

        recordPage.BackRequested += () =>
        {
            if (NavigationService != null &&
                NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        };

        recordPage.Saved += () =>
        {
            LoadPage();
        };

        NavigationService?.Navigate(
            recordPage);
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Diri maabli an Check-in Record.\n\n" +
            ex.Message,
            "Progress Check-in",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
        // =========================================================
        // SCORECARD
        // =========================================================

private void ScorecardButton_Click(
    object sender,
    RoutedEventArgs e)
{
    if (sender is not Button button)
    {
        return;
    }

    if (button.Tag is not CheckInPupilCard card)
    {
        return;
    }

    try
    {
        Pupil? pupil =
            _pupilService.GetPupilById(
                card.PupilId);


        if (pupil == null)
        {
            MessageBox.Show(
                "Hindi makita ang pupil record.",
                "Scorecard",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            return;
        }


        ProgressCheckIn? result =
            _checkInService.GetCheckIn(
                card.PupilId,
                card.CheckInNumber);


        if (result == null ||
            !result.Completed)
        {
            MessageBox.Show(
                "Wala pang naitalang resulta para " +
                "sa mag-aaral na ito.",
                "Scorecard",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            return;
        }


        CheckInScorecardPage scorecardPage =
            new CheckInScorecardPage(
                pupil,
                result);


        scorecardPage.BackRequested += () =>
        {
            if (NavigationService != null &&
                NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        };


        NavigationService?.Navigate(
            scorecardPage);
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Hindi maabli ang scorecard.\n\n" +
            ex.Message,
            "Scorecard",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
        // =========================================================
        // PUBLIC REFRESH
        // =========================================================

        public void RefreshPage()
        {
            LoadPage();
        }

        // =========================================================
        // CARD VIEW MODEL
        // =========================================================

        private sealed class CheckInPupilCard
        {
            public int PupilId { get; set; }

            public int CheckInNumber { get; set; }

            public string FullName { get; set; } =
                "";

            public string GradeLevel { get; set; } =
                "";

            public bool HasResult { get; set; }

            public string StatusText { get; set; } =
                "";

            public Brush StatusBackground { get; set; } =
                Brushes.Transparent;

            public Brush StatusForeground { get; set; } =
                Brushes.Black;

            public string ScoreText { get; set; } =
                "";

            public string DetailText { get; set; } =
                "";

            public string ActionText { get; set; } =
                "";
        }
    }
}