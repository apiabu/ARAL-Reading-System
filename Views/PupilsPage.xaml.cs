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
    public partial class PupilsPage : Page
    {
        // =====================================================
        // FIELDS
        // =====================================================

        private readonly PupilService _pupilService = new();

        private List<Pupil> _allPupils = new();

        private string _selectedGrade = "All";


        // =====================================================
        // CONSTRUCTOR
        // =====================================================

        public PupilsPage()
        {
            InitializeComponent();

            /*
             * WPF Page does not use an overridable
             * OnNavigatedTo method here.
             *
             * Loaded is enough because the list is always
             * re-read from SQLite whenever this page loads.
             */
            Loaded += PupilsPage_Loaded;
        }


        // =====================================================
        // PAGE LOADED
        // =====================================================

        private void PupilsPage_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            UpdateFilterButtons();

            RefreshPupilList();
        }


        // =====================================================
        // REFRESH PUPILS FROM SQLITE
        // =====================================================

        private void RefreshPupilList()
        {
            try
            {
                /*
                 * IMPORTANT:
                 *
                 * Never depend on the previous in-memory list.
                 * Always read the current pupil records from
                 * SQLite.
                 */
                List<Pupil> databasePupils =
                    _pupilService.GetAllPupils();


                _allPupils =
                    databasePupils ?? new List<Pupil>();


                ApplyFilters();
            }
            catch (Exception ex)
            {
                _allPupils =
                    new List<Pupil>();


                if (PupilsListView != null)
                {
                    PupilsListView.ItemsSource =
                        null;
                }


                if (PupilCountText != null)
                {
                    PupilCountText.Text =
                        "0 nga mag-aaral";
                }


                if (EmptyState != null)
                {
                    EmptyState.Visibility =
                        Visibility.Visible;
                }


                if (PupilTable != null)
                {
                    PupilTable.Visibility =
                        Visibility.Collapsed;
                }


                MessageBox.Show(
                    "Diri ma-load an mga rehistrado nga mag-aaral.\n\n" +
                    ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =====================================================
        // DISPLAY PUPILS
        // =====================================================

        private void DisplayPupils(
            List<Pupil> pupils)
        {
            if (PupilsListView == null)
            {
                return;
            }


            /*
             * Clear the old ItemsSource first so WPF
             * completely rebuilds the visible rows.
             */
            PupilsListView.ItemsSource =
                null;


            PupilsListView.ItemsSource =
                pupils.ToList();


            // =================================================
            // COUNT
            // =================================================

            if (PupilCountText != null)
            {
                PupilCountText.Text =
                    $"{pupils.Count} nga mag-aaral";
            }


            // =================================================
            // EMPTY / TABLE VISIBILITY
            // =================================================

            if (pupils.Count == 0)
            {
                if (EmptyState != null)
                {
                    EmptyState.Visibility =
                        Visibility.Visible;
                }


                if (PupilTable != null)
                {
                    PupilTable.Visibility =
                        Visibility.Collapsed;
                }
            }
            else
            {
                if (EmptyState != null)
                {
                    EmptyState.Visibility =
                        Visibility.Collapsed;
                }


                if (PupilTable != null)
                {
                    PupilTable.Visibility =
                        Visibility.Visible;
                }
            }
        }


        // =====================================================
        // APPLY SEARCH + GRADE FILTER
        // =====================================================

        private void ApplyFilters()
        {
            IEnumerable<Pupil> query =
                _allPupils;


            // =================================================
            // SEARCH
            // =================================================

            string search = "";


            if (SearchTextBox != null)
            {
                search =
                    SearchTextBox.Text?.Trim()
                    ?? "";
            }


            if (!string.IsNullOrWhiteSpace(search))
            {
                query =
                    query.Where(
                        pupil =>
                            ContainsIgnoreCase(
                                pupil.FirstName,
                                search)

                            ||

                            ContainsIgnoreCase(
                                pupil.LastName,
                                search)

                            ||

                            ContainsIgnoreCase(
                                pupil.FullName,
                                search)

                            ||

                            ContainsIgnoreCase(
                                pupil.GradeLevel,
                                search));
            }


            // =================================================
            // GRADE FILTER
            // =================================================

            if (_selectedGrade == "Grade 1")
            {
                query =
                    query.Where(
                        pupil =>
                            string.Equals(
                                pupil.GradeLevel,
                                "Grade 1",
                                StringComparison.OrdinalIgnoreCase));
            }
            else if (_selectedGrade == "Grade 2")
            {
                query =
                    query.Where(
                        pupil =>
                            string.Equals(
                                pupil.GradeLevel,
                                "Grade 2",
                                StringComparison.OrdinalIgnoreCase));
            }


            // =================================================
            // SORT
            // =================================================

            List<Pupil> result =
                query
                    .OrderBy(
                        pupil => pupil.LastName)
                    .ThenBy(
                        pupil => pupil.FirstName)
                    .ToList();


            DisplayPupils(
                result);
        }


        // =====================================================
        // CASE-INSENSITIVE SEARCH
        // =====================================================

        private static bool ContainsIgnoreCase(
            string? source,
            string search)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return false;
            }


            return source.IndexOf(
                       search,
                       StringComparison.OrdinalIgnoreCase)
                   >= 0;
        }


        // =====================================================
        // ADD PUPIL
        // =====================================================

        private void AddPupilButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                AddPupilWindow addWindow =
                    new AddPupilWindow();


                Window? owner =
                    Window.GetWindow(this);


                if (owner != null)
                {
                    addWindow.Owner =
                        owner;
                }


                bool? result =
                    addWindow.ShowDialog();


                if (result == true &&
                    addWindow.SavedPupilId.HasValue)
                {
                    /*
                     * AddPupilWindow already saved the
                     * pupil to SQLite.
                     *
                     * Read the database again immediately.
                     */
                    CurrentPupilService.SetCurrentPupil(
                        addWindow.SavedPupilId.Value);

                    RefreshPupilList();
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


        // =====================================================
        // SEARCH TEXT CHANGED
        // =====================================================

        private void SearchTextBox_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (SearchPlaceholder != null &&
                SearchTextBox != null)
            {
                SearchPlaceholder.Visibility =
                    string.IsNullOrWhiteSpace(
                        SearchTextBox.Text)
                        ? Visibility.Visible
                        : Visibility.Collapsed;
            }


            /*
             * TextChanged may fire during
             * InitializeComponent().
             */
            if (!IsLoaded)
            {
                return;
            }


            if (PupilsListView == null ||
                PupilCountText == null ||
                EmptyState == null ||
                PupilTable == null)
            {
                return;
            }


            ApplyFilters();
        }


        // =====================================================
        // ALL FILTER
        // =====================================================

        private void AllFilterButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _selectedGrade =
                "All";


            UpdateFilterButtons();

            ApplyFilters();
        }


        // =====================================================
        // GRADE 1 FILTER
        // =====================================================

        private void Grade1FilterButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _selectedGrade =
                "Grade 1";


            UpdateFilterButtons();

            ApplyFilters();
        }


        // =====================================================
        // GRADE 2 FILTER
        // =====================================================

        private void Grade2FilterButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _selectedGrade =
                "Grade 2";


            UpdateFilterButtons();

            ApplyFilters();
        }


        // =====================================================
        // UPDATE FILTER BUTTON APPEARANCE
        // =====================================================

        private void UpdateFilterButtons()
        {
            if (AllFilterButton == null ||
                Grade1FilterButton == null ||
                Grade2FilterButton == null)
            {
                return;
            }


            SolidColorBrush blue =
                new SolidColorBrush(
                    Color.FromRgb(
                        21,
                        148,
                        223));


            SolidColorBrush white =
                new SolidColorBrush(
                    Colors.White);


            SolidColorBrush normalText =
                new SolidColorBrush(
                    Color.FromRgb(
                        93,
                        111,
                        128));


            SolidColorBrush normalBorder =
                new SolidColorBrush(
                    Color.FromRgb(
                        221,
                        214,
                        203));


            ResetFilterButton(
                AllFilterButton,
                white,
                normalText,
                normalBorder);


            ResetFilterButton(
                Grade1FilterButton,
                white,
                normalText,
                normalBorder);


            ResetFilterButton(
                Grade2FilterButton,
                white,
                normalText,
                normalBorder);


            Button selectedButton;


            if (_selectedGrade == "Grade 1")
            {
                selectedButton =
                    Grade1FilterButton;
            }
            else if (_selectedGrade == "Grade 2")
            {
                selectedButton =
                    Grade2FilterButton;
            }
            else
            {
                selectedButton =
                    AllFilterButton;
            }


            selectedButton.Background =
                blue;

            selectedButton.Foreground =
                white;

            selectedButton.BorderBrush =
                blue;
        }


        // =====================================================
        // RESET FILTER BUTTON
        // =====================================================

        private static void ResetFilterButton(
            Button button,
            Brush background,
            Brush foreground,
            Brush border)
        {
            button.Background =
                background;

            button.Foreground =
                foreground;

            button.BorderBrush =
                border;
        }


        // =====================================================
        // OPEN PUPIL PROFILE
        // =====================================================

        private void OpenPupilButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }


            if (button.Tag is not Pupil pupil)
            {
                return;
            }


            try
            {
                PupilProfilePage profilePage =
                    new PupilProfilePage(
                        pupil.Id);


                profilePage.BackRequested += () =>
                {
                    if (NavigationService != null &&
                        NavigationService.CanGoBack)
                    {
                        NavigationService.GoBack();
                    }
                };


                if (NavigationService != null)
                {
                    NavigationService.Navigate(
                        profilePage);
                }
                else
                {
                    MessageBox.Show(
                        "Diri maabli an pupil profile.",
                        "Navigation Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
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


        // =====================================================
        // EDIT PUPIL
        // =====================================================

        private void EditPupilButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }


            if (button.Tag is not Pupil pupil)
            {
                return;
            }


            try
            {
                AddPupilWindow editWindow =
                    new AddPupilWindow(pupil)
                    {
                        Owner = Window.GetWindow(this)
                    };

                if (editWindow.ShowDialog() == true)
                {
                    RefreshPupilList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri ma-update an ngaran han mag-aaral.\n\n" +
                    ex.Message,
                    "Edit Pupil",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }


        // =====================================================
        // DELETE PUPIL
        // =====================================================

        private void DeletePupilButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            if (sender is not Button button)
            {
                return;
            }


            if (button.Tag is not Pupil pupil)
            {
                return;
            }


            MessageBoxResult result =
                MessageBox.Show(
                    $"Sigurado ka nga karuyag mo tanggalon hi " +
                    $"{pupil.FullName}?\n\n" +
                    "Mawawara liwat an iya session progress " +
                    "ngan progress check-in records.",
                    "Tanggalon an Mag-aaral",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);


            if (result != MessageBoxResult.Yes)
            {
                return;
            }


            try
            {
                _pupilService.DeletePupil(
                    pupil.Id);


                /*
                 * Reload directly from SQLite after delete.
                 */
                RefreshPupilList();


                MessageBox.Show(
                    "Natanggal na an mag-aaral.",
                    "Success",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Diri natanggal an mag-aaral.\n\n" +
                    ex.Message,
                    "Database Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}