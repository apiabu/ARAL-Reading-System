using ARALReadingSystem.Models;
using ARALReadingSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ARALReadingSystem.Views
{
    public partial class InstructionalSessionPage : Page
    {
        private readonly PupilService _pupilService = new();

        private readonly int _pupilId;
        private readonly int _sessionNumber;

        private Pupil? _pupil;
        private LessonSession? _lesson;

        private int _currentStepIndex;
        private bool _sessionWasAlreadyCompleted;
        private bool _completionInProgress;

        // Sessions 2 and 4
        private readonly Dictionary<int, bool> _choiceAnswers = new();
        private readonly HashSet<int> _acknowledgedRuleIndexes = new();

        // Session 3
        private bool _leftHandColoredRed;
        private bool _rightHandColoredBlue;

        // Session 5
        private readonly Dictionary<string, string> _session5MatchingAnswers = new();
        private readonly Dictionary<string, bool> _session5VowelAnswers = new();
        private readonly Dictionary<string, string> _session5ConsonantAnswers = new();
        private readonly Dictionary<string, string> _session5MissingAnswers = new();

        // Sessions 6+
        //
        // Instead of creating a new writing variable and sound dictionary
        // for every new letter session, these dictionaries store the state
        // using the session number.
        private readonly HashSet<int> _letterWritingCompletedSteps = new();
        private readonly Dictionary<int, StrokeCollection> _uppercaseWritingStrokes = new();
        private readonly Dictionary<int, StrokeCollection> _lowercaseWritingStrokes = new();

        private readonly Dictionary<int, Dictionary<string, bool>>
            _beginningSoundAnswers = new();
        private readonly Dictionary<string, string> _practiceAnswers = new();

        // Session 6 Gawain 1
        private readonly Dictionary<int, string> _session6ColorAnswers = new();

        // Session 8
        private readonly Dictionary<string, HashSet<int>>
            _session8LetterCaseAnswers = new();
        private readonly Dictionary<string, string> _session8BeginningLetterAnswers = new();

        public event Action? BackRequested;
        public event Action? SessionCompleted;

        public InstructionalSessionPage(int pupilId, int sessionNumber)
        {
            InitializeComponent();

            _pupilId = pupilId;
            _sessionNumber = sessionNumber;

            SelfDrawingCanvas.Strokes.StrokesChanged += (_, _) =>
            {
                ShowActivityFeedback(
                    "Nakita ang drawing mo!",
                    true);
            };

            Loaded += InstructionalSessionPage_Loaded;
        }

        private void ReturnToPreviousPage()
        {
            BackRequested?.Invoke();
        }

        private void InstructionalSessionPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadLesson();
        }

        // =========================================================
        // LOAD LESSON
        // =========================================================

        private void LoadLesson()
        {
            try
            {
                _pupil = _pupilService.GetPupilById(_pupilId);

                if (_pupil == null)
                {
                    MessageBox.Show(
                        "Diri nakita an rekord han mag-aaral.",
                        "Session",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    ReturnToPreviousPage();
                    return;
                }

                _lesson = CurriculumService.GetSession(_sessionNumber);

                if (_lesson == null || _lesson.Steps.Count == 0)
                {
                    MessageBox.Show(
                        $"An sulod han Sesyon {_sessionNumber} " +
                        "gin-aandam pa tikang ha ARAL workbook.",
                        "Lesson Content",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ReturnToPreviousPage();
                    return;
                }

                int expectedSession = _pupil.LastCompletedSession + 1;

                _sessionWasAlreadyCompleted =
                    _sessionNumber <= _pupil.LastCompletedSession;

                bool isCurrentSession =
                    _sessionNumber == expectedSession;

                if (!_sessionWasAlreadyCompleted && !isCurrentSession)
                {
                    MessageBox.Show(
                        $"Diri pa maaabli an Sesyon {_sessionNumber}.\n\n" +
                        $"Sunod nga sesyon: Sesyon {expectedSession}.",
                        "Locked Session",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ReturnToPreviousPage();
                    return;
                }

                CurrentPupilService.SetCurrentPupil(_pupil.Id);

                HeaderSessionTitleText.Text = _lesson.DisplayTitle;

                if (_sessionNumber == 1 &&
                    string.IsNullOrWhiteSpace(LearnerNameTextBox.Text))
                {
                    LearnerNameTextBox.Text =
                        $"{_pupil.FirstName} {_pupil.LastName}".Trim();
                }

                ResetSessionState();

                _currentStepIndex = 0;

                ShowCurrentStep();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri ma-load an instructional session.\n\n" +
                    ex.Message,
                    "Session Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ResetSessionState()
        {
            _choiceAnswers.Clear();
            _acknowledgedRuleIndexes.Clear();

            _leftHandColoredRed = false;
            _rightHandColoredBlue = false;

            _session5MatchingAnswers.Clear();
            _session5VowelAnswers.Clear();
            _session5ConsonantAnswers.Clear();
            _session5MissingAnswers.Clear();

            _letterWritingCompletedSteps.Clear();
            _uppercaseWritingStrokes.Clear();
            _lowercaseWritingStrokes.Clear();
            _beginningSoundAnswers.Clear();

            _session6ColorAnswers.Clear();
            _session8LetterCaseAnswers.Clear();
            _session8BeginningLetterAnswers.Clear();
            _practiceAnswers.Clear();

            ResetHandColors();
        }

        private LessonStep? GetCurrentStep()
        {
            if (_lesson == null)
                return null;

            if (_currentStepIndex < 0 ||
                _currentStepIndex >= _lesson.Steps.Count)
                return null;

            return _lesson.Steps[_currentStepIndex];
        }

        // =========================================================
        // SHOW STEP
        // =========================================================

        private void ShowCurrentStep()
        {
            if (_lesson == null)
                return;

            LessonStep? step = GetCurrentStep();

            if (step == null)
                return;

            HideAllPanels();
            ActivityFeedbackBorder.Visibility =
                Visibility.Collapsed;
            ChoiceValidationBorder.Visibility =
                Visibility.Collapsed;

            HeaderSessionTitleText.Text = _lesson.DisplayTitle;

            HeaderStepText.Text =
                $"{step.SectionName} · " +
                $"{_currentStepIndex + 1} SA {_lesson.TotalSteps}";

            switch (step.StepType)
            {
                case LessonStepType.Introduction:
                    ShowIntroductionStep(step);
                    break;

                case LessonStepType.Drawing:
                    ShowDrawingStep(step);
                    break;

                case LessonStepType.TextInput:
                    ShowTextInputStep(step);
                    break;

                case LessonStepType.Rules:
                    ShowRulesStep(step);
                    break;

                case LessonStepType.ChoiceActivity:
                    ShowChoiceActivityStep(step);
                    break;

                case LessonStepType.HandColoring:
                    ShowHandColoringStep(step);
                    break;

                case LessonStepType.HomeReinforcement:
                    ShowHomeReinforcementStep(step);
                    break;

                case LessonStepType.AlphabetDisplay:
                    ShowAlphabetDisplay(step);
                    break;

                case LessonStepType.LetterMatching:
                    ShowLetterMatching(step);
                    break;

                case LessonStepType.VowelActivity:
                    ShowVowelActivity(step);
                    break;

                case LessonStepType.ConsonantActivity:
                    ShowConsonantActivity(step);
                    break;

                case LessonStepType.MissingLetterActivity:
                    ShowMissingLetterActivity(step);
                    break;

                case LessonStepType.LetterWriting:
                    ShowLetterWritingActivity(step);
                    break;

                case LessonStepType.LetterColoring:
                    ShowLetterColoringActivity(step);
                    break;

                case LessonStepType.BeginningSoundActivity:
                    ShowBeginningSoundActivity(step);
                    break;

                case LessonStepType.LetterCaseActivity:
                    ShowLetterCaseActivity(step);
                    break;

                case LessonStepType.BeginningLetterActivity:
                    ShowBeginningLetterActivity(step);
                    break;

                case LessonStepType.WordChoiceActivity:
                    ShowWordChoiceActivity(step);
                    break;

                case LessonStepType.Review:
                    ShowReviewStep(step);
                    break;

                default:
                    MessageBox.Show(
                        "Ini nga lesson activity diri pa supported.",
                        "Lesson",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    break;
            }

            UpdateProgressDots();
            UpdateNavigationButtons();
        }

        private void HideAllPanels()
        {
            IntroductionPanel.Visibility = Visibility.Collapsed;
            DrawingPanel.Visibility = Visibility.Collapsed;
            NamePanel.Visibility = Visibility.Collapsed;
            InterestPanel.Visibility = Visibility.Collapsed;
            RulesPanel.Visibility = Visibility.Collapsed;
            ChoiceActivityPanel.Visibility = Visibility.Collapsed;
            HandColoringPanel.Visibility = Visibility.Collapsed;
            HomeReinforcementPanel.Visibility = Visibility.Collapsed;
            ReviewPanel.Visibility = Visibility.Collapsed;
        }

        private void UpdateProgressDots()
        {
            Border[] dots =
            {
                ProgressDot1,
                ProgressDot2,
                ProgressDot3,
                ProgressDot4,
                ProgressDot5
            };

            int total = _lesson != null
                ? Math.Max(1, _lesson.TotalSteps)
                : 1;
            int stepIndex = Math.Min(
                _currentStepIndex,
                total - 1);
            int activeDot =
                stepIndex * dots.Length / total;

            for (int i = 0; i < dots.Length; i++)
            {
                bool isActive = i == activeDot;
                dots[i].Width = isActive ? 28 : 10;
                dots[i].Background =
                    isActive
                        ? new SolidColorBrush(
                            Color.FromRgb(21, 148, 223))
                        : new SolidColorBrush(
                            Color.FromRgb(222, 216, 205));
            }
        }

        private void UpdateNavigationButtons()
        {
            if (_lesson == null)
                return;

            bool isFirst = _currentStepIndex <= 0;
            bool isLast = _currentStepIndex >= _lesson.TotalSteps - 1;

            PreviousStepButton.IsEnabled = !isFirst;
            PreviousStepButton.Opacity = isFirst ? 0.5 : 1.0;

            NextStepButton.Visibility = isLast
                ? Visibility.Collapsed
                : Visibility.Visible;

            CompleteSessionButton.Visibility = isLast
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void CloseButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBoxResult result =
                MessageBox.Show(
                    "Sigurado ka nga karuyag mo gumawas ha sesyon? " +
                    "An diri pa natatapos nga mga aktibidad diri masisave.",
                    "Lumabas sa Sesyon",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            if (BackRequested != null)
            {
                BackRequested.Invoke();
                return;
            }

            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
                return;
            }

            MessageBox.Show(
                "Diri makabalik ha nauna nga pahina.",
                "Navigation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        private void DrawToolButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SelfDrawingCanvas.EditingMode =
                InkCanvasEditingMode.Ink;

            SelfDrawingCanvas.Cursor =
                Cursors.Pen;
        }

        private void EraserToolButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SelfDrawingCanvas.EditingMode =
                InkCanvasEditingMode.EraseByStroke;

            SelfDrawingCanvas.Cursor =
                Cursors.Hand;
        }

        private void ClearDrawingButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            SelfDrawingCanvas.Strokes.Clear();
            ReviewDrawingCanvas.Strokes.Clear();

            SelfDrawingCanvas.EditingMode =
                InkCanvasEditingMode.Ink;

            SelfDrawingCanvas.Cursor =
                Cursors.Pen;

            ShowActivityFeedback(
                "Nalinis na ang drawing area. Maaari ka ulit gumuhit.",
                null);
        }

        private void LearnerNameTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LearnerNameTextBox.Text))
            {
                ShowActivityFeedback(
                    "Naitala ang pangalan mo.",
                    true);
            }
        }

        private void LearnerInterestTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(LearnerInterestTextBox.Text))
            {
                ShowActivityFeedback(
                    "Naitala ang sagot mo.",
                    true);
            }
        }

        private void PreviousStepButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_currentStepIndex > 0)
            {
                _currentStepIndex--;
                ShowCurrentStep();
            }
        }

        private void NextStepButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_lesson == null)
                return;

            if (_currentStepIndex < _lesson.TotalSteps - 1)
            {
                _currentStepIndex++;
                ShowCurrentStep();
            }
        }

        private void CompleteSessionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_completionInProgress)
            {
                return;
            }

            _completionInProgress = true;
            CompleteSessionButton.IsEnabled = false;

            try
            {
                _pupilService.CompleteSession(
                    _pupilId,
                    _sessionNumber);

                SessionCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                _completionInProgress = false;
                CompleteSessionButton.IsEnabled = true;

                MessageBox.Show(
                    "Diri na-save an pagkompleto han sesyon.\n\n" +
                    ex.Message,
                    "Session Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // INTRODUCTION
        // =========================================================

        private void ShowIntroductionStep(LessonStep step)
        {
            IntroductionPanel.Visibility = Visibility.Visible;

            IntroductionTitleText.Text =
                _lesson?.DisplayTitle ?? step.Title;

            IntroductionInstructionText.Text = step.Instruction;
            IntroductionPromptText.Text = step.Prompt;
        }

        // =========================================================
        // SESSION 1
        // =========================================================

        private void ShowDrawingStep(LessonStep step)
        {
            DrawingPanel.Visibility = Visibility.Visible;

            DrawingTitleText.Text = step.Title;
            DrawingInstructionText.Text = step.Instruction;
        }

        private void ShowTextInputStep(LessonStep step)
        {
            if (_sessionNumber != 1)
                return;

            if (step.StepNumber == 3)
            {
                NamePanel.Visibility = Visibility.Visible;
                NameTitleText.Text = step.Title;
                NameInstructionText.Text = step.Instruction;
            }
            else if (step.StepNumber == 4)
            {
                InterestPanel.Visibility = Visibility.Visible;
                InterestTitleText.Text = step.Title;
                InterestInstructionText.Text = step.Instruction;
            }
        }

        // =========================================================
        // SESSION 2 - RULES
        // =========================================================

        private void ShowRulesStep(LessonStep step)
        {
            RulesPanel.Visibility = Visibility.Visible;

            RulesTitleText.Text = step.Title;
            RulesInstructionText.Text = step.Instruction;

            RulesItemsPanel.Children.Clear();

            string[] icons =
            {
                "👂",
                "🤫",
                "🙌",
                "💭"
            };

            for (int i = 0; i < step.Items.Count; i++)
            {
                int ruleIndex = i;
                Border card = CreateWhiteCard();
                card.Cursor = Cursors.Hand;

                Grid grid = new();

                grid.ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(60)
                    });

                grid.ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(
                            1,
                            GridUnitType.Star)
                    });

                grid.ColumnDefinitions.Add(
                    new ColumnDefinition
                    {
                        Width = new GridLength(48)
                    });

                TextBlock icon = new()
                {
                    Text = i < icons.Length ? icons[i] : "•",
                    FontSize = 36,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                TextBlock text = new()
                {
                    Text = step.Items[i],
                    FontSize = 20,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = NavyBrush(),
                    VerticalAlignment = VerticalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                TextBlock practiced = new()
                {
                    Text = _acknowledgedRuleIndexes.Contains(ruleIndex)
                        ? "✓"
                        : "",
                    FontSize = 25,
                    FontWeight = FontWeights.Bold,
                    Foreground = SuccessForeground(),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Grid.SetColumn(icon, 0);
                Grid.SetColumn(text, 1);
                Grid.SetColumn(practiced, 2);

                grid.Children.Add(icon);
                grid.Children.Add(text);
                grid.Children.Add(practiced);

                card.Child = grid;
                if (_acknowledgedRuleIndexes.Contains(ruleIndex))
                {
                    card.Background = SuccessBackground();
                    card.BorderBrush = SuccessForeground();
                }

                card.MouseLeftButtonUp += (_, _) =>
                {
                    if (_acknowledgedRuleIndexes.Add(ruleIndex))
                    {
                        practiced.Text = "✓";
                        card.Background = SuccessBackground();
                        card.BorderBrush = SuccessForeground();
                        ShowActivityFeedback(
                            "Na-review mo ang tuntuning ito.",
                            true);
                    }
                    else
                    {
                        _acknowledgedRuleIndexes.Remove(ruleIndex);
                        practiced.Text = "";
                        card.Background = Brushes.White;
                        card.BorderBrush =
                            new SolidColorBrush(
                                Color.FromRgb(228, 222, 212));
                        ShowActivityFeedback(
                            "Inalis ang marka sa tuntunin.",
                            null);
                    }
                };

                RulesItemsPanel.Children.Add(card);
            }
        }

        // =========================================================
        // SESSIONS 2 AND 4 - CHOICE ACTIVITY
        // =========================================================

        private void ShowChoiceActivityStep(LessonStep step)
        {
            PrepareActivityHost(step);

            for (int i = 0; i < step.Choices.Count; i++)
            {
                int choiceIndex = i;
                LessonChoice choice = step.Choices[i];

                Border card = CreateWhiteCard();

                card.Width = 390;
                card.MinHeight = 170;
                card.Margin = new Thickness(8);

                StackPanel content = new();

                TextBlock number = new()
                {
                    Text = _sessionNumber == 4
                        ? $"Pares {i + 1}"
                        : $"Sitwasyon {i + 1}",

                    FontSize = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = BlueBrush()
                };

                TextBlock description = new()
                {
                    Text = choice.Text,
                    FontSize = _sessionNumber == 4 ? 19 : 17,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = NavyBrush(),
                    TextWrapping = TextWrapping.Wrap,

                    TextAlignment =
                        _sessionNumber == 4
                            ? TextAlignment.Center
                            : TextAlignment.Left,

                    Margin = new Thickness(0, 8, 0, 18)
                };

                StackPanel buttons = new()
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Button yesButton = CreateInteractiveChoiceButton(
                    _sessionNumber == 4 ? "✓" : "👍",
                    _sessionNumber == 4 ? "Magkatugma" : "Oo");

                Button noButton = CreateInteractiveChoiceButton(
                    _sessionNumber == 4 ? "✕" : "👎",
                    "Hindi");

                yesButton.Click += (_, _) =>
                {
                    _choiceAnswers[choiceIndex] = true;
                    bool correct =
                        true == choice.CorrectAnswer;

                    ApplyChoiceButtonState(
                        yesButton,
                        noButton,
                        correct);

                        ShowAnswerResult(correct);
                };

                noButton.Click += (_, _) =>
                {
                    _choiceAnswers[choiceIndex] = false;
                    bool correct =
                        false == choice.CorrectAnswer;

                    ApplyChoiceButtonState(
                        noButton,
                        yesButton,
                        correct);

                    ShowAnswerResult(correct);
                };

                if (_choiceAnswers.TryGetValue(
                    choiceIndex,
                    out bool savedAnswer))
                {
                    ApplyChoiceButtonState(
                        savedAnswer ? yesButton : noButton,
                        savedAnswer ? noButton : yesButton,
                        savedAnswer == choice.CorrectAnswer);
                }

                buttons.Children.Add(yesButton);
                buttons.Children.Add(noButton);

                content.Children.Add(number);
                content.Children.Add(description);
                content.Children.Add(buttons);

                card.Child = content;

                ChoiceItemsPanel.Children.Add(card);
            }
        }

        private void ShowWordChoiceActivity(
            LessonStep step)
        {
            PrepareActivityHost(step);

            for (int index = 0; index < step.Items.Count; index++)
            {
                string[] parts = step.Items[index].Split('|');
                if (parts.Length != 3)
                {
                    continue;
                }

                string question = parts[0].Trim();
                string[] options = parts[1]
                    .Split('~', StringSplitOptions.RemoveEmptyEntries)
                    .Select(option => option.Trim())
                    .ToArray();
                string correctAnswer = parts[2].Trim();

                if (string.IsNullOrWhiteSpace(question) ||
                    options.Length < 2 ||
                    !options.Contains(
                        correctAnswer,
                        StringComparer.OrdinalIgnoreCase))
                {
                    continue;
                }

                int questionIndex = index;
                string answerKey =
                    $"{_sessionNumber}:{step.StepNumber}:{questionIndex}";

                Border card = CreateWhiteCard();
                card.Width = 540;
                card.Margin = new Thickness(8);

                StackPanel content = new();
                content.Children.Add(new TextBlock
                {
                    Text = question,
                    FontSize = 21,
                    FontWeight = FontWeights.Bold,
                    Foreground = NavyBrush(),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 14)
                });

                WrapPanel optionPanel = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                foreach (string option in options)
                {
                    Button optionButton = CreateChoiceButton(option);
                    optionButton.MinWidth = 115;
                    optionButton.MinHeight = 52;
                    optionButton.Margin = new Thickness(5);
                    optionButton.FontSize = 19;

                    optionButton.Click += (_, _) =>
                    {
                        _practiceAnswers[answerKey] = option;

                        foreach (Button button in
                                 optionPanel.Children.OfType<Button>())
                        {
                            button.Background = Brushes.White;
                            button.Foreground = NavyBrush();
                            button.BorderBrush =
                                new SolidColorBrush(
                                    Color.FromRgb(214, 208, 196));
                        }

                        bool isCorrect = string.Equals(
                            option,
                            correctAnswer,
                            StringComparison.OrdinalIgnoreCase);

                        optionButton.Background = isCorrect
                            ? SuccessBackground()
                            : ErrorBackground();
                        optionButton.Foreground = isCorrect
                            ? SuccessForeground()
                            : ErrorForeground();
                        optionButton.BorderBrush = isCorrect
                            ? SuccessForeground()
                            : ErrorForeground();

                        ShowAnswerResult(isCorrect);
                    };

                    if (_practiceAnswers.TryGetValue(
                            answerKey,
                            out string? savedAnswer) &&
                        string.Equals(
                            option,
                            savedAnswer,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        bool wasCorrect = string.Equals(
                            option,
                            correctAnswer,
                            StringComparison.OrdinalIgnoreCase);

                        optionButton.Background = wasCorrect
                            ? SuccessBackground()
                            : ErrorBackground();
                        optionButton.Foreground = wasCorrect
                            ? SuccessForeground()
                            : ErrorForeground();
                        optionButton.BorderBrush = wasCorrect
                            ? SuccessForeground()
                            : ErrorForeground();
                    }

                    optionPanel.Children.Add(optionButton);
                }

                content.Children.Add(optionPanel);
                card.Child = content;
                ChoiceItemsPanel.Children.Add(card);
            }
        }

        private void SetChoiceAnswer(
            int index,
            bool answer,
            Button selectedButton,
            Button otherButton,
            bool correctAnswer)
        {
            _choiceAnswers[index] = answer;

            ApplyChoiceButtonState(
                selectedButton,
                otherButton,
                answer == correctAnswer);

            ChoiceValidationText.Visibility =
                Visibility.Collapsed;
        }

        private void ApplyChoiceButtonState(
            Button selectedButton,
            Button otherButton,
            bool correct)
        {
            selectedButton.Background =
                correct
                    ? SuccessBackground()
                    : ErrorBackground();

            selectedButton.Foreground =
                correct
                    ? SuccessForeground()
                    : ErrorForeground();

            otherButton.Background = Brushes.White;
            otherButton.Foreground = NavyBrush();
        }

        // =========================================================
        // SESSION 3 - HAND COLORING
        // =========================================================

        private void ShowHandColoringStep(LessonStep step)
        {
            HandColoringPanel.Visibility = Visibility.Visible;

            HandSectionBadgeText.Text = step.SectionName;
            HandColoringTitleText.Text = step.Title;
            HandColoringInstructionText.Text = step.Instruction;
            HandColoringPromptText.Text = step.Prompt;

            HandValidationBorder.Visibility =
                Visibility.Collapsed;

            UpdateHandVisuals();
        }

        private void LeftHandShape_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            ColorLeftHandRed();
        }

        private void RightHandShape_MouseLeftButtonUp(
            object sender,
            MouseButtonEventArgs e)
        {
            ColorRightHandBlue();
        }

        private void LeftRedButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ColorLeftHandRed();
        }

        private void RightBlueButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ColorRightHandBlue();
        }

        private void ColorLeftHandRed()
        {
            _leftHandColoredRed = true;

            UpdateHandVisuals();

            ShowAnswerResult(
                true,
                "Tama! Pula ang kaliwang kamay.");
        }

        private void ColorRightHandBlue()
        {
            _rightHandColoredBlue = true;

            UpdateHandVisuals();

            ShowAnswerResult(
                true,
                "Tama! Asul ang kanang kamay.");
        }

        private void ResetHandColors()
        {
            if (LeftHandShape == null ||
                RightHandShape == null)
                return;

            LeftHandShape.Fill = Brushes.White;
            RightHandShape.Fill = Brushes.White;
        }

        private void UpdateHandVisuals()
        {
            LeftHandShape.Fill =
                _leftHandColoredRed
                    ? new SolidColorBrush(
                        Color.FromRgb(224, 72, 67))
                    : Brushes.White;

            RightHandShape.Fill =
                _rightHandColoredBlue
                    ? new SolidColorBrush(
                        Color.FromRgb(21, 148, 223))
                    : Brushes.White;

            LeftRedButton.Content =
                _leftHandColoredRed
                    ? "✓  Kaliwa = Pula"
                    : "🔴  Kulayan ng Pula";

            RightBlueButton.Content =
                _rightHandColoredBlue
                    ? "✓  Kanan = Asul"
                    : "🔵  Kulayan ng Asul";
        }

        private void ShowHomeReinforcementStep(
            LessonStep step)
        {
            HomeReinforcementPanel.Visibility =
                Visibility.Visible;

            HomeTitleText.Text = step.Title;
            HomeInstructionText.Text = step.Instruction;
            HomePromptText.Text = step.Prompt;
        }

        // =========================================================
        // COMMON DYNAMIC ACTIVITY HOST
        // =========================================================

        private void PrepareActivityHost(LessonStep step)
        {
            ChoiceActivityPanel.Visibility =
                Visibility.Visible;

            ChoiceTitleText.Text = step.Title;
            ChoiceInstructionText.Text = step.Instruction;

            ChoiceValidationText.Visibility =
                Visibility.Collapsed;

            ChoiceItemsPanel.Children.Clear();
        }

        private void ShowSessionValidation(
            string message)
        {
            ChoiceValidationText.Text = message;
            ChoiceValidationText.Visibility =
                Visibility.Visible;
            ChoiceValidationBorder.Visibility =
                Visibility.Visible;

            ChoiceValidationBorder.Background =
                ErrorBackground();
            ChoiceValidationBorder.BorderBrush =
                ErrorForeground();

            ShowActivityFeedback(message, false);
        }

        private void HideActivityValidation()
        {
            ShowActivityFeedback(
                "Naitala ang sagot mo!",
                null);
        }

        private void ShowAnswerResult(
            bool correct,
            string? correctMessage = null,
            string? incorrectMessage = null)
        {
            ShowActivityFeedback(
                correct
                    ? correctMessage ?? "Tama! Magaling!"
                    : incorrectMessage ?? "Subukan ulit. Kaya mo ito!",
                correct);
        }

        private void ShowActivityFeedback(
            string message,
            bool? correct)
        {
            ActivityFeedbackText.Text = message;

            if (correct == true)
            {
                ActivityFeedbackBorder.Background =
                    SuccessBackground();
                ActivityFeedbackBorder.BorderBrush =
                    SuccessForeground();
                ActivityFeedbackText.Foreground =
                    SuccessForeground();
            }
            else if (correct == false)
            {
                ActivityFeedbackBorder.Background =
                    ErrorBackground();
                ActivityFeedbackBorder.BorderBrush =
                    ErrorForeground();
                ActivityFeedbackText.Foreground =
                    ErrorForeground();
            }
            else
            {
                ActivityFeedbackBorder.Background =
                    new SolidColorBrush(
                        Color.FromRgb(234, 246, 253));
                ActivityFeedbackBorder.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(184, 223, 243));
                ActivityFeedbackText.Foreground =
                    BlueBrush();
            }

            ActivityFeedbackBorder.Visibility =
                Visibility.Visible;

            ChoiceValidationText.Visibility =
                Visibility.Collapsed;
            ChoiceValidationBorder.Visibility =
                Visibility.Collapsed;
        }

        private Border CreateWhiteCard()
        {
            return new Border
            {
                Background = Brushes.White,
                BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(228, 222, 212)),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(18),
                Padding = new Thickness(14)
            };
        }

        private Border CreateHint(string text)
        {
            Border border = CreateWhiteCard();
            border.Margin = new Thickness(0, 0, 0, 12);
            border.Background =
                new SolidColorBrush(
                    Color.FromRgb(223, 243, 253));
            border.BorderBrush =
                new SolidColorBrush(
                    Color.FromRgb(198, 227, 241));

            border.Child = new TextBlock
            {
                Text = text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Foreground = NavyBrush(),
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            return border;
        }

        private Button CreateInteractiveChoiceButton(
            string icon,
            string label)
        {
            StackPanel content = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            content.Children.Add(
                new TextBlock
                {
                    Text = icon,
                    FontSize = 21,
                    Margin = new Thickness(0, 0, 8, 0),
                    VerticalAlignment = VerticalAlignment.Center
                });

            content.Children.Add(
                new TextBlock
                {
                    Text = label,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center
                });

            return new Button
            {
                Content = content,
                Width = 132,
                MinHeight = 48,
                Margin = new Thickness(6, 0, 0, 0),
                Background = Brushes.White,
                Foreground = NavyBrush(),
                BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(214, 208, 196)),
                BorderThickness = new Thickness(1),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(10, 6, 10, 6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
        }

        private Button CreateChoiceButton(string content)
        {
            return new Button
            {
                Content = content,
                Width = 120,
                MinHeight = 42,
                Margin = new Thickness(6, 0, 0, 0),
                Background = Brushes.White,
                Foreground = NavyBrush(),
                BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(214, 208, 196)),
                BorderThickness = new Thickness(1),
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                Padding = new Thickness(10, 6, 10, 6),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand
            };
        }

        private Button CreateSelectableCard(string text)
        {
            Button card = new()
            {
                Content = text,
                Width = 120,
                Height = 72,
                Margin = new Thickness(8),
                Background = Brushes.White,
                Foreground = NavyBrush(),
                BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(214, 208, 196)),
                BorderThickness = new Thickness(1),
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Cursor = Cursors.Hand,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            return card;
        }

        private TextBox CreateSingleLetterBox()
        {
            return new TextBox
            {
                Width = 58,
                Height = 52,
                TextAlignment = TextAlignment.Center,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                MaxLength = 1,
                Foreground = NavyBrush(),
                Background = Brushes.White,
                BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(214, 208, 196)),
                BorderThickness = new Thickness(1),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };
        }

        private SolidColorBrush BlueBrush()
        {
            return new SolidColorBrush(
                Color.FromRgb(8, 121, 188));
        }

        private SolidColorBrush NavyBrush()
        {
            return new SolidColorBrush(
                Color.FromRgb(24, 49, 73));
        }

        private SolidColorBrush MutedBrush()
        {
            return new SolidColorBrush(
                Color.FromRgb(96, 121, 142));
        }

        private SolidColorBrush SuccessBackground()
        {
            return new SolidColorBrush(
                Color.FromRgb(220, 245, 230));
        }

        private SolidColorBrush ErrorBackground()
        {
            return new SolidColorBrush(
                Color.FromRgb(255, 233, 231));
        }

        private SolidColorBrush SuccessForeground()
        {
            return new SolidColorBrush(
                Color.FromRgb(22, 133, 82));
        }

        private SolidColorBrush ErrorForeground()
        {
            return new SolidColorBrush(
                Color.FromRgb(198, 61, 53));
        }

        private void ApplySelectedCard(
            Button card,
            bool selected)
        {
            card.Background =
                selected
                    ? SuccessBackground()
                    : Brushes.White;

            card.BorderBrush =
                selected
                    ? new SolidColorBrush(
                        Color.FromRgb(59, 162, 110))
                    : new SolidColorBrush(
                        Color.FromRgb(214, 208, 196));

            card.Foreground =
                selected
                    ? SuccessForeground()
                    : NavyBrush();
        }

        private void ShowBeginningSoundActivity(
            LessonStep step)
        {
            PrepareActivityHost(step);

            ChoiceItemsPanel.Children.Add(
                CreateHint(
                    "Piliin ang mga larawan na nagsisimula sa tunog ng letrang tinatalakay."));

            foreach (string item in step.Items)
            {
                string[] parts = item.Split('|');

                if (parts.Length != 2)
                    continue;

                string label = parts[0];
                bool isCorrect = bool.Parse(parts[1]);

                Border card = CreateWhiteCard();
                card.Margin = new Thickness(8);

                StackPanel panel = new();
                TextBlock text = new()
                {
                    Text = label,
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = NavyBrush(),
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                Button toggle = CreateChoiceButton("Piliin");
                toggle.Width = 120;
                toggle.Click += (_, _) =>
                {
                    if (!_beginningSoundAnswers.ContainsKey(_sessionNumber))
                    {
                        _beginningSoundAnswers[_sessionNumber] =
                            new Dictionary<string, bool>();
                    }

                    bool selected =
                        !_beginningSoundAnswers[_sessionNumber]
                            .TryGetValue(label, out bool current) ||
                        !current;

                    _beginningSoundAnswers[_sessionNumber][label] =
                        selected;

                    toggle.Background =
                        selected
                            ? SuccessBackground()
                            : Brushes.White;
                    toggle.Foreground =
                        selected
                            ? SuccessForeground()
                            : NavyBrush();

                    ShowAnswerResult(
                        selected == isCorrect,
                        selected
                            ? "Tama! Ang napili mong larawan ay nagsisimula sa tunog."
                            : "Hindi ito ang tamang larawan para sa tunog na ito.",
                        isCorrect
                            ? "Piliin ang larawang nagsisimula sa tunog."
                            : "Tama! Hindi ito nagsisimula sa tunog na hinahanap.");
                };

                panel.Children.Add(text);
                panel.Children.Add(toggle);
                card.Child = panel;
                ChoiceItemsPanel.Children.Add(card);
            }
        }

        private void ShowLetterCaseActivity(
            LessonStep step)
        {
            PrepareActivityHost(step);

            string[] targetLetters =
                GetTargetLetters(step);

            foreach (string word in step.Items)
            {
                Border card = CreateWhiteCard();
                card.Margin = new Thickness(8);

                StackPanel panel = new();
                TextBlock instruction = new()
                {
                    Text = string.IsNullOrWhiteSpace(step.Prompt)
                        ? $"Pindutin ang mga titik {string.Join(", ", targetLetters)}."
                        : step.Prompt,
                    FontSize = 15,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = NavyBrush(),
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                WrapPanel letters = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                if (!_session8LetterCaseAnswers.TryGetValue(
                    word,
                    out HashSet<int>? selectedIndexes))
                {
                    selectedIndexes = new HashSet<int>();
                    _session8LetterCaseAnswers[word] =
                        selectedIndexes;
                }

                for (int i = 0; i < word.Length; i++)
                {
                    int letterIndex = i;
                    char letter = word[i];
                    bool isTargetLetter =
                        IsTargetLetter(letter, targetLetters);

                    Button letterButton = new()
                    {
                        Content = letter.ToString(),
                        Width = 52,
                        Height = 62,
                        Margin = new Thickness(3),
                        Background = Brushes.White,
                        Foreground = NavyBrush(),
                        BorderBrush =
                            new SolidColorBrush(
                                Color.FromRgb(214, 208, 196)),
                        BorderThickness = new Thickness(1),
                        FontSize = 25,
                        FontWeight = FontWeights.Bold,
                        Cursor = Cursors.Hand,
                        ToolTip = isTargetLetter
                            ? $"Pindutin para piliin ang titik {letter}."
                            : "Hindi ito ang target na titik."
                    };

                    if (selectedIndexes.Contains(letterIndex))
                    {
                        letterButton.Background = SuccessBackground();
                        letterButton.Foreground = SuccessForeground();
                        letterButton.BorderBrush = SuccessForeground();
                        letterButton.BorderThickness = new Thickness(2);
                    }

                    letterButton.Click += (_, _) =>
                    {
                        if (isTargetLetter)
                        {
                            selectedIndexes.Add(letterIndex);
                            letterButton.Background =
                                SuccessBackground();
                            letterButton.Foreground =
                                SuccessForeground();
                            letterButton.BorderBrush =
                                SuccessForeground();
                            letterButton.BorderThickness =
                                new Thickness(2);
                        }
                        else
                        {
                            letterButton.Background =
                                ErrorBackground();
                            letterButton.Foreground =
                                ErrorForeground();
                            letterButton.BorderBrush =
                                ErrorForeground();
                            letterButton.BorderThickness =
                                new Thickness(2);
                        }

                        ShowAnswerResult(
                            isTargetLetter,
                            $"Tama! Nahanap mo ang titik {letter}.",
                            "Hindi ito ang target na titik. Subukan ang ibang titik.");
                    };

                    letters.Children.Add(letterButton);
                }

                panel.Children.Add(instruction);
                panel.Children.Add(letters);
                card.Child = panel;
                ChoiceItemsPanel.Children.Add(card);
            }
        }

        private static string[] GetTargetLetters(
            LessonStep step)
        {
            string[] targets = step.Placeholder
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(target => target.Trim())
                .Where(target => !string.IsNullOrWhiteSpace(target))
                .ToArray();

            return targets.Length > 0
                ? targets
                : new[] { "A" };
        }

        private static bool IsTargetLetter(
            char letter,
            LessonStep step)
        {
            return IsTargetLetter(
                letter,
                GetTargetLetters(step));
        }

        private static bool IsTargetLetter(
            char letter,
            IReadOnlyCollection<string> targetLetters)
        {
            return targetLetters.Any(
                target => string.Equals(
                    target,
                    letter.ToString(),
                    StringComparison.OrdinalIgnoreCase));
        }

        private void ShowBeginningLetterActivity(
            LessonStep step)
        {
            PrepareActivityHost(step);

            string[] letterOptions = GetTargetLetters(step);

            foreach (string item in step.Items)
            {
                string[] parts = item.Split('|');
                if (parts.Length != 2)
                    continue;

                string label = parts[0];
                string correctLetter = parts[1];

                Border card = CreateWhiteCard();
                card.Margin = new Thickness(8);

                StackPanel panel = new();
                TextBlock text = new()
                {
                    Text = label,
                    FontSize = 26,
                    FontWeight = FontWeights.Bold,
                    Foreground = NavyBrush(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                WrapPanel options = new()
                {
                    HorizontalAlignment = HorizontalAlignment.Center
                };

                foreach (string option in letterOptions)
                {
                    Button optionButton = CreateChoiceButton(option);
                    optionButton.Width = 70;
                    optionButton.Click += (_, _) =>
                    {
                        _session8BeginningLetterAnswers[label] = option;
                        foreach (Button child in options.Children.OfType<Button>())
                        {
                            child.Background = Brushes.White;
                            child.Foreground = NavyBrush();
                        }

                        bool isCorrect =
                            string.Equals(option, correctLetter, StringComparison.OrdinalIgnoreCase);

                        optionButton.Background =
                            isCorrect
                                ? SuccessBackground()
                                : ErrorBackground();
                        optionButton.Foreground =
                            isCorrect
                                ? SuccessForeground()
                                : ErrorForeground();
                        ShowAnswerResult(isCorrect);
                    };

                    options.Children.Add(optionButton);
                }

                panel.Children.Add(text);
                panel.Children.Add(options);
                card.Child = panel;
                ChoiceItemsPanel.Children.Add(card);
            }
        }

        private void ShowReviewStep(LessonStep step)
        {
            ReviewPanel.Visibility = Visibility.Visible;

            ReviewTitleText.Text = step.Title;
            ReviewInstructionText.Text = step.Instruction;

            ReviewDrawingCanvas.Strokes =
                new StrokeCollection(SelfDrawingCanvas.Strokes);
            ReviewActivityResultText.Text =
                BuildReviewResult();

            bool showPersonalWork =
                _sessionNumber == 1;

            ReviewNameLabelText.Visibility =
                showPersonalWork ? Visibility.Visible : Visibility.Collapsed;
            ReviewNameText.Visibility =
                showPersonalWork ? Visibility.Visible : Visibility.Collapsed;
            ReviewInterestLabelText.Visibility =
                showPersonalWork ? Visibility.Visible : Visibility.Collapsed;
            ReviewInterestText.Visibility =
                showPersonalWork ? Visibility.Visible : Visibility.Collapsed;
            ReviewDrawingBorder.Visibility =
                showPersonalWork ? Visibility.Visible : Visibility.Collapsed;
            Session3ReviewPanel.Visibility =
                _sessionNumber == 3 ? Visibility.Visible : Visibility.Collapsed;

            ReviewNameText.Text =
                string.IsNullOrWhiteSpace(LearnerNameTextBox.Text)
                    ? "-"
                    : LearnerNameTextBox.Text.Trim();

            ReviewInterestText.Text =
                string.IsNullOrWhiteSpace(LearnerInterestTextBox.Text)
                    ? "-"
                    : LearnerInterestTextBox.Text.Trim();
        }

        private string BuildReviewResult()
        {
            if (_lesson == null)
            {
                return "Natapos mo ang gawain. Magaling!";
            }

            int total = 0;
            int answered = 0;
            int correct = 0;
            int rulesReviewed = 0;

            void Record(bool isCorrect)
            {
                answered++;
                if (isCorrect)
                {
                    correct++;
                }
            }

            foreach (LessonStep step in _lesson.Steps)
            {
                switch (step.StepType)
                {
                    case LessonStepType.ChoiceActivity:
                        for (int i = 0; i < step.Choices.Count; i++)
                        {
                            total++;
                            if (_choiceAnswers.TryGetValue(i, out bool answer))
                            {
                                Record(answer == step.Choices[i].CorrectAnswer);
                            }
                        }
                        break;

                    case LessonStepType.Rules:
                        rulesReviewed =
                            _acknowledgedRuleIndexes.Count;
                        break;

                    case LessonStepType.HandColoring:
                        total += 2;
                        if (_leftHandColoredRed)
                        {
                            Record(true);
                        }
                        if (_rightHandColoredBlue)
                        {
                            Record(true);
                        }
                        break;

                    case LessonStepType.LetterWriting:
                        total++;
                        if (IsLetterWritingCompleted(step.StepNumber))
                        {
                            Record(true);
                        }
                        break;

                    case LessonStepType.LetterMatching:
                        foreach (string item in step.Items)
                        {
                            string[] parts = item.Split('|');
                            if (parts.Length != 2)
                                continue;

                            total++;
                            if (_session5MatchingAnswers.TryGetValue(
                                parts[0],
                                out string? answer))
                            {
                                Record(string.Equals(
                                    parts[0],
                                    answer,
                                    StringComparison.OrdinalIgnoreCase));
                            }
                        }
                        break;

                    case LessonStepType.LetterCaseActivity:
                        foreach (string word in step.Items)
                        {
                            HashSet<int> expectedIndexes =
                                new HashSet<int>(
                                    Enumerable.Range(0, word.Length)
                                        .Where(i =>
                                            IsTargetLetter(
                                                word[i],
                                                step)));

                            total += expectedIndexes.Count;

                            if (_session8LetterCaseAnswers.TryGetValue(
                                word,
                                out HashSet<int>? selectedIndexes))
                            {
                                int correctSelections =
                                    selectedIndexes.Count(
                                        expectedIndexes.Contains);

                                answered += selectedIndexes.Count;
                                correct += correctSelections;
                            }
                        }
                        break;

                    case LessonStepType.VowelActivity:
                        foreach (string item in step.Items)
                        {
                            string[] parts = item.Split('|');
                            if (parts.Length != 2)
                                continue;

                            total++;
                            if (_session5VowelAnswers.TryGetValue(
                                parts[0],
                                out bool selected))
                            {
                                Record(selected == bool.Parse(parts[1]));
                            }
                        }
                        break;

                    case LessonStepType.ConsonantActivity:
                        foreach (string item in step.Items)
                        {
                            string[] parts = item.Split('|');
                            if (parts.Length != 3 || !bool.Parse(parts[2]))
                                continue;

                            total++;
                            if (_session5ConsonantAnswers.TryGetValue(
                                parts[0],
                                out string? answer))
                            {
                                Record(string.Equals(
                                    parts[1],
                                    answer,
                                    StringComparison.OrdinalIgnoreCase));
                            }
                        }
                        break;

                    case LessonStepType.MissingLetterActivity:
                        foreach (string item in step.Items)
                        {
                            string[] parts = item.Split('|');
                            if (parts.Length != 3)
                                continue;

                            total++;
                            if (_session5MissingAnswers.TryGetValue(
                                parts[1],
                                out string? answer))
                            {
                                Record(string.Equals(
                                    parts[2],
                                    answer,
                                    StringComparison.OrdinalIgnoreCase));
                            }
                        }
                        break;

                    case LessonStepType.BeginningSoundActivity:
                        foreach (string item in step.Items)
                        {
                            string[] parts = item.Split('|');
                            if (parts.Length != 2 ||
                                !bool.TryParse(parts[1], out bool expected))
                                continue;

                            total++;
                            if (_beginningSoundAnswers.TryGetValue(
                                    _sessionNumber,
                                    out Dictionary<string, bool>? answers) &&
                                answers.TryGetValue(parts[0], out bool selected))
                            {
                                Record(selected == expected);
                            }
                        }
                        break;

                    case LessonStepType.BeginningLetterActivity:
                        foreach (string item in step.Items)
                        {
                            string[] parts = item.Split('|');
                            if (parts.Length != 2)
                                continue;

                            total++;
                            if (_session8BeginningLetterAnswers.TryGetValue(
                                parts[0],
                                out string? answer))
                            {
                                Record(string.Equals(
                                    parts[1],
                                    answer,
                                    StringComparison.OrdinalIgnoreCase));
                            }
                        }
                        break;

                    case LessonStepType.WordChoiceActivity:
                        for (int index = 0; index < step.Items.Count; index++)
                        {
                            string[] parts = step.Items[index].Split('|');
                            if (parts.Length != 3)
                                continue;

                            total++;
                            string answerKey =
                                $"{_sessionNumber}:{step.StepNumber}:{index}";

                            if (_practiceAnswers.TryGetValue(
                                    answerKey,
                                    out string? answer))
                            {
                                Record(string.Equals(
                                    parts[2].Trim(),
                                    answer,
                                    StringComparison.OrdinalIgnoreCase));
                            }
                        }
                        break;

                    case LessonStepType.LetterColoring:
                        for (int i = 0; i < step.Items.Count; i++)
                        {
                            string[] parts = step.Items[i].Split('|');
                            if (parts.Length != 2)
                                continue;

                            total++;
                            if (_session6ColorAnswers.TryGetValue(
                                i,
                                out string? answer))
                            {
                                Record(string.Equals(
                                    parts[1],
                                    answer,
                                    StringComparison.OrdinalIgnoreCase));
                            }
                        }
                        break;
                }
            }

            if (answered == 0)
            {
                string noAnswersResult = _sessionNumber == 1
                    ? "Narito ang pangalan, sagot, at drawing na ginawa mo. Magaling!"
                    : "Natapos mo ang mga gawain sa sesyon. Magaling!";

                return rulesReviewed > 0
                    ? $"{noAnswersResult} Na-review mo ang {rulesReviewed} na tuntunin."
                    : noAnswersResult;
            }

            string result =
                $"Resulta mo: {correct} sa {answered} na nasagot ang tama. " +
                $"Nasagutan mo ang {answered} sa {total} na tanong.";

            return rulesReviewed > 0
                ? $"{result} Na-review mo rin ang {rulesReviewed} na tuntunin."
                : result;
        }

        // =========================================================
        // SESSION 5 - ALPHABET
        // =========================================================

        private void ShowAlphabetDisplay(LessonStep step)
        {
            PrepareActivityHost(step);

            Border info = CreateWhiteCard();

            info.Margin = new Thickness(8, 8, 8, 20);

            info.Child = new TextBlock
            {
                Text =
                    "Ang alpabetong Pilipino ay binubuo ng 28 na titik.\n" +
                    "Lima (5) ang patinig at 23 naman ang katinig.",

                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = NavyBrush(),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };

            ChoiceItemsPanel.Children.Add(info);

            WrapPanel alphabet = new()
            {
                HorizontalAlignment =
                    HorizontalAlignment.Center,

                MaxWidth = 800
            };

            TextBlock selectedLetterHint = new()
            {
                Text = "Pumili ng titik para makita kung patinig o katinig.",
                FontSize = 18,
                FontWeight = FontWeights.SemiBold,
                Foreground = MutedBrush(),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(8, 0, 8, 14)
            };

            ChoiceItemsPanel.Children.Add(selectedLetterHint);

            HashSet<string> vowels = new()
            {
                "Aa",
                "Ee",
                "Ii",
                "Oo",
                "Uu"
            };

            foreach (string letter in step.Items)
            {
                bool vowel = vowels.Contains(letter);

                Button tile = new()
                {
                    Width = 100,
                    Height = 82,
                    Margin = new Thickness(6),
                    Background =
                        vowel
                            ? new SolidColorBrush(
                                Color.FromRgb(
                                    255, 239, 231))
                            : new SolidColorBrush(
                                Color.FromRgb(
                                    235, 247, 255)),

                    BorderBrush =
                        vowel
                            ? new SolidColorBrush(
                                Color.FromRgb(
                                    235, 126, 92))
                            : new SolidColorBrush(
                                Color.FromRgb(
                                    122, 196, 236)),
                    BorderThickness = new Thickness(2),
                    FontSize = 28,
                    FontWeight = FontWeights.Bold,
                    Cursor = Cursors.Hand,
                    ToolTip = $"Pindutin ang titik {letter}",
                    Content = new TextBlock
                    {
                        Text = letter,
                        FontSize = 28,
                        FontWeight = FontWeights.Bold,
                        Foreground =
                            vowel
                                ? new SolidColorBrush(
                                    Color.FromRgb(220, 74, 58))
                                : new SolidColorBrush(
                                    Color.FromRgb(24, 92, 150)),
                        HorizontalAlignment =
                            HorizontalAlignment.Center,
                        VerticalAlignment =
                            VerticalAlignment.Center
                    }
                };

                tile.Click += (_, _) =>
                {
                    foreach (Button otherTile in alphabet.Children
                                 .OfType<Button>())
                    {
                        ApplySelectedCard(otherTile, false);
                    }

                    ApplySelectedCard(tile, true);
                    selectedLetterHint.Text =
                        $"{letter} — {(vowel ? "PATINIG" : "KATINIG")}";
                    selectedLetterHint.Foreground =
                        vowel ? ErrorForeground() : BlueBrush();
                    ShowActivityFeedback(
                        $"{letter} ay {(vowel ? "patinig" : "katinig")}.",
                        true);
                };

                alphabet.Children.Add(tile);
            }

            ChoiceItemsPanel.Children.Add(alphabet);
        }

        // =========================================================
        // SESSION 5 - LETTER MATCHING
        // =========================================================

        private void ShowLetterMatching(LessonStep step)
        {
            PrepareActivityHost(step);

            ChoiceItemsPanel.Children.Add(
                CreateHint(
                    "Piliin ang maliit na titik na katumbas ng malaking titik."));

            foreach (string item in step.Items)
            {
                string[] parts = item.Split('|');

                if (parts.Length != 2)
                    continue;

                string uppercase = parts[0];
                string[] options = parts[1].Split(',');

                Border card = CreateWhiteCard();

                card.Margin = new Thickness(8);

                StackPanel panel = new();

                TextBlock letter = new()
                {
                    Text = uppercase,
                    FontSize = 34,
                    FontWeight = FontWeights.Bold,
                    Foreground = BlueBrush(),

                    HorizontalAlignment =
                        HorizontalAlignment.Center,

                    Margin =
                        new Thickness(0, 0, 0, 12)
                };

                WrapPanel buttons = new()
                {
                    HorizontalAlignment =
                        HorizontalAlignment.Center
                };

                foreach (string rawOption in options)
                {
                    string option = rawOption.Trim();

                    Button button =
                        CreateChoiceButton(option);

                    button.Width = 65;
                    button.FontSize = 20;

                    button.Click += (_, _) =>
                    {
                        _session5MatchingAnswers[
                            uppercase] = option;

                        foreach (object child
                                 in buttons.Children)
                        {
                            if (child is Button b)
                            {
                                b.Background =
                                    Brushes.White;

                                b.Foreground =
                                    NavyBrush();
                            }
                        }

                        bool correct =
                            string.Equals(
                                uppercase,
                                option,
                                StringComparison.OrdinalIgnoreCase);

                        button.Background =
                            correct
                                ? SuccessBackground()
                                : ErrorBackground();

                        button.Foreground =
                            correct
                                ? SuccessForeground()
                                : ErrorForeground();

                            ShowAnswerResult(correct);
                    };

                    if (_session5MatchingAnswers
                            .TryGetValue(
                                uppercase,
                                out string? saved) &&
                        saved == option)
                    {
                        bool correct =
                            string.Equals(
                                uppercase,
                                saved,
                                StringComparison.OrdinalIgnoreCase);

                        button.Background =
                            correct
                                ? SuccessBackground()
                                : ErrorBackground();

                        button.Foreground =
                            correct
                                ? SuccessForeground()
                                : ErrorForeground();
                    }

                    buttons.Children.Add(button);
                }

                panel.Children.Add(letter);
                panel.Children.Add(buttons);

                card.Child = panel;

                ChoiceItemsPanel.Children.Add(card);
            }
        }

        // =========================================================
        // SESSION 5 - VOWELS
        // =========================================================

        private void ShowVowelActivity(LessonStep step)
        {
            PrepareActivityHost(step);

            WrapPanel wrap = new()
            {
                HorizontalAlignment =
                    HorizontalAlignment.Center
            };

            foreach (string item in step.Items)
            {
                string[] parts = item.Split('|');

                if (parts.Length != 2)
                    continue;

                string word = parts[0];

                Button card =
                    CreateSelectableCard(word);

                card.Click += (_, _) =>
                {
                    bool selected =
                        !_session5VowelAnswers
                            .TryGetValue(
                                word,
                                out bool current) ||
                        !current;

                    _session5VowelAnswers[word] =
                        selected;
                    ApplySelectedCard(
                        card,
                        selected);

                    bool startsWithVowel =
                        bool.Parse(parts[1]);

                        bool correctSelection =
                            selected == startsWithVowel;

                        ShowAnswerResult(
                            correctSelection,
                            selected
                                ? "Tama! Nagsisimula sa patinig."
                                : "Tama! Hindi ito nagsisimula sa patinig.",
                            selected
                                ? "Hindi ito nagsisimula sa patinig."
                                : "Dapat piliin ang salitang nagsisimula sa patinig.");
                    };

                bool savedSelected =
                    _session5VowelAnswers
                        .TryGetValue(
                            word,
                            out bool saved) &&
                    saved;

                ApplySelectedCard(
                    card,
                    savedSelected);

                wrap.Children.Add(card);
            }

            ChoiceItemsPanel.Children.Add(wrap);
        }

        // =========================================================
        // SESSION 5 - CONSONANTS
        // =========================================================

        private void ShowConsonantActivity(
            LessonStep step)
        {
            PrepareActivityHost(step);

            WrapPanel wrap = new()
            {
                HorizontalAlignment =
                    HorizontalAlignment.Center
            };

            foreach (string item in step.Items)
            {
                string[] parts = item.Split('|');

                if (parts.Length != 3)
                    continue;

                string word = parts[0];
                bool consonant =
                    bool.Parse(parts[2]);

                Border card = CreateWhiteCard();

                card.Width = 230;
                card.Margin = new Thickness(8);

                StackPanel panel = new();

                TextBlock wordText = new()
                {
                    Text = word,
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = NavyBrush(),
                    TextAlignment = TextAlignment.Center,

                    Margin =
                        new Thickness(0, 0, 0, 12)
                };

                panel.Children.Add(wordText);

                if (!consonant)
                {
                    panel.Children.Add(
                        new TextBlock
                        {
                            Text =
                                "Nagsisimula sa patinig",

                            FontSize = 13,

                            Foreground =
                                MutedBrush(),

                            TextAlignment =
                                TextAlignment.Center
                        });
                }
                else
                {
                    TextBox answer =
                        CreateSingleLetterBox();

                    if (_session5ConsonantAnswers
                        .TryGetValue(
                            word,
                            out string? saved))
                    {
                        answer.Text = saved;
                    }

                    answer.TextChanged += (_, _) =>
                    {
                        string response =
                            answer.Text.Trim().ToUpperInvariant();

                        _session5ConsonantAnswers[word] =
                            response;

                        if (response.Length > 0)
                        {
                            bool correct =
                                response == parts[1].Trim()
                                    .ToUpperInvariant();

                            ShowAnswerResult(
                                correct,
                                "Tama! Iyan ang simulang katinig.",
                                "Subukan ulit. Tingnan ang unang titik ng salita.");
                        }
                    };

                    panel.Children.Add(answer);
                }

                card.Child = panel;
                wrap.Children.Add(card);
            }

            ChoiceItemsPanel.Children.Add(wrap);
        }

        // =========================================================
        // SESSION 5 - MISSING LETTER
        // =========================================================

        private void ShowMissingLetterActivity(
            LessonStep step)
        {
            PrepareActivityHost(step);

            WrapPanel wrap = new()
            {
                HorizontalAlignment =
                    HorizontalAlignment.Center
            };

            foreach (string item in step.Items)
            {
                string[] parts = item.Split('|');

                if (parts.Length != 3)
                    continue;

                string icon = parts[0];
                string incompleteWord = parts[1];

                Border card = CreateWhiteCard();

                card.Width = 180;
                card.Margin = new Thickness(8);

                StackPanel panel = new();

                panel.Children.Add(
                    new TextBlock
                    {
                        Text = icon,
                        FontSize = 42,

                        HorizontalAlignment =
                            HorizontalAlignment.Center
                    });

                panel.Children.Add(
                    new TextBlock
                    {
                        Text = incompleteWord,
                        FontSize = 22,
                        FontWeight = FontWeights.Bold,
                        Foreground = NavyBrush(),

                        HorizontalAlignment =
                            HorizontalAlignment.Center,

                        Margin =
                            new Thickness(0, 8, 0, 10)
                    });

                TextBox answer =
                    CreateSingleLetterBox();

                if (_session5MissingAnswers
                    .TryGetValue(
                        incompleteWord,
                        out string? saved))
                {
                    answer.Text = saved;
                }

                answer.TextChanged += (_, _) =>
                {
                    _session5MissingAnswers[
                        incompleteWord] =
                        answer.Text
                            .Trim()
                            .ToUpperInvariant();

                    string correctLetter =
                        parts[2].Trim().ToUpperInvariant();

                    if (answer.Text.Trim().Length > 0)
                    {
                        bool correct =
                            answer.Text.Trim().Equals(
                                correctLetter,
                                StringComparison.OrdinalIgnoreCase);

                        ShowAnswerResult(
                            correct,
                            "Tama! Kumpleto na ang salita.",
                            "Hindi iyan ang nawawalang titik. Subukan ulit.");
                    }
                };

                panel.Children.Add(answer);
                card.Child = panel;

                wrap.Children.Add(card);
            }

            ChoiceItemsPanel.Children.Add(wrap);
        }

        // =========================================================
        // GENERIC LETTER WRITING
        // SESSION 6 = M/m
        // SESSION 7 = S/s
        // Future letter sessions can reuse this too.
        // =========================================================

        private void ShowLetterWritingActivity(
            LessonStep step)
        {
            PrepareActivityHost(step);

            string uppercase =
                step.Items.Count > 0
                    ? step.Items[0].Trim()
                    : "";

            string lowercase =
                step.Items.Count > 1
                    ? step.Items[1].Trim()
                    : uppercase.ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(uppercase))
            {
                ShowSessionValidation(
                    "Waray letter data para hini nga activity.");

                return;
            }

            TextBlock sample = new()
            {
                Text =
                    $"{uppercase}     {uppercase}     {uppercase}" +
                    $"        " +
                    $"{lowercase}     {lowercase}     {lowercase}",

                Width = 860,
                FontSize = 48,
                FontWeight = FontWeights.Bold,
                Foreground = NavyBrush(),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                Margin =
                    new Thickness(0, 5, 0, 20)
            };

            ChoiceItemsPanel.Children.Add(sample);

            Border writingCard =
                CreateWhiteCard();

            writingCard.Width = 700;
            writingCard.Margin =
                new Thickness(8, 0, 8, 8);

            StackPanel content = new()
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };

            content.Children.Add(
                new TextBlock
                {
                    Text =
                        $"Gamitin ang daliri para isulat ang " +
                        $"malaking {uppercase} at maliit na {lowercase}.",

                    MaxWidth = 620,
                    FontSize = 20,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = NavyBrush(),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,

                    Margin =
                        new Thickness(0, 0, 0, 18)
                });

            Grid writingGrid = new()
            {
                Width = 620,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            writingGrid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });

            writingGrid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });

            writingGrid.ColumnDefinitions.Add(
                new ColumnDefinition());

            writingGrid.ColumnDefinitions.Add(
                new ColumnDefinition());

            TextBlock uppercaseLabel = new()
            {
                Text = $"MALAKING {uppercase}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = BlueBrush(),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(8, 0, 8, 4)
            };

            TextBlock lowercaseLabel = new()
            {
                Text = $"MALIIT NA {lowercase}",
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = BlueBrush(),
                TextAlignment = TextAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(8, 0, 8, 4)
            };

            InkCanvas uppercaseBox =
                CreateLetterWritingCanvas(
                    uppercase);

            InkCanvas lowercaseBox =
                CreateLetterWritingCanvas(
                    lowercase);

            if (!_uppercaseWritingStrokes.TryGetValue(
                    step.StepNumber,
                    out StrokeCollection? uppercaseStrokes))
            {
                uppercaseStrokes = new StrokeCollection();
                _uppercaseWritingStrokes[step.StepNumber] =
                    uppercaseStrokes;
            }

            if (!_lowercaseWritingStrokes.TryGetValue(
                    step.StepNumber,
                    out StrokeCollection? lowercaseStrokes))
            {
                lowercaseStrokes = new StrokeCollection();
                _lowercaseWritingStrokes[step.StepNumber] =
                    lowercaseStrokes;
            }

            uppercaseBox.Strokes =
                new StrokeCollection(uppercaseStrokes);

            lowercaseBox.Strokes =
                new StrokeCollection(lowercaseStrokes);

            uppercaseBox.Strokes.StrokesChanged += (_, _) =>
            {
                _uppercaseWritingStrokes[step.StepNumber] =
                    new StrokeCollection(uppercaseBox.Strokes);
                ShowActivityFeedback(
                    $"Naitala ang sulat mo sa malaking {uppercase}.",
                    true);
            };

            lowercaseBox.Strokes.StrokesChanged += (_, _) =>
            {
                _lowercaseWritingStrokes[step.StepNumber] =
                    new StrokeCollection(lowercaseBox.Strokes);
                ShowActivityFeedback(
                    $"Naitala ang sulat mo sa maliit na {lowercase}.",
                    true);
            };

            Grid.SetRow(uppercaseLabel, 0);
            Grid.SetColumn(uppercaseLabel, 0);
            Grid.SetRow(lowercaseLabel, 0);
            Grid.SetColumn(lowercaseLabel, 1);

            Grid.SetRow(uppercaseBox, 1);
            Grid.SetColumn(uppercaseBox, 0);
            Grid.SetRow(lowercaseBox, 1);
            Grid.SetColumn(lowercaseBox, 1);

            Border uppercaseCanvasBorder =
                CreateLetterWritingCanvasBorder(
                    uppercaseBox);

            Border lowercaseCanvasBorder =
                CreateLetterWritingCanvasBorder(
                    lowercaseBox);

            writingGrid.Children.Add(uppercaseLabel);
            writingGrid.Children.Add(lowercaseLabel);
            writingGrid.Children.Add(uppercaseCanvasBorder);
            writingGrid.Children.Add(lowercaseCanvasBorder);

            content.Children.Add(writingGrid);

            bool completed =
                IsLetterWritingCompleted(step.StepNumber);

            Button doneButton = new()
            {
                Content =
                    completed
                        ? "✓  Tapos na ang pagsasanay"
                        : "✓  Tapos na akong magsanay",

                Height = 48,
                MinWidth = 240,

                Margin =
                    new Thickness(0, 20, 0, 0),

                Padding =
                    new Thickness(18, 0, 18, 0),

                Background =
                    completed
                        ? SuccessBackground()
                        : BlueBrush(),

                Foreground =
                    completed
                        ? SuccessForeground()
                        : Brushes.White,

                BorderThickness =
                    new Thickness(0),

                FontSize = 15,
                FontWeight = FontWeights.SemiBold,

                HorizontalAlignment =
                    HorizontalAlignment.Center,

                Cursor = Cursors.Hand
            };

            doneButton.Click += (_, _) =>
            {
                bool hasUppercase =
                    uppercaseBox.Strokes.Count > 0;

                bool hasLowercase =
                    lowercaseBox.Strokes.Count > 0;

                if (!hasUppercase || !hasLowercase)
                {
                    ShowSessionValidation(
                        $"Isulat muna gamit ang daliri ang malaking " +
                        $"{uppercase} at maliit na {lowercase}.");

                    return;
                }

                _letterWritingCompletedSteps.Add(
                    step.StepNumber);

                doneButton.Content =
                    "✓  Tapos na ang pagsasanay";

                doneButton.Background =
                    SuccessBackground();

                doneButton.Foreground =
                    SuccessForeground();

                HideActivityValidation();
            };

            content.Children.Add(doneButton);

            writingCard.Child = content;

            ChoiceItemsPanel.Children.Add(
                writingCard);
        }

        private InkCanvas CreateLetterWritingCanvas(
            string sample)
        {
            InkCanvas writingCanvas = new()
            {
                Width = 250,
                Height = 190,

                Margin =
                    new Thickness(10, 4, 10, 8),

                Background = Brushes.Transparent,

                EditingMode = InkCanvasEditingMode.Ink,
                UseLayoutRounding = true,
                Cursor = Cursors.Pen,
                ToolTip =
                    $"Isulat gamit ang daliri ang titik {sample}"
            };

            writingCanvas.DefaultDrawingAttributes.Width = 5;
            writingCanvas.DefaultDrawingAttributes.Height = 5;
            writingCanvas.DefaultDrawingAttributes.Color =
                Color.FromRgb(24, 49, 73);

            return writingCanvas;
        }

        private Border CreateLetterWritingCanvasBorder(
            InkCanvas writingCanvas)
        {
            return new Border
            {
                Width = 270,
                Height = 215,
                Margin = new Thickness(10, 4, 10, 8),
                Background =
                    new SolidColorBrush(
                        Color.FromRgb(250, 248, 243)),
                BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(205, 198, 187)),
                BorderThickness = new Thickness(2),
                Child = writingCanvas
            };
        }

        private bool IsLetterWritingCompleted(
            int stepNumber)
        {
            return _letterWritingCompletedSteps.Contains(
                stepNumber);
        }

        // =========================================================
        // SESSION 6 - LETTER COLORING
        // =========================================================

        private void ShowLetterColoringActivity(
            LessonStep step)
        {
            PrepareActivityHost(step);

            Border legend = CreateWhiteCard();

            legend.Margin =
                new Thickness(8, 0, 8, 18);

            StackPanel legendPanel = new()
            {
                HorizontalAlignment =
                    HorizontalAlignment.Center
            };

            legendPanel.Children.Add(new TextBlock
            {
                Text = "Pumili ng kulay, pagkatapos pindutin ang titik.",
                FontSize = 16,
                FontWeight = FontWeights.SemiBold,
                Foreground = NavyBrush(),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(8)
            });

            StackPanel palette = new()
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            string selectedColor =
                _session6ColorAnswers.TryGetValue(-1, out string? savedColor)
                    ? savedColor
                    : "yellow";

            Button CreateColorButton(string color, string label, Brush brush)
            {
                Button colorButton = new()
                {
                    Content = label,
                    MinWidth = 112,
                    MinHeight = 48,
                    Margin = new Thickness(6),
                    Padding = new Thickness(12, 6, 12, 6),
                    Background = brush,
                    Foreground = NavyBrush(),
                    BorderBrush = SuccessForeground(),
                    BorderThickness = new Thickness(
                        selectedColor == color ? 3 : 1),
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Cursor = Cursors.Hand
                };

                colorButton.Click += (_, _) =>
                {
                    selectedColor = color;
                    _session6ColorAnswers[-1] = color;

                    foreach (Button button in palette.Children
                                 .OfType<Button>())
                    {
                        button.BorderThickness = new Thickness(1);
                    }

                    colorButton.BorderThickness = new Thickness(3);
                    ShowActivityFeedback(
                        $"Napili mo ang kulay na {label}. Pindutin ang titik para kulayan ito.",
                        null);
                };

                return colorButton;
            }

            palette.Children.Add(
                CreateColorButton(
                    "yellow",
                    "🟡 Dilaw",
                    new SolidColorBrush(Color.FromRgb(255, 221, 102))));

            palette.Children.Add(
                CreateColorButton(
                    "red",
                    "🔴 Pula",
                    new SolidColorBrush(Color.FromRgb(225, 92, 83))));

            palette.Children.Add(
                CreateColorButton(
                    "none",
                    "⚪ Walang kulay",
                    new SolidColorBrush(Color.FromRgb(245, 245, 245))));

            legendPanel.Children.Add(palette);
            legendPanel.Children.Add(new TextBlock
            {
                Text = "M = Dilaw  •  m = Pula  •  ibang titik = walang kulay",
                FontSize = 14,
                Foreground = MutedBrush(),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(8)
            });

            legend.Child = legendPanel;

            ChoiceItemsPanel.Children.Add(legend);

            UniformGrid grid = new()
            {
                Columns = 3,

                HorizontalAlignment =
                    HorizontalAlignment.Center
            };

            for (int i = 0;
                 i < step.Items.Count;
                 i++)
            {
                string item = step.Items[i];
                string[] parts = item.Split('|');
                string letter = parts[0];
                string color = parts.Length > 1
                    ? parts[1].Trim().ToLowerInvariant()
                    : "none";

                int tileIndex = i;
                Button tile = new()
                {
                    Width = 100,
                    Height = 100,
                    Margin = new Thickness(8),
                    Padding = new Thickness(0),
                    Background = Brushes.White,
                    BorderBrush =
                        new SolidColorBrush(Color.FromRgb(205, 198, 187)),
                    BorderThickness = new Thickness(2),
                    Cursor = Cursors.Hand,
                    ToolTip = "Pindutin para kulayan ang titik"
                };

                TextBlock letterText = new()
                {
                    Text = letter,
                    FontSize = 34,
                    FontWeight = FontWeights.Bold,
                    Foreground = NavyBrush(),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                tile.Content = letterText;

                void PaintTile(string answer)
                {
                    if (answer == color)
                    {
                        tile.Background =
                            color == "yellow"
                                ? new SolidColorBrush(
                                    Color.FromRgb(255, 221, 102))
                                : color == "red"
                                    ? new SolidColorBrush(
                                        Color.FromRgb(225, 92, 83))
                                    : Brushes.White;

                        tile.BorderBrush = SuccessForeground();
                        tile.BorderThickness = new Thickness(3);
                        letterText.Foreground =
                            color == "red" ? Brushes.White : NavyBrush();
                    }
                    else
                    {
                        tile.Background = ErrorBackground();
                        tile.BorderBrush = ErrorForeground();
                        tile.BorderThickness = new Thickness(3);
                        letterText.Foreground = ErrorForeground();
                    }
                }

                if (_session6ColorAnswers.TryGetValue(
                    tileIndex,
                    out string? selected))
                {
                    PaintTile(selected);
                }

                tile.Click += (_, _) =>
                {
                    _session6ColorAnswers[tileIndex] = selectedColor;
                    PaintTile(selectedColor);

                    bool correct =
                        selectedColor == color;

                    ShowAnswerResult(
                        correct,
                        "Tama ang kulay na pinili mo!",
                        "Hindi tugma ang kulay sa titik. Tingnan ang gabay at subukan ulit.");
                };

                grid.Children.Add(tile);
            }

            ChoiceItemsPanel.Children.Add(grid);
        }
    }
}
