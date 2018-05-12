using CsharpHttpHelper;
using CsharpHttpHelper.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Threading;

namespace Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string Cookies = "";
        int index = 0;
        private void btn_start_Click(object sender, EventArgs e)
        {

           // 获取学生考试成绩的试卷图片
             //pai_StudentImg();//获取图片有点慢，建议另开一个线程启动
            ////获取所有教师的信息
            //label3.Text = "所有老师信息采集中...";
            string str_insert1= pai_Teach();
            MysqlHelper.ConnString = str_connn;
            MysqlHelper.ExecuteNonQuery(str_insert1);
            //label3.Text = "所有学生信息采集中...";
            ////获取所有学生的信息
            string str_insert2=  pai_Student();
            MysqlHelper.ConnString = str_connn;
            MysqlHelper.ExecuteNonQuery(str_insert2);

            ////获取教师对试卷的评阅信息（不用这个功能了，只写了一半还未完成）
            //// pai_JiaoShiPinYue();
            //label3.Text = "所有班级信息采集中...";
            ////获取所有班级的信息
            string str_insert3= pai_banjiLieBiao();
            MysqlHelper.ConnString = str_connn;
            MysqlHelper.ExecuteNonQuery(str_insert3);
            //label3.Text = "所有授课教师信息采集中...";
            ////获取所有授课老师的信息
            //string str_insert4= pai_jiaoshi_souke();
            //MysqlHelper.ConnString = str_connn;
            //MysqlHelper.ExecuteNonQuery(str_insert4);
            //label3.Text = "所有学生成绩信息采集中...";
            ////获取所有学生的成绩
            string str_insert5= pai_StudentChengJi();
            //int len = str_insert5.Length;//看一下字符串多长哈。。
            MysqlHelper.ConnString = str_connn;
            MysqlHelper.ExecuteNonQuery(str_insert5);
            ////获取学生考试成绩的试卷图片
            //// pai_StudentImg();//获取图片有点慢，建议另开一个线程启动
            ////获取题目类型，客观题与主观题的
            //label3.Text = "所有客观题/主观题信息采集中...";
            //string str_insert6=  pai_timu();
            //MysqlHelper.ConnString = str_connn;
            //MysqlHelper.ExecuteNonQuery(str_insert6);
            //label3.Text = "所有信息已经采集完成！！";

            MessageBox.Show("完成采集了....");



        }
        public static void Execute(Dictionary<string, string> stringDic, Dictionary<string, List<string>> listDic, Dictionary<string, int> intDic)
        {
            string str = string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9]','{10}','{11}','{12}','{13}')", stringDic["URL"], stringDic["category"], stringDic["type1"], stringDic["type1id"], stringDic["type2"], stringDic["type2id"], stringDic["type3"], stringDic["type3id"], stringDic["type4"], stringDic["type4id"], stringDic["价格倍数"], stringDic["原价倍数"], stringDic["特色"], stringDic["描述"]);
            listDic["公共1_list"].Add(str);
        }

        /// <summary>
        /// 攫取客观题与主观题
        /// </summary>
        /// <returns>返回生成的sql语句</returns>
        private string pai_timu()
        {
            //先生成最初始的sql语句
            string insert_ke = "insert IGNORE into keguangtifengzhi(TiHao_Id,TI_Type,DaAn,FeiZhi,BuFenDeFen,ZhiShi_JiNeng,Ke_Mu,Kao_Type)values";
            string insert_zhu = "insert IGNORE into zhuguangtifenzhi(TiHao_Id,Fen_zhi,PinFenBiaoBuChang,PinFenBiaoXuLie,PinFenBiaoQian,ZhiShi_JiNeng,ZuiDiPinYue,Ke_Mu,Kao_Type)values";
            List<string> list_Year = new List<string>();
            string htmls = Get("http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=2018-01-09&viewIndex=1", "office.7net.cc", "http://office.7net.cc/YJPlan/process", false);

            HtmlAgilityPack.HtmlDocument htm = new HtmlAgilityPack.HtmlDocument();
            htm.LoadHtml(htmls);
            HtmlNode html_nodes = htm.DocumentNode;
            HtmlNodeCollection hnc = html_nodes.SelectNodes("//*[@id=\"ContentPlaceHolder1_DropDownList1\"]/option");
            foreach (HtmlNode item in hnc)
            {
                list_Year.Add("http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=" + item.Attributes["value"].Value.ToString() + "&viewIndex=1");
            }
            foreach (string item in list_Year)
            {
                string html_Year = Get(item, "office.7net.cc", "http://office.7net.cc/Answer?t=1&km=0", false);
                HtmlAgilityPack.HtmlDocument html_Years = new HtmlAgilityPack.HtmlDocument();
                html_Years.LoadHtml(html_Year);
                HtmlNode htmlnodes_year = html_Years.DocumentNode;
                HtmlNodeCollection htnonode_Years = htmlnodes_year.SelectNodes("//*[@id=\"rsView\"]");
                if (htnonode_Years[0].ChildNodes.Count > 3)
                {
                    int index = 3;
                    for (int i = 3; i < htnonode_Years[0].ChildNodes.Count; i++)
                    {
                        HtmlNode htn_year = htnonode_Years[0].ChildNodes[index];
                        index += 2;
                        string KaoShi_Type = htn_year.ChildNodes[1].InnerText;
                        string htn_value = htn_year.ChildNodes[1].ChildNodes[0].Attributes[1].Value;
                        string[] str_split = htn_value.Split(',');
                        //获取学校代码
                        string school_code = Regex.Match(htn_value, @"\d+").Value;
                        //获取guid
                        string guid = str_split[1].Substring(1, str_split[1].Length - 3);
                        //if (Post("http://office.7net.cc/examplan/rec".Trim(), "ruCode=" + school_code + "&guid=" + guid + "".Trim())=="+ok") {
                        //    string htmlsss= Get("http://office.7net.cc/Score", "office.7net.cc", "http://office.7net.cc/Score", true);
                        //}
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("ruCode", school_code);
                        dic.Add("guid", guid);
                       
                        HttpWebResponse response = CreatePostHttpResponse("http://office.7net.cc/examplan/rec", dic, "http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=2017%u5E741%u67081%u65E5");
                        string result = GetResponseString(response);
                        if (result.Contains("+OK"))
                        {

                            if (response.Cookies.Count > 0)
                            {
                                Cookies = "";
                                foreach (var item_cookie in response.Cookies)
                                {

                                    Cookies += item_cookie;

                                }
                            }
                            string htmlss = Get("http://office.7net.cc/Answer?t=1&km=0", "office.7net.cc", "http://office.7net.cc/Answer?t=1&km=0", false);
                            HtmlAgilityPack.HtmlDocument htn_Kemu = new HtmlAgilityPack.HtmlDocument();
                            htn_Kemu.LoadHtml(htmlss);
                            HtmlNode hn = htn_Kemu.DocumentNode;
                            HtmlNodeCollection hnc_kemu = hn.SelectNodes("//*[@id=\"ContentPlaceHolder1_ddlKM\"]");
                            if (hnc_kemu == null || hnc_kemu.Count <= 0)
                            {

                                continue;
                            }
                            HtmlNode node_kemu = hnc_kemu[0];
                            HtmlNodeCollection hnc_ke_mus= node_kemu.ChildNodes;
                            foreach (var item_kemu in hnc_ke_mus)
                            {

                                //取课目
                                if (item_kemu.Attributes.Count == 0) {
                                    continue;
                                }
                                string ke_id= item_kemu.Attributes["value"].Value;
                                string ke_Name = "";
                                switch (ke_id) {
                                   case "0" :ke_Name = "语文" ; break;
                                    case "1":ke_Name = "数学"; break;
                                    case "2": ke_Name = "英语"; break;
                                    case "13": ke_Name = "科学基础"; break;
                                    case "21": ke_Name = "社会";break;

                                }
                                    
                                
                                //判断学生一共有多少人，一共要取几次才能取完
                                //km代表课目编号（语文0，数学1，英语2，科学技术13） t代表题目类型(客观题1，主观题2)
                                //课观题分值情况
                                int counts = pai_KeTi_count("http://office.7net.cc/Answer?t=1&km="+ke_id+"", "office.7net.cc", "http://office.7net.cc/Answer?t=1&km=0", false);
                                for (int j = 1; j <= counts; j++)
                                {
                                    //获取到学生数据
                                    string chu_htmls = Get("http://office.7net.cc/Answer?t=1&km=" + ke_id + "&viewIndex=" + j + "", "office.7net.cc", "http://office.7net.cc/Answer?t=1&km=0", false);
                                    HtmlAgilityPack.HtmlDocument hdc = new HtmlAgilityPack.HtmlDocument();
                                    hdc.LoadHtml(chu_htmls);
                                    HtmlNode node_table = hdc.DocumentNode;
                                    HtmlNodeCollection node_tab = node_table.SelectNodes("//*[@id=\"form2\"]/div[3]/div/div[4]/table");
                                    if (node_tab != null && node_tab.Count > 0)
                                    {
                                        HtmlNode nodesss = node_tab[0];

                                        //只有大于三的时候，才能判断表格里有数据
                                        int indexxx = 3;
                                        if (nodesss.ChildNodes.Count > 3)
                                        {
                                            //计数，数取到最后一条数据的时候，判断是否等于或大于，则跳出循环取
                                            int indexxxxx = 3;
                                            while (true)
                                            {
                                                string Ti_hao = nodesss.ChildNodes[indexxxxx].ChildNodes[1].InnerText;
                                                string Ti_Type = nodesss.ChildNodes[indexxxxx].ChildNodes[3].InnerText;
                                                string Da_An = nodesss.ChildNodes[indexxxxx].ChildNodes[5].InnerText;
                                                string Fen_Zhi = nodesss.ChildNodes[indexxxxx].ChildNodes[7].InnerText;
                                                string BuFen_DeFen = nodesss.ChildNodes[indexxxxx].ChildNodes[9].InnerText;
                                                string ZhiShi_JiNeng = nodesss.ChildNodes[indexxxxx].ChildNodes[11].InnerText;
                                                 insert_ke += string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}'),", Ti_hao, Ti_Type, Da_An,Fen_Zhi, BuFen_DeFen, ZhiShi_JiNeng,ke_Name,KaoShi_Type);
                                                indexxxxx += 2;
                                                if (indexxxxx >= nodesss.ChildNodes.Count)
                                                {
                                                    indexxxxx = 3;

                                                    break;
                                                }
                                            }

                                            //node_tab.ChildNodes
                                        }
                                    }
                                }
                                //摄取主观题分值情况
                                int zhu_counts = pai_KeTi_count("http://office.7net.cc/Answer?t=2&km=" + ke_id + "", "office.7net.cc", "http://office.7net.cc/Answer?t=2&km=0", false);
                                for (int j = 1; j <= zhu_counts; j++)
                                {
                                    //获取到学生数据
                                    string chu_htmls = Get("http://office.7net.cc/Answer?t=2&km=" + ke_id + "&viewIndex="+j+"", "office.7net.cc", "http://office.7net.cc/Answer?t=2&km=0", false);
                                    HtmlAgilityPack.HtmlDocument hdc = new HtmlAgilityPack.HtmlDocument();
                                    hdc.LoadHtml(chu_htmls);
                                    HtmlNode node_table = hdc.DocumentNode;
                                    HtmlNodeCollection node_tab = node_table.SelectNodes("//*[@id=\"form2\"]/div[3]/div/div[5]/table");
                                    if (node_tab != null && node_tab.Count > 0)
                                    {
                                        HtmlNode nodesss = node_tab[0];

                                        //只有大于三的时候，才能判断表格里有数据
                                        if (nodesss.ChildNodes.Count > 3)
                                        {
                                            //计数，数取到最后一条数据的时候，判断是否等于或大于，则跳出循环取
                                            int indexxxxx = 3;
                                            while (true)
                                            {
                                                string Ti_hao = nodesss.ChildNodes[indexxxxx].ChildNodes[1].InnerText;
                                                string Fen_zhi = nodesss.ChildNodes[indexxxxx].ChildNodes[3].InnerText;
                                                string PinFenBiaoBuChang = nodesss.ChildNodes[indexxxxx].ChildNodes[5].InnerText;
                                                string PinFenBiaoXuLie = nodesss.ChildNodes[indexxxxx].ChildNodes[7].InnerText;
                                                string PinFenBiaoQian = nodesss.ChildNodes[indexxxxx].ChildNodes[9].InnerText;
                                                string ZhiShi_JiNeng = nodesss.ChildNodes[indexxxxx].ChildNodes[11].InnerText;
                                                string ZuiDiPinYue = nodesss.ChildNodes[indexxxxx].ChildNodes[13].InnerText;


                                                insert_zhu += string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}'),", Ti_hao, Fen_zhi,PinFenBiaoBuChang,PinFenBiaoXuLie,PinFenBiaoQian, ZhiShi_JiNeng,ZuiDiPinYue,ke_Name,KaoShi_Type);
                                                indexxxxx += 2;
                                                if (indexxxxx >= nodesss.ChildNodes.Count)
                                                {
                                                    indexxxxx = 3;

                                                    break;
                                                }
                                            }

                                            //node_tab.ChildNodes
                                        }
                                    }
                                }

                            }
                        }
                        if (index >= htnonode_Years[0].ChildNodes.Count)
                        {
                            break;
                        }
                    }
                }

            }
            //合成最终的sql语句
            insert_ke = insert_ke.Substring(0, insert_ke.Length - 1);
            insert_zhu = insert_zhu.Substring(0, insert_zhu.Length - 1);
            return insert_ke+";"+insert_zhu;
        }

        /// <summary>
        /// 获取学生成绩单图片
        /// </summary>
        private void pai_StudentImg()
        {

            List<Thread> list_Thred = new List<Thread>();
            string insert = "insert IGNORE into XueShen_Img(`Name`,School_Name,Kao_code,Ban_Ji,Ke_Mu,Chen_ji,Ru_Ku_Time,path)values";

            List<string> list_Year = new List<string>();
            string htmls = Get("http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=2018-01-09&viewIndex=1", "office.7net.cc", "http://office.7net.cc/YJPlan/process", false);

            HtmlAgilityPack.HtmlDocument htm = new HtmlAgilityPack.HtmlDocument();
            htm.LoadHtml(htmls);
            HtmlNode html_nodes = htm.DocumentNode;
            HtmlNodeCollection hnc = html_nodes.SelectNodes("//*[@id=\"ContentPlaceHolder1_DropDownList1\"]/option");
            foreach (HtmlNode item in hnc)
            {
                list_Year.Add("http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=" + item.Attributes["value"].Value.ToString() + "&viewIndex=1");
            }

            foreach (string item in list_Year)
            {
                string html_Year = Get(item, "office.7net.cc", "http://office.7net.cc/YJPlan/process", false);
                HtmlAgilityPack.HtmlDocument html_Years = new HtmlAgilityPack.HtmlDocument();
                html_Years.LoadHtml(html_Year);
                HtmlNode htmlnodes_year = html_Years.DocumentNode;
                HtmlNodeCollection htnonode_Years = htmlnodes_year.SelectNodes("//*[@id=\"rsView\"]");
                if (htnonode_Years[0].ChildNodes.Count > 3)
                {
                    int index = 3;
                    for (int i = 3; i < htnonode_Years[0].ChildNodes.Count; i++)
                    {
                        HtmlNode htn_year = htnonode_Years[0].ChildNodes[index];
                        index += 2;
                        string KaoShi_Type = htn_year.ChildNodes[1].InnerText;
                        string htn_value = htn_year.ChildNodes[1].ChildNodes[0].Attributes[1].Value;
                        string[] str_split = htn_value.Split(',');
                        //获取学校代码
                        string school_code = Regex.Match(htn_value, @"\d+").Value;
                        //获取guid
                        string guid = str_split[1].Substring(1, str_split[1].Length - 3);
                        //if (Post("http://office.7net.cc/examplan/rec".Trim(), "ruCode=" + school_code + "&guid=" + guid + "".Trim())=="+ok") {
                        //    string htmlsss= Get("http://office.7net.cc/Score", "office.7net.cc", "http://office.7net.cc/Score", true);
                        //}
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("ruCode", school_code);
                        dic.Add("guid", guid);
                        HttpWebResponse response = CreatePostHttpResponse("http://office.7net.cc/examplan/rec", dic);
                        string result = GetResponseString(response);
                        if (result == "+OK")
                        {

                            if (response.Cookies.Count > 0)
                            {
                                Cookies = "";
                                foreach (var item_cookie in response.Cookies)
                                {

                                    Cookies += item_cookie;

                                }
                            }
                            //判断一共循环数据多少次才能将其全部取出来
                            int counts= pai_StudentImg_Count("http://office.7net.cc/SeptnetOMR/ASIResponse?viewIndex=1", "office.7net.cc", "http://office.7net.cc/SeptnetOMR/ASIResponse", false);
                            //获取学生详细的信息页面
                          
                            string htmlsss= Get("http://office.7net.cc/SeptnetOMR/ASIResponse?viewIndex=2", "office.7net.cc", "http://office.7net.cc/SeptnetOMR/ASIResponse", false);
                            //开始循环不断地取学生的信息
                            for (int j = 1; j <= counts; j++)
                            {
                                //获取到学生数据
                                string chu_htmls = Get("http://office.7net.cc/SeptnetOMR/ASIResponse?viewIndex=" + j + "", "office.7net.cc", "http://office.7net.cc/SeptnetOMR/ASIResponse", false);
                                HtmlAgilityPack.HtmlDocument hdc = new HtmlAgilityPack.HtmlDocument();
                                hdc.LoadHtml(chu_htmls);
                                HtmlNode node_table = hdc.DocumentNode;
                                HtmlNodeCollection node_tab = node_table.SelectNodes("//*[@id=\"form2\"]/div[3]/div/div[2]/table");
                                if (node_tab != null && node_tab.Count > 0)
                                {
                                    HtmlNode nodesss = node_tab[0];

                                    //只有大于三的时候，才能判断表格里有数据
                                    int indexxx = 3;
                                    if (nodesss.ChildNodes.Count > 0)
                                    {
                                        //计数，数取到最后一条数据的时候，判断是否等于或大于，则跳出循环取
                                        int indexxxxx = 3;
                                        while (true)
                                        {
                                            string Url = nodesss.ChildNodes[indexxxxx].ChildNodes[1].ChildNodes[1].Attributes["href"].Value;
                                            string Kao_code = nodesss.ChildNodes[indexxxxx].ChildNodes[3].InnerText;
                                            string Name = nodesss.ChildNodes[indexxxxx].ChildNodes[5].InnerText;
                                            string School_Name = nodesss.ChildNodes[indexxxxx].ChildNodes[7].InnerText;
                                            string BanJi = nodesss.ChildNodes[indexxxxx].ChildNodes[9].ChildNodes[0].InnerText;
                                            string Ke_Mu = nodesss.ChildNodes[indexxxxx].ChildNodes[11].ChildNodes[0].InnerText;
                                            string Chen_ji = nodesss.ChildNodes[indexxxxx].ChildNodes[13].ChildNodes[0].InnerText;
                                            string Ru_Ku_Time = nodesss.ChildNodes[indexxxxx].ChildNodes[15].ChildNodes[0].InnerText.Replace("&nbsp;","");
                                            // string  = nodesss.ChildNodes[indexxxxx].ChildNodes[13].ChildNodes[0].InnerText;
                                            string img_html= Get(Url, "office.7net.cc", "http://office.7net.cc/SeptnetOMR/ASIResponse", false);
                                            HtmlAgilityPack.HtmlDocument html_img = new HtmlAgilityPack.HtmlDocument();
                                            html_img.LoadHtml(img_html);
                                             HtmlNode hnd_img= html_img.DocumentNode;
                                            HtmlNodeCollection hnc_img= hnd_img.SelectNodes("//*[@id=\"ContentPlaceHolder1_showType\"]");
                                            if (hnc_img!=null&&hnc_img.Count>0) {
                                               string html_imgs= Get(hnc_img[0].Attributes["href"].Value, "office.7net.cc", "http://office.7net.cc/SeptnetOMR/ASIResponse",false);
                                                HtmlAgilityPack.HtmlDocument hnc_imgs = new HtmlAgilityPack.HtmlDocument();
                                                hnc_imgs.LoadHtml(html_imgs);
                                                HtmlNode hndimg= hnc_imgs.DocumentNode;
                                                HtmlNodeCollection hnc_imgss= hndimg.SelectNodes("//*[@id=\"IMGPanel\"]/img");
                                                if (hnc_imgss!=null&&hnc_imgss.Count>0) {
                                                    HtmlNode hn_imgss= hnc_imgss[0];
                                                    string img_url= "http://office.7net.cc/septnetOMR/" + hnc_imgss[0].Attributes["src"].Value;
                                                  string path=  Pai_img(img_url,"office.7net.cc", Name, BanJi,Ke_Mu);
                                                    insert += string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}'),",Name,School_Name,Kao_code,BanJi,Ke_Mu,Chen_ji,Ru_Ku_Time,path);

                                                }
                                                   
                                            }
                                            //  Pai_img(, "", "http://office.7net.cc/SeptnetOMR/ASIResponse   ", false);
                                            //insert += string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}'),", SchoolName, Ban_ji, Kao_Code, Name, Zhong_Feng, Yu_wen, Shu_xue, Yan_Yu, Ke_XueJiChu, KaoShi_Type);
                                            indexxxxx += 2;
                                            if (indexxxxx >= nodesss.ChildNodes.Count)
                                            {
                                                indexxxxx = 3;

                                                break;
                                            }
                                        }

                                        //node_tab.ChildNodes
                                    }
                                }
                            }

                        }
                        if (index >= htnonode_Years[0].ChildNodes.Count)
                        {
                            break;
                        }
                    }
                }
            }
            //合成最终的sql语句
            insert = insert.Substring(0, insert.Length - 1);
        }
        //获取图片
        /// <summary>
        /// 获取图片,对应方法pai_StudentImg
        /// </summary>
        /// <param name="url">需要攫取的图片的url地址</param>
        /// <param name="host">需要攫取的图片的host</param>
        /// <param name="name">给图片命名的信息（Name+Banji+ke_mu）</param>
        /// <param name="banji">给图片命名的信息（Name+Banji+ke_mu）</param>
        /// <param name="ke_mu">给图片命名的信息（Name+Banji+ke_mu）</param>
        /// <returns>返回图片存放的路径</returns>
        public string Pai_img(string url,string host,string name,string banji,string ke_mu) {
            
          

            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;//创建请求对象
            request.Method = "GET";//请求方式
            //request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";//链接类型
            request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.9 Safari/537.36";
            request.Headers.Add("Cookie", Cookies);
            request.Host = "office.7net.cc";
            request.Timeout = 100000;
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
           // request.Headers.Add("Origin", "http://www.7net.cc");

            request.Accept = "*/*";

            request.KeepAlive = true;


           // request.Referer = "http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=2018-01-09&viewIndex=1";

            WebResponse response = request.GetResponse();
            Stream reader = response.GetResponseStream();
            //放置的目录
            //可自行设定放置图片路径的代码
            if (!Directory.Exists(@"D:\学生图片")) {
                Directory.CreateDirectory(@"D:\学生图片");
            }
            //这个字段里的路径是为了放进数据库中而建立的因为一个/在数据库中是会被转义掉的
             string path = @"D:\\学生图片\"+name+banji+ke_mu+".jpg";
            FileStream writer = new FileStream(@"D:\学生图片\"+name+banji+ke_mu+".jpg", FileMode.OpenOrCreate, FileAccess.Write);
            byte[] buff = new byte[512];
            int c = 0; //实际读取的字节数
            while ((c = reader.Read(buff, 0, buff.Length)) > 0)
            {
                writer.Write(buff, 0, c);
            }
            writer.Close();
            writer.Dispose();
            reader.Close();
            reader.Dispose();
            response.Close();
            return path;
        }

        /// <summary>
        /// 获取到学生图片需要一共取页面多少次
        /// </summary>
        public int pai_StudentImg_Count(string url,string host,string referer, bool is_cookie ) {
            HttpHelper http = new HttpHelper();
            HttpResult result = new HttpResult();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项  
                KeepAlive = true,
                Host = host,//"office.7net.cc",
                Method = "GET",//URL     可选项 默认为Get   
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                Cookie = Cookies,//"ds_session=86rvpd3qtvpjjnqhdnua2vfo57; Hm_lvt_a68414d98536efc52eeb879f984d8923=1518145614; Hm_lpvt_a68414d98536efc52eeb879f984d8923=1518147484",//字符串Cookie     可选项   
                Referer = referer,// == "" ? "http://office.7net.cc/Score":url,//来源URL     可选项   
                //Postdata = "",//",//Post数据     可选项GET时不需要写 这个数据是从我们数据库中读取到的   
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.9 Safari/537.36",//用户的浏览器类型，版本，操作系统     可选项有默认值   
                                                                                                                                           //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
                Allowautoredirect = false,//是否根据301跳转     可选项   
                                          //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数   
                                          //Connectionlimit = 1024,//最大连接数     可选项 默认为1024    
                                          //ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数    
                                          //ProxyPwd = "123456",//代理服务器密码     可选项    
                                          //ProxyUserName = "administrator",//代理服务器账户名     可选项   
                ResultType = ResultType.String,
                //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
            };
            item.Header.Add("Upgrade-Insecure-Requests:1");
            item.Header.Add("Accept-Encoding:gzip, deflate, br");
            item.Header.Add("Accept-Language:zh-CN,zh;q=0.9");
            //将网页的返回的报文存在result当中
            result = http.GetHtml(item);
            //是否更换cookies
            if (is_cookie)
            {
                Cookies = "";
                Cookies = result.Cookie;
            }

            //Cookies = "";
            //Cookies += result.Cookie;
            //获取返回的请求报文
            string html = result.Html;
            // Cookies= result.Cookie;
            Cookies = Cookies.Replace("HttpOnly", "");
            Cookies = Cookies.Replace("Path=/;", "");

            HtmlAgilityPack.HtmlDocument hdmt = new HtmlAgilityPack.HtmlDocument();
            hdmt.LoadHtml(html);
            HtmlNode hnd = hdmt.DocumentNode;
            HtmlNodeCollection hnc = hnd.SelectNodes("//*[@id=\"form2\"]/div[3]/div/div[3]/script");
            int counts = 0;
            if (hnc != null && hnc.Count > 0)
            {
                HtmlNode hnNode = hnc[0];
                string hnNode_Text = hnNode.InnerText;
                //获取到学生人数的总和
                string num_count = Regex.Match(hnNode_Text, @"\d+").Value;
                //计算需要一共循环几次才能取完
                double countsss = Convert.ToDouble(num_count);
                counts = Convert.ToInt32(Math.Round(countsss) / 10) + 1;

            }
            return counts;
        }

        /// <summary>
        /// 获取所有学生成绩
        /// </summary>
        /// <returns></returns>
        private string pai_StudentChengJi()
        {
            string insert = "insert IGNORE into kaoshi_chenji(Xue_Xiao,Ban_Ji,Kao_hao,`Name`,Zhong_Feng,Yu_Wen,Shu_Xue,Yan_Yu,KeXue_JiChu,Kao_Shi)values";
            List<string> list_Year = new List<string>();
            string htmls = Get("http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=2018-01-09&viewIndex=1", "office.7net.cc", "http://office.7net.cc/YJPlan/process", false);

            HtmlAgilityPack.HtmlDocument htm = new HtmlAgilityPack.HtmlDocument();
            htm.LoadHtml(htmls);
            HtmlNode html_nodes = htm.DocumentNode;
            HtmlNodeCollection hnc = html_nodes.SelectNodes("//*[@id=\"ContentPlaceHolder1_DropDownList1\"]/option");
            foreach (HtmlNode item in hnc)
            {
                list_Year.Add("http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=" + item.Attributes["value"].Value.ToString() + "&viewIndex=1");
            }

            foreach (string item in list_Year)
            {
                string html_Year = Get(item, "office.7net.cc", "http://office.7net.cc/YJPlan/process", false);
                HtmlAgilityPack.HtmlDocument html_Years = new HtmlAgilityPack.HtmlDocument();
                html_Years.LoadHtml(html_Year);
                HtmlNode htmlnodes_year = html_Years.DocumentNode;
                HtmlNodeCollection htnonode_Years = htmlnodes_year.SelectNodes("//*[@id=\"rsView\"]");
                if (htnonode_Years[0].ChildNodes.Count > 3)
                {
                    int index = 3;
                    for (int i = 3; i < htnonode_Years[0].ChildNodes.Count; i++)
                    {
                        HtmlNode htn_year = htnonode_Years[0].ChildNodes[index];
                        index += 2;
                        string KaoShi_Type = htn_year.ChildNodes[1].InnerText;
                        string htn_value = htn_year.ChildNodes[1].ChildNodes[0].Attributes[1].Value;
                        string[] str_split = htn_value.Split(',');
                        //获取学校代码
                        string school_code = Regex.Match(htn_value, @"\d+").Value;
                        //获取guid
                        string guid = str_split[1].Substring(1, str_split[1].Length - 3);
                        //if (Post("http://office.7net.cc/examplan/rec".Trim(), "ruCode=" + school_code + "&guid=" + guid + "".Trim())=="+ok") {
                        //    string htmlsss= Get("http://office.7net.cc/Score", "office.7net.cc", "http://office.7net.cc/Score", true);
                        //}
                        Dictionary<string, string> dic = new Dictionary<string, string>();
                        dic.Add("ruCode", school_code);
                        dic.Add("guid", guid);
                        HttpWebResponse response = CreatePostHttpResponse("http://office.7net.cc/examplan/rec", dic);
                        string result = GetResponseString(response);
                        if (result == "+OK")
                        {

                            if (response.Cookies.Count > 0)
                            {
                                Cookies = "";
                                foreach (var item_cookie in response.Cookies)
                                {

                                    Cookies += item_cookie;

                                }
                            }
                            //判断学生一共有多少人，一共要取几次才能取完
                          int counts= pai_Student_count("http://office.7net.cc/Score", "office.7net.cc", "http://office.7net.cc/Score", false);
                            for (int j = 1; j <= counts; j++)
                            {
                                //获取到学生数据
                                string chu_htmls = Get("http://office.7net.cc/Score?viewIndex="+j+"", "office.7net.cc", "http://office.7net.cc/Score", false);
                                HtmlAgilityPack.HtmlDocument hdc = new HtmlAgilityPack.HtmlDocument();
                                hdc.LoadHtml(chu_htmls);
                                HtmlNode node_table = hdc.DocumentNode;
                                HtmlNodeCollection node_tab = node_table.SelectNodes("//*[@id=\"form2\"]/div[3]/div/div[3]/table");
                                if (node_tab != null && node_tab.Count > 0)
                                {
                                    HtmlNode nodesss = node_tab[0];

                                    //只有大于三的时候，才能判断表格里有数据
                                    int indexxx = 3;
                                    if (nodesss.ChildNodes.Count > 3)
                                    {
                                        //计数，数取到最后一条数据的时候，判断是否等于或大于，则跳出循环取
                                        int indexxxxx = 3;
                                        while (true)
                                        {
                                            string SchoolName = nodesss.ChildNodes[indexxxxx].ChildNodes[1].InnerText;
                                            string Ban_ji = nodesss.ChildNodes[indexxxxx].ChildNodes[3].InnerText;
                                            string Kao_Code = nodesss.ChildNodes[indexxxxx].ChildNodes[5].InnerText;
                                            string Name = nodesss.ChildNodes[indexxxxx].ChildNodes[7].InnerText;
                                            string Zhong_Feng = nodesss.ChildNodes[indexxxxx].ChildNodes[9].ChildNodes[0].InnerText;
                                            string Yu_wen = nodesss.ChildNodes[indexxxxx].ChildNodes[10].ChildNodes[0].InnerText;
                                            string Shu_xue = nodesss.ChildNodes[indexxxxx].ChildNodes[11].ChildNodes[0].InnerText;
                                            string Yan_Yu = nodesss.ChildNodes[indexxxxx].ChildNodes[12].ChildNodes[0].InnerText;
                                            string Ke_XueJiChu = nodesss.ChildNodes[indexxxxx].ChildNodes[13].ChildNodes[0].InnerText;
                                            
                                            insert += string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}'),",SchoolName,Ban_ji,Kao_Code,Name,Zhong_Feng,Yu_wen,Shu_xue,Yan_Yu,Ke_XueJiChu,KaoShi_Type);
                                            indexxxxx += 2;
                                            if (indexxxxx>=nodesss.ChildNodes.Count) {
                                                indexxxxx = 3;
                                               
                                                break;
                                            }
                                        }

                                        //node_tab.ChildNodes
                                    }
                                }
                            }
                           
                          


                        }

                        if (index >= htnonode_Years[0].ChildNodes.Count)
                        {
                            break;
                        }
                    }
                }

            }
            //合成最终的sql语句
            insert = insert.Substring(0, insert.Length - 1);
            return insert;
        }
        /// <summary>
        ///攫取题目数目
        /// </summary>
        public int pai_KeTi_count(string url, string host, string referer, bool is_cookie)
        {
            HttpHelper http = new HttpHelper();
            HttpResult result = new HttpResult();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项  
                KeepAlive = true,
                Host = host,//"office.7net.cc",
                Method = "GET",//URL     可选项 默认为Get   
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                Cookie = Cookies,//"ds_session=86rvpd3qtvpjjnqhdnua2vfo57; Hm_lvt_a68414d98536efc52eeb879f984d8923=1518145614; Hm_lpvt_a68414d98536efc52eeb879f984d8923=1518147484",//字符串Cookie     可选项   
                Referer = referer,// == "" ? "http://office.7net.cc/Score":url,//来源URL     可选项   
                //Postdata = "",//",//Post数据     可选项GET时不需要写 这个数据是从我们数据库中读取到的   
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.9 Safari/537.36",//用户的浏览器类型，版本，操作系统     可选项有默认值   
                                                                                                                                           //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
                Allowautoredirect = false,//是否根据301跳转     可选项   
                                          //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数   
                                          //Connectionlimit = 1024,//最大连接数     可选项 默认为1024    
                                          //ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数    
                                          //ProxyPwd = "123456",//代理服务器密码     可选项    
                                          //ProxyUserName = "administrator",//代理服务器账户名     可选项   
                ResultType = ResultType.String,
                //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
            };
            item.Header.Add("Upgrade-Insecure-Requests:1");
            item.Header.Add("Accept-Encoding:gzip, deflate, br");
            item.Header.Add("Accept-Language:zh-CN,zh;q=0.9");
            //将网页的返回的报文存在result当中
            result = http.GetHtml(item);
            //是否更换cookies
            if (is_cookie)
            {
                Cookies = "";
                Cookies = result.Cookie;
            }

            //Cookies = "";
            //Cookies += result.Cookie;
            //获取返回的请求报文
            string html = result.Html;
            // Cookies= result.Cookie;
            Cookies = Cookies.Replace("HttpOnly", "");
            Cookies = Cookies.Replace("Path=/;", "");

            HtmlAgilityPack.HtmlDocument hdmt = new HtmlAgilityPack.HtmlDocument();
            hdmt.LoadHtml(html);
            HtmlNode hnd = hdmt.DocumentNode;
            HtmlNodeCollection hnc = hnd.SelectNodes("//*[@id=\"form2\"]/div[3]/div/div/script[1]");
            int counts = 0;
            if (hnc != null && hnc.Count > 0)
            {
                HtmlNode hnNode = hnc[1];
                string hnNode_Text = hnNode.InnerText;
                //获取到学生人数的总和
                string num_count = Regex.Match(hnNode_Text, @"\d+").Value;
                //计算需要一共循环几次才能取完
                double countsss = Convert.ToDouble(num_count);
                counts = Convert.ToInt32(Math.Round(countsss) / 10) + 1;

            }
            return counts;
        }

        /// <summary>
        ///攫取学生成绩时计算学生人数
        /// </summary>
        public int pai_Student_count(string url,string host,string referer,bool is_cookie) {
            HttpHelper http = new HttpHelper();
            HttpResult result = new HttpResult();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项  
                KeepAlive = true,
                Host = host,//"office.7net.cc",
                Method = "GET",//URL     可选项 默认为Get   
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                Cookie = Cookies,//"ds_session=86rvpd3qtvpjjnqhdnua2vfo57; Hm_lvt_a68414d98536efc52eeb879f984d8923=1518145614; Hm_lpvt_a68414d98536efc52eeb879f984d8923=1518147484",//字符串Cookie     可选项   
                Referer = referer,// == "" ? "http://office.7net.cc/Score":url,//来源URL     可选项   
                //Postdata = "",//",//Post数据     可选项GET时不需要写 这个数据是从我们数据库中读取到的   
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.9 Safari/537.36",//用户的浏览器类型，版本，操作系统     可选项有默认值   
                                                                                                                                           //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
                Allowautoredirect = false,//是否根据301跳转     可选项   
                                          //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数   
                                          //Connectionlimit = 1024,//最大连接数     可选项 默认为1024    
                                          //ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数    
                                          //ProxyPwd = "123456",//代理服务器密码     可选项    
                                          //ProxyUserName = "administrator",//代理服务器账户名     可选项   
                ResultType = ResultType.String,
                //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
            };
            item.Header.Add("Upgrade-Insecure-Requests:1");
            item.Header.Add("Accept-Encoding:gzip, deflate, br");
            item.Header.Add("Accept-Language:zh-CN,zh;q=0.9");
            //将网页的返回的报文存在result当中
            result = http.GetHtml(item);
            //是否更换cookies
            if (is_cookie)
            {
                Cookies = "";
                Cookies = result.Cookie;
            }

            //Cookies = "";
            //Cookies += result.Cookie;
            //获取返回的请求报文
            string html = result.Html;
            // Cookies= result.Cookie;
            Cookies = Cookies.Replace("HttpOnly", "");
            Cookies = Cookies.Replace("Path=/;", "");

            HtmlAgilityPack.HtmlDocument hdmt = new HtmlAgilityPack.HtmlDocument();
            hdmt.LoadHtml(html);
            HtmlNode hnd= hdmt.DocumentNode;
            HtmlNodeCollection hnc= hnd.SelectNodes("//*[@id=\"form2\"]/div[3]/div/div[4]/script");
            int counts = 0;
            if (hnc != null && hnc.Count > 0) {
                HtmlNode hnNode= hnc[0];
                string hnNode_Text= hnNode.InnerText;
                //获取到学生人数的总和
                string num_count= Regex.Match(hnNode_Text, @"\d+").Value;
                //计算需要一共循环几次才能取完
                double countsss= Convert.ToDouble(num_count);
             counts= Convert.ToInt32( Math.Round(countsss)/10)+1;

            }
            return counts;
        }






        /// <summary>
        /// 攫取教师授课
        /// </summary>
        /// <returns>返回生成的sql语句</returns>
        private string pai_jiaoshi_souke()
        {
            string insert = "insert into jiaoshi_souke(BanJi,BanZhu_Ren,Yu_Wen,Shu_Xue,Yin_Yu,Wu_Li,Hua_Xue,Zhen_Zi,Li_Shi,Di_Li,Shen_Wu,XinXi_JiShu,TongYong_jiShu,KeXue_JiChu,RenWen_JiChu,JiShu_SuYang,Ti_Yu,Yan_Yue,Mei_Shu,E_Yu,ZiXuan_MoKuai,She_Hui) values";
        string json=    pai_jiaoshi_souke_post("http://office.7net.cc/EDUClassLesson/rsView", "http://office.7net.cc/EDUClassLesson", "_septnet_document=%7B%22group%22%3A%22%22%2C%22type%22%3A1%7D").Replace("+OK","");
          JObject json_jiaoshi_souke=(JObject)JsonConvert.DeserializeObject(json);
            JToken json_list= json_jiaoshi_souke["List"];
            foreach (var item in json_list)
            {
                string BanJi= item["orgName"].ToString();
                 string BanZhu_Ren= item["bzr"].ToString();
                JArray banzhu=(JArray) JsonConvert.DeserializeObject(BanZhu_Ren);
                BanZhu_Ren= banzhu[0]["name"].ToString();
                insert+="('"+BanJi+"','"+BanZhu_Ren+"',";
                string teachers= item["teachers"].ToString();
                JArray teacher_jarray = (JArray)JsonConvert.DeserializeObject(teachers);
                
                for (int i = 0; i < teacher_jarray.Count; i++)
                {
                    JToken teacher_token = teacher_jarray[i];
                   string kemu=  teacher_token["km"].ToString();
                    string teacher= teacher_token["teacher"].ToString();
                    JArray tea= (JArray) JsonConvert.DeserializeObject(teacher);
                    string teacher_name = "";
                    if (tea.Count > 0) {
                        JToken jt_tea = tea[0];
                        teacher_name = jt_tea["name"].ToString();
                    }
                    insert += "'" + teacher_name + "',";
                    
                    if (true) { }
                }
                insert= insert.Substring(0, insert.Length - 1);
                insert += "),";
               
                
                
                if (true) {

                }
            }
            insert = insert.Substring(0, insert.Length-1);
            return insert;
        }
        /// <summary>
        /// 对授课教师网页的原始信息（对应方法pai_jiaoshi_souke）
        /// </summary>
        private string pai_jiaoshi_souke_post(string url,string refere,string post_data)
        {
            HttpHelper http = new HttpHelper();
            HttpResult result = new HttpResult();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项  
                Host = "office.7net.cc".Trim(),
                Encoding = Encoding.UTF8,
                KeepAlive = true,
                Method = "POST",//URL     可选项 默认为Get   
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                Cookie = Cookies,//"ds_session=86rvpd3qtvpjjnqhdnua2vfo57; Hm_lvt_a68414d98536efc52eeb879f984d8923=1518145614; Hm_lpvt_a68414d98536efc52eeb879f984d8923=1518147484",//字符串Cookie     可选项   
                Referer = refere,//来源URL     可选项   
                Postdata = post_data,//",//Post数据     可选项GET时不需要写 这个数据是从我们数据库中读取到的   
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                UserAgent = "Mozilla/5.0(WindowsNT10.0;WOW64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/62.0.3202.9Safari/537.36".Trim(),//用户的浏览器类型，版本，操作系统     可选项有默认值   
                ContentType = "application/x-www-form-urlencoded;charset=UTF-8".Trim(),//返回类型    可选项有默认值   
                Allowautoredirect = true,//是否根据301跳转     可选项   
                                         //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数   
                                         //Connectionlimit = 1024,//最大连接数     可选项 默认为1024    
                                         //ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数    
                                         //ProxyPwd = "123456",//代理服务器密码     可选项    
                                         //ProxyUserName = "administrator",//代理服务器账户名     可选项   
                ResultType = ResultType.String,
                Accept = "*/*",
            };
            item.Header.Add("X-Requested-With", "XMLHttpRequest");
            item.Header.Add("Accept-Encoding", "gzip,deflate");
            item.Header.Add("Accept-Language", "zh-CN,zh;q=0.9");
            item.Header.Add("Origin", "http://office.7net.cc");
            //将网页的返回的报文存在result当中
            result = http.GetHtml(item);
            //获取返回的请求报文
            string html = result.Html;
            return html;
        }



        /// <summary>
        /// 获取到所有班级
        /// </summary>
        /// <returns>返回生成的sql语句</returns>
        public string pai_banjiLieBiao() {
            try { 
            //第一次获取到所有班级的数量
            string banji_LieBiaoJson= pai_banjiLieBiao_post("http://office.7net.cc/EDUTheclass/RSView?schoolguid=c47396f2-1f32-11e6-9a2b-02004c4f4f50&key=&viewIndex=2&viewLength=10&httpmethod=get", "http://office.7net.cc/EDUTheclass", "7net=septnet").Replace("+OK","");
            JObject  json_post= (JObject)JsonConvert.DeserializeObject(banji_LieBiaoJson);
            string All= json_post["All"].ToString();
            //第二次请求获取到所有班的数据
            string banji_LieBiaoJson2=pai_banjiLieBiao_post("http://office.7net.cc/EDUTheclass/RSView?schoolguid=c47396f2-1f32-11e6-9a2b-02004c4f4f50&key=&viewIndex=1&viewLength="+All+"&httpmethod=get", "http://office.7net.cc/EDUTheclass", "7net=septnet").Replace("+OK", "");
            JObject json_post2 = (JObject)JsonConvert.DeserializeObject(banji_LieBiaoJson2);

            JToken data= json_post2["RSData"];
            string insert = "insert into banji(danwei_ID,danwei_Name,Feize_name,Nianji_DaiMa)values";
            foreach (var item in data)
            {
                //查询单位代码
                string Danwei_id= item["Code"].ToString();
                //查询单位名称
                string Danwei_Name = item["Name"].ToString();
                //查询分组名称
                string Feize_name = item["GroupName"].ToString();
                //合成年级代码
                string Nianji_DaiMa = "";
                if (Danwei_id.ToUpper().Contains("X")) {
                    if (Danwei_id[1]=='1') {
                        Nianji_DaiMa = "X1";
                    }
                    if (Danwei_id[1] == '2')
                    {
                        Nianji_DaiMa = "X2";
                    }
                    if (Danwei_id[1] == '3')
                    {
                        Nianji_DaiMa = "X3";
                    }
                    if (Danwei_id[1] == '4')
                    {
                        Nianji_DaiMa = "X4";
                    }
                    if (Danwei_id[1] == '5')
                    {
                        Nianji_DaiMa = "X5";
                    }
                    if (Danwei_id[1] == '6')
                    {
                        Nianji_DaiMa = "X6";
                    }

                } else if (Danwei_id.ToUpper().Contains("C")) {
                    if (Danwei_id[1]=='1') {
                        Nianji_DaiMa = "C1";
                    }
                    if (Danwei_id[1] == '2')
                    {
                        Nianji_DaiMa = "C2";
                    }
                    if (Danwei_id[1] == '3')
                    {
                        Nianji_DaiMa = "C3";

                    }
                } else {
                    if (Danwei_id[1] == '1')
                    {
                        Nianji_DaiMa = "G1";
                    }
                    if (Danwei_id[1] == '2')
                    {
                        Nianji_DaiMa = "G1";
                    }
                    if (Danwei_id[1] == '3')
                    {
                        Nianji_DaiMa = "G1";
                    }
                }
                insert += "('"+Danwei_id+"','"+Danwei_Name+"','"+Feize_name+"','"+Nianji_DaiMa+"'),";
            }
            insert= insert.Substring(0, insert.Length - 1);
            return insert;
            }
            catch {
            return "";
            }
        }

        /// <summary>
        /// 获取班级列表的原始网页数据(对应方法pai_banjiLieBiao)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="refere"></param>
        /// <param name="post_data"></param>
        /// <returns></returns>
        private string pai_banjiLieBiao_post(string url,string refere,string post_data)
        {

            HttpHelper http = new HttpHelper();
            HttpResult result = new HttpResult();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项  
                Host = "office.7net.cc".Trim(),
                Encoding = Encoding.UTF8,
                KeepAlive = true,
                Method = "POST",//URL     可选项 默认为Get   
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                Cookie = Cookies,//"ds_session=86rvpd3qtvpjjnqhdnua2vfo57; Hm_lvt_a68414d98536efc52eeb879f984d8923=1518145614; Hm_lpvt_a68414d98536efc52eeb879f984d8923=1518147484",//字符串Cookie     可选项   
                Referer = refere,//来源URL     可选项   
                Postdata = post_data,//",//Post数据     可选项GET时不需要写 这个数据是从我们数据库中读取到的   
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                UserAgent = "Mozilla/5.0(WindowsNT10.0;WOW64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/62.0.3202.9Safari/537.36".Trim(),//用户的浏览器类型，版本，操作系统     可选项有默认值   
                ContentType = "application/x-www-form-urlencoded;charset=UTF-8".Trim(),//返回类型    可选项有默认值   
                Allowautoredirect = true,//是否根据301跳转     可选项   
                                         //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数   
                                         //Connectionlimit = 1024,//最大连接数     可选项 默认为1024    
                                         //ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数    
                                         //ProxyPwd = "123456",//代理服务器密码     可选项    
                                         //ProxyUserName = "administrator",//代理服务器账户名     可选项   
                ResultType = ResultType.String,
                Accept = "*/*",
            };
            item.Header.Add("X-Requested-With", "XMLHttpRequest");
            item.Header.Add("Accept-Encoding", "gzip,deflate");
            item.Header.Add("Accept-Language", "zh-CN,zh;q=0.9");
            item.Header.Add("Origin", "http://office.7net.cc");
            //将网页的返回的报文存在result当中
            result = http.GetHtml(item);
            //获取返回的请求报文
            string html = result.Html;
            return html;
        }

      

      



           
        
        /// <summary>
        /// 从HttpWebResponse对象中提取响应的数据转换为字符串
        /// </summary>
        /// <param name="webresponse"></param>
        /// <returns></returns>
        public string GetResponseString(HttpWebResponse webresponse)
        {
            using (Stream s = webresponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// 获取所有教师的信息
        /// </summary>
        /// <returns></returns>
        public string pai_Teach()
        {
            //第一次获取到所有老师的人数
             string jsonns= pai_Teach_post("http://office.7net.cc/EDUTeacher/rsview?sort=YJUser_Teacher_id&subject=&tags=&schoolguid=c47396f2-1f32-11e6-9a2b-02004c4f4f50&key=&viewIndex=2&viewLength=10&httpmethod=get", "http://office.7net.cc/EDUTeacher", "7net= septnet").Replace("+OK","");
             JObject json= (JObject)JsonConvert.DeserializeObject(jsonns);
            string values = json["All"].ToString();
            string json_counts= pai_Teach_post("http://office.7net.cc/EDUTeacher/rsview?sort=YJUser_Teacher_id&subject=&tags=&schoolguid=c47396f2-1f32-11e6-9a2b-02004c4f4f50&key=&viewIndex=1&viewLength="+values+"&httpmethod=get", "http://office.7net.cc/EDUTeacher", "7net= septnet").Replace("+OK", "");
            //将接收到的最终json数据开始进行解析
            JObject json_obj_counts=(JObject) JsonConvert.DeserializeObject(json_counts);
            
            JToken jtoken = json_obj_counts["RSData"];
            string insert = "insert into jiaoshi_mince(Zhang_Hao,`Name`,Xue_ke,Mi_Ma,Biao_Qian,Xue_Xiao)VALUE";
            foreach (var item in jtoken)
            {
                string zhang_Hao = item["Code"].ToString();
                string namne = item["Name"].ToString();
                string Xue_ke = item["Subject"].ToString();
                string MiMa = item["Pass"].ToString();
                string Biao_Qian = item["Tag"].ToString();
                string Xue_Xiao = item["SchoolName"].ToString();
                insert += "('" + zhang_Hao + "','" + namne + "','" + Xue_ke + "','" + MiMa + "','" + Biao_Qian + "','" + Xue_Xiao + "'),";

            }
            insert = insert.Substring(0,insert.Length-1);
            return insert;
        }

        /// <summary>
        /// 获取所有教师信息的原始网页数据(对应方法pai_Teach)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="refere"></param>
        /// <param name="post_data"></param>
        /// <returns></returns>
        public string pai_Teach_post(string url,string refere,string post_data) {
            HttpHelper http = new HttpHelper();
            HttpResult result = new HttpResult();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项  
                Host = "office.7net.cc".Trim(),
                Encoding = Encoding.UTF8,
                KeepAlive = true,
                Method = "POST",//URL     可选项 默认为Get   
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                Cookie = Cookies,//"ds_session=86rvpd3qtvpjjnqhdnua2vfo57; Hm_lvt_a68414d98536efc52eeb879f984d8923=1518145614; Hm_lpvt_a68414d98536efc52eeb879f984d8923=1518147484",//字符串Cookie     可选项   
                Referer = refere,//来源URL     可选项   
                Postdata = post_data,//",//Post数据     可选项GET时不需要写 这个数据是从我们数据库中读取到的   
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                UserAgent = "Mozilla/5.0(WindowsNT10.0;WOW64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/62.0.3202.9Safari/537.36".Trim(),//用户的浏览器类型，版本，操作系统     可选项有默认值   
                ContentType = "application/x-www-form-urlencoded;charset=UTF-8".Trim(),//返回类型    可选项有默认值   
                Allowautoredirect = true,//是否根据301跳转     可选项   
                                         //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数   
                                         //Connectionlimit = 1024,//最大连接数     可选项 默认为1024    
                                         //ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数    
                                         //ProxyPwd = "123456",//代理服务器密码     可选项    
                                         //ProxyUserName = "administrator",//代理服务器账户名     可选项   
                ResultType = ResultType.String,
                Accept = "*/*",
            };
            item.Header.Add("X-Requested-With", "XMLHttpRequest");
            item.Header.Add("Accept-Encoding", "gzip,deflate");
            item.Header.Add("Accept-Language", "zh-CN,zh;q=0.9");
            item.Header.Add("Origin", "http://office.7net.cc");
            //将网页的返回的报文存在result当中
            result = http.GetHtml(item);
            //获取返回的请求报文
            string html = result.Html;
            return html;

        }


        /// <summary>
        /// 获取所有学生的信息
        /// </summary>
        /// <returns>返回已经生成的sql语句</returns>
        public string pai_Student() {
            //第一次请求，获取到学生人数的总和
           string jsonss= pai_Student_post("http://office.7net.cc/EDUStudent/RSView?schoolguid=c47396f2-1f32-11e6-9a2b-02004c4f4f50&key=&viewIndex=1&viewLength=10&httpmethod=get", "7net=septnet", "http://office.7net.cc/EDUStudent").Replace("+OK","");
            JObject json = (JObject)JsonConvert.DeserializeObject(jsonss);
            string values = json["All"].ToString();
            string json_counts= pai_Student_post("http://office.7net.cc/EDUStudent/RSView?schoolguid=c47396f2-1f32-11e6-9a2b-02004c4f4f50&key=&viewIndex=1&viewLength="+values+"&httpmethod=get", "7net=septnet", "http://office.7net.cc/EDUStudent").Replace("+OK", "");
            JObject json_count_object= (JObject) JsonConvert.DeserializeObject(json_counts);
            //解析返回的数据
           JToken countss=  json_count_object["RSData"];
            int json_cou= countss.Count();
          string  insert = "insert into KaoShen_mince(Name,Kao_Hao,KaoShen_DanWei,ShenFenZhen_HaoMa)VALUE ";
            foreach (var item in countss)
            {
                string name= item["Name"].ToString();
                string orgName = item["OrgName"].ToString();
                string kao_hao = item["Code"].ToString();
                string sfz= item["Sfz"].ToString();
                insert += "('" + name + "','" + kao_hao + "','" + orgName + "','" + sfz + "'),";
            }
            //最终合成的sql语句
            insert= insert.Substring(0, insert.Length - 1);
            return insert;
        }
        /// <summary>
        /// 获取所有学生信息的原始网页数据(对应方法pai_student)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="post_data"></param>
        /// <param name="refere"></param>
        /// <returns></returns>
        public string pai_Student_post(string url,string post_data,string refere) {
            HttpHelper http = new HttpHelper();
            HttpResult result = new HttpResult();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项  
                Host = "office.7net.cc".Trim(),
                Encoding = Encoding.UTF8,
                KeepAlive = true,
                Method = "POST",//URL     可选项 默认为Get   
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                Cookie = Cookies,//"ds_session=86rvpd3qtvpjjnqhdnua2vfo57; Hm_lvt_a68414d98536efc52eeb879f984d8923=1518145614; Hm_lpvt_a68414d98536efc52eeb879f984d8923=1518147484",//字符串Cookie     可选项   
                Referer = refere,//来源URL     可选项   
                Postdata = post_data,//",//Post数据     可选项GET时不需要写 这个数据是从我们数据库中读取到的   
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                UserAgent = "Mozilla/5.0(WindowsNT10.0;WOW64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/62.0.3202.9Safari/537.36".Trim(),//用户的浏览器类型，版本，操作系统     可选项有默认值   
                ContentType = "application/x-www-form-urlencoded;charset=UTF-8".Trim(),//返回类型    可选项有默认值   
                Allowautoredirect = true,//是否根据301跳转     可选项   
                                         //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数   
                                         //Connectionlimit = 1024,//最大连接数     可选项 默认为1024    
                                         //ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数    
                                         //ProxyPwd = "123456",//代理服务器密码     可选项    
                                         //ProxyUserName = "administrator",//代理服务器账户名     可选项   
                ResultType = ResultType.String,
                Accept = "*/*",
            };
            item.Header.Add("X-Requested-With", "XMLHttpRequest");
            item.Header.Add("Accept-Encoding", "gzip,deflate");
            item.Header.Add("Accept-Language", "zh-CN,zh;q=0.9");
            item.Header.Add("Origin", "http://office.7net.cc");
            //将网页的返回的报文存在result当中
            result = http.GetHtml(item);
            //获取返回的请求报文
            string html = result.Html;
            return html;
        }


        /// <summary>
        /// 对url编码的解析
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string UrlEncode(string str)
        {
            StringBuilder sb = new StringBuilder();
            byte[] byStr = System.Text.Encoding.UTF8.GetBytes(str); //默认是System.Text.Encoding.Default.GetBytes(str)
            for (int i = 0; i < byStr.Length; i++)
            {
                sb.Append(@"%" + Convert.ToString(byStr[i], 16));
            }

            return (sb.ToString());
        }
        /// <summary>
        /// 替代post方法解决弹窗问题的方法，post不能解决可尝试使用此方法
        /// </summary>
        /// <param name="url">需要post的url地址</param>
        /// <param name="parameters">需要对post的参数</param>
        /// <param name="refere">需要post的referer参数</param>
        /// <returns></returns>
        public HttpWebResponse CreatePostHttpResponse(string url, IDictionary<string, string> parameters, string refere = "")
        {
            //string Cook= UrlEncode(Cookies);
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;//创建请求对象
            request.Method = "POST";//请求方式
            request.ContentType = "application/x-www-form-urlencoded;charset=UTF-8";//链接类型
            request.UserAgent = @"Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.9 Safari/537.36";
            request.Headers.Add("Cookie", Cookies);
            request.Host= "office.7net.cc";
            request.Timeout = 100000;
            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
            request.Headers.Add("Accept-Encoding", "gzip,deflate");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            request.Headers.Add("Origin", "http://www.7net.cc");
           
            request.Accept = "*/*";
            
            request.KeepAlive = true;
            
            
            request.Referer= refere==""?"http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=2018-01-09&viewIndex=1":refere;



            //构造查询字符串
            if (!(parameters == null || parameters.Count == 0))
            {
                StringBuilder buffer = new StringBuilder();
                bool first = true;
                foreach (string key in parameters.Keys)
                {

                    if (!first)
                    {
                        buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                    }
                    else
                    {
                        buffer.AppendFormat("{0}={1}", key, parameters[key]);
                        first = false;
                    }
                }
                byte[] data = Encoding.UTF8.GetBytes(buffer.ToString());
                //写入请求流
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            return request.GetResponse() as HttpWebResponse;
        }


        /// <summary>
        /// 为解决弹窗问题的方法,一般调用此方法后立即调用GET方法
        /// </summary>
        /// <param name="post_data">请求的data数据</param>
        /// <returns>返回请求的结果</returns>
        public string Post(string url,string post_data) {
            HttpHelper http = new HttpHelper();
            HttpResult result = new HttpResult();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项  
                Host = "www.7net.cc".Trim(),
                Encoding=Encoding.UTF8,
                KeepAlive=true,
                Method = "POST",//URL     可选项 默认为Get   
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                Cookie = Cookies,//"ds_session=86rvpd3qtvpjjnqhdnua2vfo57; Hm_lvt_a68414d98536efc52eeb879f984d8923=1518145614; Hm_lpvt_a68414d98536efc52eeb879f984d8923=1518147484",//字符串Cookie     可选项   
                Referer = "http://office.7net.cc/View/Private/Office/dlgSelectExam.aspx?date=2017%u5E741%u67081%u65E5",//来源URL     可选项   
                Postdata = post_data,//",//Post数据     可选项GET时不需要写 这个数据是从我们数据库中读取到的   
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                UserAgent = "Mozilla/5.0(WindowsNT10.0;WOW64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/62.0.3202.9Safari/537.36".Trim(),//用户的浏览器类型，版本，操作系统     可选项有默认值   
                ContentType = "application/x-www-form-urlencoded;charset=UTF-8".Trim(),//返回类型    可选项有默认值   
                Allowautoredirect = true,//是否根据301跳转     可选项   
                                          //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数   
                                          //Connectionlimit = 1024,//最大连接数     可选项 默认为1024    
                                          //ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数    
                                          //ProxyPwd = "123456",//代理服务器密码     可选项    
                                          //ProxyUserName = "administrator",//代理服务器账户名     可选项   
                ResultType = ResultType.String,
                Accept = "*/*",
            };
            item.Header.Add("X-Requested-With","XMLHttpRequest");
            item.Header.Add("Accept-Encoding","gzip,deflate");
            item.Header.Add("Accept-Language","zh-CN,zh;q=0.9");
            item.Header.Add("Origin","http://www.7net.cc");
            //将网页的返回的报文存在result当中
            result = http.GetHtml(item);
            //获取返回的请求报文
            string html = result.Html;
            //更新Cookies
            Cookies = "";
            Cookies= result.Cookie;
            return html;
        }

        /// <summary>
        /// 为解决弹窗问题的方法，一般调用了POST方法立即调用此方法
        /// </summary>
        /// <param name="url">输入请求的网址</param>
        /// <returns>返回请求到的网页报文数据</returns>
        public string Get(string url,string host,string referer,bool is_cookie) {
            HttpHelper http = new HttpHelper();
            HttpResult result = new HttpResult();
            HttpItem item = new HttpItem()
            {
                URL =url,//URL     必需项  
                KeepAlive=true,
                Host = host,//"office.7net.cc",
                Method = "GET",//URL     可选项 默认为Get   
                IsToLower = false,//得到的HTML代码是否转成小写     可选项默认转小写   
                Cookie = Cookies,//"ds_session=86rvpd3qtvpjjnqhdnua2vfo57; Hm_lvt_a68414d98536efc52eeb879f984d8923=1518145614; Hm_lpvt_a68414d98536efc52eeb879f984d8923=1518147484",//字符串Cookie     可选项   
                Referer = referer,// == "" ? "http://office.7net.cc/Score":url,//来源URL     可选项   
                //Postdata = "",//",//Post数据     可选项GET时不需要写 这个数据是从我们数据库中读取到的   
                Timeout = 100000,//连接超时时间     可选项默认为100000    
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000   
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.9 Safari/537.36",//用户的浏览器类型，版本，操作系统     可选项有默认值   
                                                                                                                                           //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",//返回类型    可选项有默认值   
                Allowautoredirect = false,//是否根据301跳转     可选项   
                                          //CerPath = "d:\123.cer",//证书绝对路径     可选项不需要证书时可以不写这个参数   
                                          //Connectionlimit = 1024,//最大连接数     可选项 默认为1024    
                                          //ProxyIp = "",//代理服务器ID     可选项 不需要代理 时可以不设置这三个参数    
                                          //ProxyPwd = "123456",//代理服务器密码     可选项    
                                          //ProxyUserName = "administrator",//代理服务器账户名     可选项   
                ResultType = ResultType.String,
                //ContentType = "application/x-www-form-urlencoded; charset=UTF-8",
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8",
            };
            item.Header.Add("Upgrade-Insecure-Requests:1");
            item.Header.Add("Accept-Encoding:gzip, deflate, br");
            item.Header.Add("Accept-Language:zh-CN,zh;q=0.9");
            //将网页的返回的报文存在result当中
            result = http.GetHtml(item);
            //是否更换cookies
            if (is_cookie) {
                Cookies = "";
                Cookies = result.Cookie;
            }
            
            //Cookies = "";
            //Cookies += result.Cookie;
            //获取返回的请求报文
            string html = result.Html;
           // Cookies= result.Cookie;
            Cookies= Cookies.Replace("HttpOnly","");
            Cookies= Cookies.Replace("Path=/;", "");
            return html;
        }
        
       
      
        private void webBrow_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

            if (index==1) {
                Cookies = "";
                Cookies += webBrow.Document.Cookie;
                HtmlElement hec= webBrow.Document.GetElementsByTagName("a")[7];
                
                hec.InvokeMember("onclick");
               



            }
            if (index == 2)
            {
                Cookies = "";
                Cookies+= webBrow.Document.Cookie;
                btn_start.Enabled = true;
                string htc = webBrow.DocumentText;
               
                lab_status.Text = "登陆成功";
            }
           

            // HtmlElementCollection hec_input = webBrow.Document.GetElementsByTagName("input");
            if (index==0)
            {

               webBrow.Document.GetElementsByTagName("input")[1].SetAttribute("type", "password");
                webBrow.Document.GetElementsByTagName("input")[1].Focus();
            }
            index += 1;

            //webBrow.Document.GetElementsByTagName("input")[1].InnerText = txt_pwd.Text;//SetAttribute("value", txt_pwd.Text);

            //var login_btn = webBrow.Document.InvokeScript("login()");

            //qing_login();
            //index_login();
            //login();
            //Cookies= Cookies.Replace("HttpOnly", "");
            //if (login_btn != null)
            //{

            //}


        }

  
    
        private void Form1_Load(object sender, EventArgs e)
        {
            

            //WebBrowser webbrows = new WebBrowser();
            //string head = "";
            //webbrows.Url = new Uri("http://www.7net.cc/");
            //webbrows.Navigate("http://www.7net.cc/","", Encoding.ASCII.GetBytes("_septnet_document=%7B%22type%22%3A%22WP0001%22%2C%22value%22%3A%22%22%2C%22source%22%3A%22pc%22%2C%22medium%22%3A%22%22%2C%22usercode%22%3A%2217051107900%22%7D"),"Header");
            //string cookies= webbrows.Document.Cookie;
        }

        private void btn_login_Click(object sender, EventArgs e)
        {
            string pwd= txt_pwd.Text;
            string Name= txt_userName.Text;
            webBrow.Document.GetElementsByTagName("input")[1].SetAttribute("value", txt_pwd.Text);
            webBrow.Document.GetElementsByTagName("input")[0].SetAttribute("value", txt_userName.Text);
            HtmlElement html_a = webBrow.Document.GetElementsByTagName("a")[8];
            var result = html_a.InvokeMember("onclick");
            Cookies += webBrow.Document.Cookie;
          
           
        }
        /// <summary>
        /// 连接到主数据库的字符串
        /// </summary>
        private static string str_connn { get; set; }
        private void Form1_Load_1(object sender, EventArgs e)
        {
            str_connn = HappyGhostMySql_help.run(); //返回 root 链接字符串
            string[] str_con = HappyGhostMySql_help.test_data("root", "HappyGhost001!", "School");

          //  string[] str_result_info = HappyGhostMySql_help.test_data(str_con[0], str_con[1], str_con[0]);
            HappyGhostMySql_help.test_data(str_con[0], str_con[1], str_con[0]);
            if (str_con.Length > 0)
            {
                str_connn = str_con[1];
            }
            else
            {
                MessageBox.Show("数据库服务未开启", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }


            webBrow.Url = new Uri("http://www.7net.cc/");
            btn_start.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            index = 0;
            webBrow.Url = new Uri("http://www.7net.cc/");
        }
    }
}
