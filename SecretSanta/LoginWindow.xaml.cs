using System.Text;
using System.Windows;
using System.Security.Cryptography;
using System.Data.SqlClient;
using System;

namespace SecretSanta
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public bool SuccessfulLogin { get; set; } = false;
        public bool signup { get; set; } = false;
        MainWindow mainWindow;
        public static byte[] GetHash(string input)
        {
            HashAlgorithm algo = SHA256Managed.Create();
            return algo.ComputeHash(Encoding.UTF8.GetBytes(input));
        }
        public static string GetHashString(string input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(input))
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString().Substring(0, 40);
        }
        public LoginWindow(MainWindow parent)
        {
            InitializeComponent();
            isSignUp();
            mainWindow = parent;
            
        }
        private void isSignUp()
        {
            if (signup)
            {
                label1_Copy.Visibility = Visibility.Visible;
                pb_password_repeat.Visibility = Visibility.Visible;
                btn_login.Content = "Erstellen";
                btn_newUser.Content = "Anmelden";
            }
            else
            {
                label1_Copy.Visibility = Visibility.Hidden;
                pb_password_repeat.Visibility = Visibility.Hidden;
                btn_login.Content = "Anmelden";
                btn_newUser.Content = "Neuer\r\nBenutzer";
            }
        }
        private bool checkPassword(string username, string password)
        {
            bool match = false;
            SQLCommandBuilder SQLCmd = new SQLCommandBuilder(mainWindow.localConnection);
            using (SQLCmd.Command)
            {
                string storedPassword = "";
                // Search for user with <username> 
                SQLCmd.buildSQLStatement
                (
                    SQLCommandType.Select,
                    "Users",
                    new SQLCondition("UserName", SQLOperator.Equal, username)
                );
                using (SqlDataReader reader = SQLCmd.Command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int ord = reader.GetOrdinal("Password");
                        storedPassword = reader.GetString(ord);
                        int idord = reader.GetOrdinal("UsersID");
                        mainWindow.UserID = reader.GetString(ord);
                    }
                }
                if (GetHashString(password) == storedPassword)
                {
                    match = true;
                }
            }
            return match;
        }
        private void btn_login_cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            string message = "";
            SQLCommandBuilder SQLCmd = new SQLCommandBuilder(mainWindow.localConnection);
            using (SQLCmd.Command)
            {
                // Neuen Benutzer Erstellen
                if (signup)
                {
                    // Create new user Logic
                    if (pb_password.Password == pb_password_repeat.Password)
                    {
                        // Password and Username should be longer than 3 characters
                        if (pb_password.Password.Length > 3 && tb_username.Text.Length > 3)
                        {
                            // Check if user exists
                            SQLCmd.buildSQLStatement(SQLCommandType.Select, "Users", new SQLCondition("UserName", SQLOperator.Equal, tb_username.Text));
                            int exists = SQLCmd.Command.ExecuteNonQuery();
                            if (exists > 0)
                            {
                                message = "Benutzer existiert bereits.";
                            }
                            else
                            {
                                // Create new user
                                SQLCmd.buildSQLStatement(SQLCommandType.Insert, "Users", new string[] { "UserName", "Password" }, new string[] {tb_username.Text, GetHashString(pb_password.Password)});
                                SQLCmd.Command.ExecuteNonQuery();
                                message = "Benutzer '" + tb_username.Text + "' Erstellt";
                                SuccessfulLogin = true;
                            }
                        }
                        else
                        {
                            message = "Passwort oder Benutzername zu kurz.";
                        }
                    }
                    else
                    {
                        message = "Passwörter stimmen nicht überein.";
                    }
                }
                // Login Existing User
                else
                {
                    if (checkPassword(tb_username.Text, pb_password.Password))
                    {
                        message = "Erfolgreich Angemeldet als " + tb_username.Text;
                        SuccessfulLogin = true;
                    }
                    else
                    {
                        message = "Benutzer / Passwort kombination nicht gefunden.";
                    }
                }
            }
            MessageBox.Show(message);
            if (SuccessfulLogin)
            {
                mainWindow.User = tb_username.Text;
                Close();
            }
        }

        private void btn_newUser_Click(object sender, RoutedEventArgs e)
        {
            if (signup)
            {
                signup = false;

            }
            else
            {
                signup = true;
            }
            isSignUp();
        }
    }
}
