using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AnimeRouletV0
{
    public partial class AnimeIDCheckcs : Form
    {
        public string IDfromMAL;
        public string IDfromShiki;
        public string URLNow;

        public AnimeIDCheckcs()
        {
            InitializeComponent();
        }

        public void button1_Click(object sender, EventArgs e)
        {

        }

        public void animeIDCheckcs_Load(object sender, EventArgs e)
        {

            bool whereFrom = mainScreen.whereFrom;
            string choiceAnime = mainScreen.choiceAnime;
            string choiceAnimeID = mainScreen.choiceAnimeID;


            textBox2.Text = choiceAnime;
            textBox6.Text = choiceAnimeID;

            var answer = getAnimeIDandName(choiceAnime, whereFrom);

            textBox3.Text = answer[1];
            textBox5.Text = answer[0];
            textBox7.Text = answer[2] + answer[1];

            URLNow = answer[2];
        }

        public void button2_Click(object sender, EventArgs e)
        {
            bool whereFrom = mainScreen.whereFrom;
            string choiceAnime = mainScreen.choiceAnime;
            string choiceAnimeID = mainScreen.choiceAnimeID;

            var prog = new Program();

            //textBox3.Text = answer[1];
            //textBox5.Text = answer[0];
            //textBox7.Text = answer[2];
            if (!whereFrom)
            {
                IDfromMAL = choiceAnimeID;
            }
            if (whereFrom)
            {
                IDfromMAL = textBox5.Text;
            }

            var generesList = prog.GetAnimeGenresList(IDfromMAL);
            var animeScore = prog.GetAnimeSccoreAndName(IDfromMAL);
            textBox8.Text = null;
            foreach (var item in generesList)
            {
                textBox8.Text += item + " \r\n";
            }
            textBox9.Text = animeScore[1];
            textBox7.Text = URLNow + animeScore[0];
        }

        public string[] getAnimeIDandName(string choiceAnime, bool whereFrom)
        {
            string[] answer;
            answer = new string[3];
            var prog = new Program();
            if (!whereFrom)
            {
                textBox1.Text = "Мала или Шики";
                textBox4.Text = "Ани листе";

                answer = prog.GetAnimeIDAniList(choiceAnime);
                return answer;
            }
            else
            {
                textBox1.Text = "Ани листа";
                textBox4.Text = "Мале или Шики";

                answer = prog.GetAnimeIDMAL(choiceAnime);
                return answer;
            }
        }
    }
}
