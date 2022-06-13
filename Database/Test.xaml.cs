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
using System.ComponentModel;
using Npgsql;

namespace Database
{
    /// <summary>
    /// Логика взаимодействия для Test.xaml
    /// </summary>
    /// 
    public static class getchildoftype
    {
        public static T GetChildOfType<T>(this DependencyObject depObj)
            where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }
    }

    public partial class Test : Window
    {
        public int stud_id;
        public List<string> all_question_ids = new List<string>();

        //все данные о выбранных вопросах
        public string[] chosen_ids = new string[5];
        public string[] chosen_question = new string[5];
        public string[] chosen_answers = new string[5];
        public string[] chosen_types = new string[5];
        public bool[] chosen_q3 = new bool[5];
        public bool[] chosen_q4 = new bool[5];
        public bool[] chosen_q5 = new bool[5];
        List<string>[] variant_text = new List<string>[5];
        List<string>[] variant_header = new List<string>[5];

        Grid[] grids = new Grid[5];

        DateTime test_time;

        public bool test_done = false;
        public bool closing = false;

        public Test()
        {
            InitializeComponent();
        }
        public Test(DependencyObject depObject) : this()
        {
            Owner = Window.GetWindow(depObject);
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //завершение теста
            try
            {
                TextBox[] answers = new TextBox[] { a1, a2, a3, a4, a5 };
                TextBlock[] answertexts = new TextBlock[] { answertext1, answertext2, answertext3, answertext4, answertext5 };
                TabItem[] tabs = new TabItem[] { tab1, tab2, tab3, tab4, tab5 };
                bool[] ifcorrect = new bool[5];
                int correct_count = 0;
                string test_result;
                if (test_done)
                {
                    this.DialogResult = true;
                    return;
                }
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = Connection.con;

                /*
                if (a1.Text.Length == 0 || a2.Text.Length == 0 || a3.Text.Length == 0 || a4.Text.Length == 0 || a5.Text.Length == 0)
                {
                    throw new Exception("Чтобы завершить тест, введите все ответы");
                }
                */

                for (int i = 0; i < 5; i++)
                {
                    if (!int.TryParse(answers[i].Text, out int a) && chosen_types[i] == "Числовой" && answers[i].Text.Length != 0) 
                    {
                        throw new Exception("В вопросах с числовым ответом введено не число");
                    }
                }
                if (MessageBox.Show("Завершить тест?", "Подтверждение", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    //создание транзакции
                    NpgsqlTransaction transaction;
                    transaction = Connection.con.BeginTransaction();
                    cmd.Transaction = transaction;
                    try
                    {
                        result1.Visibility = Visibility.Visible;
                        result2.Visibility = Visibility.Visible;

                        for (int i = 0; i < 5; i++)
                        {
                            switch (chosen_types[i])
                            {
                                case "Один":
                                    {
                                        int j = 0;
                                        foreach (string s in variant_header[i])//для каждого варианта ответа
                                        {
                                            if ((grids[i].Children.Cast<UIElement>().First(q => Grid.GetRow(q) == j && Grid.GetColumn(q) == 0) as RadioButton).IsChecked == true)
                                            {
                                                answers[i].Text = s;
                                            }
                                            (grids[i].Children.Cast<UIElement>().First(q => Grid.GetRow(q) == j && Grid.GetColumn(q) == 0) as RadioButton).IsEnabled = false;
                                            if (s == chosen_answers[i])
                                            {
                                                (grids[i].Children.Cast<UIElement>().First(q => Grid.GetRow(q) == j && Grid.GetColumn(q) == 1) as TextBlock).Foreground = Brushes.Green;
                                            }
                                            else
                                            {
                                                (grids[i].Children.Cast<UIElement>().First(q => Grid.GetRow(q) == j && Grid.GetColumn(q) == 1) as TextBlock).Foreground = Brushes.Black;
                                            }
                                            j++;
                                        }
                                        if (answers[i].Text == chosen_answers[i])
                                        {
                                            ifcorrect[i] = true;
                                            tabs[i].Foreground = Brushes.Green;
                                            correct_count++;
                                        }
                                        else
                                        {
                                            ifcorrect[i] = false;
                                            tabs[i].Foreground = Brushes.Red;
                                        }
                                        break;
                                    }
                                case "Несколько":
                                    {
                                        int j = 0;
                                        foreach (string s in variant_header[i])//для каждого варианта ответа
                                        {
                                            if ((grids[i].Children.Cast<UIElement>().First(q => Grid.GetRow(q) == j && Grid.GetColumn(q) == 0) as CheckBox).IsChecked == true)
                                            {
                                                if (answers[i].Text.Length == 0)
                                                    answers[i].Text = s;
                                                else
                                                    answers[i].Text += ' ' + s;
                                            }
                                            (grids[i].Children.Cast<UIElement>().First(q => Grid.GetRow(q) == j && Grid.GetColumn(q) == 0) as CheckBox).IsEnabled = false;
                                            if (chosen_answers[i].Contains(s))
                                            {
                                                (grids[i].Children.Cast<UIElement>().First(q => Grid.GetRow(q) == j && Grid.GetColumn(q) == 1) as TextBlock).
                                                    Foreground = Brushes.Green;
                                            }
                                            else
                                            {
                                                (grids[i].Children.Cast<UIElement>().First(q => Grid.GetRow(q) == j && Grid.GetColumn(q) == 1) as TextBlock).
                                                    Foreground = Brushes.Black;
                                            }
                                            j++;
                                        }
                                        if (answers[i].Text == chosen_answers[i])
                                        {
                                            ifcorrect[i] = true;
                                            tabs[i].Foreground = Brushes.Green;
                                            correct_count++;
                                        }
                                        else
                                        {
                                            ifcorrect[i] = false;
                                            tabs[i].Foreground = Brushes.Red;
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        if (answers[i].Text == chosen_answers[i])
                                        {
                                            ifcorrect[i] = true;
                                            answertexts[i].Foreground = Brushes.Green;
                                            tabs[i].Foreground = Brushes.Green;
                                            correct_count++;
                                        }
                                        else
                                        {
                                            ifcorrect[i] = false;
                                            answertexts[i].Foreground = Brushes.Red;
                                            tabs[i].Foreground = Brushes.Red;
                                            answers[i].Text += $" Правильный ответ: {chosen_answers[i]}";
                                        }
                                        answers[i].IsEnabled = false;
                                        break;
                                    }
                            }
                        }
                        if (correct_count > 3)
                        {
                            test_result = "Пройден";
                            result2.Text = "Пройден";
                            result2.Foreground = Brushes.Green;
                        }
                        else
                        {
                            test_result = "Не пройден";
                            result2.Text = "Не пройден";
                            result2.Foreground = Brushes.Red;
                        }

                        //Добавление записи о результате теста
                        var sql = "INSERT INTO test_result(result_id, student_id, subject_id, test_level, attempt_number, " +
                                "test_datetime, question_1_id, question_2_id, question_3_id, question_4_id, question_5_id, " +
                                "answer_1_correctness, answer_2_correctness, answer_3_correctness, answer_4_correctness, answer_5_correctness, " +
                                "test_result) " +
                                "VALUES(DEFAULT, @student_id, " +
                                "(SELECT subject_id FROM subject where subject_name=@subject_name), " +
                                "@test_level, @attempt_number, " +
                                "@test_datetime, @question_1_id, @question_2_id, @question_3_id, @question_4_id, @question_5_id, " +
                                "@answer_1_correctness, @answer_2_correctness, @answer_3_correctness, @answer_4_correctness, @answer_5_correctness, " +
                                "@test_result)";
                        cmd = new NpgsqlCommand(sql, Connection.con);

                        NpgsqlParameter[] param = new NpgsqlParameter[16];
                        param[0] = new NpgsqlParameter("@student_id", stud_id);
                        param[1] = new NpgsqlParameter("@subject_name", subject.Text);
                        param[2] = new NpgsqlParameter("@test_level", level.Text);
                        param[3] = new NpgsqlParameter("@attempt_number", int.Parse(attempt.Text));
                        param[4] = new NpgsqlParameter("@test_datetime", test_time);
                        for (int i = 0; i < 5; i++)
                        {
                            param[i + 5] = new NpgsqlParameter($"@question_{i + 1}_id", int.Parse(chosen_ids[i]));
                            param[i + 10] = new NpgsqlParameter($"@answer_{i + 1}_correctness", ifcorrect[i]);
                        }
                        param[15] = new NpgsqlParameter("@test_result", test_result);
                        for (int i = 0; i < 16; i++)
                        {
                            cmd.Parameters.Add(param[i]);
                        }

                        if (cmd.ExecuteNonQuery() < 1)
                        {
                            throw new Exception("Ошибка при добавлении результата теста");
                        }

                        //Добавление записи об итоговой оценке по предмету
                        if (int.Parse(attempt.Text) == 2 && test_result == "Не пройден" || level.Text == "Отлично" && test_result == "Пройден")
                        {
                            sql = "INSERT INTO subject_result(student_id, subject_id, mark) " +
                                "VALUES(@student_id, (SELECT subject_id FROM subject where subject_name=@subject_name), @mark)";
                            cmd = new NpgsqlCommand(sql, Connection.con);
                            cmd.Parameters.AddWithValue("@student_id", stud_id);
                            cmd.Parameters.AddWithValue("@subject_name", subject.Text);

                            mark1.Visibility = Visibility.Visible;
                            mark2.Visibility = Visibility.Visible;
                            if (test_result == "Пройден")
                            {
                                cmd.Parameters.AddWithValue("@mark", level.Text);
                                mark2.Text = level.Text;
                            }
                            else
                            {
                                switch (level.Text)
                                {
                                    case "Удовлетворительно":
                                        cmd.Parameters.AddWithValue("@mark", "Неудовлетворительно");
                                        mark2.Text = "Неудовлетворительно";
                                        break;
                                    case "Хорошо":
                                        cmd.Parameters.AddWithValue("@mark", "Удовлетворительно");
                                        mark2.Text = "Удовлетворительно";
                                        break;
                                    case "Отлично":
                                        cmd.Parameters.AddWithValue("@mark", "Хорошо");
                                        mark2.Text = "Хорошо";
                                        break;
                                }
                            }

                            if (cmd.ExecuteNonQuery() < 1)
                            {
                                throw new Exception("Ошибка при добавлении оценки по предмету");
                            }
                            finishbutton.IsEnabled = false;
                        }
                        else
                        {
                            finishbutton.Content = "Перейти к следующему тесту";
                        }
                        //фиксирование транзакции
                        test_done = true;
                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        //откат транзакции
                        transaction.Rollback();
                        MessageBox.Show(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            result1.Visibility = Visibility.Collapsed;
            result2.Visibility = Visibility.Collapsed;
            mark1.Visibility = Visibility.Collapsed;
            mark2.Visibility = Visibility.Collapsed;
            //получение всех вопросов по предмету
            string sql = "";
            switch(level.Text)
            {
                case "Удовлетворительно":
                    sql = "SELECT * FROM question WHERE subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name) " +
                        "AND level_satisfactory=true";
                    break;
                case "Хорошо":
                    sql = "SELECT * FROM question WHERE subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name) " +
                        "AND level_good=true";
                    break;
                case "Отлично":
                    sql = "SELECT * FROM question WHERE subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name) " +
                        "AND level_excellent=true";
                    break;
            }
            var cmd = new NpgsqlCommand(sql, Connection.con);
            cmd.Parameters.AddWithValue("@subject_name", subject.Text);
            NpgsqlDataReader sqlReader = cmd.ExecuteReader();
            while (sqlReader.Read())
            {
                all_question_ids.Add(sqlReader["question_id"].ToString());
            }
            sqlReader.Close();
            //исключение уже правильно отвеченных вопросов
            sql = "SELECT * FROM test_result WHERE subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name) " +
                "AND student_id=@student_id";
            cmd = new NpgsqlCommand(sql, Connection.con);
            cmd.Parameters.AddWithValue("@subject_name", subject.Text);
            cmd.Parameters.AddWithValue("@student_id", stud_id);
            sqlReader = cmd.ExecuteReader();
            while (sqlReader.Read())
            {
                for (int i = 0; i < 5; i++)
                {
                    if(bool.Parse(sqlReader[$"answer_{i+1}_correctness"].ToString()))
                    {
                        all_question_ids.Remove(sqlReader[$"question_{i + 1}_id"].ToString());
                    }
                }
            }
            sqlReader.Close();

            //проверка достаточного числа вопросов для составления теста
            if (all_question_ids.Count < 5)
            {
                MessageBox.Show("Недостаточно вопросов для составления теста");
                test_done = true;
                this.DialogResult = false;
                return;
            }
            //выбор случайных вопросов
            Random rnd = new Random();
            TextBlock[] txt = new TextBlock[] { q1, q2, q3, q4, q5 };
            DockPanel[] dock = new DockPanel[] { answer_panel1, answer_panel2, answer_panel3, answer_panel4, answer_panel5 };
            TabItem[] tabItems = new TabItem[] { tab1, tab2, tab3, tab4, tab5 };
            for (int i = 0; i < 5; i++)
            {
                var a = all_question_ids.Count();
                var rnd_id = rnd.Next(a);
                chosen_ids[i] = all_question_ids[rnd_id];
                all_question_ids.RemoveAt(rnd_id);

                sql = "SELECT * FROM question WHERE question_id = @question_id;";
                cmd = new NpgsqlCommand(sql, Connection.con);
                cmd.Parameters.AddWithValue("@question_id", int.Parse(chosen_ids[i]));
                sqlReader = cmd.ExecuteReader();
                sqlReader.Read();

                chosen_question[i] = sqlReader["question_content"].ToString();
                chosen_answers[i] = sqlReader["answer"].ToString();
                chosen_types[i] = sqlReader["question_type"].ToString();
                chosen_q3[i] = bool.Parse(sqlReader["level_satisfactory"].ToString());
                chosen_q4[i] = bool.Parse(sqlReader["level_good"].ToString());
                chosen_q5[i] = bool.Parse(sqlReader["level_excellent"].ToString());

                txt[i].Text = chosen_question[i];
                switch(sqlReader["question_type"].ToString())
                {
                    case "Один":
                        {
                            string [] lines = txt[i].Text.Split('\n');
                            variant_text[i] = new List<string>();
                            variant_header[i] = new List<string>();
                            foreach (string s in lines)//выделение из текста вопроса вариантов ответа
                            {
                                if (s[1] == ')')
                                {
                                    variant_text[i].Add(s.Substring(3,s.Length-3));
                                    variant_header[i].Add(s.Substring(0,1));
                                    txt[i].Text = txt[i].Text.Replace("\n" + s, " ");
                                }
                            }
                            dock[i].Visibility = Visibility.Collapsed;
                            //создание сетки с вариантами ответа
                            Grid DynamicGrid = new Grid();
                            ColumnDefinition gridCol1 = new ColumnDefinition();
                            gridCol1.Width = GridLength.Auto;
                            ColumnDefinition gridCol2 = new ColumnDefinition();
                            DynamicGrid.ColumnDefinitions.Add(gridCol1);
                            DynamicGrid.ColumnDefinitions.Add(gridCol2); 
                            int j = 0;
                            foreach(string s in variant_text[i])
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                DynamicGrid.RowDefinitions.Add(gridRow1);

                                RadioButton rb = new RadioButton();
                                DynamicGrid.Children.Add(rb);
                                Grid.SetRow(rb, j);
                                Grid.SetColumn(rb, 0);

                                TextBlock text = new TextBlock();
                                text.TextWrapping = TextWrapping.Wrap;
                                text.Text = s;
                                DynamicGrid.Children.Add(text);
                                Grid.SetRow(text, j);
                                Grid.SetColumn(text, 1);

                                j++;
                            }

                            grids[i] = DynamicGrid;
                            GroupBox groupBox = new GroupBox();
                            TextBlock txt1 = new TextBlock();
                            txt1.Text = "Выберите один вариант ответа";
                            txt1.Foreground = Brushes.Gray;
                            groupBox.Header = txt1;
                            groupBox.Content = DynamicGrid;
                            DynamicGrid.Margin = new Thickness(10, 10, 10, 10);

                            ((tabItems[i].Content as ScrollViewer).Content as DockPanel).Children.Add(groupBox);
                            break;
                        }
                    case "Несколько":
                        {
                            string[] lines = txt[i].Text.Split('\n');
                            variant_text[i] = new List<string>();
                            variant_header[i] = new List<string>();
                            foreach (string s in lines)//выделение из текста вопроса вариантов ответа
                            {
                                Console.WriteLine(s);
                                if (s[1] == ')')
                                {
                                    variant_text[i].Add(s.Substring(3, s.Length - 3));
                                    variant_header[i].Add(s.Substring(0, 1));
                                    txt[i].Text = txt[i].Text.Replace("\n" + s, " ");
                                }
                            }
                            dock[i].Visibility = Visibility.Collapsed;

                            //создание сетки с вариантами ответа
                            Grid DynamicGrid = new Grid();
                            ColumnDefinition gridCol1 = new ColumnDefinition();
                            gridCol1.Width = GridLength.Auto;
                            ColumnDefinition gridCol2 = new ColumnDefinition();
                            DynamicGrid.ColumnDefinitions.Add(gridCol1);
                            DynamicGrid.ColumnDefinitions.Add(gridCol2);
                            int j = 0;
                            foreach (string s in variant_text[i])
                            {
                                RowDefinition gridRow1 = new RowDefinition();
                                gridRow1.Height = GridLength.Auto;
                                DynamicGrid.RowDefinitions.Add(gridRow1);

                                CheckBox chkbx = new CheckBox();
                                DynamicGrid.Children.Add(chkbx);
                                Grid.SetRow(chkbx, j);
                                Grid.SetColumn(chkbx, 0);

                                TextBlock text = new TextBlock();
                                text.Text = s;
                                text.TextWrapping = TextWrapping.Wrap;
                                DynamicGrid.Children.Add(text);
                                Grid.SetRow(text, j);
                                Grid.SetColumn(text, 1);

                                j++;
                            }

                            grids[i] = DynamicGrid; 
                            GroupBox groupBox = new GroupBox();
                            TextBlock txt1 = new TextBlock();
                            txt1.Text = "Выберите один/несколько вариантов ответа";
                            txt1.Foreground = Brushes.Gray;
                            groupBox.Header = txt1;
                            groupBox.Content = DynamicGrid;
                            DynamicGrid.Margin = new Thickness(10, 10, 10, 10);

                            ((tabItems[i].Content as ScrollViewer).Content as DockPanel).Children.Add(groupBox);
                            break;
                        }
                    case "Текстовый":
                        {
                            txt[i].Text += "\n\n(В ответ введите текстовое значение)\n";
                            break;
                        }
                    case "Числовой":
                        {
                            txt[i].Text += "\n\n(В ответ введите число)\n";
                            break;
                        }
                }
                sqlReader.Close();
            }
            test_time = DateTime.Now;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!test_done) 
            {
                closing = true;
                finishbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                //e.Cancel = true;
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            if((sender as TextBox).Text != "")
            {
                switch((sender as TextBox).Name)
                {
                    case "a1":
                        tab1.Foreground = Brushes.DimGray;
                        break;
                    case "a2":
                        tab2.Foreground = Brushes.DimGray;
                        break;
                    case "a3":
                        tab3.Foreground = Brushes.DimGray;
                        break;
                    case "a4":
                        tab4.Foreground = Brushes.DimGray;
                        break;
                    case "a5":
                        tab5.Foreground = Brushes.DimGray;
                        break;
                }
            }
            else
            {
                switch ((sender as TextBox).Name)
                {
                    case "a1":
                        tab1.Foreground = Brushes.Black;
                        break;
                    case "a2":
                        tab2.Foreground = Brushes.Black;
                        break;
                    case "a3":
                        tab3.Foreground = Brushes.Black;
                        break;
                    case "a4":
                        tab4.Foreground = Brushes.Black;
                        break;
                    case "a5":
                        tab5.Foreground = Brushes.Black;
                        break;
                }
            }
            */
        }
    }
}
