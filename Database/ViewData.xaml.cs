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
    /// Логика взаимодействия для Result.xaml
    /// </summary>
    public class PrintDG
    {
        public void printDG(DataGrid dataGrid, ViewData tempviewdata)
        {
            PrintDialog printDialog = new PrintDialog();

            if (printDialog.ShowDialog() == true)
            {
                FlowDocument fd = new FlowDocument();

                Paragraph p = new Paragraph(new Run("Министерство образования и науки РФ\n" +
                    "Федеральное государственное бюджетное образовательное учреждение высшего образования\n«Мурманский государственный технический университет»"));
                p.FontStyle = dataGrid.FontStyle;
                p.FontFamily = new FontFamily("Times New Roman");
                p.FontSize = 14;
                fd.Blocks.Add(p);

                p = new Paragraph(new Run($"Экзаменационная ведомость\nНаименование дисциплины: {tempviewdata.subjectcombobox.SelectedItem}\n\nФ.И.О. преподавателя: " +
                    $"______________________________________________\n" +
                    $"Форма обучения: ___________. Группа: ___________. Курс __. Семестр __. Дата:  {DateTime.Now.ToString("dd.MM.yyyy")}"));
                p.FontStyle = dataGrid.FontStyle;
                p.FontFamily = new FontFamily("Times New Roman");
                p.FontSize = 16;
                fd.Blocks.Add(p);

                Table table = new Table();
                TableRowGroup tableRowGroup = new TableRowGroup();
                TableRow r = new TableRow();
                fd.PageWidth = printDialog.PrintableAreaWidth;
                fd.PageHeight = printDialog.PrintableAreaHeight;
                fd.BringIntoView();

                fd.TextAlignment = TextAlignment.Center;
                fd.ColumnWidth = 500;
                table.CellSpacing = 0;

                var headerList = dataGrid.Columns.Select(e => e.Header.ToString()).ToList();
                List<dynamic> bindList = new List<dynamic>();

                for (int j = 0; j < 5; j++)
                {
                    switch (j)
                    {
                        case 0:
                            r.Cells.Add(new TableCell(new Paragraph(new Run("№"))));
                            r.Cells[j].ColumnSpan = 4;
                            break;
                        case 1:
                            r.Cells.Add(new TableCell(new Paragraph(new Run("Фамилия, имя, отчество"))));
                            r.Cells[j].ColumnSpan = 28;
                            break;
                        case 2:
                            r.Cells.Add(new TableCell(new Paragraph(new Run("№ зачетной книжки"))));
                            r.Cells[j].ColumnSpan = 8;
                            break;
                        case 3:
                            r.Cells.Add(new TableCell(new Paragraph(new Run("Оценка"))));
                            r.Cells[j].ColumnSpan = 15;
                            break;
                        case 4:
                            r.Cells.Add(new TableCell(new Paragraph(new Run("Подпись преподавателя"))));
                            r.Cells[j].ColumnSpan = 10;
                            break;
                    }
                    //r.Cells[j].ColumnSpan = 4;
                    r.Cells[j].Padding = new Thickness(4);
                    r.Cells[j].BorderBrush = Brushes.Black;
                    //r.Cells[j].FontWeight = FontWeights.Bold;
                    r.Cells[j].BorderThickness = new Thickness(1, 1, 1, 1);

                    var binding = (dataGrid.Columns[j] as DataGridBoundColumn).Binding as Binding;
                    bindList.Add(binding.Path.Path);
                }

                tableRowGroup.Rows.Add(r);
                table.RowGroups.Add(tableRowGroup);
                int excellent = 0;
                int good = 0;
                int satisfactory = 0;
                int unsatisfactory = 0;
                int attended = 0;
                int absent = 0;

                var cmd = new NpgsqlCommand("SELECT * FROM student_subject NATURAL JOIN student " +
                    "NATURAL JOIN subject WHERE subject_name=@subject_name ORDER BY full_name", Connection.con);
                cmd.Parameters.AddWithValue("@subject_name", tempviewdata.subjectcombobox.SelectedItem.ToString());
                NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                int i = 0;
                int k = 1;
                while (sqlReader.Read())
                {
                    Result_Row row;

                    //if (dataGrid.ItemsSource.ToString().ToLower() == "result_List")
                    if (dataGrid.Items.Count == i)
                        i--;
                    row = (Result_Row)dataGrid.Items.GetItemAt(i);

                    table.BorderBrush = Brushes.Black;
                    table.BorderThickness = new Thickness(1, 1, 1, 1);
                    table.FontStyle = dataGrid.FontStyle;
                    table.FontFamily = dataGrid.FontFamily;
                    table.FontSize = 13;
                    tableRowGroup = new TableRowGroup();
                    r = new TableRow();

                    if(row.full_name == sqlReader["full_name"].ToString())
                    {
                        r.Cells.Add(new TableCell(new Paragraph(new Run($"{k}"))));
                        r.Cells.Add(new TableCell(new Paragraph(new Run(sqlReader["full_name"].ToString()))));
                        r.Cells.Add(new TableCell(new Paragraph(new Run(sqlReader["credit_book_number"].ToString()))));
                        r.Cells.Add(new TableCell(new Paragraph(new Run(row.mark))));
                        r.Cells.Add(new TableCell(new Paragraph(new Run("            "))));
                        switch (row.mark)
                        {
                            case "Отлично":
                                excellent++;
                                attended++;
                                break;
                            case "Хорошо":
                                good++;
                                attended++;
                                break;
                            case "Удовлетворительно":
                                satisfactory++;
                                attended++;
                                break;
                            case "Неудовлетворительно":
                                unsatisfactory++;
                                attended++;
                                break;
                        }
                    }
                    else
                    {
                        r.Cells.Add(new TableCell(new Paragraph(new Run($"{k}"))));
                        r.Cells.Add(new TableCell(new Paragraph(new Run(sqlReader["full_name"].ToString()))));
                        r.Cells.Add(new TableCell(new Paragraph(new Run(sqlReader["credit_book_number"].ToString()))));
                        r.Cells.Add(new TableCell(new Paragraph(new Run("-"))));
                        r.Cells.Add(new TableCell(new Paragraph(new Run("            "))));
                        absent++;
                        i--;
                    }
                    
                    for (int j = 0; j < 5; j++)
                    {
                        switch (j)
                        {
                            case 0:
                                r.Cells[j].ColumnSpan = 4;
                                break;
                            case 1:
                                r.Cells[j].ColumnSpan = 28;
                                r.Cells[j].TextAlignment = TextAlignment.Left;
                                break;
                            case 2:
                                r.Cells[j].ColumnSpan = 8;
                                break;
                            case 3:
                                r.Cells[j].ColumnSpan = 15;
                                break;
                            case 4:
                                r.Cells[j].ColumnSpan = 10;
                                break;
                        }
                        //r.Cells[j].ColumnSpan = 4;
                        r.Cells[j].Padding = new Thickness(4);
                        r.Cells[j].BorderBrush = Brushes.Black;
                        r.Cells[j].BorderThickness = new Thickness(1, 1, 1, 1);
                    }
                    tableRowGroup.Rows.Add(r);
                    table.RowGroups.Add(tableRowGroup);
                    i++;
                    k++;
                }
                sqlReader.Close();
                fd.Blocks.Add(table);

                p = new Paragraph(new Run($"Число студентов на экзамене   {attended}  \n" +
                    $"Из них сдали на «отлично»   {excellent}  \n" +
                    $"«хорошо»   {good}  \n" +
                    $"«удовлетворительно»   {satisfactory}  \n" +
                    $"«неудовлетворительно»   {unsatisfactory}  \n" +
                    $"Число студентов, не явившихся на экзамен   {table.RowGroups.Count - attended - 1}  "));
                p.FontStyle = dataGrid.FontStyle;
                p.FontFamily = new FontFamily("Times New Roman");
                p.FontSize = 16;
                p.TextAlignment = TextAlignment.Right;
                fd.Blocks.Add(p);

                p = new Paragraph(new Run($"Директор / Декан   ________________________"));
                p.FontStyle = dataGrid.FontStyle;
                p.FontFamily = new FontFamily("Times New Roman");
                p.FontSize = 16;
                p.TextAlignment = TextAlignment.Left;
                fd.Blocks.Add(p);

                printDialog.PrintDocument(((IDocumentPaginatorSource)fd).DocumentPaginator, "");
            }
        }
    }

    public class Result_Row // класс для представления результатов в datagrid
    {
        public string full_name { get; set; }
        public string subject_name { get; set; }
        public string test_level { get; set; }
        public int attempt { get; set; }
        public DateTime datetime { get; set; }
        public int correct_count { get; set; }
        public string test_result { get; set; }
        public string mark { get; set; }
        public int student_id { get; set; }
    }
    public class Question_Row //класс для представления вопросов в datagrid
    {
        public string subject_name { get; set; }
        public string content_collapsed { get; set; }
        public string answer { get; set; }
        public string type { get; set; }
        public bool satisfactory { get; set; }
        public bool good { get; set; }
        public bool excellent { get; set; }
        public int id { get; set; }
        public string content_full { get; set; }
    }
    public class Student_Row //класс для представления студентов в datagrid
    {
        public string full_name { get; set; }
        public DateTime date_of_birth { get; set; }
        public string sex { get; set; }
        public int credit_book_number { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public int id { get; set; }
    }

    public partial class ViewData : Window
    {
        public int stud_id;
        public enum ViewTypeEnum//тип наполнения
        {
            student_test_results,
            student_subject_marks,
            admin_test_results,
            admin_subject_marks,
            admin_subject_student,
            admin_questions,
            admin_students,
            admin_subjects
        }
        public ViewTypeEnum view_type;


        public ViewData()
        {
            InitializeComponent();
        }

        public void UpdateComboBox()
        {
            string student = "";
            string subject = "";
            if (studentcombobox.SelectedItem != null) 
                student = studentcombobox.SelectedItem.ToString();
            if (subjectcombobox.SelectedItem != null)
                subject = subjectcombobox.SelectedItem.ToString();

            subjectcombobox.Items.Clear();
            studentcombobox.Items.Clear();
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

            if (subjectcombobox.Items.Contains(subject))
                subjectcombobox.SelectedItem = subject;
            if (studentcombobox.Items.Contains(student))
                subjectcombobox.SelectedItem = student;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateComboBox();
                subjectcombobox.IsEnabled = false;
                subjecttext.Foreground = Brushes.Gray;
                studentcombobox.IsEnabled = true;
                studenttext.Foreground = Brushes.Black;

                switch (view_type)
                {
                    case ViewTypeEnum.student_test_results:
                        {
                            adminviewgrid.Visibility = Visibility.Collapsed;
                            menu.Visibility = Visibility.Collapsed;
                            statusbar.Visibility = Visibility.Collapsed;
                            //получение результатов тестов
                            var sql = "SELECT * FROM test_result NATURAL JOIN subject WHERE student_id = @student_id";
                            NpgsqlCommand cmd = new NpgsqlCommand(sql, Connection.con);
                            cmd.Parameters.AddWithValue("@student_id", stud_id);

                            NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                            List<Result_Row> result_List = new List<Result_Row>();
                            while (sqlReader.Read())
                            {
                                int correct_count = 0;
                                for (int i = 0; i < 5; i++)
                                {
                                    if (bool.Parse(sqlReader[$"answer_{i + 1}_correctness"].ToString()) == true)
                                        correct_count++;
                                }
                                result_List.Add(new Result_Row
                                {
                                    subject_name = sqlReader["subject_name"].ToString(),
                                    test_level = sqlReader["test_level"].ToString(),
                                    attempt = int.Parse(sqlReader["attempt_number"].ToString()),
                                    datetime = DateTime.Parse(sqlReader["test_datetime"].ToString()),
                                    correct_count = correct_count,
                                    test_result = sqlReader["test_result"].ToString()
                                });
                            }
                            sqlReader.Close();

                            result_grid.ItemsSource = result_List;
                            result_grid.Columns[0].Visibility = Visibility.Collapsed;
                            result_grid.Columns[1].Header = "Дисциплина";
                            result_grid.Columns[2].Header = "Уровень билета";
                            result_grid.Columns[3].Header = "Попытка";
                            result_grid.Columns[4].Header = "Время";
                            result_grid.Columns[5].Header = "Число верных ответов";
                            result_grid.Columns[6].Header = "Результат билета";
                            result_grid.Columns[7].Visibility = Visibility.Collapsed;
                            result_grid.Columns[8].Visibility = Visibility.Collapsed;
                            this.Title = "Результаты билетов";
                            break;
                        }
                    case ViewTypeEnum.student_subject_marks:
                        {
                            adminviewgrid.Visibility = Visibility.Collapsed;
                            menu.Visibility = Visibility.Collapsed;
                            statusbar.Visibility = Visibility.Collapsed;
                            //получение оценок по предметам
                            NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM subject_result " +
                                "NATURAL JOIN subject where student_id = @student_id ORDER BY subject_name", Connection.con);
                            cmd.Parameters.AddWithValue("@student_id", stud_id);
                            NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                            List<Result_Row> result_List = new List<Result_Row>();
                            while (sqlReader.Read())
                            {
                                result_List.Add(new Result_Row
                                {
                                    subject_name = sqlReader["subject_name"].ToString(),
                                    mark = sqlReader["mark"].ToString()
                                });
                            }
                            sqlReader.Close();

                            result_grid.ItemsSource = result_List;
                            result_grid.Columns[0].Visibility = Visibility.Collapsed;
                            result_grid.Columns[1].Header = "Дисциплина";
                            result_grid.Columns[2].Visibility = Visibility.Collapsed;
                            result_grid.Columns[3].Visibility = Visibility.Collapsed;
                            result_grid.Columns[4].Visibility = Visibility.Collapsed;
                            result_grid.Columns[5].Visibility = Visibility.Collapsed;
                            result_grid.Columns[6].Visibility = Visibility.Collapsed;
                            result_grid.Columns[7].Header = "Оценка";
                            result_grid.Columns[8].Visibility = Visibility.Collapsed;
                            this.Title = "Оценки";
                            break;
                        }
                    default:
                        {
                            showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            break;
                        }
                        /*
                    case ViewTypeEnum.admin_test_results:
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
                            addbutton.IsEnabled = false;
                            deletebutton.IsEnabled = false;
                            break;
                        }
                    case ViewTypeEnum.admin_subject_marks:
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
                            this.Title = "Оценки";
                            addbutton.IsEnabled = false;
                            deletebutton.IsEnabled = false;
                            break;
                        }
                    case ViewTypeEnum.admin_subject_student:
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
                            this.Title = "Студенты с дисциплинами";
                            break;
                        }
                    case ViewTypeEnum.admin_questions:
                        {
                            //заполнение combobox-ов
                            NpgsqlCommand sqlCmd = new NpgsqlCommand("SELECT * FROM subject ORDER BY subject_name", Connection.con);
                            NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();
                            while (sqlReader.Read())
                            {
                                subjectcombobox.Items.Add(sqlReader["subject_name"].ToString());
                            }
                            sqlReader.Close();
                            studentcombobox.Visibility = Visibility.Collapsed;
                            studenttext.Visibility = Visibility.Collapsed;
                            this.Title = "Вопросы";
                            break;
                        }
                    case ViewTypeEnum.admin_students:
                        {
                            //заполнение combobox-ов
                            NpgsqlCommand sqlCmd = new NpgsqlCommand("SELECT * FROM student ORDER BY full_name", Connection.con);
                            NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();
                            while (sqlReader.Read())
                            {
                                studentcombobox.Items.Add(sqlReader["full_name"].ToString());
                            }
                            sqlReader.Close();
                            subjectcombobox.Visibility = Visibility.Collapsed;
                            subjecttext.Visibility = Visibility.Collapsed;
                            this.Title = "Студенты";
                            break;
                        }
                    case ViewTypeEnum.admin_subjects:
                        {
                            //заполнение combobox-ов
                            NpgsqlCommand sqlCmd = new NpgsqlCommand("SELECT * FROM subject ORDER BY subject_name", Connection.con);
                            NpgsqlDataReader sqlReader = sqlCmd.ExecuteReader();
                            while (sqlReader.Read())
                            {
                                subjectcombobox.Items.Add(sqlReader["subject_name"].ToString());
                            }
                            sqlReader.Close();
                            studentcombobox.Visibility = Visibility.Collapsed;
                            studenttext.Visibility = Visibility.Collapsed;
                            this.Title = "Дисциплины";
                            break;
                        }*/
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Show_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (view_type)
                {
                    case ViewTypeEnum.admin_subject_marks:
                        {
                            //получение оценок по предметам
                            NpgsqlCommand cmd = new NpgsqlCommand();
                            if (subjectcombobox.SelectedItem == null && studentcombobox.SelectedItem == null)//ничего не выбрано
                            {
                                cmd = new NpgsqlCommand("SELECT full_name, subject_name, mark FROM subject_result NATURAL JOIN subject NATURAL JOIN student ORDER BY full_name", Connection.con);
                            }
                            if (subjectcombobox.SelectedItem != null && studentcombobox.SelectedItem == null)//выбран только предмет
                            {
                                cmd = new NpgsqlCommand("SELECT full_name, subject_name, mark FROM subject_result NATURAL JOIN subject NATURAL JOIN student " +
                                    "WHERE subject_name=@subject_name ORDER BY full_name", Connection.con);
                                cmd.Parameters.AddWithValue("@subject_name", subjectcombobox.SelectedItem.ToString());
                            }
                            if (subjectcombobox.SelectedItem == null && studentcombobox.SelectedItem != null)//выбран только студент
                            {
                                cmd = new NpgsqlCommand("SELECT full_name, subject_name, mark FROM subject_result NATURAL JOIN subject NATURAL JOIN student " +
                                    "WHERE full_name=@full_name ORDER BY full_name", Connection.con);
                                cmd.Parameters.AddWithValue("@full_name", studentcombobox.SelectedItem.ToString());
                            }
                            if (subjectcombobox.SelectedItem != null && studentcombobox.SelectedItem != null)//выбраны оба
                            {
                                cmd = new NpgsqlCommand("SELECT full_name, subject_name, mark FROM subject_result NATURAL JOIN subject NATURAL JOIN student " +
                                    "WHERE full_name=@full_name AND subject_name=@subject_name ORDER BY full_name", Connection.con);
                                cmd.Parameters.AddWithValue("@full_name", studentcombobox.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@subject_name", subjectcombobox.SelectedItem.ToString());
                            }
                            NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                            List<Result_Row> result_List = new List<Result_Row>();
                            while (sqlReader.Read())
                            {
                                result_List.Add(new Result_Row
                                {
                                    full_name = sqlReader["full_name"].ToString(),
                                    subject_name = sqlReader["subject_name"].ToString(),
                                    mark = sqlReader["mark"].ToString()
                                });
                            }
                            sqlReader.Close();

                            result_grid.ItemsSource = result_List;
                            result_grid.Columns[0].Header = "Студент";
                            result_grid.Columns[1].Header = "Дисциплина";
                            result_grid.Columns[2].Visibility = Visibility.Collapsed;
                            result_grid.Columns[3].Visibility = Visibility.Collapsed;
                            result_grid.Columns[4].Visibility = Visibility.Collapsed;
                            result_grid.Columns[5].Visibility = Visibility.Collapsed;
                            result_grid.Columns[6].Visibility = Visibility.Collapsed;
                            result_grid.Columns[7].Header = "Оценка";
                            result_grid.Columns[8].Visibility = Visibility.Collapsed;
                            break;
                        }
                    case ViewTypeEnum.admin_test_results:
                        {
                            //получение результатов тестов
                            NpgsqlCommand cmd = new NpgsqlCommand();
                            if (subjectcombobox.SelectedItem == null && studentcombobox.SelectedItem == null)//ничего не выбрано
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM test_result NATURAL JOIN student NATURAL JOIN subject ORDER BY result_id DESC", Connection.con);
                            }
                            if (subjectcombobox.SelectedItem != null && studentcombobox.SelectedItem == null)//выбран только предмет
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM test_result NATURAL JOIN student NATURAL JOIN subject WHERE subject_name=@subject_name ORDER BY result_id DESC", Connection.con);
                                cmd.Parameters.AddWithValue("@subject_name", subjectcombobox.SelectedItem.ToString());
                            }
                            if (subjectcombobox.SelectedItem == null && studentcombobox.SelectedItem != null)//выбран только студент
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM test_result NATURAL JOIN student NATURAL JOIN subject WHERE full_name=@full_name ORDER BY result_id DESC", Connection.con);
                                cmd.Parameters.AddWithValue("@full_name", studentcombobox.SelectedItem.ToString());
                            }
                            if (subjectcombobox.SelectedItem != null && studentcombobox.SelectedItem != null)//выбраны оба
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM test_result NATURAL JOIN student NATURAL JOIN subject WHERE subject_name=@subject_name AND full_name=@full_name ORDER BY result_id DESC", Connection.con);
                                cmd.Parameters.AddWithValue("@full_name", studentcombobox.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@subject_name", subjectcombobox.SelectedItem.ToString());
                            }

                            NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                            List<Result_Row> result_List = new List<Result_Row>();
                            while (sqlReader.Read())
                            {
                                int correct_count = 0;
                                for (int i = 0; i < 5; i++)
                                {
                                    if (bool.Parse(sqlReader[$"answer_{i + 1}_correctness"].ToString()) == true)
                                        correct_count++;
                                }
                                result_List.Add(new Result_Row
                                {
                                    full_name = sqlReader["full_name"].ToString(),
                                    subject_name = sqlReader["subject_name"].ToString(),
                                    test_level = sqlReader["test_level"].ToString(),
                                    attempt = int.Parse(sqlReader["attempt_number"].ToString()),
                                    datetime = DateTime.Parse(sqlReader["test_datetime"].ToString()),
                                    correct_count = correct_count,
                                    test_result = sqlReader["test_result"].ToString()
                                });
                            }
                            sqlReader.Close();

                            result_grid.ItemsSource = result_List;
                            result_grid.Columns[0].Header = "Студент";
                            result_grid.Columns[1].Header = "Дисциплина";
                            result_grid.Columns[2].Header = "Уровень билета";
                            result_grid.Columns[3].Header = "Попытка";
                            result_grid.Columns[4].Header = "Дата и время";
                            result_grid.Columns[5].Header = "Число верных ответов";
                            result_grid.Columns[6].Header = "Результат билета";
                            result_grid.Columns[7].Visibility = Visibility.Collapsed;
                            result_grid.Columns[8].Visibility = Visibility.Collapsed;
                            break;
                        }
                    case ViewTypeEnum.admin_subject_student:
                        {
                            //получение связей студент-дисциплина
                            NpgsqlCommand cmd = new NpgsqlCommand();
                            if (subjectcombobox.SelectedItem == null && studentcombobox.SelectedItem == null)//ничего не выбрано
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM student_subject NATURAL JOIN student NATURAL JOIN subject ORDER BY full_name", Connection.con);
                            }
                            if (subjectcombobox.SelectedItem != null && studentcombobox.SelectedItem == null)//выбран только предмет
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM student_subject NATURAL JOIN student NATURAL JOIN subject " +
                                    "WHERE subject_name=@subject_name ORDER BY full_name", Connection.con);
                                cmd.Parameters.AddWithValue("@subject_name", subjectcombobox.SelectedItem.ToString());
                            }
                            if (subjectcombobox.SelectedItem == null && studentcombobox.SelectedItem != null)//выбран только студент
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM student_subject NATURAL JOIN student NATURAL JOIN subject " +
                                    "WHERE full_name=@full_name ORDER BY full_name", Connection.con);
                                cmd.Parameters.AddWithValue("@full_name", studentcombobox.SelectedItem.ToString());
                            }
                            if (subjectcombobox.SelectedItem != null && studentcombobox.SelectedItem != null)//выбраны оба
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM student_subject NATURAL JOIN student NATURAL JOIN subject " +
                                    "WHERE subject_name=@subject_name AND full_name=@full_name ORDER BY full_name", Connection.con);
                                cmd.Parameters.AddWithValue("@full_name", studentcombobox.SelectedItem.ToString());
                                cmd.Parameters.AddWithValue("@subject_name", subjectcombobox.SelectedItem.ToString());
                            }

                            NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                            List<Result_Row> result_List = new List<Result_Row>();
                            while (sqlReader.Read())
                            {
                                result_List.Add(new Result_Row
                                {
                                    full_name = sqlReader["full_name"].ToString(),
                                    subject_name = sqlReader["subject_name"].ToString(),
                                    student_id = int.Parse(sqlReader["student_id"].ToString())
                                });
                            }
                            sqlReader.Close();

                            result_grid.ItemsSource = result_List;
                            result_grid.Columns[0].Header = "Студент";
                            result_grid.Columns[1].Header = "Дисциплина";
                            result_grid.Columns[2].Visibility = Visibility.Collapsed;
                            result_grid.Columns[3].Visibility = Visibility.Collapsed;
                            result_grid.Columns[4].Visibility = Visibility.Collapsed;
                            result_grid.Columns[5].Visibility = Visibility.Collapsed;
                            result_grid.Columns[6].Visibility = Visibility.Collapsed;
                            result_grid.Columns[7].Visibility = Visibility.Collapsed;
                            result_grid.Columns[8].Visibility = Visibility.Collapsed;
                            break;
                        }
                    case ViewTypeEnum.admin_questions:
                        {
                            //получение вопросов
                            NpgsqlCommand cmd = new NpgsqlCommand();
                            if (subjectcombobox.SelectedItem == null)//предмет не выбран
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM question NATURAL JOIN subject ORDER BY subject_name", Connection.con);
                            }
                            if (subjectcombobox.SelectedItem != null)//предмет выбран 
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM question NATURAL JOIN subject WHERE subject_name=@subject_name", Connection.con);
                                cmd.Parameters.AddWithValue("@subject_name", subjectcombobox.SelectedItem.ToString());
                            }

                            NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                            List<Question_Row> question_List = new List<Question_Row>();
                            while (sqlReader.Read())
                            {
                                question_List.Add(new Question_Row
                                {
                                    subject_name = sqlReader["subject_name"].ToString(),
                                    content_collapsed = sqlReader["question_content"].ToString().Replace('\n', ' ').Replace("\r", " "),
                                    answer = sqlReader["answer"].ToString(),
                                    type = sqlReader["question_type"].ToString(),
                                    satisfactory = bool.Parse(sqlReader["level_satisfactory"].ToString()),
                                    good = bool.Parse(sqlReader["level_good"].ToString()),
                                    excellent = bool.Parse(sqlReader["level_excellent"].ToString()),
                                    id = int.Parse(sqlReader["question_id"].ToString()),
                                    content_full = sqlReader["question_content"].ToString()
                                });
                            }
                            sqlReader.Close();

                            result_grid.ItemsSource = question_List;
                            result_grid.Columns[0].Header = "Дисциплина";
                            result_grid.Columns[1].Header = "Вопрос";
                            result_grid.Columns[1].Width = 400;
                            result_grid.Columns[2].Header = "Ответ";
                            result_grid.Columns[3].Header = "Тип ответа";
                            result_grid.Columns[4].Header = "Удовлетв.";
                            result_grid.Columns[5].Header = "Хорошо";
                            result_grid.Columns[6].Header = "Отлично";
                            result_grid.Columns[7].Visibility = Visibility.Collapsed;
                            result_grid.Columns[8].Visibility = Visibility.Collapsed;
                            break;
                        }
                    case ViewTypeEnum.admin_students:
                        {
                            //получение студентов
                            NpgsqlCommand cmd = new NpgsqlCommand();
                            if (studentcombobox.SelectedItem == null)//студент не выбран
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM student ORDER BY full_name", Connection.con);
                            }
                            if (studentcombobox.SelectedItem != null)//студент выбран 
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM student WHERE full_name=@full_name ORDER BY full_name", Connection.con);
                                cmd.Parameters.AddWithValue("@full_name", studentcombobox.SelectedItem.ToString());
                            }

                            NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                            List<Student_Row> student_List = new List<Student_Row>();
                            while (sqlReader.Read())
                            {
                                student_List.Add(new Student_Row
                                {
                                    full_name = sqlReader["full_name"].ToString(),
                                    date_of_birth = DateTime.Parse(sqlReader["date_of_birth"].ToString()),
                                    sex = sqlReader["sex"].ToString(),
                                    credit_book_number = int.Parse(sqlReader["credit_book_number"].ToString()),
                                    login = sqlReader["login"].ToString(),
                                    password = sqlReader["password"].ToString(),
                                    id = int.Parse(sqlReader["student_id"].ToString())
                                });
                            }
                            sqlReader.Close();

                            result_grid.ItemsSource = student_List;
                            result_grid.Columns[0].Header = "ФИО";
                            result_grid.Columns[1].Header = "Дата рождения";
                            result_grid.Columns[2].Header = "Пол";
                            result_grid.Columns[3].Header = "Номер зачетной книжки";
                            result_grid.Columns[4].Header = "Логин";
                            result_grid.Columns[5].Header = "Пароль";
                            result_grid.Columns[6].Visibility = Visibility.Collapsed;
                            break;
                        }
                    case ViewTypeEnum.admin_subjects:
                        {
                            //получение дисциплин
                            NpgsqlCommand cmd = new NpgsqlCommand();
                            if (subjectcombobox.SelectedItem == null)//предмет не выбран
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM subject ORDER BY subject_name", Connection.con);
                            }
                            if (subjectcombobox.SelectedItem != null)//предмет выбран 
                            {
                                cmd = new NpgsqlCommand("SELECT * FROM subject WHERE subject_name=@subject_name ORDER BY subject_name", Connection.con);
                                cmd.Parameters.AddWithValue("@subject_name", subjectcombobox.SelectedItem.ToString());
                            }

                            NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                            List<Question_Row> subject_List = new List<Question_Row>();
                            while (sqlReader.Read())
                            {
                                subject_List.Add(new Question_Row
                                {
                                    subject_name = sqlReader["subject_name"].ToString(),
                                    id = int.Parse(sqlReader["subject_id"].ToString())
                                });
                            }
                            sqlReader.Close();

                            result_grid.ItemsSource = subject_List;
                            result_grid.Columns[0].Header = "Дисциплина";
                            result_grid.Columns[1].Visibility = Visibility.Collapsed;
                            result_grid.Columns[2].Visibility = Visibility.Collapsed;
                            result_grid.Columns[3].Visibility = Visibility.Collapsed;
                            result_grid.Columns[4].Visibility = Visibility.Collapsed;
                            result_grid.Columns[5].Visibility = Visibility.Collapsed;
                            result_grid.Columns[6].Visibility = Visibility.Collapsed;
                            result_grid.Columns[7].Visibility = Visibility.Collapsed;
                            result_grid.Columns[8].Visibility = Visibility.Collapsed;
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            subjectcombobox.SelectedItem = null;
            subjectcombobox.Text = null;
            studentcombobox.SelectedItem = null;
            studentcombobox.Text = null;
            showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (result_grid.SelectedItem == null)
            {
                MessageBox.Show("Выберите строку для удаления");
            }
            else
            {
                try
                {
                    switch (view_type)
                    {
                        case ViewTypeEnum.admin_questions:
                            {
                                var cmd = new NpgsqlCommand("DELETE FROM question WHERE question_id=@question_id", Connection.con);
                                cmd.Parameters.AddWithValue("@question_id", ((Question_Row)result_grid.SelectedItem).id);
                                if (MessageBox.Show($"Удалить вопрос?\n" +
                                    $"{((Question_Row)result_grid.SelectedItem).content_collapsed.Substring(0, Math.Min(((Question_Row)result_grid.SelectedItem).content_collapsed.Length, 150))}", "Подтверждение удаления", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                                {
                                    if (cmd.ExecuteNonQuery() != -1)
                                    {
                                        actionstatus.Text = "";
                                        actionstatus.Inlines.Add(new Run("Вопрос удален: ") { FontWeight = FontWeights.Bold });
                                        actionstatus.Inlines.Add(new Run($"{((Question_Row)result_grid.SelectedItem).content_collapsed}"));
                                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                    }
                                    else
                                        MessageBox.Show("Что-то пошло не так");
                                }
                                else
                                {
                                    return;
                                }
                                break;
                            }
                        case ViewTypeEnum.admin_subjects:
                            {
                                var cmd = new NpgsqlCommand("DELETE FROM subject WHERE subject_id=@subject_id", Connection.con);
                                cmd.Parameters.AddWithValue("@subject_id", ((Question_Row)result_grid.SelectedItem).id);
                                if (MessageBox.Show($"Удалить дисциплину?\n" +
                                    $"{((Question_Row)result_grid.SelectedItem).subject_name}", "Подтверждение удаления", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                                {
                                    if (cmd.ExecuteNonQuery() != -1)
                                    {
                                        actionstatus.Text = "";
                                        actionstatus.Inlines.Add(new Run("Дисциплина удалена: ") { FontWeight = FontWeights.Bold });
                                        actionstatus.Inlines.Add(new Run($"{((Question_Row)result_grid.SelectedItem).subject_name}"));
                                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                    }
                                    else
                                        MessageBox.Show("Что-то пошло не так");
                                }
                                else
                                {
                                    return;
                                }
                                UpdateComboBox();
                                break;
                            }
                        case ViewTypeEnum.admin_students:
                            {
                                var cmd = new NpgsqlCommand("DELETE FROM student WHERE student_id=@student_id", Connection.con);
                                cmd.Parameters.AddWithValue("@student_id", ((Student_Row)result_grid.SelectedItem).id);
                                if (MessageBox.Show($"Удалить студента?\n" +
                                    $"{((Student_Row)result_grid.SelectedItem).full_name}", "Подтверждение удаления", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                                {
                                    if (cmd.ExecuteNonQuery() != -1)
                                    {
                                        actionstatus.Text = "";
                                        actionstatus.Inlines.Add(new Run("Студент удален: ") { FontWeight = FontWeights.Bold });
                                        actionstatus.Inlines.Add(new Run($"{((Student_Row)result_grid.SelectedItem).full_name}"));
                                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                    }
                                    else
                                        MessageBox.Show("Что-то пошло не так");
                                }
                                else
                                {
                                    return;
                                }
                                UpdateComboBox();
                                break;
                            }
                        case ViewTypeEnum.admin_subject_student:
                            {
                                var cmd = new NpgsqlCommand("DELETE FROM student_subject WHERE student_id=@student_id " +
                                    "AND subject_id=(SELECT subject_id FROM subject WHERE subject_name=@subject_name)", Connection.con);
                                cmd.Parameters.AddWithValue("@student_id", ((Result_Row)result_grid.SelectedItem).student_id);
                                cmd.Parameters.AddWithValue("@subject_name", ((Result_Row)result_grid.SelectedItem).subject_name);
                                if (MessageBox.Show($"Удалить связь?\n" +
                                    $"{((Result_Row)result_grid.SelectedItem).full_name} - {((Result_Row)result_grid.SelectedItem).subject_name}", "Подтверждение удаления", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                                {
                                    if (cmd.ExecuteNonQuery() != -1)
                                    {
                                        actionstatus.Text = "";
                                        actionstatus.Inlines.Add(new Run("Связь студент-дисциплина удалена: ") { FontWeight = FontWeights.Bold });
                                        actionstatus.Inlines.Add(new Run($"{((Result_Row)result_grid.SelectedItem).full_name} - {((Result_Row)result_grid.SelectedItem).subject_name}"));
                                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                    }
                                    else
                                        MessageBox.Show("Что-то пошло не так");
                                }
                                else
                                {
                                    return;
                                }
                                break;
                            }
                        case ViewTypeEnum.admin_subject_marks:
                            {
                                if (MessageBox.Show($"Внимание! Удаление оценки по предмету также приведет к удалению соответствующего последнего результата билета\n" +
                                    $"Удалить оценку?\n" +
                                    $"{((Result_Row)result_grid.SelectedItem).full_name} - {((Result_Row)result_grid.SelectedItem).subject_name} - {((Result_Row)result_grid.SelectedItem).mark}",
                                    "Подтверждение удаления", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                                {
                                    NpgsqlCommand cmd = new NpgsqlCommand();
                                    cmd.Connection = Connection.con;
                                    //создание транзакции
                                    NpgsqlTransaction transaction;
                                    transaction = Connection.con.BeginTransaction();
                                    cmd.Transaction = transaction;
                                    try
                                    {
                                        //удаление оценки
                                        cmd = new NpgsqlCommand("DELETE FROM subject_result " +
                                            "WHERE student_id=(SELECT student_id FROM student WHERE full_name=@full_name) " +
                                            "AND subject_id=(SELECT subject_id FROM subject WHERE subject_name=@subject_name)", Connection.con);
                                        cmd.Parameters.AddWithValue("@full_name", ((Result_Row)result_grid.SelectedItem).full_name);
                                        cmd.Parameters.AddWithValue("@subject_name", ((Result_Row)result_grid.SelectedItem).subject_name);
                                        if (cmd.ExecuteNonQuery() < 1)
                                        {
                                            throw new Exception("Ошибка при удалении оценки по предмету");
                                        }

                                        //поиск последнего результата
                                        cmd = new NpgsqlCommand("SELECT result_id FROM public.test_result " +
                                            "WHERE student_id=(SELECT student_id FROM student WHERE full_name=@full_name) " +
                                            "AND subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name) " +
                                            "ORDER BY(CASE test_level WHEN 'Удовлетворительно' THEN 3 WHEN 'Хорошо' THEN 4 " +
                                            "WHEN 'Отлично' THEN 5 END) DESC, attempt_number DESC LIMIT 1", Connection.con);
                                        cmd.Parameters.AddWithValue("@full_name", ((Result_Row)result_grid.SelectedItem).full_name);
                                        cmd.Parameters.AddWithValue("@subject_name", ((Result_Row)result_grid.SelectedItem).subject_name);
                                        if (cmd.ExecuteScalar() == null)
                                        {
                                            throw new Exception("Ошибка при получении последнего результата теста");
                                        }
                                        int result_id = int.Parse(cmd.ExecuteScalar().ToString());

                                        //удаление последнего результата
                                        cmd = new NpgsqlCommand("DELETE FROM test_result WHERE result_id=@result_id", Connection.con);
                                        cmd.Parameters.AddWithValue("@result_id", result_id);
                                        if (cmd.ExecuteNonQuery() < 1)
                                        {
                                            throw new Exception("Ошибка при удалении последнего результата теста");
                                        }

                                        actionstatus.Text = "";
                                        actionstatus.Inlines.Add(new Run("Оценка удалена: ") { FontWeight = FontWeights.Bold });
                                        actionstatus.Inlines.Add(new Run($"{((Result_Row)result_grid.SelectedItem).full_name} - {((Result_Row)result_grid.SelectedItem).subject_name} - {((Result_Row)result_grid.SelectedItem).mark}"));
                                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                        transaction.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        //откат транзакции
                                        transaction.Rollback();
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                                break;
                            }
                        case ViewTypeEnum.admin_test_results:
                            {
                                if (MessageBox.Show($"Внимание! Удаление результата билета также приведет к удалению добавленных позже результатов по выбранной дисциплине у выбранного студента и соответствующей итоговой оценки по предмету (при наличии)\n" +
                                    $"Удалить результат билета?\n" +
                                            $"{((Result_Row)result_grid.SelectedItem).full_name} - " +
                                            $"{((Result_Row)result_grid.SelectedItem).subject_name} - " +
                                            $"{((Result_Row)result_grid.SelectedItem).test_level} - Попытка №" +
                                            $"{((Result_Row)result_grid.SelectedItem).attempt}",
                                    "Подтверждение удаления", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                                {
                                    int deleted_count = 0;
                                    bool if_mark_deleted = false;
                                    NpgsqlCommand cmd = new NpgsqlCommand();
                                    cmd.Connection = Connection.con;
                                    //создание транзакции
                                    NpgsqlTransaction transaction;
                                    transaction = Connection.con.BeginTransaction();
                                    cmd.Transaction = transaction;
                                    try
                                    {
                                        //удаление оценки
                                        cmd = new NpgsqlCommand("DELETE FROM subject_result " +
                                            "WHERE student_id=(SELECT student_id FROM student WHERE full_name=@full_name) " +
                                            "AND subject_id=(SELECT subject_id FROM subject WHERE subject_name=@subject_name)", Connection.con);
                                        cmd.Parameters.AddWithValue("@full_name", ((Result_Row)result_grid.SelectedItem).full_name);
                                        cmd.Parameters.AddWithValue("@subject_name", ((Result_Row)result_grid.SelectedItem).subject_name);
                                        int result = cmd.ExecuteNonQuery();
                                        if (result < 0)
                                        {
                                            throw new Exception("Ошибка при удалении оценки по предмету");
                                        }
                                        if (result > 0) 
                                        {
                                            if_mark_deleted = true;
                                        }

                                        //поиск последнего результата
                                        cmd = new NpgsqlCommand("SELECT * FROM public.test_result " +
                                            "WHERE student_id=(SELECT student_id FROM student WHERE full_name=@full_name) " +
                                            "AND subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name) " +
                                            "ORDER BY(CASE test_level WHEN 'Удовлетворительно' THEN 3 WHEN 'Хорошо' THEN 4 " +
                                            "WHEN 'Отлично' THEN 5 END) DESC, attempt_number DESC LIMIT 1", Connection.con);
                                        cmd.Parameters.AddWithValue("@full_name", ((Result_Row)result_grid.SelectedItem).full_name);
                                        cmd.Parameters.AddWithValue("@subject_name", ((Result_Row)result_grid.SelectedItem).subject_name);
                                        NpgsqlDataReader sqlReader = cmd.ExecuteReader();
                                        sqlReader.Read();
                                        int result_id;
                                        while (sqlReader["test_level"].ToString() != ((Result_Row)result_grid.SelectedItem).test_level 
                                            || int.Parse(sqlReader["attempt_number"].ToString()) != ((Result_Row)result_grid.SelectedItem).attempt)
                                        {
                                            result_id = int.Parse(sqlReader["result_id"].ToString());
                                            sqlReader.Close();
                                            cmd = new NpgsqlCommand("DELETE FROM test_result WHERE result_id=@result_id", Connection.con);
                                            cmd.Parameters.AddWithValue("@result_id", result_id);

                                            if (cmd.ExecuteNonQuery() < 1)
                                            {
                                                throw new Exception("Ошибка при удалении результата одного из билетов");
                                            }
                                            deleted_count++;

                                            cmd = new NpgsqlCommand("SELECT * FROM public.test_result " +
                                            "WHERE student_id=(SELECT student_id FROM student WHERE full_name=@full_name) " +
                                            "AND subject_id = (SELECT subject_id FROM subject where subject_name=@subject_name) " +
                                            "ORDER BY(CASE test_level WHEN 'Удовлетворительно' THEN 3 WHEN 'Хорошо' THEN 4 " +
                                            "WHEN 'Отлично' THEN 5 END) DESC, attempt_number DESC LIMIT 1", Connection.con);
                                            cmd.Parameters.AddWithValue("@full_name", ((Result_Row)result_grid.SelectedItem).full_name);
                                            cmd.Parameters.AddWithValue("@subject_name", ((Result_Row)result_grid.SelectedItem).subject_name);
                                            sqlReader = cmd.ExecuteReader();
                                            sqlReader.Read();
                                        }
                                        result_id = int.Parse(sqlReader["result_id"].ToString());
                                        sqlReader.Close();
                                        cmd = new NpgsqlCommand("DELETE FROM test_result WHERE result_id=@result_id", Connection.con);
                                        cmd.Parameters.AddWithValue("@result_id", result_id);

                                        if (cmd.ExecuteNonQuery() < 1)
                                        {
                                            throw new Exception("Ошибка при удалении выбранного результата билета");
                                        }
                                        deleted_count++;

                                        actionstatus.Text = "";
                                        if(if_mark_deleted)
                                        {
                                            actionstatus.Inlines.Add(new Run("Оценка по предмету удалена. ") { FontWeight = FontWeights.Bold });
                                        }
                                        actionstatus.Inlines.Add(new Run("Результатов билетов удалено: ") { FontWeight = FontWeights.Bold });
                                        actionstatus.Inlines.Add(new Run($"{deleted_count}, в том числе выбранный: " +
                                            $"{((Result_Row)result_grid.SelectedItem).full_name} - " +
                                            $"{((Result_Row)result_grid.SelectedItem).subject_name} - " +
                                            $"{((Result_Row)result_grid.SelectedItem).test_level} - Попытка №" +
                                            $"{((Result_Row)result_grid.SelectedItem).attempt}"));
                                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                                        transaction.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        //откат транзакции
                                        transaction.Rollback();
                                        MessageBox.Show(ex.Message);
                                    }
                                }
                                break;
                            }
                    }
                }
                catch (PostgresException ex)
                {
                    if (ex.SqlState == "23503")
                        MessageBox.Show("Удаление невозможно: эта запись используется в других таблицах\n" + ex.Message);
                    else
                        MessageBox.Show(ex.Message, "Error message");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (view_type)
                {
                    case ViewTypeEnum.admin_questions:
                        {
                            AddQuestion add_question = new AddQuestion(sender as DependencyObject);
                            if(add_question.ShowDialog() == true)
                            {
                                showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            }
                            break;
                        }
                    case ViewTypeEnum.admin_students:
                        {
                            AddStudent add_student = new AddStudent(sender as DependencyObject);
                            if (add_student.ShowDialog() == true)
                            {
                                showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            }
                            UpdateComboBox();
                            break;
                        }
                    case ViewTypeEnum.admin_subjects:
                        {
                            AddSubject add_subject = new AddSubject(sender as DependencyObject);
                            if (add_subject.ShowDialog() == true)
                            {
                                showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            }
                            UpdateComboBox();
                            break;
                        }
                    case ViewTypeEnum.admin_subject_student:
                        {
                            AddStudentSubject add_student_subject = new AddStudentSubject(sender as DependencyObject);
                            if (add_student_subject.ShowDialog() == true)
                            {
                                showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            }
                            break;
                        }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void tablecombobox_DropDownClosed(object sender, EventArgs e)
        {
            switch (tablecombobox.Text)
            {
                case "Банк вопросов":
                    {
                        if(view_type == ViewTypeEnum.admin_questions)
                        {
                            return;
                        }
                        result_grid.ItemsSource = null;
                        addbutton.IsEnabled = true;
                        deletebutton.IsEnabled = true;
                        studentcombobox.SelectedItem = null;
                        subjectcombobox.SelectedItem = null;

                        subjectcombobox.IsEnabled = true;
                        subjecttext.Foreground = Brushes.Black;
                        studentcombobox.IsEnabled = false;
                        studenttext.Foreground = Brushes.Gray;

                        view_type = ViewTypeEnum.admin_questions;
                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

                        result_grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.VisibleWhenSelected;
                        //actionstatus.Text = "";
                        break;
                    }
                case "Студенты":
                    {
                        if (view_type == ViewTypeEnum.admin_students)
                        {
                            return;
                        }
                        result_grid.ItemsSource = null;
                        addbutton.IsEnabled = true;
                        deletebutton.IsEnabled = true;
                        studentcombobox.SelectedItem = null;
                        subjectcombobox.SelectedItem = null;

                        subjectcombobox.IsEnabled = false;
                        subjecttext.Foreground = Brushes.Gray;
                        studentcombobox.IsEnabled = true;
                        studenttext.Foreground = Brushes.Black;

                        view_type = ViewTypeEnum.admin_students;
                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        result_grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                        //actionstatus.Text = "";
                        break;
                    }
                case "Дисциплины":
                    {
                        if (view_type == ViewTypeEnum.admin_subjects)
                        {
                            return;
                        }
                        result_grid.ItemsSource = null;
                        addbutton.IsEnabled = true;
                        deletebutton.IsEnabled = true;
                        studentcombobox.SelectedItem = null;
                        subjectcombobox.SelectedItem = null;

                        subjectcombobox.IsEnabled = true;
                        subjecttext.Foreground = Brushes.Black;
                        studentcombobox.IsEnabled = false;
                        studenttext.Foreground = Brushes.Gray;

                        view_type = ViewTypeEnum.admin_subjects;
                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        result_grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                        //actionstatus.Text = "";
                        break;
                    }
                case "Связи студент-дисциплина":
                    {
                        if (view_type == ViewTypeEnum.admin_subject_student)
                        {
                            return;
                        }
                        result_grid.ItemsSource = null;
                        addbutton.IsEnabled = true;
                        deletebutton.IsEnabled = true;
                        studentcombobox.SelectedItem = null;
                        subjectcombobox.SelectedItem = null;

                        subjectcombobox.IsEnabled = true;
                        subjecttext.Foreground = Brushes.Black;
                        studentcombobox.IsEnabled = true;
                        studenttext.Foreground = Brushes.Black;

                        view_type = ViewTypeEnum.admin_subject_student;
                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        result_grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                        //actionstatus.Text = "";
                        break;
                    }
                case "Результаты билетов":
                    {
                        if (view_type == ViewTypeEnum.admin_test_results)
                        {
                            return;
                        }
                        result_grid.ItemsSource = null;
                        addbutton.IsEnabled = false;
                        deletebutton.IsEnabled = true;
                        studentcombobox.SelectedItem = null;
                        subjectcombobox.SelectedItem = null;

                        subjectcombobox.IsEnabled = true;
                        subjecttext.Foreground = Brushes.Black;
                        studentcombobox.IsEnabled = true;
                        studenttext.Foreground = Brushes.Black;

                        view_type = ViewTypeEnum.admin_test_results;
                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        result_grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                        //actionstatus.Text = "";
                        break;
                    }
                case "Оценки по дисциплинам":
                    {
                        if (view_type == ViewTypeEnum.admin_subject_marks)
                        {
                            return;
                        }
                        result_grid.ItemsSource = null;
                        addbutton.IsEnabled = false;
                        deletebutton.IsEnabled = true;
                        studentcombobox.SelectedItem = null;
                        subjectcombobox.SelectedItem = null;

                        subjectcombobox.IsEnabled = true;
                        subjecttext.Foreground = Brushes.Black;
                        studentcombobox.IsEnabled = true;
                        studenttext.Foreground = Brushes.Black;

                        view_type = ViewTypeEnum.admin_subject_marks;
                        showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        result_grid.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
                        //actionstatus.Text = "";
                        break;
                    }
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

        private void studentcombobox_DropDownClosed(object sender, EventArgs e)
        {
            showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void subjectcombobox_DropDownClosed(object sender, EventArgs e)
        {
            showbutton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        }

        private void OnAutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.PropertyType == typeof(System.DateTime) && e.PropertyName == "date_of_birth")
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy";

            if (e.PropertyType == typeof(System.DateTime) && e.PropertyName == "datetime")
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy hh:mm:ss";

        }

        private void Print_Click(object sender, RoutedEventArgs e)
        {
            if(view_type == ViewTypeEnum.admin_subject_marks && subjectcombobox.SelectedItem != null && result_grid.Items.Count != 0)
            {
                PrintDG print = new PrintDG();
                print.printDG(result_grid, this);
            }
            else
            {
                MessageBox.Show("Выберите таблицу оценок и дисциплину");
            }
        }
    }
}
