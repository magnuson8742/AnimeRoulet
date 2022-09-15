using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Text;
using System.IO;

namespace AnimeRouletV0
{
    public class UserAnimeList
    {
        public string userName;
        public bool FromAniList;
        public List<string> animeList = null;
        //public List<string> animeCompleteList; Добавить
        public List<string> animeIDList = null;
        public List<string> animeScoreList = null;
        //public List<string> animeEngList; Добавить


    }

    public class GetRequest
    {
        HttpWebRequest _request;
        string _address;

        public Dictionary<string, string> Headers { get; set; }

        public string Response { get; set; }
        public string Accept { get; set; }
        public string Host { get; set; }
        public string Referer { get; set; }
        public string Useragent { get; set; }
        public int ContentLength { get; set; }
        public WebProxy Proxy { get; set; }


        public GetRequest(string address)
        {
            _address = address;
            Headers = new Dictionary<string, string>();
        }

        public void Run()
        {
            _request = (HttpWebRequest)WebRequest.Create(_address);
            _request.Method = "Get";

            try
            {
                HttpWebResponse response = (HttpWebResponse)_request.GetResponse();
                var stream = response.GetResponseStream();
                if (stream != null) Response = new StreamReader(stream).ReadToEnd();
            }
            catch (Exception)
            {
            }
        }

