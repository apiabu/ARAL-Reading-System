using ARALReadingSystem.Models;
using ARALReadingSystem.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ARALReadingSystem.Views
{
    public partial class PupilProfilePage : Page
    {
        private readonly int _pupilId;

        private readonly PupilService _pupilService =
            new();

        private Pupil? _pupil;

        public event Action? BackRequested;

        public PupilProfilePage(int pupilId)
        {
            InitializeComponent();

            _pupilId = pupilId;

            Loaded += PupilProfilePage_Loaded;
        }

private void PupilProfilePage_Loaded(
    object sender,
    RoutedEventArgs e)
{
    LoadPupil();
}
        private void LoadPupil()
        {
            try
            {
                _pupil =
                    _pupilService.GetPupilById(_pupilId);

                if (_pupil == null)
                {
                    MessageBox.Show(
                        "Diri nakita an rekord han mag-aaral.",
                        "Pupil Profile",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    BackRequested?.Invoke();

                    return;
                }

                PupilNameText.Text =
                    _pupil.FullName;

                GradeLevelText.Text =
                    _pupil.GradeLevel;

                ProgressBar.Value =
                    _pupil.LastCompletedSession;

                ProgressNumberText.Text =
                    $"{_pupil.LastCompletedSession} / 36";

                ProgressDescriptionText.Text =
                    $"{_pupil.LastCompletedSession} han 36 nga sesyon an natapos.";

                if (_pupil.LastCompletedSession >= 36)
                {
                    ResumeButton.Content =
                        "Natapos na";

                    ResumeButton.IsEnabled =
                        false;
                }
                else
                {
                    int nextSession =
                        _pupil.LastCompletedSession + 1;

                    ResumeButton.Content =
                        $"Ipadayon an Sesyon {nextSession}";
                }

                UpdateCheckInStatus();

                BuildSessionButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri ma-load an pupil profile.\n\n" +
                    ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BuildSessionButtons()
        {
            if (_pupil == null)
                return;

            SessionsGrid.Children.Clear();

            int lastCompleted =
                _pupil.LastCompletedSession;

            int nextSession =
                Math.Min(lastCompleted + 1, 36);

            for (int sessionNumber = 1;
                 sessionNumber <= 36;
                 sessionNumber++)
            {
                Button button = new();

                button.Style =
                    (Style)FindResource("SessionButton");

                button.Tag =
                    sessionNumber;

                // Completed
                if (sessionNumber <= lastCompleted)
                {
                    button.Content =
                        $"✓\nSesyon {sessionNumber}";

                    button.Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                226,
                                245,
                                235));

                    button.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                49,
                                122,
                                79));

                    button.IsEnabled = false;
                }

                // Next/current session
                else if (sessionNumber == nextSession &&
                         lastCompleted < 36)
                {
                    button.Content =
                        $"▶\nSesyon {sessionNumber}";

                    button.Background =
                        new SolidColorBrush(
                            Color.FromRgb(
                                221,
                                241,
                                253));

                    button.Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                21,
                                148,
                                223));

                    button.Click +=
                        SessionButton_Click;
                }

                // Locked future session
                else
                {
                    button.Content =
                        $"🔒\nSesyon {sessionNumber}";

                    button.IsEnabled =
                        false;
                }

                SessionsGrid.Children.Add(
                    button);
            }
        }

        private void UpdateCheckInStatus()
        {
            if (_pupil == null)
                return;

            if (_pupil.LastCompletedSession >= 9)
            {
                CheckIn1StatusText.Text =
                    "✓ Available na an Progress Check-in #1";

                CheckIn1BadgeText.Text =
                    "AVAILABLE";

                CheckIn1Badge.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            226,
                            245,
                            235));

                CheckIn1BadgeText.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            49,
                            122,
                            79));
            }
            else
            {
                int remaining =
                    9 - _pupil.LastCompletedSession;

                CheckIn1StatusText.Text =
                    $"🔒 {remaining} pa nga sesyon antes maabli.";

                CheckIn1BadgeText.Text =
                    "LOCKED";
            }
        }

        private void SessionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
                return;

            if (button.Tag is not int sessionNumber)
                return;

            OpenSession(sessionNumber);
        }

        private void ResumeButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_pupil == null)
                return;

            if (_pupil.LastCompletedSession >= 36)
                return;

            int sessionNumber =
                _pupil.LastCompletedSession + 1;

            OpenSession(sessionNumber);
        }

     private void OpenSession(int sessionNumber)
{
    if (_pupil == null)
    {
        return;
    }


    // Only the next unfinished session can open.
    int expectedSession =
        _pupil.LastCompletedSession + 1;


    if (sessionNumber != expectedSession)
    {
        MessageBox.Show(
            $"Sunod nga available nga sesyon: " +
            $"Sesyon {expectedSession}.",
            "Session",
            MessageBoxButton.OK,
            MessageBoxImage.Information);

        return;
    }


    InstructionalSessionPage sessionPage =
        new InstructionalSessionPage(
            _pupilId,
            sessionNumber);


    // The session page handles actual GoBack().
    // We only reload the pupil when completion occurs.
    sessionPage.SessionCompleted += () =>
    {
        LoadPupil();
    };


    if (NavigationService != null)
    {
        NavigationService.Navigate(
            sessionPage);
    }
    else
    {
        MessageBox.Show(
            "Diri maabli an instructional session.",
            "Navigation Error",
            MessageBoxButton.OK,
            MessageBoxImage.Warning);
    }
}

        private void BackButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            BackRequested?.Invoke();
        }
    }
}