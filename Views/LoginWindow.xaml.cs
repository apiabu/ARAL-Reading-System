using ARALReadingSystem.Services;
using System;
using System.Windows;
using System.Windows.Input;

namespace ARALReadingSystem.Views
{
    public partial class LoginWindow : Window
    {
        // =========================================================
        // SERVICE
        // =========================================================

        private readonly AuthenticationService
            _authenticationService = new();


        // =========================================================
        // CONSTRUCTOR
        // =========================================================

        public LoginWindow()
        {
            InitializeComponent();

            Loaded +=
                LoginWindow_Loaded;
        }


        // =========================================================
        // WINDOW LOADED
        // =========================================================

        private void LoginWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            UsernameTextBox.Focus();
        }


        // =========================================================
        // LOGIN BUTTON
        // =========================================================

        private void LoginButton_Click(
            object sender,
            RoutedEventArgs e)
        {
            AttemptLogin();
        }


        // =========================================================
        // LOGIN
        // =========================================================

        private void AttemptLogin()
        {
            ErrorText.Visibility =
                Visibility.Collapsed;


            string username =
                UsernameTextBox.Text.Trim();

            string password =
                PasswordBox.Password;


            // -----------------------------------------------------
            // VALIDATE USERNAME
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    username))
            {
                ShowError(
                    "Ibutang an username.");

                UsernameTextBox.Focus();

                return;
            }


            // -----------------------------------------------------
            // VALIDATE PASSWORD
            // -----------------------------------------------------

            if (string.IsNullOrWhiteSpace(
                    password))
            {
                ShowError(
                    "Ibutang an password.");

                PasswordBox.Focus();

                return;
            }


            // -----------------------------------------------------
            // AUTHENTICATE
            // -----------------------------------------------------

            try
            {
                bool valid =
                    _authenticationService.Login(
                        username,
                        password,
                        out string teacherName);


                if (!valid)
                {
                    ShowError(
                        "Sayop an username o password.");

                    PasswordBox.Clear();

                    PasswordBox.Focus();

                    return;
                }


                // -------------------------------------------------
                // OPEN TEACHER DASHBOARD
                // -------------------------------------------------

                TeacherDashboard dashboard =
                    new TeacherDashboard(
                        teacherName);


                Application.Current.MainWindow =
                    dashboard;


                dashboard.Show();


                Close();
            }
            catch (Exception ex)
            {
                ShowError(
                    "Diri makapag-login.\n" +
                    ex.Message);
            }
        }


        // =========================================================
        // SHOW ERROR
        // =========================================================

        private void ShowError(
            string message)
        {
            ErrorText.Text =
                message;

            ErrorText.Visibility =
                Visibility.Visible;
        }


        // =========================================================
        // ENTER KEY
        // =========================================================

        protected override void OnKeyDown(
            KeyEventArgs e)
        {
            base.OnKeyDown(e);


            if (e.Key != Key.Enter)
            {
                return;
            }


            AttemptLogin();

            e.Handled =
                true;
        }
    }
}