        public void Run(CookieContainer cookieContainer)
        {
            _request = (HttpWebRequest)WebRequest.Create(_address);
            _request.Method = "Get";
            _request.CookieContainer = cookieContainer;
            _request.Proxy = Proxy;
            _request.Accept = Accept;
            _request.Host = Host;
            _request.Referer = Referer;

            _request.UserAgent = Useragent;

            foreach (var pair in Headers)
                _request.Headers.Add(pair.Key, pair.Value);

            try
            {
                HttpWebResponse response = (HttpWebResponse)_request.GetResponse();
                var stream = response.GetResponseStream();
                if (stream != null) Response = new StreamReader(stream).ReadToEnd();
            }
            catch (Exception)
            {
            }
        }
    }
    public class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            var tryAnimeID = new Program();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new mainScreen());
        }

        public string inputValidation(string findWord)
        {
            char[] arrayChars = findWord.ToCharArray();
            for (int i = 0; i<findWord.Length; i++)
            {
                if (findWord[i] - 0 > 255)
                    arrayChars[i] = '?';
            }
            findWord = new string(arrayChars);

            return findWord;
        }

        public string GetCore(string require)
        {
            var getRequest = new GetRequest($"{require}");
            getRequest.Referer = $"{require}";

            var proxy = new WebProxy("127.0.0.1:8888");
            var cookieContainer = new CookieContainer();

            getRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            getRequest.Useragent = "Mozilla/5.0(Windows NT 10.0; Win64; x64) AppleWebKit/537.36(KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36";
            getRequest.ContentLength = 1;
            getRequest.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"104\", \" Not A;Brand\";v=\"99\", \"Google Chrome\";v=\"104\"");
            getRequest.Headers.Add("sec-ch-ua-mobile", "?0");
            getRequest.Headers.Add("Sec-Fetch-Dest", "document");
            getRequest.Headers.Add("Sec-Fetch-Mode", "navigate");
            getRequest.Headers.Add("Sec-Fetch-Site", "same-origin");
            getRequest.Headers.Add("Upgrade-Insecure-Requests", "1");
            string[] requireCutArr = require.Split('/', '?');
            getRequest.Host = requireCutArr[2];
            getRequest.Proxy = proxy;
            getRequest.Run(cookieContainer);

            return getRequest.Response;
        }

        public string ParseText(string text, string startKeyWord, string endKeyWord)
        {
            var indexStartPos = text.IndexOf(startKeyWord) + startKeyWord.Length;
            if (indexStartPos == startKeyWord.Length - 1)
                return null;
            var indexEndPos = text.IndexOf(endKeyWord, indexStartPos);

            return text.Substring(indexStartPos, indexEndPos - indexStartPos);
        }
        public string TextCut(string text, string startKeyWord, string endKeyWord)
        {
            var indexStartPos = text.IndexOf(startKeyWord) + startKeyWord.Length;
            var indexEndPos = text.IndexOf(endKeyWord, indexStartPos);
            return text.Substring(indexStartPos);
        }
        //public string GetListID(string findWord, bool malOrAni)
        //{
        //    if (malOrAni)
        //        return GetAnimeIDMAL(findWord);
        //    return GetAnimeIDAniList(findWord);
        //}
        public string[] GetAnimeIDMAL(string findWord)
        {
            findWord = inputValidation(findWord);

            string require = $"https://myanimelist.net/anime.php?cat=anime&q={findWord}&type=0&score=0&status=0&p=0&r=0&sm=0&sd=0&sy=0&em=0&ed=0&ey=0&c%5B%5D=a&c%5B%5D=b&c%5B%5D=c&c%5B%5D=f";
            string text = GetCore(require);
            if (text == null)
                return null;

            string startKeyWord = "<a class=\"hoverinfo_trigger\" href=\"https://myanimelist.net/anime/";
            string endKeyWord = "\"";

            
            string[] answer;
            answer = new string[3];

            string[] answerSplit = ParseText(text, startKeyWord, endKeyWord).Split('/');

            answer[0] = answerSplit[0];
            answer[1] = answerSplit[1];
            answer[2] = $"https://myanimelist.net/anime/{answer[0]}";

            return answer;
        }
        public string [] GetAnimeIDAniList(string findWord) //Добавить в парсеры английский и сюда
        {
            string require = $"https://unofficial-anilist-parser.herokuapp.com/anime/{findWord}";
            string text = GetCore(require);
            if (text == null)
                return null;

            string firstWord = "{\"URL\":\"https://anilist.co/anime/";
            string secondWord = "\"";
            string third = "{\"type\":\"Romaji\",\"value\":\"";
            string fifth = "\"},";

            string[] answer;
            answer = new string[3];
            answer[0] = ParseText(text, firstWord, secondWord);
            answer[1] = ParseText(text, third, fifth);
            answer[2] = "https://anilist.co/anime/";

            return answer;
        }
        public List<string> GetAnimeGenresList(string animeID)
        {

            string require = $"https://myanimelist.net/anime/{animeID}";
            string text = GetCore(require);

            string startKeyWord = "<span itemprop=\"genre\" style=\"display: none\">";
            string endKeyWord = "</span>";

            List<string> generesList = new List<string>();
            while (true)
            {
                string answer = ParseText(text, startKeyWord, endKeyWord);
                if (answer == null)
                    break;
                generesList.Add(answer);
                text = TextCut(text, startKeyWord, endKeyWord);
            }
            return generesList;
        }

        public string[] GetAnimeSccoreAndName(string animeID)
        {
            string require = $"https://myanimelist.net/anime/{animeID}";
            string text = GetCore(require);

            string secondKeyWord = "\"og: title\" content=\"";
            string secondTwoKeyWord = "\"><meta";

            string startKeyWord = "<span itemprop=\"ratingValue\"";
            string endKeyWord = "</span>";
            string answer = ParseText(text, startKeyWord, secondTwoKeyWord);

            string[] answerArr = answer.Split('>');

            string[] finalyAnswer;
            finalyAnswer = new string[2];
            finalyAnswer[1] = answerArr[1].ToString();
            finalyAnswer[2] = ParseText(text, secondKeyWord, secondTwoKeyWord);
            return finalyAnswer;
        }

        public AnimeRouletV0.UserAnimeList GetAnimeList(string link)
        {
            string[] partsLink = link.Split('/', '?');
            if (partsLink[2] == "myanimelist.net")
            {
                return GetAnimeListFromMAL(partsLink[4]);
            }
            else if (partsLink[2] == "shikimori.one")
            {
                return GetAnimeListFromShiki(partsLink[3]);
            }
            //else if (partsLink[2] == "anilist.co")
            //{
            //    string userName = "magnuson#0000";//имя участника с #, объявлять в граф программе
            //    return GetAnimeListAniList(userName); ;
            //}
            else
            {
                return null;
            }
        }
        public void WriteToFile(string path, List<string> List)
        {
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                int i = 0;
                foreach (var item in List)
                {
                    i++;
                    sw.WriteLine(i.ToString() + ' ' + item);
                }
            }
        }
        public AnimeRouletV0.UserAnimeList GetAnimeListFromMAL(string userName)
        {

            string require = $"https://myanimelist.net/animelist/{userName}";
            string text = GetCore(require);

            string startKeyWord = "<a href=\"/anime/";
            string startKeyWordTwo = "<span class=\"score-label score-";
            string startKeyWordThree = "<span>Plan to Watch</span>";
            string startKeyWordFour = "title=\"Anime Information\">";
            string startKeyWordFive = "<span>";

            string endKeyWord = "\"";
            string endKeyWordTwo = "</span>";
            
            List<string> newAnimeList = new List<string>();
            List<string> newIDList = new List<string>();
            List<string> newAnimeScoreList = new List<string>();

            var userAnimeList = new UserAnimeList();

            userAnimeList.FromAniList = false;
            userAnimeList.userName = userName;

            string path = Directory.GetCurrentDirectory()+"\\AnilistUsers\\";
            int indexOfEnd;
            try
            {
                indexOfEnd = text.IndexOf(startKeyWordThree);
            }
            catch (Exception)
            {
                return null;
            }
            
            try
            {
                text = text.Remove(text.LastIndexOf(startKeyWordThree), text.Length - 1 - text.IndexOf(startKeyWordThree));
            }
            catch (Exception)
            {
                return null;
            }

            while (!(text.IndexOf(startKeyWord) > indexOfEnd))
            {
                string answer = ParseText(text, startKeyWord, endKeyWord);
                if (answer == null)
                    break;
                string[] split = answer.Split('/');
                newIDList.Add(split[0]);
                newAnimeList.Add(split[1]);
                text.Substring(text.IndexOf(startKeyWordFour));
                
                answer = ParseText(text, startKeyWordTwo, endKeyWord);
                if (answer == "na")
                    answer = "0";
                newAnimeScoreList.Add(answer);

                text = TextCut(text, startKeyWord, endKeyWord);
            }
            userAnimeList.animeIDList = newIDList;
            userAnimeList.animeList = newAnimeList;
            userAnimeList.animeScoreList = newAnimeScoreList;

            //WriteToFile(path + "AnimeListFromMAL.txt", animeList);
            //WriteToFile(path + "IDListFromMAL.txt", iDList);
            //WriteToFile(path + "AnimeScoreFromMAL.txt", animeScoreList);
            return userAnimeList;
        }

        public AnimeRouletV0.UserAnimeList GetAnimeListFromShiki(string userName)
        {
            int i = 1;
            bool skipPlan = false;
            List<string> newAnimeList = new List<string>();
            List<string> newIDList = new List<string>();
            List<string> newAnimeScoreList = new List<string>();

            var userAnimeList = new UserAnimeList();

            userAnimeList.FromAniList = false;
            userAnimeList.userName = userName;

            string path = Directory.GetCurrentDirectory()+"\\AnilistUsers\\";
            while (true)
            {
link1:
                string require = $"https://shikimori.one/{userName}/list/anime/order-by/rate_score/page/{i}"; ///order-by/rate_score
                string text = GetCore(require);
                if (skipPlan == false)
                {
                    string startCutyWord = "</span></div><div class=\"subheadline m5\">";
                    try
                    {
                        text = text.Substring(text.IndexOf(startCutyWord)+startCutyWord.Length); //Возможно тут ошибка
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                   
                    string examination = text;
                    try
                    {
                        text = text.Substring(text.IndexOf(startCutyWord));
                    }
                    catch (Exception)
                    {
                        i++;
                        goto link1;
                    }

                    skipPlan = true;
                }
                string startKeyWord = "data-target_name=\"";
                string startKeyWordTwo = "href=\"/animes/";
                string startKeyThree = "data-field=\"score\" data-max=\"10\" data-min=\"0\">";
                string endKeyWord = "\"";
                string endKeyWordTwo = "</span></td><td";

                int lengthList = newAnimeList.Count;

                while (true)
                {
                    string answer = ParseText(text, startKeyWord, endKeyWord);
                    if (answer == null)
                        break;
                    newAnimeList.Add(answer);

                    answer = ParseText(text, startKeyThree, endKeyWordTwo);
                    if (answer == "&ndash;")
                        answer = "0";
                    newAnimeScoreList.Add(answer);

                    text = TextCut(text, startKeyWord, endKeyWord);

                    answer  = ParseText(text, startKeyWordTwo, endKeyWord);
                    newIDList.Add(answer);
                }
                i++;
                int newLengthList = newAnimeList.Count;
                if (newLengthList == lengthList)
                    break;
            }

            for (int k = 0; k <newIDList.Count; k++)
            {
                string changeString = newIDList[k];
                newIDList[k] = changeString.TrimStart(new Char[] { 'x', 'y', 'z' });
            }

            userAnimeList.animeIDList = newIDList;
            userAnimeList.animeList = newAnimeList;
            userAnimeList.animeScoreList = newAnimeScoreList;

            //WriteToFile(path + "AnimeListFromShiki.txt", animeList);
            //WriteToFile(path + "IDListFromShiki.txt", iDList);
            //WriteToFile(path + "ScoreListFromShiki.txt", animeScore);

            return userAnimeList;
        }
        public AnimeRouletV0.UserAnimeList GetAnimeListAniList(string userName)  //Решить проблему с юникодом
        {
            string text = null;
            string path = Directory.GetCurrentDirectory()+"\\AnilistUsers\\";
            using (StreamReader sr = File.OpenText(path + userName + ".txt"))
                text = sr.ReadToEnd();

            var userAnimeListAni = new UserAnimeList();

            userAnimeListAni.FromAniList = true;
            userAnimeListAni.userName = userName;

            string startKeyWordOne = "\"userPreferred\":\"";
            string startKeyWordTwo = "\"status\":\"";


            string endKeyWord = "\",\"romaji\"";
            string endkeyWordTwo = "\",\"";

            string startKeyWordThree = ",\"score\":";
            string endKeyWordThree = ",\"";

            string startKeyWordFour = "\"english\":";

            string startKeyWordFive = ",\"mediaId\":";

            List<string> newAnimeList = new List<string>();
            List<string> newAnimeScoreList = new List<string>();
           // List<string> animeListEng = new List<string>(); 
            List<string> newIDList = new List<string>();

            string check = null;

            while (true)
            {
                string animeIDAnswer;
                string scoreAnswer;
                string answer;
                string answerEng;

                scoreAnswer = ParseText(text, startKeyWordThree, endKeyWordThree);
                animeIDAnswer = ParseText(text, startKeyWordFive, endKeyWordThree);
                answer = ParseText(text, startKeyWordOne, endKeyWord);
                if (answer == null)
                    break;
                text = TextCut(text, startKeyWordOne, endKeyWord);
                answerEng = ParseText(text, startKeyWordFour, endKeyWordThree);
                if (answerEng == "null")
                    answerEng = "\"" + answer + "\"";

                check = ParseText(text, startKeyWordTwo, endkeyWordTwo);
                text = TextCut(text, startKeyWordTwo, endkeyWordTwo);

                check = ParseText(text, startKeyWordTwo, endkeyWordTwo);

                if (check == "COMPLETED" || check == "REPEATING" || check == "CURRENT" || check == "DROPPED" || check == "PAUSED")  // "CURRENT" , "PLANNING", "DROPPED" , "PAUSED", "REPEATING"
                {
                    newAnimeList.Add(answer);
                    newAnimeScoreList.Add(scoreAnswer);
                    answerEng=answerEng.TrimEnd('\"');
                   // animeListEng.Add(answerEng.TrimStart('\"'));
                    newIDList.Add(animeIDAnswer);
                }
            }

            userAnimeListAni.animeIDList = newIDList;
            userAnimeListAni.animeList = newAnimeList;
            userAnimeListAni.animeScoreList = newAnimeScoreList;

            //WriteToFile(path + "AnimeListFromANI.txt", animeList);
            //WriteToFile(path + "ScoreListFromANI.txt", scoreList);
            //WriteToFile(path + "AnimeEngListFromANI.txt", animeListEng);
            //WriteToFile(path + "AnimeListNewIDFromANI.txt", animeListID);

            return userAnimeListAni;
        }
    }
}
