using ARALReadingSystem.Models;
using ARALReadingSystem.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ARALReadingSystem.Views
{
    public partial class AddPupilWindow : Window
    {
        // =========================================================
        // SERVICE
        // =========================================================

        private readonly PupilService _pupilService =
            new PupilService();

        private readonly Pupil? _pupilBeingEdited;

        public int? SavedPupilId { get; private set; }


        // =========================================================
        // SELECTED VALUES
        // =========================================================

        private string _selectedGrade =
            "Grade 1";

        private string _selectedGender =
            "Lalaki";


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public AddPupilWindow()
        {
            InitializeComponent();

            UpdateChoiceButtons();

            FirstNameTextBox.Focus();
        }

        public AddPupilWindow(Pupil pupil)
            : this()
        {
            _pupilBeingEdited =
                pupil ?? throw new ArgumentNullException(nameof(pupil));

            Title = "I-edit an Mag-aaral";
            HeaderText.Text = "I-edit an Mag-aaral";
            SubtitleText.Text =
                "Baguhon an ngaran. Diri maaapektuhan an iya session progress ngan check-in records.";
            SaveButton.Content = "I-save an pagbabago";

            FirstNameTextBox.Text =
                pupil.FirstName;

            LastNameTextBox.Text =
                pupil.LastName;

            GradeLevelLabel.Visibility =
                Visibility.Collapsed;
            GradeLevelOptions.Visibility =
                Visibility.Collapsed;
            GenderLabel.Visibility =
                Visibility.Collapsed;
            GenderOptions.Visibility =
                Visibility.Collapsed;
            BirthdateLabel.Visibility =
                Visibility.Collapsed;
            BirthdatePicker.Visibility =
                Visibility.Collapsed;
        }


        // =========================================================
        // GRADE LEVEL
        // =========================================================

        private void Grade1Button_Click(
            object sender,
            RoutedEventArgs e)
        {
            _selectedGrade =
                "Grade 1";

            UpdateChoiceButtons();
        }


        private void Grade2Button_Click(
            object sender,
            RoutedEventArgs e)
        {
            _selectedGrade =
                "Grade 2";

            UpdateChoiceButtons();
        }


        // =========================================================
        // GENDER
        // =========================================================

        private void MaleButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _selectedGender =
                "Lalaki";

            UpdateChoiceButtons();
        }


        private void FemaleButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            _selectedGender =
                "Babae";

            UpdateChoiceButtons();
        }


        // =========================================================
        // UPDATE CHOICE BUTTONS
        // =========================================================

        private void UpdateChoiceButtons()
        {
            SetSelected(
                Grade1Button,
                _selectedGrade == "Grade 1");

            SetSelected(
                Grade2Button,
                _selectedGrade == "Grade 2");

            SetSelected(
                MaleButton,
                _selectedGender == "Lalaki");

            SetSelected(
                FemaleButton,
                _selectedGender == "Babae");
        }


        private static void SetSelected(
            Button button,
            bool selected)
        {
            if (selected)
            {
                button.Background =
                    new SolidColorBrush(
                        Color.FromRgb(
                            230,
                            245,
                            253));

                button.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            21,
                            148,
                            223));

                button.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            21,
                            104,
                            158));
            }
            else
            {
                button.Background =
                    Brushes.White;

                button.BorderBrush =
                    new SolidColorBrush(
                        Color.FromRgb(
                            227,
                            216,
                            200));

                button.Foreground =
                    new SolidColorBrush(
                        Color.FromRgb(
                            82,
                            104,
                            123));
            }
        }


        // =========================================================
        // SAVE
        // =========================================================

        private void SaveButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            ErrorText.Visibility =
                Visibility.Collapsed;


            string firstName =
                FirstNameTextBox.Text.Trim();

            string lastName =
                LastNameTextBox.Text.Trim();


            // -----------------------------------------------------
            // FIRST NAME
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    firstName))
            {
                ShowError(
                    "Ibutang ang pangalan ng mag-aaral.");

                FirstNameTextBox.Focus();

                return;
            }


            // -----------------------------------------------------
            // LAST NAME
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    lastName))
            {
                ShowError(
                    "Ibutang ang apelyido ng mag-aaral.");

                LastNameTextBox.Focus();

                return;
            }


            // -----------------------------------------------------
            // BIRTHDATE
            // -----------------------------------------------------

            DateTime? birthdate =
                BirthdatePicker.SelectedDate?.Date;

            if (_pupilBeingEdited == null &&
                !birthdate.HasValue)
            {
                ShowError(
                    "Piliin ang petsa ng kapanganakan.");

                BirthdatePicker.Focus();

                return;
            }


            if (_pupilBeingEdited == null &&
                birthdate > DateTime.Today)
            {
                ShowError(
                    "Hindi maaaring nasa hinaharap ang petsa ng kapanganakan.");

                BirthdatePicker.Focus();

                return;
            }


            // =====================================================
            // SAVE
            // =====================================================
            //
            // Figure 18 PUPIL:
            //
            // Pupil_ID       -> generated by SQLite
            // Teacher_ID     -> supplied by PupilService
            // First_Name     -> firstName
            // Last_Name      -> lastName
            // Grade_Level    -> _selectedGrade
            // Gender         -> _selectedGender
            // Birthdate      -> birthdate
            //
            // No Age.
            // No GuardianName.
            // =====================================================

            try
            {
                if (_pupilBeingEdited != null)
                {
                    _pupilService.UpdatePupilName(
                        _pupilBeingEdited.Id,
                        firstName,
                        lastName);

                    SavedPupilId =
                        _pupilBeingEdited.Id;
                }
                else
                {
                    SavedPupilId =
                        _pupilService.AddPupil(
                            firstName,
                            lastName,
                            _selectedGrade,
                            _selectedGender,
                            birthdate!.Value);
                }

                DialogResult =
                    true;

                Close();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Diri na-save an impormasyon han mag-aaral.\n" +
                    ex.Message);
            }
        }


        // =========================================================
        // CANCEL
        // =========================================================

        private void CancelButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            DialogResult =
                false;

            Close();
        }


        // =========================================================
        // ERROR
        // =========================================================

        private void ShowError(
            string message)
        {
            ErrorText.Text =
                message;

            ErrorText.Visibility =
                Visibility.Visible;
        }
    }
}