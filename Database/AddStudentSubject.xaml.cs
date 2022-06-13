using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;

namespace Database
{
    /// <summary>
    /// Логика взаимодействия для AddStudentSubject.xaml
    /// </summary>
    public partial class AddStudentSubject : Window
    {
        public int student_id;
        public int subject_id;
        public AddStudentSubject()
        {
            InitializeComponent();
        }
        public AddStudentSubject(DependencyObject depObject) : this()
        {
            Owner = Window.GetWindow(depObject);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //заполнение combobox-ов
            NpgsqlCommand sqlCmd = new NpgsqlCommand("SELECT * FROM subject ORDER BY subject_name", Connection.con);
            NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();
            while (sqlReader.Read())
            {
                subjectcombobox.Items.Add(sqlReader["subject_name"].ToString());
            }
            sqlReader.Close();

            sqlCmd = new NpgsqlCommand("SELECT * FROM student ORDER BY full_name", Connection.con);
            sqlReader = sqlCmd.ExecuteReader();
            while (sqlReader.Read())
            {
                studentcombobox.Items.Add(sqlReader["full_name"].ToString());
            }
            sqlReader.Close();

            subjectcombobox.SelectedItem = (this.Owner as ViewData).subjectcombobox.SelectedItem;
            studentcombobox.SelectedItem = (this.Owner as ViewData).studentcombobox.SelectedItem;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (studentcombobox.SelectedItem == null)
                {
                    studenttext.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    studenttext.Foreground = new SolidColorBrush(Colors.Black);

                if (subjectcombobox.SelectedItem == null)
                {
                    subjecttext.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    subjecttext.Foreground = new SolidColorBrush(Colors.Black);

                var sql = "INSERT INTO student_subject(student_id, subject_id) " +
                    "VALUES((SELECT student_id FROM student " +
                    "WHERE full_name = @full_name), (SELECT subject_id FROM subject " +
                    "WHERE subject_name = @subject_name))";
                var cmd = new NpgsqlCommand(sql, Connection.con);
                cmd.Parameters.AddWithValue("@full_name", studentcombobox.Text);
                cmd.Parameters.AddWithValue("@subject_name", subjectcombobox.Text);

                if (MessageBox.Show($"Добавить связь студент-дисциплина\n" +
                                    $"{studentcombobox.Text} - {subjectcombobox.Text}?", "Подтверждение", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        (this.Owner as ViewData).actionstatus.Text = "";
                        (this.Owner as ViewData).actionstatus.Inlines.Add(new Run("Связь студент-дисциплина добавлена: ") { FontWeight = FontWeights.Bold });
                        (this.Owner as ViewData).actionstatus.Inlines.Add(new Run($"{studentcombobox.Text} - {subjectcombobox.Text}"));
                        this.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show($"Что-то пошло не так");
                        this.DialogResult = false;
                    }
                }
                else
                {
                    return;
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == "23505")
                    MessageBox.Show("Такая связь уже существует");
                else
                    MessageBox.Show(ex.Message, "Error message");
            }

        }

        private void studentcombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            if(subjectcombobox.SelectedItem == null)
            {
                NpgsqlCommand sqlCmd = new NpgsqlCommand("SELECT subject_name FROM subject EXCEPT " +
                    "SELECT subject_name FROM student_subject NATURAL JOIN subject NATURAL JOIN student " +
                    "WHERE full_name = @full_name ORDER BY subject_name", Connection.con);
                sqlCmd.Parameters.AddWithValue("@full_name", studentcombobox.Text);
                NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();
                subjectcombobox.Items.Clear();
                while (sqlReader.Read())
                {
                    subjectcombobox.Items.Add(sqlReader["subject_name"].ToString());
                }
                sqlReader.Close();
            }
            */
        }

        private void subjectcombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            /*
            if (studentcombobox.SelectedItem == null)
            {
                NpgsqlCommand sqlCmd = new NpgsqlCommand("SELECT full_name FROM student EXCEPT " +
                    "SELECT full_name FROM student_subject NATURAL JOIN subject NATURAL JOIN student " +
                    "WHERE subject_name = @subject_name ORDER BY full_name", Connection.con);
                sqlCmd.Parameters.AddWithValue("@subject_name", subjectcombobox.Text);
                NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();
                subjectcombobox.Items.Clear();
                while (sqlReader.Read())
                {
                    studentcombobox.Items.Add(sqlReader["subject_name"].ToString());
                }
                sqlReader.Close();
            }
            */
        }
    }
}
