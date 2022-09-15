using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using System.IO;


namespace AnimeRouletV0
{
    public partial class mainScreen : Form
    {
        private SqlConnection sqlConnection = null;

        private SqlCommandBuilder sqlBuilder = null;

        private SqlDataAdapter sqlDataAdapter = null;

        private DataSet dataSet = null;

        List<string> animeList = new List<string>();
        List<string> animeIDList = new List<string>();
        List<string> animeScoreList = new List<string>();
        
        int resultIndex;

        public static bool whereFrom;
        public static string choiceAnime;
        public static string choiceAnimeID;

        AnimeIDCheckcs Form2 = new AnimeIDCheckcs();


        public mainScreen()
        {
            InitializeComponent();
        }

        private void LoadData()
        {
            try
            {
                sqlDataAdapter = new SqlDataAdapter("SELECT *, 'Delete' AS [Delete] FROM mainTable", sqlConnection);//Убрать ID // Убрать последнюю пустую строку у дата грид вью
                sqlBuilder = new SqlCommandBuilder(sqlDataAdapter);
                sqlBuilder.GetInsertCommand();
                sqlBuilder.GetUpdateCommand();
                sqlBuilder.GetDeleteCommand();

                dataSet = new DataSet();

                sqlDataAdapter.Fill(dataSet, "mainTable");

                dataGridView1.DataSource = dataSet.Tables["mainTable"];

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridView1[9, i] = linkCell;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ReloadData()
        {
            try
            {
                dataSet.Tables["mainTable"].Clear();

                sqlDataAdapter.Fill(dataSet, "mainTable");

                dataGridView1.DataSource = dataSet.Tables["mainTable"];

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataGridViewLinkCell linkCell = new DataGridViewLinkCell();
                    dataGridView1[9, i] = linkCell;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sqlConnection = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\dron4\source\repos\AnimeRouletV0\AnimeRouletV0\Database1.mdf;Integrated Security=True");
            sqlConnection.Open();
            LoadData();
        }

        private void rouletHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void rouletToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 9)
                {
                    string task = dataGridView1.Rows[e.RowIndex].Cells[9].Value.ToString();

                    if (task ==  "Delete")
                    {
                        if (MessageBox.Show("Действительно удалить", "Удаление", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) //Добавить вывод ника
                        {
                            int rowIndex = e.RowIndex;

                            dataGridView1.Rows.RemoveAt(rowIndex);
                            dataSet.Tables["mainTable"].Rows[rowIndex].Delete();
                            sqlDataAdapter.Update(dataSet, "mainTable");
                        }
                    }
                }
                //ReloadData();
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ReloadData();
        }

        public void button3_Click(object sender, EventArgs e)
        {
            if (!CheckUser(textBox1.Text))  //Добавить проверку на повтор ника из добавленных. //Вывести отдельные ошибки на каждый случай
            {
                MessageBox.Show("Некорректное имя пользователя");
                goto Link1;
            }

            if (!CheckRepeatUsers(textBox1.Text))  //Добавить проверку на повтор ника из добавленных. //Вывести отдельные ошибки на каждый случай
            {
                MessageBox.Show("Данный участник уже добавлен");
                goto Link1;
            }

            if (!CheckURL(textBox2.Text))
            {
                MessageBox.Show("Некорректный URL");
            }

            if (textBox2.Text.Split('/', '?')[2] == "anilist.co")
            {
                var progAni = new Program();
                AnimeRouletV0.UserAnimeList userAnimeListAni;

                userAnimeListAni = progAni.GetAnimeListAniList(textBox1.Text);

                if (userAnimeListAni == null)
                {
                    MessageBox.Show("Не удалось получить данные с Ani. Проверьте наличие подготовленного текстового файла");
                    goto Link1;
                }

                animeList = userAnimeListAni.animeList;
                animeScoreList = userAnimeListAni.animeScoreList;
                whereFrom = userAnimeListAni.FromAniList;
                animeIDList = userAnimeListAni.animeIDList;

                goto Link1; 
            }

            var prog = new Program();
            AnimeRouletV0.UserAnimeList userAnimeList;

            userAnimeList = prog.GetAnimeList(textBox2.Text);

            if (userAnimeList == null)
            {
                MessageBox.Show("Не удалось получить данные с MAL. Проверьте ввод на коректность, а , также включен ли Fiddler.");
                goto Link1;
            }
            
            animeList = userAnimeList.animeList;
            animeScoreList = userAnimeList.animeScoreList;
            whereFrom = userAnimeList.FromAniList;
            animeIDList = userAnimeList.animeIDList;
Link1:
            comboBox2.DataSource = animeList;
            
            //ReloadData();
        }

        private bool CheckUser(string userName)
        {
            if (userName.Length<5)
                return false;
            string[] subs = userName.Split('#');
            try
            {
                if (subs[1].Length != 4)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }
            foreach (var item in subs[1])
            {
                if (!char.IsDigit(item))
                    return false;
            }
            if (subs[0].Contains(' '))
                return false;

            if (Regex.IsMatch(subs[0], @"[^0-9a-zA-Z\d_]"))
                return false;

            return true;
        }
        private bool CheckRepeatUsers(string userName)
        {
            if (dataGridView1.Rows.Count > 0)
            {
                for (int i = 0; i < dataGridView1.Rows.Count - 1; i++)
                {
                    string userNameCheck = dataGridView1[1, i].Value.ToString();
                    if (userNameCheck == userName)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool CheckURL(string URL)
        {
            string[] partsLink;
            try
            {
                partsLink = URL.Split('/', '?');
            }
            catch (Exception)
            {
                return false;
            }
            if (partsLink[2] == "myanimelist.net" || partsLink[2] == "shikimori.one" || partsLink[2] == "anilist.co")
            {
                return true;
            }
            return false;
        }

        public void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            resultIndex = comboBox2.SelectedIndex;
            MessageBox.Show(resultIndex.ToString());
        }

        public void textBox3_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                textBox3.Text = animeScoreList[resultIndex];
            }
            catch (Exception)
            {

            }
        }

        public void button1_Click(object sender, EventArgs e)
        {
            if (textBox3.Text == "0")
            {
                MessageBox.Show("Уточните значение оценки пользователя, оно не может быть равно 0");
                goto Link2;
            }
            if (textBox3.Text == "10")
            {
                goto Link3;
            }
            if (textBox3.Text.Length == 3)
            {
                char[] text = textBox3.ToString().ToCharArray();
                if (!(text[36] != '.' && text[37] == '.' && text[38] == '5'))
                {
                    MessageBox.Show("Некорректный ввод. Допустимы числа с шагом 0.5");
                    goto Link2;
                }
            }
            if (textBox3.Text.Length == 2 || textBox3.Text.Length >= 4)
            {
                MessageBox.Show("Некорректный ввод. Допустимы числа с шагом 0.5");
                goto Link2;
            }
            if (textBox3.Text == ".")
            {
                MessageBox.Show("Некорректный ввод. Нахуй ты просто точку ввел долбаеб? Из за таких малолетних дибилов мне и пришлось писать дохуя кода, чтобы он не ломался");
                goto Link2;
            }

Link3:
            SqlCommand command = new SqlCommand(
    $"INSERT INTO [mainTable] ([User], [URL], [AnimeGive],[AnimeGiveScore]) VALUES (@User, @URL, @AnimeGive, @AnimeGiveScore)",
    sqlConnection);

            command.Parameters.AddWithValue("User", textBox1.Text);
            command.Parameters.AddWithValue("URL", textBox2.Text);
            command.Parameters.AddWithValue("AnimeGive", comboBox2.Text);//Возможна ошибка
            command.Parameters.AddWithValue("AnimeGiveScore", textBox3.Text);

            choiceAnime = comboBox2.Text;
            choiceAnimeID = animeIDList[resultIndex];

            MessageBox.Show(command.ExecuteNonQuery().ToString());

            textBox1.Clear();
            textBox2.Clear();
            comboBox2.DataSource = null;
            textBox3.Clear();

            this.Hide();
            Form2.ShowDialog();
            this.Show();
            Close();
Link2:
            ReloadData();
        }

        public void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if (!Char.IsDigit(number) && number != 8 && number != 46) // цифры и клавиша BackSpace
            {
                e.Handled = true;
            }
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            char number = e.KeyChar;
            if ((e.KeyChar <= 33|| e.KeyChar >= 126) && number != 8) //калькулятор
            {
                e.Handled = true;
            }

        }
    }
}
