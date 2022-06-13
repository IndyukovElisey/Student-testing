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
    /// Логика взаимодействия для StudentWindow.xaml
    /// </summary>
    public partial class StudentWindow : Window
    {
        public int stud_id;
        public bool continue_test = false;

        public StudentWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //пройти тест
            try
            {
                if(subject.SelectedItem == null)//если не выбран предмет
                {
                    subjecttext.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    subjecttext.Foreground = new SolidColorBrush(Colors.Black);

                //проверяем наличие оценки по предмету
                var sql = "SELECT mark FROM subject_result " +
                    "WHERE student_id = @student_id " +
                    "AND subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name)";
                var cmd = new NpgsqlCommand(sql, Connection.con);
                cmd.Parameters.AddWithValue("@student_id", stud_id);
                cmd.Parameters.AddWithValue("@subject_name", subject.SelectedItem.ToString());
                if(cmd.ExecuteScalar() != null)
                {
                    MessageBox.Show("Тест уже пройден");
                    return;
                }

                //получаем запись о последней попытке 
                sql = "SELECT * FROM public.test_result WHERE student_id = @student_id " +
                    "AND subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name) " +
                    "ORDER BY(CASE test_level WHEN 'Удовлетворительно' THEN 3 WHEN 'Хорошо' THEN 4 " +
                    "WHEN 'Отлично' THEN 5 END) DESC, attempt_number DESC LIMIT 1";
                cmd = new NpgsqlCommand(sql, Connection.con);
                cmd.Parameters.AddWithValue("@student_id", stud_id);
                cmd.Parameters.AddWithValue("@subject_name", subject.SelectedItem.ToString());
                NpgsqlDataReader sqlReader = cmd.ExecuteReader();

                string result_level;
                int result_attempt;

                if (sqlReader.Read())
                {
                    //инкрементируем попытку/уровень теста
                    result_level = sqlReader["test_level"].ToString();
                    result_attempt = int.Parse(sqlReader["attempt_number"].ToString());
                    if(sqlReader["test_result"].ToString() == "Пройден")
                    {
                        switch (result_level)
                        {
                            case "Удовлетворительно":
                                {
                                    result_level = "Хорошо";
                                    break;
                                }
                            case "Хорошо":
                                {
                                    result_level = "Отлично";
                                    break;
                                }
                            case "Отлично":
                                {
                                    sqlReader.Close();
                                    throw new Exception("Тест уже пройден");
                                }
                        }
                        result_attempt = 1;
                    }
                    else
                    {
                        result_attempt++;
                    }
                }
                else
                {
                    //задаем первую попытку
                    result_level = "Удовлетворительно";
                    result_attempt = 1;
                }
                sqlReader.Close();

                //подтверждение начала теста

                if (continue_test || MessageBox.Show($"Начать тест?", "Подтверждение", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    Test test = new Test(sender as DependencyObject);
                    test.subject.Text = subject.SelectedItem.ToString();
                    test.level.Text = result_level;
                    test.attempt.Text = result_attempt.ToString();
                    test.stud_id = stud_id;
                    if (test.ShowDialog() == true)
                    {
                        continue_test = true;
                        testbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                    else
                    {
                        continue_test = false;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Программа тестирования обучающихся\nРазработчик: Индюков Е.П.\n2021");
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //выход к форме авторизации
            if (MessageBox.Show($"Выйти из учетной записи?", "Подтверждение", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
            {
                if (Connection.con != null)
                {
                    Connection.con.Close();
                    Connection.con.Dispose();
                    NpgsqlConnection.ClearPool(Connection.con);
                }
                Login login = new Login();
                login.Show();
                Close();
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //закрыть программу
            Close();
        }

        private void Result_Click_1(object sender, RoutedEventArgs e)
        {
            //Просмотр результатов билетов
            try
            {
                /*
                //если не выбран предмет
                if (subject.SelectedItem == null)
                {
                    subjecttext.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    subjecttext.Foreground = new SolidColorBrush(Colors.Black);
                */

                //проверяем наличие результатов билетов
                var sql = "SELECT * FROM test_result WHERE student_id = @student_id";
                var cmd = new NpgsqlCommand(sql, Connection.con);
                cmd.Parameters.AddWithValue("@student_id", stud_id);
                //cmd.Parameters.AddWithValue("@subject_name", subject.SelectedItem.ToString());
                if (cmd.ExecuteScalar() == null)
                {
                    MessageBox.Show("Результаты отсутствуют");
                    return;
                }

                /*
                //проверяем наличие оценки по предмету
                sql = "SELECT mark FROM subject_result WHERE student_id = @student_id " +
                    "AND subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name)";
                cmd = new NpgsqlCommand(sql, Connection.con);
                cmd.Parameters.AddWithValue("@student_id", stud_id);
                cmd.Parameters.AddWithValue("@subject_name", subject.SelectedItem.ToString());
                string mark;
                if (cmd.ExecuteScalar() == null)
                {
                    mark = "-";
                }
                else
                {
                    mark = cmd.ExecuteScalar().ToString();
                }
                */

                ViewData result_window = new ViewData();
                //result_window.subject.Text = subject.SelectedItem.ToString();
                //result_window.mark.Text = mark;
                result_window.stud_id = stud_id;
                result_window.view_type = ViewData.ViewTypeEnum.student_test_results;
                result_window.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Result_Click_2(object sender, RoutedEventArgs e)
        {
            //Просмотр оценок

            //проверяем наличие оценки по предмету
            var sql = "SELECT mark FROM subject_result WHERE student_id = @student_id";
            var cmd = new NpgsqlCommand(sql, Connection.con);
            cmd.Parameters.AddWithValue("@student_id", stud_id);
            if(cmd.ExecuteScalar() == null)
            {
                MessageBox.Show("Оценки отсутствуют");
                return;
            }

            ViewData result_window = new ViewData();
            result_window.stud_id = stud_id;
            result_window.view_type = ViewData.ViewTypeEnum.student_subject_marks;
            result_window.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //заполнение списка предметов
            try
            {
                NpgsqlCommand sqlCmd = new NpgsqlCommand("SELECT * FROM subject NATURAL INNER JOIN student_subject WHERE student_id=@student_id", Connection.con);
                sqlCmd.Parameters.AddWithValue("@student_id", stud_id);
                NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

                while (sqlReader.Read())
                {
                    subject.Items.Add(sqlReader["subject_name"].ToString());
                }

                sqlReader.Close();

                sqlCmd = new NpgsqlCommand("SELECT full_name FROM student WHERE student_id=@student_id", Connection.con);
                sqlCmd.Parameters.AddWithValue("@student_id", stud_id);
                user.Text = sqlCmd.ExecuteScalar().ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
