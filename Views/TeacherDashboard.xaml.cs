using ARALReadingSystem.Models;
using ARALReadingSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace ARALReadingSystem.Views
{
    public partial class TeacherDashboard : Window
    {
        private readonly string _teacherName;

        private readonly PupilService _pupilService =
            new PupilService();

        private List<Pupil> _pupils =
            new List<Pupil>();

        private Pupil? _resumePupil;

        private static int? _currentPupilId;
        private bool _isInstructionalSessionLayout;

        public TeacherDashboard(string teacherName)
        {
            InitializeComponent();

            _teacherName = teacherName;

            TeacherNameText.Text =
                string.IsNullOrWhiteSpace(_teacherName)
                    ? "ARAL Teacher"
                    : _teacherName;

            ShowDashboardHeader();

            LoadDashboard();
        }

        // =========================================================
        // SHARED HEADER
        // =========================================================

        private void UpdateGreeting()
        {
            string name =
                _teacherName?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(name))
            {
                SharedPageTitleText.Text =
                    "Maupay, Titser!";

                return;
            }

            string[] parts =
                name.Split(
                    ' ',
                    StringSplitOptions.RemoveEmptyEntries);

            string lastName =
                parts.Length > 0
                    ? parts[^1]
                    : name;

            SharedPageTitleText.Text =
                $"Maupay, {lastName}!";
        }

        private void ShowDashboardHeader()
        {
            UpdateGreeting();

            SharedPageSubtitleText.Text =
                "Gapas-gapas Central Elementary School";

            SharedAddPupilButton.Visibility =
                Visibility.Collapsed;
        }

        private void ShowPupilsHeader()
        {
            SharedPageTitleText.Text =
                "Mga Mag-aaral";

            SharedPageSubtitleText.Text =
                "Itala ang mag-aaral at ilagay sa tamang ARAL cohort.";

            SharedAddPupilButton.Visibility =
                Visibility.Visible;
        }

        private void ShowSessionsHeader()
        {
            SharedPageTitleText.Text =
                "Mga Sesyon";

            SharedPageSubtitleText.Text =
                "36 na gabay na sesyon para sa ARAL Reading Intervention.";

            SharedAddPupilButton.Visibility =
                Visibility.Collapsed;
        }

        private void ShowCheckInsHeader()
        {
            SharedPageTitleText.Text =
                "Progress Check-in";

            SharedPageSubtitleText.Text =
                "Digitized na ARAL checklist — binary na score (1 tama, 0 mali) para sa Titik, Pantig, Salita at Parirala.";

            SharedAddPupilButton.Visibility =
                Visibility.Collapsed;
        }

        // =========================================================
        // SHARED HEADER - ADD PUPIL
        // =========================================================

        private void SharedAddPupilButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                AddPupilWindow addWindow =
                    new AddPupilWindow
                    {
                        Owner = this
                    };

                bool? result =
                    addWindow.ShowDialog();

                if (result == true &&
                    addWindow.SavedPupilId.HasValue)
                {
                    _currentPupilId =
                        addWindow.SavedPupilId.Value;

                    CurrentPupilService.SetCurrentPupil(
                        _currentPupilId.Value);

                    LoadDashboard();

                    OpenPupilsPage();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri maabli an Add Pupil window.\n\n" +
                    ex.Message,
                    "Add Pupil",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // LOAD DASHBOARD
        // =========================================================

        private void LoadDashboard()
        {
            try
            {
                _pupils =
                    _pupilService.GetAllPupils();

                int pupilCount =
                    _pupils.Count;

                // -------------------------------------------------
                // COHORT COUNTS
                // -------------------------------------------------

                int grade1Count =
                    _pupils.Count(
                        p =>
                            string.Equals(
                                p.GradeLevel?.Trim(),
                                "Grade 1",
                                StringComparison.OrdinalIgnoreCase));

                int grade2Count =
                    _pupils.Count(
                        p =>
                            string.Equals(
                                p.GradeLevel?.Trim(),
                                "Grade 2",
                                StringComparison.OrdinalIgnoreCase));

                PupilCountText.Text =
                    pupilCount.ToString();

                Grade1CountText.Text =
                    grade1Count.ToString();

                Grade2CountText.Text =
                    grade2Count.ToString();

                CohortSummaryText.Text =
                    $"{pupilCount} mag-aaral ang nakatala " +
                    "sa lokal na database.";

                // -------------------------------------------------
                // NO PUPILS
                // -------------------------------------------------

                if (pupilCount == 0)
                {
                    _resumePupil = null;
                    _currentPupilId = null;

                    CurrentSessionText.Text =
                        "Waray pa nakarehistro nga mag-aaral.";

                    CurrentSessionTitleText.Text =
                        "Wala pang mag-aaral";

                    CurrentSessionSubtitleText.Text =
                        "Magdagdag muna ng mag-aaral upang magsimula.";

                    ResumeButtonText.Text =
                        "Magdagdag ng mag-aaral";

                    ResumeSessionButton.IsEnabled =
                        true;

                    SessionProgressBar.Minimum = 0;
                    SessionProgressBar.Maximum = 36;
                    SessionProgressBar.Value = 0;

                    SessionProgressText.Text =
                        "0 / 36";

                    CompletedSessionText.Text =
                        "0";

                    UpdateCheckIns(0);

                    return;
                }

                // -------------------------------------------------
                // RESTORE CURRENT PUPIL
                // -------------------------------------------------

                _currentPupilId =
                    CurrentPupilService.GetCurrentPupilId()
                    ?? _currentPupilId;

                _resumePupil = null;

                if (_currentPupilId.HasValue)
                {
                    _resumePupil =
                        _pupils.FirstOrDefault(
                            pupil =>
                                pupil.Id ==
                                _currentPupilId.Value);
                }

                if (_resumePupil == null)
                {
                    _resumePupil =
                        _pupils
                            .OrderBy(
                                pupil => pupil.Id)
                            .FirstOrDefault();

                    if (_resumePupil != null)
                    {
                        _currentPupilId =
                            _resumePupil.Id;
                    }
                }

                if (_resumePupil == null)
                {
                    CurrentSessionTitleText.Text =
                        "Walang napiling mag-aaral";

                    CurrentSessionSubtitleText.Text =
                        "Pumili muna ng mag-aaral.";

                    ResumeButtonText.Text =
                        "Pumili ng mag-aaral";

                    ResumeSessionButton.IsEnabled =
                        false;

                    SessionProgressBar.Value = 0;

                    SessionProgressText.Text =
                        "0 / 36";

                    CompletedSessionText.Text =
                        "0";

                    UpdateCheckIns(0);

                    return;
                }

                int lastCompleted =
                    _resumePupil.LastCompletedSession;

                // -------------------------------------------------
                // ALL 36 FINISHED
                // -------------------------------------------------

                if (lastCompleted >= 36)
                {
                    CurrentSessionText.Text =
                        $"{_resumePupil.FullName} • " +
                        "Natapos na an 36 nga sesyon.";

                    CurrentSessionTitleText.Text =
                        "Natapos na ang 36 na sesyon";

                    CurrentSessionSubtitleText.Text =
                        _resumePupil.FullName;

                    ResumeButtonText.Text =
                        "Tapos na";

                    ResumeSessionButton.IsEnabled =
                        false;

                    SessionProgressBar.Minimum = 0;
                    SessionProgressBar.Maximum = 36;
                    SessionProgressBar.Value = 36;

                    SessionProgressText.Text =
                        "36 / 36";

                    CompletedSessionText.Text =
                        "36";

                    UpdateCheckIns(36);

                    return;
                }

                // -------------------------------------------------
                // NEXT SESSION
                // -------------------------------------------------

                int nextSession =
                    lastCompleted + 1;

                if (nextSession < 1)
                    nextSession = 1;

                if (nextSession > 36)
                    nextSession = 36;

                CurrentSessionText.Text =
                    $"{_resumePupil.FullName} • " +
                    $"Sunod nga sesyon: Sesyon {nextSession}";

                try
                {
                    LessonSession? lesson =
                        CurriculumService.GetSession(
                            nextSession);

                    if (lesson != null)
                    {
                        CurrentSessionTitleText.Text =
                            lesson.DisplayTitle;
                    }
                    else
                    {
                        CurrentSessionTitleText.Text =
                            $"Sesyon {nextSession}";
                    }
                }
                catch
                {
                    CurrentSessionTitleText.Text =
                        $"Sesyon {nextSession}";
                }

                CurrentSessionSubtitleText.Text =
                    _resumePupil.FullName;

                ResumeButtonText.Text =
                    "Ipagpatuloy ang sesyon";

                ResumeSessionButton.IsEnabled =
                    true;

                SessionProgressBar.Minimum = 0;
                SessionProgressBar.Maximum = 36;

                SessionProgressBar.Value =
                    lastCompleted;

                SessionProgressText.Text =
                    $"{lastCompleted} / 36";

                CompletedSessionText.Text =
                    lastCompleted.ToString();

                UpdateCheckIns(
                    lastCompleted);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri ma-load an dashboard.\n\n" +
                    ex.Message,
                    "Dashboard Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // CURRENT PUPIL
        // =========================================================

        private void SetCurrentPupil(
            int pupilId)
        {
            Pupil? pupil =
                _pupilService.GetPupilById(
                    pupilId);

            if (pupil == null)
                return;

            _currentPupilId =
                pupil.Id;

            CurrentPupilService.SetCurrentPupil(
                pupil.Id);

            _resumePupil =
                pupil;

            LoadDashboard();
        }

        // =========================================================
        // CHECK-INS
        //
        // #1 after Session 9
        // #2 after Session 18
        // #3 after Session 27
        // #4 after Session 36
        // =========================================================

        private void UpdateCheckIns(
            int lastCompletedSession)
        {
            int availableCheckIns = 0;

            UpdateCheckInCard(
                CheckIn1Button,
                CheckIn1StatusIcon,
                CheckIn1ActionText,
                lastCompletedSession,
                9,
                ref availableCheckIns);

            UpdateCheckInCard(
                CheckIn2Button,
                CheckIn2StatusIcon,
                CheckIn2ActionText,
                lastCompletedSession,
                18,
                ref availableCheckIns);

            UpdateCheckInCard(
                CheckIn3Button,
                CheckIn3StatusIcon,
                CheckIn3ActionText,
                lastCompletedSession,
                27,
                ref availableCheckIns);

            UpdateCheckInCard(
                CheckIn4Button,
                CheckIn4StatusIcon,
                CheckIn4ActionText,
                lastCompletedSession,
                36,
                ref availableCheckIns);

            AvailableCheckInsText.Text =
                availableCheckIns.ToString();
        }

        private void UpdateCheckInCard(
            Button button,
            TextBlock statusIcon,
            TextBlock actionText,
            int lastCompletedSession,
            int milestone,
            ref int availableCount)
        {
            bool available =
                lastCompletedSession >= milestone;

            button.IsEnabled =
                available;

            if (available)
            {
                statusIcon.Text =
                    "✓";

                statusIcon.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            30,
                            194,
                            116));

                actionText.Text =
                    "Magtala ng score  ›";

                actionText.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            0,
                            103,
                            178));

                availableCount++;
            }
            else
            {
                statusIcon.Text =
                    "▢";

                statusIcon.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            155,
                            176,
                            191));

                actionText.Text =
                    $"Bubukas kapag tapos ang Sesyon {milestone}";

                actionText.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            158,
                            173,
                            186));
            }
        }

        // =========================================================
        // DASHBOARD
        // =========================================================

        private void DashboardButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ShowDashboardHome();
        }

        // =========================================================
        // PUPILS
        // =========================================================

        private void PupilsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenPupilsPage();
        }

        private void OpenPupilsPage()
        {
            try
            {
                PupilsPage pupilsPage =
                    new PupilsPage();

                ShowPupilsHeader();

                Panel.SetZIndex(
                    PageContent,
                    100);

                PageContent.Visibility =
                    Visibility.Visible;

                PageContent.Navigate(
                    pupilsPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri maabli an listahan han mga mag-aaral.\n\n" +
                    ex.Message,
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // SESSIONS
        // =========================================================

        private void SessionsButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenSessionsPage();
        }

        private void OpenSessionsPage()
        {
            try
            {
                SessionsPage sessionsPage =
                    new SessionsPage();

                sessionsPage.ReturnToSessionsDashboardRequested +=
                    OpenSessionsPage;

                ShowSessionsHeader();

                Panel.SetZIndex(
                    PageContent,
                    100);

                PageContent.Visibility =
                    Visibility.Visible;

                PageContent.Navigate(
                    sessionsPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri maabli an mga sesyon.\n\n" +
                    ex.Message,
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void PageContent_Navigated(
            object sender,
            NavigationEventArgs e)
        {
            SetInstructionalSessionLayout(
                e.Content is InstructionalSessionPage);
        }

        private void SetInstructionalSessionLayout(
            bool showInstructionalSession)
        {
            if (_isInstructionalSessionLayout ==
                showInstructionalSession)
            {
                return;
            }

            _isInstructionalSessionLayout =
                showInstructionalSession;

            Sidebar.Visibility =
                showInstructionalSession
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            SidebarColumn.Width =
                showInstructionalSession
                    ? new GridLength(0)
                    : new GridLength(252);

            Grid.SetColumn(
                MainArea,
                showInstructionalSession ? 0 : 1);

            Grid.SetColumnSpan(
                MainArea,
                showInstructionalSession ? 2 : 1);

            SharedHeader.Visibility =
                showInstructionalSession
                    ? Visibility.Collapsed
                    : Visibility.Visible;

            Grid.SetRow(
                PageContent,
                showInstructionalSession ? 0 : 1);

            Grid.SetRowSpan(
                PageContent,
                showInstructionalSession ? 2 : 1);
        }

        // =========================================================
// CHECK-IN PAGE
// =========================================================

private void CheckInsButton_Click(
    object sender,
    RoutedEventArgs e)
{
    OpenProgressCheckInPage();
}

private void OpenProgressCheckInPage()
{
    try
    {
        ProgressCheckInPage checkInPage =
            new ProgressCheckInPage();

        ShowCheckInsHeader();

        Panel.SetZIndex(
            PageContent,
            100);

        PageContent.Visibility =
            Visibility.Visible;

        PageContent.Navigate(
            checkInPage);
    }
    catch (Exception ex)
    {
        MessageBox.Show(
            "Diri maabli an Progress Check-in.\n\n" +
            ex.Message,
            "Navigation Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}

        // =========================================================
        // RESUME SESSION
        // =========================================================

        private void ResumeSessionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                if (_currentPupilId.HasValue)
                {
                    _resumePupil =
                        _pupilService.GetPupilById(
                            _currentPupilId.Value);
                }

                if (_resumePupil == null)
                {
                    LoadDashboard();
                }

                if (_resumePupil == null)
                {
                    OpenPupilsPage();
                    return;
                }

                if (_resumePupil.LastCompletedSession >= 36)
                {
                    MessageBox.Show(
                        $"{_resumePupil.FullName} " +
                        "natapos na an ngatanan nga 36 nga sesyon.",
                        "ARAL Reading",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    LoadDashboard();

                    return;
                }

                int sessionNumber =
                    _resumePupil.LastCompletedSession + 1;

                if (sessionNumber < 1)
                    sessionNumber = 1;

                if (sessionNumber > 36)
                    sessionNumber = 36;

                InstructionalSessionPage sessionPage =
                    new InstructionalSessionPage(
                        _resumePupil.Id,
                        sessionNumber);

                sessionPage.BackRequested += () =>
                {
                    ShowDashboardHome();
                };

                sessionPage.SessionCompleted += () =>
                {
                    LoadDashboard();
                    ShowDashboardHome();
                };

                Panel.SetZIndex(
                    PageContent,
                    100);

                PageContent.Visibility =
                    Visibility.Visible;

                PageContent.Navigate(
                    sessionPage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri maabli an instructional session.\n\n" +
                    ex.Message,
                    "Session Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // CHECK-IN CARD CLICKS
        // =========================================================

        private void CheckIn1Button_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenProgressCheckInPage();
        }

        private void CheckIn2Button_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenProgressCheckInPage();
        }

        private void CheckIn3Button_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenProgressCheckInPage();
        }

        private void CheckIn4Button_Click(
            object sender,
            RoutedEventArgs e)
        {
            OpenProgressCheckInPage();
        }

        // =========================================================
        // OPEN PUPIL PROFILE
        // =========================================================

        // =========================================================
        // OPEN PUPIL PROFILE
        // =========================================================

        private void OpenPupilProfile(
            int pupilId)
        {
            try
            {
                _currentPupilId =
                    pupilId;

                CurrentPupilService.SetCurrentPupil(
                    pupilId);

                _resumePupil =
                    _pupilService.GetPupilById(
                        pupilId);

                PupilProfilePage profilePage =
                    new PupilProfilePage(
                        pupilId);

                profilePage.BackRequested += () =>
                {
                    if (PageContent.CanGoBack)
                    {
                        PageContent.GoBack();
                    }
                    else
                    {
                        OpenPupilsPage();
                    }

                    LoadDashboard();
                };

                Panel.SetZIndex(
                    PageContent,
                    100);

                PageContent.Visibility =
                    Visibility.Visible;

                PageContent.Navigate(
                    profilePage);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri maabli an pupil profile.\n\n" +
                    ex.Message,
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        // =========================================================
        // SHOW DASHBOARD HOME
        // =========================================================

        private void ShowDashboardHome()
        {
            SetInstructionalSessionLayout(false);

            PageContent.Visibility =
                Visibility.Collapsed;

            if (PageContent.NavigationService != null)
            {
                while (
                    PageContent.NavigationService.CanGoBack)
                {
                    PageContent.NavigationService
                        .RemoveBackEntry();
                }
            }

            ShowDashboardHeader();

            LoadDashboard();
        }

        // =========================================================
        // WINDOW ACTIVATED
        // =========================================================

        protected override void OnActivated(
            EventArgs e)
        {
            base.OnActivated(e);

            if (IsLoaded)
            {
                LoadDashboard();
            }
        }

        // =========================================================
        // LOGOUT
        // =========================================================

        private void LogoutButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            MessageBoxResult result =
                MessageBox.Show(
                    "Sigurado ka nga karuyag mo mag-logout?",
                    "Mag-logout",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            _currentPupilId = null;
            _resumePupil = null;

            LoginWindow loginWindow =
                new LoginWindow();

            loginWindow.Show();

            Close();
        }
    }
}