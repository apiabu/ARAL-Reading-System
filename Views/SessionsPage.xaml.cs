using ARALReadingSystem.Models;
using ARALReadingSystem.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ARALReadingSystem.Views
{
    public partial class SessionsPage : Page
    {
        private readonly PupilService _pupilService =
            new PupilService();


        private Pupil? _selectedPupil;

        public event Action? ReturnToSessionsDashboardRequested;


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public SessionsPage()
        {
            InitializeComponent();
        }


        // =====================================================
        // PAGE LOADED
        // =====================================================

        private void Page_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            LoadSessions();
        }


        // =====================================================
        // LOAD PAGE
        // =====================================================

        private void LoadSessions()
        {
            try
            {
                /*
                 * IMPORTANT:
                 *
                 * We no longer select the pupil with the
                 * greatest progress.
                 *
                 * The entire application now uses the
                 * CurrentPupilService.
                 */
                _selectedPupil =
                    CurrentPupilService
                        .GetOrSelectCurrentPupil();


                if (_selectedPupil == null)
                {
                    ShowNoPupil();

                    return;
                }


                LoadSelectedPupil();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri ma-load an mga sesyon.\n\n" +
                    ex.Message,
                    "Mga Sesyon",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =====================================================
        // LOAD CURRENT PUPIL
        // =====================================================

        private void LoadSelectedPupil()
        {
            if (_selectedPupil == null)
            {
                ShowNoPupil();

                return;
            }


            /*
             * Refresh pupil data from SQLite so progress
             * is always current.
             */
            Pupil? refreshed =
                _pupilService.GetPupilById(
                    _selectedPupil.Id);


            if (refreshed == null)
            {
                CurrentPupilService
                    .ClearCurrentPupil();


                _selectedPupil = null;


                ShowNoPupil();

                return;
            }


            _selectedPupil =
                refreshed;


            // =================================================
            // PUPIL INFORMATION
            // =================================================

            PupilNameText.Text =
                _selectedPupil.FullName;


            PupilProgressText.Text =
                $"{_selectedPupil.LastCompletedSession} " +
                "han 36 nga sesyon";


            UpdateResumeButton();

            BuildSessionCards();
        }


        // =====================================================
        // NO PUPIL
        // =====================================================

        private void ShowNoPupil()
        {
            SessionsPanel.Children.Clear();


            SessionsPanel.Children.Add(
                CreateEmptyMessage());


            ResumeButton.Content =
                "Waray mag-aaral";


            ResumeButton.IsEnabled =
                false;


            PupilNameText.Text =
                "Waray mag-aaral";


            PupilProgressText.Text =
                "0 han 36 nga sesyon";
        }


        // =====================================================
        // UPDATE RESUME BUTTON
        // =====================================================

        private void UpdateResumeButton()
        {
            if (_selectedPupil == null)
            {
                ResumeButton.IsEnabled =
                    false;

                return;
            }


            if (_selectedPupil.LastCompletedSession >= 36)
            {
                ResumeButton.Content =
                    "✓ Natapos na an 36 nga Sesyon";


                ResumeButton.IsEnabled =
                    false;


                return;
            }


            int nextSession =
                _selectedPupil.LastCompletedSession + 1;


            ResumeButton.Content =
                $"▣  Ipagpatuloy an Sesyon {nextSession}";


            ResumeButton.IsEnabled =
                true;
        }


        // =====================================================
        // RESUME
        // =====================================================

        private void ResumeButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (_selectedPupil == null)
            {
                return;
            }


            if (_selectedPupil.LastCompletedSession >= 36)
            {
                return;
            }


            int nextSession =
                _selectedPupil.LastCompletedSession + 1;


            OpenSession(
                nextSession);
        }


        // =====================================================
        // BUILD 36 SESSION CARDS
        // =====================================================

        private void BuildSessionCards()
        {
            SessionsPanel.Children.Clear();


            if (_selectedPupil == null)
            {
                return;
            }


            for (int sessionNumber = 1;
                 sessionNumber <= 36;
                 sessionNumber++)
            {
                Border card =
                    CreateSessionCard(
                        sessionNumber);


                SessionsPanel.Children.Add(
                    card);
            }
        }


        // =====================================================
        // CREATE SESSION CARD
        // =====================================================

        private Border CreateSessionCard(
            int sessionNumber)
        {
            if (_selectedPupil == null)
            {
                return new Border();
            }


            bool completed =
                sessionNumber <=
                _selectedPupil.LastCompletedSession;


            int nextSession =
                _selectedPupil.LastCompletedSession + 1;


            bool current =
                sessionNumber == nextSession &&
                _selectedPupil.LastCompletedSession < 36;


            bool locked =
                !completed &&
                !current;


            Border card = new Border
            {
                Width = 300,

                MinHeight = 245,

                Style =
                    (Style)FindResource(
                        "SessionCardStyle")
            };


            Grid grid =
                new Grid();


            // =================================================
            // ROWS
            // =================================================

            grid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });


            grid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });


            grid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });


            grid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });


            grid.RowDefinitions.Add(
                new RowDefinition
                {
                    Height = GridLength.Auto
                });


            // =================================================
            // TOP ROW
            // =================================================

            Grid topGrid =
                new Grid();


            topGrid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width = GridLength.Auto
                });


            topGrid.ColumnDefinitions.Add(
                new ColumnDefinition
                {
                    Width =
                        new GridLength(
                            1,
                            GridUnitType.Star)
                });


            Border numberBadge =
                CreateNumberBadge(
                    sessionNumber,
                    completed,
                    current);


            topGrid.Children.Add(
                numberBadge);


            Border statusBadge =
                CreateStatusBadge(
                    completed,
                    current,
                    locked);


            Grid.SetColumn(
                statusBadge,
                1);


            topGrid.Children.Add(
                statusBadge);


            Grid.SetRow(
                topGrid,
                0);


            grid.Children.Add(
                topGrid);


            // =================================================
            // TITLE
            // =================================================

            TextBlock title =
                new TextBlock
                {
                    Text =
                        GetSessionTitle(
                            sessionNumber),

                    FontSize = 18,

                    FontWeight =
                        FontWeights.Bold,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                19,
                                43,
                                66)),

                    TextWrapping =
                        TextWrapping.Wrap,

                    Margin =
                        new Thickness(
                            0,
                            18,
                            0,
                            0)
                };


            Grid.SetRow(
                title,
                1);


            grid.Children.Add(
                title);


            // =================================================
            // DESCRIPTION
            // =================================================

            TextBlock description =
                new TextBlock
                {
                    Text =
                        GetSessionDescription(
                            sessionNumber),

                    FontSize = 13,

                    Foreground =
                        new SolidColorBrush(
                            Color.FromRgb(
                                102,
                                120,
                                138)),

                    TextWrapping =
                        TextWrapping.Wrap,

                    Margin =
                        new Thickness(
                            0,
                            7,
                            0,
                            14)
                };


            Grid.SetRow(
                description,
                2);


            grid.Children.Add(
                description);


            // =================================================
            // ACTION BUTTON
            // =================================================

            Button actionButton =
                new Button
                {
                    Tag = sessionNumber,

                    Style =
                        (Style)FindResource(
                            "SessionActionButtonStyle")
                };


            if (completed)
            {
                actionButton.Content =
                    "▷  Ulitin";


                actionButton.Click +=
                    SessionButton_Click;
            }
            else if (current)
            {
                actionButton.Content =
                    "▶  Simulan";


                actionButton.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            21,
                            148,
                            223));


                actionButton.Foreground =
                    Brushes.White;


                actionButton.BorderThickness =
                    new Thickness(0);


                actionButton.Click +=
                    SessionButton_Click;
            }
            else
            {
                actionButton.Content =
                    "🔒  Naka-lock";


                actionButton.IsEnabled =
                    false;


                actionButton.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            145,
                            156,
                            168));


                actionButton.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            220,
                            224,
                            228));
            }


            Grid.SetRow(
                actionButton,
                4);


            grid.Children.Add(
                actionButton);


            card.Child =
                grid;


            return card;
        }


        // =====================================================
        // NUMBER BADGE
        // =====================================================

        private Border CreateNumberBadge(
            int sessionNumber,
            bool completed,
            bool current)
        {
            Color backgroundColor;

            Color foregroundColor;


            if (completed)
            {
                backgroundColor =
                    Color.FromRgb(
                        224,
                        246,
                        234);


                foregroundColor =
                    Color.FromRgb(
                        17,
                        137,
                        72);
            }
            else if (current)
            {
                backgroundColor =
                    Color.FromRgb(
                        221,
                        241,
                        253);


                foregroundColor =
                    Color.FromRgb(
                        0,
                        111,
                        183);
            }
            else
            {
                backgroundColor =
                    Color.FromRgb(
                        241,
                        243,
                        245);


                foregroundColor =
                    Color.FromRgb(
                        120,
                        130,
                        140);
            }


            return new Border
            {
                Width = 48,

                Height = 48,

                CornerRadius =
                    new CornerRadius(15),

                Background =
                    new SolidColorBrush(
                        backgroundColor),

                Child =
                    new TextBlock
                    {
                        Text =
                            sessionNumber.ToString(),

                        FontSize = 17,

                        FontWeight =
                            FontWeights.Bold,

                        Foreground =
                            new SolidColorBrush(
                                foregroundColor),

                        HorizontalAlignment =
                            HorizontalAlignment.Center,

                        VerticalAlignment =
                            VerticalAlignment.Center
                    }
            };
        }


        // =====================================================
        // STATUS BADGE
        // =====================================================

        private Border CreateStatusBadge(
            bool completed,
            bool current,
            bool locked)
        {
            string text;

            Color background;

            Color foreground;


            if (completed)
            {
                text =
                    "✓ Tapos";


                background =
                    Color.FromRgb(
                        224,
                        246,
                        234);


                foreground =
                    Color.FromRgb(
                        17,
                        137,
                        72);
            }
            else if (current)
            {
                text =
                    "KASUNOD";


                background =
                    Color.FromRgb(
                        221,
                        241,
                        253);


                foreground =
                    Color.FromRgb(
                        0,
                        111,
                        183);
            }
            else
            {
                text =
                    "🔒 Locked";


                background =
                    Color.FromRgb(
                        241,
                        243,
                        245);


                foreground =
                    Color.FromRgb(
                        126,
                        138,
                        150);
            }


            return new Border
            {
                Background =
                    new SolidColorBrush(
                        background),

                CornerRadius =
                    new CornerRadius(12),

                Padding =
                    new Thickness(
                        10,
                        4,
                        10,
                        4),

                HorizontalAlignment =
                    HorizontalAlignment.Right,

                VerticalAlignment =
                    VerticalAlignment.Top,

                Child =
                    new TextBlock
                    {
                        Text =
                            text,

                        FontSize = 11,

                        FontWeight =
                            FontWeights.SemiBold,

                        Foreground =
                            new SolidColorBrush(
                                foreground)
                    }
            };
        }


        // =====================================================
        // SESSION BUTTON
        // =====================================================

        private void SessionButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }


            if (button.Tag is not int sessionNumber)
            {
                return;
            }


            OpenSession(
                sessionNumber);
        }


        // =====================================================
        // OPEN SESSION
        // =====================================================

        private void OpenSession(
            int sessionNumber)
        {
            if (_selectedPupil == null)
            {
                return;
            }

            LessonSession lesson =
                CurriculumService.GetSession(
                    sessionNumber);

            if (lesson.IsCheckIn &&
                lesson.CheckInNumber.HasValue)
            {
                OpenCheckInSession(
                    sessionNumber,
                    lesson.CheckInNumber.Value);

                return;
            }

            /*
             * Make absolutely sure the session belongs to
             * the same current pupil.
             */
            CurrentPupilService.SetCurrentPupil(
                _selectedPupil.Id);


            InstructionalSessionPage sessionPage =
                new InstructionalSessionPage(
                    _selectedPupil.Id,
                    sessionNumber);


            sessionPage.BackRequested += () =>
            {
                ReturnFromSession();
            };


            sessionPage.SessionCompleted += () =>
            {
                ReturnFromSession();
            };


            if (NavigationService != null)
            {
                NavigationService.Navigate(
                    sessionPage);
            }
            else
            {
                MessageBox.Show(
                    "Diri maabli an sesyon.",
                    "Navigation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private void OpenCheckInSession(
            int sessionNumber,
            int checkInNumber)
        {
            if (_selectedPupil == null)
            {
                return;
            }

            if (sessionNumber >
                _selectedPupil.LastCompletedSession + 1)
            {
                MessageBox.Show(
                    $"Hindi pa maaaring buksan ang Sesyon {sessionNumber}.\n\n" +
                    $"Unahin muna ang Sesyon " +
                    $"{_selectedPupil.LastCompletedSession + 1}.",
                    "Naka-lock na Sesyon",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                return;
            }

            try
            {
                CurrentPupilService.SetCurrentPupil(
                    _selectedPupil.Id);

                CheckInEntryWindow checkInWindow =
                    new CheckInEntryWindow(
                        _selectedPupil,
                        checkInNumber)
                    {
                        Owner = Window.GetWindow(this)
                    };

                if (checkInWindow.ShowDialog() != true)
                {
                    return;
                }

                _pupilService.CompleteSession(
                    _selectedPupil.Id,
                    sessionNumber);

                LoadSelectedPupil();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Hindi nakumpleto ang Check-in session.\n\n" +
                    ex.Message,
                    "Check-in Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =====================================================
        // RETURN FROM SESSION
        // =====================================================

        private void ReturnFromSession()
        {
            LoadSessions();

            if (ReturnToSessionsDashboardRequested != null)
            {
                ReturnToSessionsDashboardRequested.Invoke();
                return;
            }

            if (NavigationService != null)
            {
                NavigationService.Navigate(
                    new SessionsPage());
                return;
            }

            MessageBox.Show(
                "Diri makabalik ha dashboard han mga sesyon.",
                "Navigation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }


        // =====================================================
        // SESSION TITLES
        // =====================================================

        private string GetSessionTitle(
            int sessionNumber)
        {
            return sessionNumber switch
            {
                1 => "Ako si...",

                2 => "Tayo ay...",

                3 => "Kanan-Kaliwa",

                4 => "Pakikinig sa Tunog",

                5 => "Alpabetong Filipino",

                6 => "Titik Mm",

                7 => "Titik Ss",

                8 => "Titik Aa",

                9 =>
                    "Pagsasanay: Titik M, S, A at Pagpapantig",

                10 =>
                    "Pagsasanay: Titik M, S, A at Salitandaan",

                11 => "Titik Ii",

                12 => "Titik Oo",

                13 => "Titik Ee",

                14 =>
                    "Pagsasanay: Titik I, O at E",

                15 =>
                    "Progress Check-in #1",

                16 => "Titik Bb",

                17 => "Titik Uu",

                18 =>
                    "Salitandaan: May, Mga",

                19 =>
                    "Pagsasanay: Titik B, U at Salitandaan",

                20 =>
                    "Titik Kk at Ll",

                21 =>
                    "Titik Tt at Nn",

                22 =>
                    "Titik Ww at Yy",

                23 =>
                    "Pagsasanay: Titik K, L, T, N, W at Y",

                24 =>
                    "Progress Check-in #2",

                25 =>
                    "Titik Gg at NGng",

                26 =>
                    "Titik Pp at Rr",

                27 =>
                    "Pagsasanay: Titik G, NG, P at R",

                28 =>
                    "Titik Dd at Hh",

                29 =>
                    "Kambal Katinig / Klaster",

                30 =>
                    "Pagsasanay: Titik D, H at Klaster",

                31 =>
                    "Progress Check-in #3",

                32 =>
                    "Titik Cc at Ff",

                33 =>
                    "Titik Ññ, Jj at Qq",

                34 =>
                    "Titik Vv, Xx at Zz",

                35 =>
                    "Pagsasanay: Titik C, F, Ñ, J, Q, V, X at Z",

                36 =>
                    "Progress Check-in #4",

                _ =>
                    $"Sesyon {sessionNumber}"
            };
        }


        // =====================================================
        // SESSION DESCRIPTIONS
        // =====================================================

        private string GetSessionDescription(
            int sessionNumber)
        {
            return sessionNumber switch
            {
                1 =>
                    "Pagkilala sa sarili",

                2 =>
                    "Mga tuntunin sa pakikinig at klase",

                3 =>
                    "Pagkilala sa kanan at kaliwa",

                4 =>
                    "Pakikinig at pagkilala sa tunog",

                5 =>
                    "28 titik ng Alpabetong Filipino",

                6 =>
                    "Tunog /m/",

                7 =>
                    "Tunog /s/",

                8 =>
                    "Tunog /a/",

                9 =>
                    "Pagbuo ng pantig gamit ang M, S at A",

                10 =>
                    "Pagbuo ng salita at paggamit ng salitandaan",

                11 =>
                    "Titik at tunog Ii",

                12 =>
                    "Titik at tunog Oo",

                13 =>
                    "Titik at tunog Ee",

                14 =>
                    "Pagsasanay sa I, O at E",

                15 =>
                    "Unang pagtataya ng progreso",

                16 =>
                    "Titik at tunog Bb",

                17 =>
                    "Titik at tunog Uu",

                18 =>
                    "Paggamit ng may at mga",

                19 =>
                    "Pagsasanay sa B, U at salitandaan",

                20 =>
                    "Mga titik Kk at Ll",

                21 =>
                    "Mga titik Tt at Nn",

                22 =>
                    "Mga titik Ww at Yy",

                23 =>
                    "Pagsasanay sa K, L, T, N, W at Y",

                24 =>
                    "Ikalawang pagtataya ng progreso",

                25 =>
                    "Mga titik Gg at NGng",

                26 =>
                    "Mga titik Pp at Rr",

                27 =>
                    "Pagsasanay sa G, NG, P at R",

                28 =>
                    "Mga titik Dd at Hh",

                29 =>
                    "Pagkilala sa kambal-katinig / klaster",

                30 =>
                    "Pagsasanay sa D, H at klaster",

                31 =>
                    "Ikatlong pagtataya ng progreso",

                32 =>
                    "Mga titik Cc at Ff",

                33 =>
                    "Mga titik Ññ, Jj at Qq",

                34 =>
                    "Mga titik Vv, Xx at Zz",

                35 =>
                    "Pagsasanay sa natitirang mga titik",

                36 =>
                    "Panghuling pagtataya ng progreso",

                _ =>
                    "ARAL Reading Session"
            };
        }


        // =====================================================
        // EMPTY STATE
        // =====================================================

        private Border CreateEmptyMessage()
        {
            return new Border
            {
                Width = 500,

                Background =
                    Brushes.White,

                CornerRadius =
                    new CornerRadius(20),

                Padding =
                    new Thickness(30),

                Child =
                    new TextBlock
                    {
                        Text =
                            "Waray pa nakarehistro nga mag-aaral.\n" +
                            "Pagdugang anay hin mag-aaral.",

                        FontSize = 15,

                        Foreground =
                            new SolidColorBrush(
                                Color.FromRgb(
                                    90,
                                    105,
                                    120)),

                        TextWrapping =
                            TextWrapping.Wrap
                    }
            };
        }
    }
}