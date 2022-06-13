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
    /// Логика взаимодействия для AddSubject.xaml
    /// </summary>
    public partial class AddSubject : Window
    {
        public AddSubject()
        {
            InitializeComponent();
        }
        public AddSubject(DependencyObject depObject) : this()
        {
            Owner = Window.GetWindow(depObject);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if(subjname.Text == "")
                    throw new Exception("Название дисциплины не может быть пустым");

                if (!subjname.Text.All(c => Char.IsLetter(c) || c == '.' || c == ',' || c == ' ' ))
                    throw new Exception("Название дисциплины может содержать только буквы, точки, запятые и пробелы");

                var sql = "INSERT INTO subject(subject_id, subject_name) VALUES(DEFAULT, @subjname)";
                var cmd = new NpgsqlCommand(sql, Connection.con);
                cmd.Parameters.AddWithValue("@subjname", subjname.Text);

                if (MessageBox.Show($"Добавить дисциплину\n" +
                                    $"{subjname.Text}?", "Подтверждение", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        (this.Owner as ViewData).actionstatus.Text = "";
                        (this.Owner as ViewData).actionstatus.Inlines.Add(new Run("Дисциплина добавлена: ") { FontWeight = FontWeights.Bold });
                        (this.Owner as ViewData).actionstatus.Inlines.Add(new Run($"{subjname.Text}"));
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
                    MessageBox.Show("Такая дисциплина уже существует");
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
    }
}
