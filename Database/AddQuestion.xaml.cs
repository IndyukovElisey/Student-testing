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
    /// Логика взаимодействия для AddQuestion.xaml
    /// </summary>
    public partial class AddQuestion : Window
    {
        public RadioButton questiontype;
        public AddQuestion()
        {
            InitializeComponent();
        }
        public AddQuestion(DependencyObject depObject) : this()
        {
            Owner = Window.GetWindow(depObject);
        }

        public bool CheckUnique(string str)//проверка, что строка содержит неповторяющиеся символы
        {
            string one = "";
            string two = "";
            for (int i = 0; i < str.Length; i++)
            {
                one = str.Substring(i, 1);
                for (int j = 0; j < str.Length; j++)
                {
                    two = str.Substring(j, 1);
                    if ((one == two) && (i != j))
                        return false;
                }
            }
            return true;
        }

        public string RemoveWhitespace(string input)//возвращает строку без пробелов
        {
            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        public bool CheckMultipleAnswer(string answer, string text)
        {
            //проверка соответствия ответа предложенным вариантам
            string[] lines = text.Split('\n');
            var variant_header = new List<char>();
            if (text.Length < 2)
                return false;
            foreach (string s in lines)//выделение из текста вопроса вариантов ответа
            {
                if (s[1] == ')')
                {
                    variant_header.Add(s[0]);
                }
            }
            answer = RemoveWhitespace(answer);
            if (variant_header.Count == 0 || variant_header.Count < answer.Length)
                return false;
            foreach (char c in variant_header)
            {
                if (answer.Length != 0 && c == answer[0])
                {
                    if (answer.Length == 1)
                        answer = "";
                    else
                        answer = answer.Substring(1);
                }
            }
            if (answer.Length != 0)
            {
                return false;
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //studname1.Text = string.Join(" ", studname1.Text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)); //removes extra spaces
                if (subject.SelectedItem == null) //если не выбран предмет
                {
                    subjectname.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    subjectname.Foreground = new SolidColorBrush(Colors.Black);
                
                if (question.Text.Length == 0) //если вопрос пустой
                {
                    questionname.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    questionname.Foreground = new SolidColorBrush(Colors.Black);

                if (answer.Text.Length == 0) //если ответ пустой
                {
                    answertext.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    answertext.Foreground = new SolidColorBrush(Colors.Black);

                if (satisfactory.IsChecked == false && good.IsChecked == false && excellent.IsChecked == false) //если не выбран ни один уровень вопроса
                {
                    questionlevel.Foreground = new SolidColorBrush(Colors.Red);
                    return;
                }
                else
                    questionlevel.Foreground = new SolidColorBrush(Colors.Black);

                if(Числовой.IsChecked == true && !double.TryParse(answer.Text, out var a)) //если несоответствие числового ответа
                    throw new Exception("В ответе может быть только число");

                if (Один.IsChecked == true && (answer.Text.Length > 1 || !answer.Text.All(c => Char.IsLetter(c)))) //если несоответствие одного варианта ответа
                    throw new Exception("В ответе может быть только одна буква");

                if (Несколько.IsChecked == true && answer.Text.Length > 0 && !answer.Text.All(c => Char.IsLetter(c) || c == ' ')) //если посторонние символы в нескольких вариантах ответа
                    throw new Exception("В ответе могут быть только буквы, разделенные пробелом \n(Пример: \"б в д\")");

                if (Несколько.IsChecked == true)
                {
                    int k = 1;
                    foreach(char c in answer.Text)
                    {
                        if (k == 2 && Char.IsLetter(c))//если четная буква
                            throw new Exception("В ответе могут быть только буквы, разделенные пробелом \n(Пример: \"б в д\")");
                        if (k == 1 && c == ' ')//если нечетный пробел
                            throw new Exception("В ответе могут быть только буквы, разделенные пробелом \n(Пример: \"б в д\")");
                        if (k == 1)
                            k = 2;
                        else
                            k = 1;
                    }
                }

                if (Несколько.IsChecked == true && answer.Text.Length % 2 == 0)  //если посторонние символы в нескольких вариантах ответа
                    throw new Exception("В ответе могут быть только буквы, разделенные пробелом. Возможно введены лишние пробелы");

                if (!CheckUnique(RemoveWhitespace(answer.Text)))  //если повторяющиеся буквы в ответе
                    throw new Exception("В ответе содержатся повторяющиеся буквы");

                //проверка соответствия ответа предложенным вариантам
                if ((Несколько.IsChecked == true || Один.IsChecked == true) && !CheckMultipleAnswer(answer.Text, question.Text))
                    throw new Exception("Буквы в ответе не соответствуют вариантам ответа в тексте вопроса");

                //введение вопроса в БД
                var sql = "INSERT INTO question(question_id, subject_id, question_type, question_content, " +
                "level_satisfactory, level_good, level_excellent, answer) " +
                "VALUES(DEFAULT, (SELECT subject_id FROM subject where subject_name=@subjname), @question_type, @question_content, " +
                "@level_satisfactory, @level_good, @level_excellent, @answer)";
                var cmd = new NpgsqlCommand(sql, Connection.con);

                cmd.Parameters.AddWithValue("@subjname", subject.SelectedItem.ToString());
                cmd.Parameters.AddWithValue("@question_type", questiontype.Name);
                cmd.Parameters.AddWithValue("@question_content", question.Text);
                cmd.Parameters.AddWithValue("@level_satisfactory", satisfactory.IsChecked);
                cmd.Parameters.AddWithValue("@level_good", good.IsChecked);
                cmd.Parameters.AddWithValue("@level_excellent", excellent.IsChecked);
                cmd.Parameters.AddWithValue("@answer", answer.Text);

                if (MessageBox.Show($"Добавить вопрос?\n" +
                                    $"{question.Text}", "Подтверждение", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    if (cmd.ExecuteNonQuery() > 0)
                    {
                        (this.Owner as ViewData).actionstatus.Text = "";
                        (this.Owner as ViewData).actionstatus.Inlines.Add(new Run("Вопрос добавлен: ") { FontWeight = FontWeights.Bold });
                        (this.Owner as ViewData).actionstatus.Inlines.Add(new Run($"{question.Text.Replace('\n', ' ').Replace("\r", " ")}"));
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
                    MessageBox.Show("Вопрос с таким содержимым по данной дисциплине уже существует");
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

        private void Radiobutton_Click(object sender, RoutedEventArgs e)
        {
            questiontype = (RadioButton)sender;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //заполнение combobox дисциплин
            NpgsqlCommand sqlCmd = new NpgsqlCommand("SELECT * FROM subject", Connection.con);
            NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();

            while (sqlReader.Read())
            {
                subject.Items.Add(sqlReader["subject_name"].ToString());
            }

            sqlReader.Close();

            subject.SelectedItem = (this.Owner as ViewData).subjectcombobox.SelectedItem;

            questiontype = Один;
        }
    }
}
