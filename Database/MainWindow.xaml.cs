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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;


namespace Database
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            switch (actionlist.Text)
            {
                case "Добавление":
                    switch(tablelist.Text)
                    {
                        case "Вопросы": 
                            {
                                AddQuestion add_question = new AddQuestion();
                                add_question.ShowDialog();
                                break;
                            }
                        case "Студенты":
                            {
                                AddStudent add_student = new AddStudent();
                                add_student.ShowDialog();
                                break;
                            }
                        case "Дисциплины":
                            {
                                AddSubject add_subject = new AddSubject();
                                add_subject.ShowDialog();
                                break;
                            }
                        case "Связи студент-дисциплина":
                            {
                                AddStudentSubject add_student_subject = new AddStudentSubject();
                                add_student_subject.ShowDialog();
                                break;
                            }
                        default:
                            {
                                MessageBox.Show("Действие недоступно");
                                break;
                            }
                    }
                    break;
                case "Просмотр":
                    ViewData result_window = new ViewData();

                    switch (tablelist.Text)
                    {
                        case "Вопросы":
                            {
                                result_window.view_type = ViewData.ViewTypeEnum.admin_questions;
                                break;
                            }
                        case "Студенты":
                            {
                                result_window.view_type = ViewData.ViewTypeEnum.admin_students;
                                break;
                            }
                        case "Дисциплины":
                            {
                                result_window.view_type = ViewData.ViewTypeEnum.admin_subjects;
                                break;
                            }
                        case "Оценки по дисциплинам":
                            {
                                result_window.view_type = ViewData.ViewTypeEnum.admin_subject_marks;
                                break;
                            }
                        case "Результаты билетов":
                            {
                                result_window.view_type = ViewData.ViewTypeEnum.admin_test_results;
                                break;
                            }
                        case "Связи студент-дисциплина":
                            {
                                result_window.view_type = ViewData.ViewTypeEnum.admin_subject_student;
                                break;
                            }
                    }
                    result_window.ShowDialog();
                    break;
                case "Удаление":
                    MessageBox.Show("Недоступно");
                    break;
                default:
                    MessageBox.Show("???");
                    break;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Программа тестирования обучающихся\nРазработчик: Индюков Е.П.\n2021");
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            //выход к форме авторизации
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

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //закрыть программу
            Close();
        }
    }
}
