using ARALReadingSystem.Models;
using ARALReadingSystem.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ARALReadingSystem.Views
{
    public partial class CheckInEntryWindow : Window
    {
        private readonly Pupil _pupil;
        private readonly int _checkInNumber;

        private readonly ProgressCheckInService _checkInService =
            new ProgressCheckInService();

        private readonly Dictionary<string, int?> _scores =
            new Dictionary<string, int?>();

        private readonly Dictionary<string, Button> _zeroButtons =
            new Dictionary<string, Button>();

        private readonly Dictionary<string, Button> _oneButtons =
            new Dictionary<string, Button>();

        private readonly Stopwatch _stopwatch =
            new Stopwatch();

        private readonly DispatcherTimer _timer =
            new DispatcherTimer();

        private ProgressCheckIn? _existingResult;

        public CheckInEntryWindow(
            Pupil pupil,
            int checkInNumber)
        {
            InitializeComponent();

            _pupil =
                pupil ?? throw new ArgumentNullException(
                    nameof(pupil));

            _checkInNumber =
                checkInNumber;

            if (_checkInNumber < 1 ||
                _checkInNumber > 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(checkInNumber));
            }

            WindowTitleText.Text =
                $"Progress Check-in #{_checkInNumber}";

            PupilNameText.Text =
                $"{_pupil.FullName} · {_pupil.GradeLevel}";

            CreateAllItems();

            LoadExistingResult();

            _timer.Interval =
                TimeSpan.FromSeconds(1);

            _timer.Tick +=
                Timer_Tick;

            _stopwatch.Start();
            _timer.Start();

            Closed +=
                CheckInEntryWindow_Closed;

            UpdateSummary();
        }

        // =========================================================
        // CREATE ITEMS
        // =========================================================

        private void CreateAllItems()
        {
            CreateCategoryItems(
                TitikItemsPanel,
                "Titik",
                6);

            CreateCategoryItems(
                PantigItemsPanel,
                "Pantig",
                4);

            CreateCategoryItems(
                SalitaItemsPanel,
                "Salita",
                4);

            CreateCategoryItems(
                PariralaItemsPanel,
                "Parirala",
                3);
        }

        private void CreateCategoryItems(
            StackPanel panel,
            string category,
            int count)
        {
            for (int i = 1;
                 i <= count;
                 i++)
            {
                string key =
                    $"{category}{i}";

                _scores[key] =
                    null;

                Grid row =
                    new Grid
                    {
                        Margin =
                            new Thickness(
                                0,
                                0,
                                0,
                                i == count ? 0 : 9)
                    };

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

                TextBlock label =
                    new TextBlock
                    {
                        Text =
                            $"{category} {i}",

                        FontSize =
                            14,

                        Foreground =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    45,
                                    66,
                                    79)),

                        VerticalAlignment =
                            VerticalAlignment.Center
                    };

                row.Children.Add(label);

                StackPanel scorePanel =
                    new StackPanel
                    {
                        Orientation =
                            Orientation.Horizontal
                    };

                Grid.SetColumn(
                    scorePanel,
                    1);

                Button zeroButton =
                    CreateScoreButton(
                        key,
                        0);

                Button oneButton =
                    CreateScoreButton(
                        key,
                        1);

                _zeroButtons[key] =
                    zeroButton;

                _oneButtons[key] =
                    oneButton;

                scorePanel.Children.Add(
                    zeroButton);

                scorePanel.Children.Add(
                    oneButton);

                row.Children.Add(
                    scorePanel);

                panel.Children.Add(
                    row);
            }
        }

        private Button CreateScoreButton(
            string key,
            int score)
        {
            Button button =
                new Button
                {
                    Content =
                        score.ToString(),

                    Tag =
                        new ScoreButtonTag
                        {
                            Key =
                                key,

                            Score =
                                score
                        }
                };

            button.SetResourceReference(
                StyleProperty,
                "ScoreButtonStyle");

            button.Click +=
                ScoreButton_Click;

            return button;
        }

        // =========================================================
        // LOAD EXISTING RESULT
        // =========================================================

        private void LoadExistingResult()
        {
            _existingResult =
                _checkInService.GetCheckIn(
                    _pupil.Id,
                    _checkInNumber);

            if (_existingResult == null ||
                !_existingResult.Completed)
            {
                return;
            }

            SetExistingScore(
                "Titik1",
                _existingResult.Titik1);

            SetExistingScore(
                "Titik2",
                _existingResult.Titik2);

            SetExistingScore(
                "Titik3",
                _existingResult.Titik3);

            SetExistingScore(
                "Titik4",
                _existingResult.Titik4);

            SetExistingScore(
                "Titik5",
                _existingResult.Titik5);

            SetExistingScore(
                "Titik6",
                _existingResult.Titik6);

            SetExistingScore(
                "Pantig1",
                _existingResult.Pantig1);

            SetExistingScore(
                "Pantig2",
                _existingResult.Pantig2);

            SetExistingScore(
                "Pantig3",
                _existingResult.Pantig3);

            SetExistingScore(
                "Pantig4",
                _existingResult.Pantig4);

            SetExistingScore(
                "Salita1",
                _existingResult.Salita1);

            SetExistingScore(
                "Salita2",
                _existingResult.Salita2);

            SetExistingScore(
                "Salita3",
                _existingResult.Salita3);

            SetExistingScore(
                "Salita4",
                _existingResult.Salita4);

            SetExistingScore(
                "Parirala1",
                _existingResult.Parirala1);

            SetExistingScore(
                "Parirala2",
                _existingResult.Parirala2);

            SetExistingScore(
                "Parirala3",
                _existingResult.Parirala3);
        }

        private void SetExistingScore(
            string key,
            int score)
        {
            if (score != 0 &&
                score != 1)
            {
                return;
            }

            _scores[key] =
                score;

            UpdateScoreButtonAppearance(
                key);
        }

        // =========================================================
        // SCORE CLICK
        // =========================================================

        private void ScoreButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.Tag is not ScoreButtonTag tag)
                return;

            _scores[tag.Key] =
                tag.Score;

            UpdateScoreButtonAppearance(
                tag.Key);

            UpdateSummary();
        }

        private void UpdateScoreButtonAppearance(
            string key)
        {
            if (!_scores.TryGetValue(
                    key,
                    out int? selectedScore))
            {
                return;
            }

            if (!_zeroButtons.TryGetValue(
                    key,
                    out Button? zeroButton))
            {
                return;
            }

            if (!_oneButtons.TryGetValue(
                    key,
                    out Button? oneButton))
            {
                return;
            }

            ResetScoreButton(
                zeroButton);

            ResetScoreButton(
                oneButton);

            if (selectedScore == 0)
            {
                zeroButton.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            255,
                            239,
                            239));

                zeroButton.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            218,
                            87,
                            87));

                zeroButton.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            177,
                            49,
                            49));
            }
            else if (selectedScore == 1)
            {
                oneButton.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            231,
                            247,
                            238));

                oneButton.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            30,
                            173,
                            111));

                oneButton.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            25,
                            133,
                            82));
            }
        }

        private static void ResetScoreButton(
            Button button)
        {
            button.Background =
                Brushes.White;

            button.BorderBrush =
                new SolidColorBrush(
                    Color.FromRgb(
                        204,
                        217,
                        226));

            button.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        23,
                        50,
                        77));
        }

        // =========================================================
        // SUMMARY
        // =========================================================

        private void UpdateSummary()
        {
            int answered =
                _scores.Values.Count(
                    score =>
                        score.HasValue);

            int totalScore =
                _scores.Values
                    .Where(
                        score =>
                            score.HasValue)
                    .Sum(
                        score =>
                            score!.Value);

            AnsweredText.Text =
                $"{answered} / 17 aytem ang namarkahan";

            LiveScoreText.Text =
                $"{totalScore} / 17 tama";
        }

        // =========================================================
        // TIMER
        // =========================================================

        private void Timer_Tick(
            object? sender,
            EventArgs e)
        {
            UpdateElapsedTime();
        }

        private void UpdateElapsedTime()
        {
            int seconds =
                (int)_stopwatch.Elapsed.TotalSeconds;

            TimeSpan duration =
                TimeSpan.FromSeconds(
                    seconds);

            if (duration.TotalHours >= 1)
            {
                ElapsedTimeText.Text =
                    $"Oras: " +
                    $"{(int)duration.TotalHours}h " +
                    $"{duration.Minutes}m " +
                    $"{duration.Seconds}s";
            }
            else if (duration.Minutes > 0)
            {
                ElapsedTimeText.Text =
                    $"Oras: " +
                    $"{duration.Minutes}m " +
                    $"{duration.Seconds}s";
            }
            else
            {
                ElapsedTimeText.Text =
                    $"Oras: {duration.Seconds}s";
            }
        }

        // =========================================================
        // SAVE
        // =========================================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                int answered =
                    _scores.Values.Count(
                        score =>
                            score.HasValue);

                if (answered != 17)
                {
                    MessageBox.Show(
                        $"May {17 - answered} aytem pa na " +
                        "hindi namarkahan.\n\n" +
                        "Markahan ang bawat aytem bilang " +
                        "1 (tama) o 0 (mali) bago i-save.",
                        "Kumpletuhin ang Check-in",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    return;
                }

                _stopwatch.Stop();
                _timer.Stop();

                ProgressCheckIn result =
                    _existingResult ??
                    new ProgressCheckIn();

                result.PupilId =
                    _pupil.Id;

                result.CheckInNumber =
                    _checkInNumber;

                result.Titik1 =
                    GetScore("Titik1");

                result.Titik2 =
                    GetScore("Titik2");

                result.Titik3 =
                    GetScore("Titik3");

                result.Titik4 =
                    GetScore("Titik4");

                result.Titik5 =
                    GetScore("Titik5");

                result.Titik6 =
                    GetScore("Titik6");

                result.Pantig1 =
                    GetScore("Pantig1");

                result.Pantig2 =
                    GetScore("Pantig2");

                result.Pantig3 =
                    GetScore("Pantig3");

                result.Pantig4 =
                    GetScore("Pantig4");

                result.Salita1 =
                    GetScore("Salita1");

                result.Salita2 =
                    GetScore("Salita2");

                result.Salita3 =
                    GetScore("Salita3");

                result.Salita4 =
                    GetScore("Salita4");

                result.Parirala1 =
                    GetScore("Parirala1");

                result.Parirala2 =
                    GetScore("Parirala2");

                result.Parirala3 =
                    GetScore("Parirala3");

                result.DurationSeconds =
                    (int)_stopwatch.Elapsed.TotalSeconds;

                _checkInService.SaveCheckIn(
                    result);

                MessageBox.Show(
                    $"Naitala na ang Check-in " +
                    $"#{_checkInNumber} ni " +
                    $"{_pupil.FullName}.\n\n" +
                    $"Kabuuang score: " +
                    $"{result.TotalScore} / 17",
                    "Naitala ang Score",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                DialogResult =
                    true;

                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Hindi ma-save ang Progress Check-in.\n\n" +
                    ex.Message,
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                if (!_stopwatch.IsRunning)
                {
                    _stopwatch.Start();
                    _timer.Start();
                }
            }
        }

        private int GetScore(
            string key)
        {
            if (!_scores.TryGetValue(
                    key,
                    out int? score))
            {
                return 0;
            }

            return score ?? 0;
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

        private void CheckInEntryWindow_Closed(
            object? sender,
            EventArgs e)
        {
            _timer.Stop();
            _stopwatch.Stop();
        }

        // =========================================================
        // BUTTON TAG
        // =========================================================

        private sealed class ScoreButtonTag
        {
            public string Key { get; set; } =
                "";

            public int Score { get; set; }
        }
    }
}