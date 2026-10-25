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
    public partial class CheckInRecordPage : Page
    {
        // =========================================================
        // SERVICES / DATA
        // =========================================================

        private readonly Pupil _pupil;
        private readonly int _checkInNumber;

        private readonly ProgressCheckInService _checkInService =
            new ProgressCheckInService();

        private ProgressCheckIn? _existingResult;


        // =========================================================
        // ANSWER STATE
        // =========================================================

        private readonly Dictionary<string, int?> _answers =
            new Dictionary<string, int?>();

        private readonly Dictionary<string, Button> _correctButtons =
            new Dictionary<string, Button>();

        private readonly Dictionary<string, Button> _wrongButtons =
            new Dictionary<string, Button>();


        // =========================================================
        // EVENTS
        // =========================================================

        public event Action? BackRequested;

        public event Action? Saved;


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public CheckInRecordPage(
            Pupil pupil,
            int checkInNumber)
        {
            InitializeComponent();


            _pupil =
                pupil ??
                throw new ArgumentNullException(
                    nameof(pupil));


            if (checkInNumber < 1 ||
                checkInNumber > 4)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(checkInNumber),
                    "Check-in number must be from 1 to 4.");
            }


            _checkInNumber =
                checkInNumber;


            Loaded +=
                CheckInRecordPage_Loaded;
        }


        // =========================================================
        // PAGE LOAD
        // =========================================================

        private void CheckInRecordPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            Loaded -=
                CheckInRecordPage_Loaded;


            try
            {
                LoadHeader();

                CreateAssessmentItems();

                LoadExistingResult();

                UpdateSummary();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Hindi ma-load ang Check-in Record.\n\n" +
                    ex.Message,
                    "Check-in Record",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // HEADER
        // =========================================================

        private void LoadHeader()
        {
            PageTitleText.Text =
                $"Check-in #{_checkInNumber} — " +
                GetDisplayName(_pupil);


            int requiredSession =
                _checkInService.GetRequiredSession(
                    _checkInNumber);


            PageSubtitleText.Text =
                $"{_pupil.GradeLevel} ARAL cohort · " +
                $"Matapos ang Sesyon {requiredSession}";
        }


        private static string GetDisplayName(
            Pupil pupil)
        {
            string displayName =
                $"{pupil.FirstName} {pupil.LastName}"
                    .Trim();


            if (!string.IsNullOrWhiteSpace(
                    displayName))
            {
                return displayName;
            }


            return pupil.FullName;
        }


        // =========================================================
        // CREATE ASSESSMENT ITEMS
        // =========================================================

        private void CreateAssessmentItems()
        {
            _answers.Clear();

            _correctButtons.Clear();

            _wrongButtons.Clear();


            // =====================================================
            // CURRENT CHECK-IN ITEMS
            // =====================================================

            string[] titikItems =
            {
                "m",
                "s",
                "a",
                "M",
                "S",
                "A"
            };


            string[] pantigItems =
            {
                "ma",
                "sa",
                "am",
                "as"
            };


            string[] salitaItems =
            {
                "mama",
                "asa",
                "masa",
                "sama"
            };


            string[] pariralaItems =
            {
                "si mama",
                "sa amin",
                "ang masa"
            };


            CreateCategory(
                TitikItemsPanel,
                "Titik",
                titikItems);


            CreateCategory(
                PantigItemsPanel,
                "Pantig",
                pantigItems);


            CreateCategory(
                SalitaItemsPanel,
                "Salita",
                salitaItems);


            CreateCategory(
                PariralaItemsPanel,
                "Parirala",
                pariralaItems);
        }


        // =========================================================
        // CREATE CATEGORY
        // =========================================================

        private void CreateCategory(
            StackPanel panel,
            string category,
            string[] labels)
        {
            panel.Children.Clear();


            for (int index = 0;
                 index < labels.Length;
                 index++)
            {
                string key =
                    $"{category}{index + 1}";


                _answers[key] =
                    null;


                // -------------------------------------------------
                // ITEM CONTAINER
                // -------------------------------------------------

                Border itemBorder =
                    new Border
                    {
                        Background =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    255,
                                    247,
                                    233)),

                        CornerRadius =
                            new CornerRadius(16),

                        Padding =
                            new Thickness(
                                17,
                                10,
                                12,
                                10),

                        Margin =
                            new Thickness(
                                0,
                                0,
                                0,
                                index ==
                                labels.Length - 1
                                    ? 0
                                    : 8)
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


                // -------------------------------------------------
                // STIMULUS
                // -------------------------------------------------

                TextBlock stimulusText =
                    new TextBlock
                    {
                        Text =
                            labels[index],

                        VerticalAlignment =
                            VerticalAlignment.Center,

                        FontSize =
                            category == "Titik"
                                ? 21
                                : 17,

                        FontWeight =
                            FontWeights.SemiBold,

                        Foreground =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    29,
                                    48,
                                    67))
                    };


                Grid.SetColumn(
                    stimulusText,
                    0);


                row.Children.Add(
                    stimulusText);


                // -------------------------------------------------
                // 1 / 0 BUTTONS
                // -------------------------------------------------

                StackPanel choicePanel =
                    new StackPanel
                    {
                        Orientation =
                            Orientation.Horizontal,

                        VerticalAlignment =
                            VerticalAlignment.Center
                    };


                Grid.SetColumn(
                    choicePanel,
                    1);


                Button correctButton =
                    CreateChoiceButton(
                        key,
                        1,
                        "✓ 1");


                Button wrongButton =
                    CreateChoiceButton(
                        key,
                        0,
                        "✕ 0");


                _correctButtons[key] =
                    correctButton;

                _wrongButtons[key] =
                    wrongButton;


                choicePanel.Children.Add(
                    correctButton);

                choicePanel.Children.Add(
                    wrongButton);


                row.Children.Add(
                    choicePanel);


                itemBorder.Child =
                    row;


                panel.Children.Add(
                    itemBorder);
            }
        }


        // =========================================================
        // CREATE SCORE BUTTON
        // =========================================================

        private Button CreateChoiceButton(
            string key,
            int score,
            string content)
        {
            Button button =
                new Button
                {
                    Content =
                        content,

                    Tag =
                        new AnswerButtonTag
                        {
                            Key =
                                key,

                            Score =
                                score
                        }
                };


            button.SetResourceReference(
                FrameworkElement.StyleProperty,
                "ScoreChoiceButtonStyle");


            button.Click +=
                ScoreChoiceButton_Click;


            return button;
        }


        // =========================================================
        // SCORE BUTTON CLICK
        // =========================================================

        private void ScoreChoiceButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }


            if (button.Tag is not AnswerButtonTag tag)
            {
                return;
            }


            _answers[tag.Key] =
                tag.Score;


            UpdateChoiceAppearance(
                tag.Key);


            UpdateSummary();
        }


        // =========================================================
        // SCORE BUTTON APPEARANCE
        // =========================================================

        private void UpdateChoiceAppearance(
            string key)
        {
            if (!_answers.TryGetValue(
                    key,
                    out int? score))
            {
                return;
            }


            if (!_correctButtons.TryGetValue(
                    key,
                    out Button? correctButton))
            {
                return;
            }


            if (!_wrongButtons.TryGetValue(
                    key,
                    out Button? wrongButton))
            {
                return;
            }


            ResetChoiceButton(
                correctButton);

            ResetChoiceButton(
                wrongButton);


            // -----------------------------------------------------
            // CORRECT / 1
            // -----------------------------------------------------

            if (score == 1)
            {
                correctButton.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            231,
                            247,
                            238));


                correctButton.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            30,
                            173,
                            111));


                correctButton.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            25,
                            139,
                            87));
            }


            // -----------------------------------------------------
            // INCORRECT / 0
            // -----------------------------------------------------

            if (score == 0)
            {
                wrongButton.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            255,
                            237,
                            237));


                wrongButton.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            218,
                            87,
                            87));


                wrongButton.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            180,
                            52,
                            52));
            }
        }


        private static void ResetChoiceButton(
            Button button)
        {
            button.Background =
                Brushes.White;


            button.BorderBrush =
                new SolidColorBrush(
                    Color.FromRgb(
                        222,
                        217,
                        208));


            button.Foreground =
                new SolidColorBrush(
                    Color.FromRgb(
                        145,
                        162,
                        175));
        }


        // =========================================================
        // UPDATE SUMMARY
        // =========================================================

        private void UpdateSummary()
        {
            int answeredCount =
                _answers.Values.Count(
                    value =>
                        value.HasValue);


            // =====================================================
            // CURRENT ASSESSMENT
            // =====================================================
            //
            // If the teacher has started marking the 17 items,
            // display the totals from those current answers.
            // =====================================================

            if (answeredCount > 0)
            {
                int totalScore =
                    _answers.Values
                        .Where(
                            value =>
                                value.HasValue)
                        .Sum(
                            value =>
                                value ?? 0);


                TotalScoreText.Text =
                    totalScore.ToString();


                AnsweredCountText.Text =
                    $"{answeredCount} sa " +
                    $"{ProgressCheckIn.TotalItems} " +
                    "aytem ang namarkahan.";


                TitikScoreText.Text =
                    $"{GetCategoryScore("Titik")} / 6";

                PantigScoreText.Text =
                    $"{GetCategoryScore("Pantig")} / 4";

                SalitaScoreText.Text =
                    $"{GetCategoryScore("Salita")} / 4";

                PariralaScoreText.Text =
                    $"{GetCategoryScore("Parirala")} / 3";


                return;
            }


            // =====================================================
            // EXISTING FIGURE 18 RECORD
            // =====================================================
            //
            // Historical item-level answers are not stored.
            //
            // Show only the truthful saved aggregate totals.
            // =====================================================

            if (_existingResult != null &&
                _existingResult.Completed)
            {
                TotalScoreText.Text =
                    _existingResult.TotalScore.ToString();


                AnsweredCountText.Text =
                    "May naitalang resulta. " +
                    "Markahan muli ang lahat ng 17 aytem " +
                    "kung nais baguhin ang assessment.";


                TitikScoreText.Text =
                    $"{_existingResult.ScoreTitik} / 6";

                PantigScoreText.Text =
                    $"{_existingResult.ScorePantig} / 4";

                SalitaScoreText.Text =
                    $"{_existingResult.ScoreSalita} / 4";

                PariralaScoreText.Text =
                    $"{_existingResult.ScoreParirala} / 3";


                return;
            }


            // =====================================================
            // NEW / EMPTY ASSESSMENT
            // =====================================================

            TotalScoreText.Text =
                "0";


            AnsweredCountText.Text =
                $"0 sa {ProgressCheckIn.TotalItems} " +
                "aytem ang namarkahan.";


            TitikScoreText.Text =
                "0 / 6";

            PantigScoreText.Text =
                "0 / 4";

            SalitaScoreText.Text =
                "0 / 4";

            PariralaScoreText.Text =
                "0 / 3";
        }


        private int GetCategoryScore(
            string category)
        {
            return _answers
                .Where(
                    pair =>
                        pair.Key.StartsWith(
                            category,
                            StringComparison.Ordinal))
                .Where(
                    pair =>
                        pair.Value.HasValue)
                .Sum(
                    pair =>
                        pair.Value ?? 0);
        }


        // =========================================================
        // LOAD EXISTING RESULT
        // =========================================================
        //
        // Figure 18 stores only:
        //
        // Score_Titik
        // Score_Pantig
        // Score_Salita
        // Score_Parirala
        //
        // It does NOT store the exact historical answer for each
        // of the 17 individual items.
        //
        // Therefore:
        //
        // - We load the aggregate scores.
        // - We load time and remark.
        // - We leave all 17 item buttons UNSELECTED.
        //
        // If the teacher wants to replace the saved assessment,
        // all 17 items must be marked again.
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
                MinutesTextBox.Text =
                    "0";

                SecondsTextBox.Text =
                    "0";

                TeacherNotesTextBox.Text =
                    "";


                return;
            }


            // -----------------------------------------------------
            // DO NOT RECONSTRUCT INDIVIDUAL ANSWERS
            // -----------------------------------------------------

            ClearCurrentAnswers();


            // -----------------------------------------------------
            // DURATION
            // -----------------------------------------------------

            int minutes =
                _existingResult.DurationSeconds / 60;

            int seconds =
                _existingResult.DurationSeconds % 60;


            MinutesTextBox.Text =
                minutes.ToString();

            SecondsTextBox.Text =
                seconds.ToString();


            // -----------------------------------------------------
            // REMARK
            // -----------------------------------------------------

            TeacherNotesTextBox.Text =
                _existingResult.TeacherNotes ?? "";


            UpdateSummary();
        }


        // =========================================================
        // CLEAR CURRENT ITEM ANSWERS
        // =========================================================

        private void ClearCurrentAnswers()
        {
            foreach (string key in
                     _answers.Keys.ToList())
            {
                _answers[key] =
                    null;


                if (_correctButtons.TryGetValue(
                        key,
                        out Button? correctButton))
                {
                    ResetChoiceButton(
                        correctButton);
                }


                if (_wrongButtons.TryGetValue(
                        key,
                        out Button? wrongButton))
                {
                    ResetChoiceButton(
                        wrongButton);
                }
            }
        }


        // =========================================================
        // SAVE BUTTON
        // =========================================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                // =================================================
                // REQUIRE ALL 17 ITEMS
                // =================================================

                int answeredCount =
                    _answers.Values.Count(
                        value =>
                            value.HasValue);


                if (answeredCount !=
                    ProgressCheckIn.TotalItems)
                {
                    int remaining =
                        ProgressCheckIn.TotalItems -
                        answeredCount;


                    string message;


                    if (_existingResult != null &&
                        _existingResult.Completed)
                    {
                        message =
                            "May naitalang resulta na para sa " +
                            $"Check-in #{_checkInNumber}.\n\n" +

                            "Dahil aggregate scores lamang ang " +
                            "naka-save sa database, hindi maaaring " +
                            "ibalik ang dating eksaktong 1/0 ng " +
                            "bawat aytem.\n\n" +

                            $"Markahan muli ang natitirang " +
                            $"{remaining} aytem bago i-save ang " +
                            "bagong assessment.";
                    }
                    else
                    {
                        message =
                            $"May {remaining} aytem pa na " +
                            "hindi namarkahan.\n\n" +
                            "Markahan muna ang lahat ng " +
                            "aytem bilang 1 o 0.";
                    }


                    MessageBox.Show(
                        message,
                        "Hindi pa Kumpleto",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);


                    return;
                }


                // =================================================
                // MINUTES
                // =================================================

                if (!int.TryParse(
                        MinutesTextBox.Text.Trim(),
                        out int minutes) ||
                    minutes < 0)
                {
                    MessageBox.Show(
                        "Maglagay ng wastong bilang " +
                        "sa MINUTO.",
                        "Hindi Wastong Oras",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);


                    MinutesTextBox.Focus();


                    return;
                }


                // =================================================
                // SECONDS
                // =================================================

                if (!int.TryParse(
                        SecondsTextBox.Text.Trim(),
                        out int seconds) ||
                    seconds < 0 ||
                    seconds > 59)
                {
                    MessageBox.Show(
                        "Ang SEGUNDO ay dapat mula " +
                        "0 hanggang 59.",
                        "Hindi Wastong Oras",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);


                    SecondsTextBox.Focus();


                    return;
                }


                // =================================================
                // CREATE / UPDATE RESULT
                // =================================================

                ProgressCheckIn result =
                    _existingResult ??
                    new ProgressCheckIn();


                result.PupilId =
                    _pupil.Id;


                result.CheckInNumber =
                    _checkInNumber;


                // =================================================
                // TITIK
                // =================================================

                result.Titik1 =
                    GetAnswer("Titik1");

                result.Titik2 =
                    GetAnswer("Titik2");

                result.Titik3 =
                    GetAnswer("Titik3");

                result.Titik4 =
                    GetAnswer("Titik4");

                result.Titik5 =
                    GetAnswer("Titik5");

                result.Titik6 =
                    GetAnswer("Titik6");


                // =================================================
                // PANTIG
                // =================================================

                result.Pantig1 =
                    GetAnswer("Pantig1");

                result.Pantig2 =
                    GetAnswer("Pantig2");

                result.Pantig3 =
                    GetAnswer("Pantig3");

                result.Pantig4 =
                    GetAnswer("Pantig4");


                // =================================================
                // SALITA
                // =================================================

                result.Salita1 =
                    GetAnswer("Salita1");

                result.Salita2 =
                    GetAnswer("Salita2");

                result.Salita3 =
                    GetAnswer("Salita3");

                result.Salita4 =
                    GetAnswer("Salita4");


                // =================================================
                // PARIRALA
                // =================================================

                result.Parirala1 =
                    GetAnswer("Parirala1");

                result.Parirala2 =
                    GetAnswer("Parirala2");

                result.Parirala3 =
                    GetAnswer("Parirala3");


                // =================================================
                // TIME TAKEN
                // =================================================

                result.DurationSeconds =
                    (minutes * 60) +
                    seconds;


                // =================================================
                // REMARK
                // =================================================

                result.TeacherNotes =
                    TeacherNotesTextBox.Text.Trim();


                // =================================================
                // SAVE
                // =================================================

                _checkInService.SaveCheckIn(
                    result);


                _existingResult =
                    result;


                // =================================================
                // SUCCESS
                // =================================================

                MessageBox.Show(
                    $"Naitala na ang Check-in " +
                    $"#{_checkInNumber} ni " +
                    $"{GetDisplayName(_pupil)}.\n\n" +
                    $"Kabuuan: " +
                    $"{result.TotalScore} / " +
                    $"{ProgressCheckIn.TotalItems}",
                    "Naitala ang Check-in",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                Saved?.Invoke();


                // =================================================
                // RELOAD SAVED FIGURE 18 RECORD
                // =================================================

                ProgressCheckIn? savedResult =
                    _checkInService.GetCheckIn(
                        _pupil.Id,
                        _checkInNumber);


                if (savedResult == null)
                {
                    MessageBox.Show(
                        "Naitala ang check-in ngunit hindi " +
                        "ma-load ang scorecard.",
                        "Scorecard",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);


                    BackRequested?.Invoke();


                    return;
                }


                // =================================================
                // OPEN SCORECARD
                // =================================================

                CheckInScorecardPage scorecardPage =
                    new CheckInScorecardPage(
                        _pupil,
                        savedResult);


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
                    "Hindi ma-save ang " +
                    "Check-in Record.\n\n" +
                    ex.Message,
                    "Save Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =========================================================
        // GET ANSWER
        // =========================================================

        private int GetAnswer(
            string key)
        {
            if (!_answers.TryGetValue(
                    key,
                    out int? value))
            {
                return 0;
            }


            return value ?? 0;
        }


        // =========================================================
        // BACK BUTTON
        // =========================================================

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }


        // =========================================================
        // BUTTON TAG
        // =========================================================

        private sealed class AnswerButtonTag
        {
            public string Key { get; set; } =
                "";

            public int Score { get; set; }
        }
    }
}