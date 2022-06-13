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
    /// Логика взаимодействия для AddStudent.xaml
    /// </summary>

    public partial class AddStudent : Window
    {
        public AddStudent()
        {
            InitializeComponent();
        }
        public AddStudent(DependencyObject depObject) : this()
        {
            Owner = Window.GetWindow(depObject);
        }
        public string selected_sex = "М";

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (studname1.Text == "" || studname2.Text == "" || studname3.Text == "")
                    throw new Exception("ФИО не может быть пустым");

                if (!studname1.Text.All(c => Char.IsLetter(c)) || !studname2.Text.All(c => Char.IsLetter(c)) || !studname3.Text.All(c => Char.IsLetter(c)))
                    throw new Exception("ФИО содержит запрещенные символы");

                //studname1.Text = string.Join(" ", studname1.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)); //removes extra spaces
                if (!dateofbirth.SelectedDate.HasValue)//дата рождения не выбрана
                {
                    dateofbirthtext.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    dateofbirthtext.Foreground = new SolidColorBrush(Colors.Black);

                if (dateofbirth.SelectedDate.Value > DateTime.Now.Date)
                    throw new Exception("Дата рождения содержит недопустимое значение");

                if (creditbooknumber.Text == "")//номер зачетной книжки пуст
                {
                    credbooknumbertext.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    credbooknumbertext.Foreground = new SolidColorBrush(Colors.Black);

                if (!creditbooknumber.Text.All(c => char.IsDigit(c)))
                    throw new Exception("Номер зачетной книжки должен содержать только цифры");

                if (creditbooknumber.Text.Length != 5)
                    throw new Exception("Номер зачетной книжки должен содержать 5 цифр");

                if (login.Text == "")//логин пуст
                {
                    logintext.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    logintext.Foreground = new SolidColorBrush(Colors.Black);

                if (!login.Text.All(c => Char.IsLetter(c)))
                    throw new Exception("Логин содержит запрещенные символы");

                if (password.Text == "")//пароль пуст
                {
                    passwordtext.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    passwordtext.Foreground = new SolidColorBrush(Colors.Black);

                if (!password.Text.All(c => Char.IsLetterOrDigit(c)))
                    throw new Exception("Пароль содержит запрещенные символы");

                var sql = "INSERT INTO student(student_id, full_name, date_of_birth, sex, credit_book_number, login, password) " +
                    "VALUES(DEFAULT, @studname, @date, @sex, @creditbook, @login, @password)";
                var cmd = new NpgsqlCommand(sql, Connection.con);
                cmd.Parameters.AddWithValue("@studname", studname1.Text + " " + studname2.Text + " " + studname3.Text);
                cmd.Parameters.AddWithValue("@date", dateofbirth.SelectedDate);
                cmd.Parameters.AddWithValue("@sex", selected_sex);
                cmd.Parameters.AddWithValue("@creditbook", int.Parse(creditbooknumber.Text));
                cmd.Parameters.AddWithValue("@login", login.Text);
                cmd.Parameters.AddWithValue("@password", password.Text);

                if (MessageBox.Show($"Добавить студента\n" +
                                    $"{studname1.Text + " " + studname2.Text + " " + studname3.Text}?", "Подтверждение", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        (this.Owner as ViewData).actionstatus.Text = "";
                        (this.Owner as ViewData).actionstatus.Inlines.Add(new Run("Студент добавлен: ") { FontWeight = FontWeights.Bold });
                        (this.Owner as ViewData).actionstatus.Inlines.Add(new Run($"{studname1.Text} {studname2.Text} {studname3.Text}"));
                        this.DialogResult = true;
                    }
                    else
                    {
                        MessageBox.Show("Что-то пошло не так");
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
                    MessageBox.Show("Такой студент уже существует");
                else
                    MessageBox.Show(ex.Message, "Error message");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error message");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton ck = sender as RadioButton;
            selected_sex = ck.Name;
        }
    }
}
