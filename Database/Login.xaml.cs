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
    /// Логика взаимодействия для Авторизация.xaml
    /// </summary>
    /// 
    public static class Connection
    {
        public static NpgsqlConnection con;
    }

    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string login = loginbox1.Text;
            string password = passwordbox1.Password;
            int stud_id;
            try
            {
                if(login=="admin")//если входит админ
                {
                    string strConnString = $"Host=localhost;Username={login};Password={password};Database=Testing";
                    Connection.con = new NpgsqlConnection(strConnString);
                    Connection.con.Open();

                    ViewData viewData = new ViewData();
                    viewData.view_type = ViewData.ViewTypeEnum.admin_students;

                    viewData.Show();
                    Close();
                }
                else//если входит студент
                {
                    string strConnString = $"Host=localhost;Username=admin;Password=password;Database=Testing";
                    Connection.con = new NpgsqlConnection(strConnString);
                    Connection.con.Open();
                    //проверка логина и пароля студента
                    var sql = "SELECT student_id FROM student WHERE login=@login AND password=@password";
                    var cmd = new NpgsqlCommand(sql, Connection.con);
                    cmd.Parameters.AddWithValue("@login", login);
                    cmd.Parameters.AddWithValue("@password", password);
                    if (cmd.ExecuteScalar() == null)
                    {
                        throw new Exception("Неверный логин или пароль");
                    }
                    stud_id = int.Parse(cmd.ExecuteScalar().ToString());
                    Connection.con.Close();

                    strConnString = $"Host=localhost;Username=student;Password=password;Database=Testing";
                    Connection.con = new NpgsqlConnection(strConnString);
                    Connection.con.Open();

                    StudentWindow studentwindow = new StudentWindow();
                    studentwindow.stud_id = stud_id;
                    studentwindow.Show();
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error message");
                return;
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Программа тестирования обучающихся\nРазработчик: Индюков Е.П.\n2021");
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            //закрыть программу
            Close();
        }
    }

}
