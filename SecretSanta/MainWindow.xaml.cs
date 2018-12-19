using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net;
using System.Net.Mail;
using System.Web.UI.WebControls;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Windows.Data;

namespace SecretSanta
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string here = Directory.GetCurrentDirectory();
        public string User { get; set; }
        public string UserID { get; set; }
        public bool isLoggedIn { get; set; }

        public SqlConnection localConnection = new SqlConnection("Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=ssdb;Integrated Security=True;Connect Timeout=15;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");

        /// <summary>
        /// Inizialisierung der Teilnehmerliste
        /// </summary>
        public MemberList CurrentGroup { get; set; } = new MemberList("Aktuelle Gruppe.");
        public MainWindow()
        {
            InitializeComponent();
            setAdminCode();
            AdminWindow outputWindow = new AdminWindow(this);

            for (int i = 0; i < 5; i++)
            {
                CurrentGroup.Members.Add(new Person("P" + i, "P" + i, "P" + i));
            }

            try
            {
                localConnection.Open();
                Console.WriteLine("Connected to -" + localConnection.Database + "- Database");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            // TODO load all people
            SQLCommandBuilder SQLCmd = new SQLCommandBuilder(localConnection);
            SQLCmd.buildSQLStatement
                (
                   SQLCommandType.Select,
                   "Person"
                );
            using (SqlDataReader reader = SQLCmd.Command.ExecuteReader())
            {
                CurrentGroup.Members.Clear();
                while (reader.Read())
                {
                    CurrentGroup.Members.Add(new Person(reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                }
            }
            //connectDataView();
        }
        public void connectDataView()
        {
            BindingList<Person> binding = new BindingList<Person>(CurrentGroup.Members);

            lb_people.ItemsSource = binding;
            //lb_people.Columns.Clear();
            //DataGridTextColumn col = new DataGridTextColumn();
            //col.Header = "Vorname";
            //col.Binding = "{FirstName}";
            //BindingList<Person> bl = new BindingList<Person>();
            //bl.Add()
            //lb_people.DataContext = binding;
        }
        /// <summary>
        /// "Hinzufügen" Knopf Funktion
        /// Fügt die Person in den Textboxen der Teilnehmerliste hinzu
        /// </summary>
        private void btn_add_Click(object sender, RoutedEventArgs e)
        {
            if (tb_firstname.Text != "" && tb_lastname.Text != "" && tb_email.Text != "")
            {
                addPerson(tb_firstname.Text, tb_lastname.Text, tb_email.Text);
            }
            else
            {
                MessageBox.Show("Bitte kein Feld leer lassen.");
            }
        }
        private void addPerson(string firstname, string lastname, string email)
        {
            /// Regex check ob Email möglich ist
            if (isValidEmail(email))
            {
                SQLCommandBuilder SQLCmd = new SQLCommandBuilder(localConnection);
                SQLCmd.buildSQLStatement
                (
                    SQLCommandType.Distinct,
                    "Person",
                    new SQLCondition[]
                    {
                        new SQLCondition("FirstName", SQLOperator.Equal, firstname),
                        new SQLCondition("LastName", SQLOperator.Equal, lastname, SQLConditionType.And),
                        new SQLCondition("Email", SQLOperator.Equal, email, SQLConditionType.And)
                    }
                );
                SqlDataReader dr = SQLCmd.Command.ExecuteReader();
                if (dr.HasRows)
                {
                    CurrentGroup.Members.Add(new Person());
                }
                else
                {
                    SQLCmd.buildSQLStatement(SQLCommandType.Insert, "Person", null, new string[] { "FirstName", "LastName", "Email" }, new string[] { firstname, lastname, email });
                    SQLCmd.Command.ExecuteNonQuery();
                }
            }
            else
            {
                MessageBox.Show("Bitte eine gültige Email Adresse eingeben.");
            }
        }

        public bool isValidEmail(string email)
        {
            Regex regex = new Regex(
                @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*" +
                "@" +
                @"((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))$");
            Match match = regex.Match(email);
            return match.Success;
        }
        private void btn_new_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(this, "Liste leeren?", "Warnung", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation) == MessageBoxResult.OK)
            {
                CurrentGroup.Members.Clear();
                clearTextBoxes();
            }
        }
        private void btn_save_Click(object sender, RoutedEventArgs e)
        {
            AddGroup newGroup = new AddGroup(this);
            newGroup.ShowDialog();

        }
        public void saveGroup(string groupName)
        {
            // TODO Save group with current name
            SQLCommandBuilder insert = new SQLCommandBuilder(localConnection);
            insert.buildSQLStatement(SQLCommandType.Insert, "Groups", new string[] { "GroupName", "User_ID" }, new string[] { groupName, UserID.ToString() });
            insert.Command.ExecuteNonQuery();
            // TODO add members to group
            foreach (Person member in CurrentGroup.Members)
            {
                insert.buildSQLStatement(SQLCommandType.Insert, "Members", new string[] { "Group_ID", "Person_ID" }, new string[] { "", member.PersonID.ToString() });
                insert.Command.ExecuteNonQuery();
            }
        }
        private void clearTextBoxes()
        {
            tb_firstname.Text = "";
            tb_lastname.Text = "";
            tb_email.Text = "";
        }

        private void btn_email_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool matched = true;
                int m = CurrentGroup.Members.Count;
                while (matched && m > 0)
                {
                    m--;
                    if (CurrentGroup.Members[m].TargetPerson == null)
                    {
                        matched = false;
                    }
                }
                if (matched)
                {
                    SmtpClient mailClient = new SmtpClient();

                    //NetworkCredential auth = new NetworkCredential(mailUser, mailPW);
                    //mailClient.Credentials = auth;
                    //mailClient.UseDefaultCredentials = false;

                    mailClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    mailClient.PickupDirectoryLocation = @"C:\Users\elias.styner.INF\Documents\test\Mail";

                    MailMessage message = new MailMessage();
                    MailDefinition md = new MailDefinition();
                    md.From = "elias.styner@band.edu.ch";
                    md.IsBodyHtml = true;
                    md.Subject = "Wichteln";

                    string msgBody = "";
                    CurrentGroup.Members.OrderBy(x => x.FirstName);

                    foreach (Person p in CurrentGroup.Members)
                    {
                        if (p.TargetPerson != null)
                        {
                            ListDictionary replacements = new ListDictionary();
                            replacements.Add("{name}", p.FirstName);
                            replacements.Add("{group}", CurrentGroup.GroupName);
                            replacements.Add("{ziel}", p.TargetPerson.FirstName + " " + p.TargetPerson.LastName);

                            msgBody = File.ReadAllText(here + @"\MailTemplates\MailTemplate.html");

                            message = md.CreateMailMessage(p.Email, replacements, msgBody, new System.Web.UI.Control());
                            message.Subject = CurrentGroup.GroupName;
                            mailClient.Send(message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Die Teilnehmer sind noch nicht verteilt.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        Key[] AdminCode = new Key[11];
        int counter = 0;
        public void setAdminCode()
        {
            InitializeComponent();
            AdminCode[0] = AdminCode[1] = Key.Up;
            AdminCode[2] = AdminCode[3] = Key.Down;
            AdminCode[4] = AdminCode[6] = Key.Left;
            AdminCode[5] = AdminCode[7] = Key.Right;
            AdminCode[8] = Key.B;
            AdminCode[9] = Key.A;
            AdminCode[10] = Key.Enter;
        }
        private void wnd_main_KeyDown(object sender, KeyEventArgs e)
        {
            if (counter < AdminCode.Count() - 1)
            {
                if (e.Key == AdminCode[counter])
                {
                    counter++;
                }
                else
                {
                    counter = 0;
                }
            }
            else
            {
                /// Admin Code activated
                AdminWindow adminwnd = new AdminWindow(this);
                adminwnd.Show();
            }
        }
        private void cb_group_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // TODO add {Load group based on selection} logic
            // Get Group ID
            SQLCommandBuilder SQLCmd = new SQLCommandBuilder(localConnection);
            using (SQLCmd.Command)
            {
                SQLCmd.buildSQLStatement
                (
                    SQLCommandType.Select,
                    "Groups",
                    new SQLCondition("GroupName", SQLOperator.Equal, cb_group.Text)
                );
                string groupID = "";
                using (SqlDataReader reader = SQLCmd.Command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        groupID = reader.GetString(0);
                    }
                }
                SQLCmd.buildSQLStatement
                (
                   SQLCommandType.Select,
                   "Members",
                   new SQLCondition("Group_ID", SQLOperator.Equal, groupID)
                );
                using (SqlDataReader reader = SQLCmd.Command.ExecuteReader())
                {
                    CurrentGroup.Members.Clear();
                    foreach (DataRow row in reader)
                    {
                        CurrentGroup.Members.Add(new Person(row[1].ToString(), row[2].ToString(), row[3].ToString()));
                    }
                }
            }
        }

        private void btn_match_Click(object sender, RoutedEventArgs e)
        {
            // TODO add {match current group} and {save final group to events table} logic
            string eventName = "";
            SQLCommandBuilder cmd = new SQLCommandBuilder(localConnection);
            cmd.buildSQLStatement(SQLCommandType.Insert, "Events", "EventName", eventName);
            cmd.Command.ExecuteNonQuery();
            

            cmd.buildSQLStatement(SQLCommandType.Insert, "Participants", new string[] { "", "" }, new string[] { "", "" });
            foreach (Person p in CurrentGroup.Members)
            {
                if (p.Participates)
                {

                }
            }
        }
        private void showLogin()
        {
            isLoggedIn = true;
            loginIcon.Source = new BitmapImage(new Uri("Pictures/loginIconLogedin.png", UriKind.Relative));
            lb_login.Header = "Angemeldet als: " + User;
            btn_login.Header = "Abmelden";
            cb_group.IsEnabled = true;
        }
        private void showLogout()
        {
            User = "";
            isLoggedIn = false;
            loginIcon.Source = new BitmapImage(new Uri("Pictures/loginIcon.png", UriKind.Relative));
            lb_login.Header = "Nicht Angemeldet";
            btn_login.Header = "Anmelden";
            cb_group.IsEnabled = false;
        }
        private void btn_login_Click(object sender, RoutedEventArgs e)
        {
            if (isLoggedIn)
            {
                showLogout();
            }
            else
            {
                LoginWindow login = new LoginWindow(this);
                login.ShowDialog();

                if (login.SuccessfulLogin)
                {
                    showLogin();
                    cb_group.Items.Clear();

                    // read groups of user
                    SQLCommandBuilder SQLCmd = new SQLCommandBuilder(localConnection);
                    using (SQLCmd.Command)
                    {
                        SQLCmd.buildSQLStatement
                        (
                            SQLCommandType.Select,
                            "Groups",
                            new SQLCondition("GroupID", SQLOperator.Equal, UserID)
                        );
                        using (SqlDataReader reader = SQLCmd.Command.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    cb_group.Items.Add(reader.GetString(1));
                                }
                            }
                        }
                    }

                }
            }
        }

        private void wnd_main_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            localConnection.Close();
        }
    }
    /// <summary>
    /// Klasse Person
    /// Vorlage für die Teilnehmer im Wichtel Programm
    /// </summary>
    public class Person
    {
        public int PersonID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Person TargetPerson { get; set; }
        public bool Participates { get; set; } = true;
        public Person()
        {

        }
        public Person(string vorname, string nachname)
        {
            FirstName = vorname;
            LastName = nachname;
        }
        /// <summary>
        /// Erstellt die Vorlage für ein Teilnehmer der Wichtel Aktion
        /// </summary>
        /// <param name="vorname">Vorname der Person</param>
        /// <param name="nachname">Nachname der Person</param>
        /// <param name="email">Email an welche die Zielperson geschickt werden kann</param>
        public Person(string vorname, string nachname, string email) : this(vorname, nachname)
        {
            Email = email;
        }
    }
    /// <summary>
    /// Liste der Teilnehmer
    /// </summary>
    public class MemberList
    {
        public string GroupName { get; set; }
        public List<Person> Members { get; set; }
        public MemberList(string name)
        {
            GroupName = name;
            Members = new List<Person>();
        }
        public void unmatch()
        {
            foreach (Person p in Members)
            {
                p.TargetPerson = null;
            }
        }
        public List<Person> match()
        {
            unmatch();
            int i = 1;
            foreach (Person id in Members)
            {
                id.PersonID = i;
                i++;
            }
            List<Person> result = Members.FindAll(x => x.Participates);
            List<Person> temp = Members.FindAll(x => x.Participates);

            foreach (Person p in result)
            {
                Person t = new Person();
                do
                {
                    temp.Shuffle();
                    t = temp[0];
                }
                while (p.PersonID != t.PersonID && result.FindIndex(x => x.TargetPerson == t) == -1);
                p.TargetPerson = temp[0];
            }
            if (result.FindIndex(x => x.TargetPerson == null) != -1)
            {
                match();
            }
            Members.OrderBy(x => x.FirstName);
            return result;
        }
    }
    /// <summary>
    /// Baut eine sicherere version der pseudorandom klasse auf
    /// </summary>
    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
    /// <summary>
    /// Zusaetzliche funktionen für bestehende Klassen
    /// Muss nicht instanziert werden, da statisch
    /// </summary>
    static class MyExtensions
    {
        /// <summary>
        /// Fuegt jeder Klasse die das "IList" interface implementiert die "Shuffle" Methode hinzu, um Listen mischen zu können
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
