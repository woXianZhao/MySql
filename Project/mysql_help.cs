using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.IO;
using System.Diagnostics;
using System.Data;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Collections;
using System.Data.Odbc;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Threading;


namespace Project
{
    public class HappyGhostMySql_help
    {
        static int i = 0;
        //数据库主目录，这个是基于我自己的电脑的目录需要用的话自己可以改
        static string  str_path = System.AppDomain.CurrentDomain.BaseDirectory+ "RunTime\\data_bases5.1\\";
        //mysqld 文件名
        private static string File_newname = "";
        //启动成功后的PID号
        private static int pid = 0;

        /// <summary>
        /// 自己设置的数据库路径
        /// </summary>
        public static string str_paths { get; set; }

        /// <summary>
        /// 开启mysql服务(不建议单独使用，会造成未知错误)
        /// </summary>
        private static void run_Mysqld()
        {
            Random random=new Random();
            int r_index= random.Next(3, 9000);
            File_newname = "mysqld_"+r_index+".exe";
            //如果在调用期间,多次打开了服务,则我们先将其他服务依次关闭.再重新开启
            //如果有服务则关闭
            //无,则跳此次语句
            Process[] pros = Process.GetProcessesByName("mysqld_2");
            if (pros.Length > 0)
            {
                foreach (Process item in pros)
                {
                    item.Kill();
                }
            }
            File.Copy(str_path+ @"bin\mysqld_2.exe",str_path+@"bin\"+File_newname,true);

            if (str_paths != "")
            {
                string path = "" + str_path + "\\my.ini";
                string[] strInfo = File.ReadAllLines("" + str_path + "\\my.ini");
                //提取文件里面的端口数据，用来启动我们自己的数据库
                int port = Convert.ToInt32(strInfo[1].Substring(7));

                Process pro = new Process();
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.CreateNoWindow = true;
                pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pro.StartInfo.RedirectStandardError = false;
                pro.StartInfo.Arguments = string.Join(" ", "/C start /b \"\" \"" + str_path + "\\bin\\"+File_newname+"\" --defaults-file=\" " + str_path + "\\my.ini\" --port=" + port + "");
                Thread.Sleep(500);
                pro.StartInfo.FileName = "cmd";
                pro.StartInfo.WorkingDirectory = "c:\\Windows\\System32\\";
                pro.Start();
            }
            else
            {
                string[] strInfo = File.ReadAllLines("" + str_path + "\\my.ini");
                //提取文件里面的端口数据，用来启动我们自己的数据库
                int port = Convert.ToInt32(strInfo[1].Substring(7));

                Process pro = new Process();
                pro.StartInfo.UseShellExecute = false;
                pro.StartInfo.CreateNoWindow = true;
                pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                pro.StartInfo.RedirectStandardError = false;
                pro.StartInfo.Arguments = string.Join(" ", "/C start /b \"\" \"" + str_path + "\\bin\\"+File_newname+"\" --defaults-file=\" " + str_path + "\\my.ini\" --port=" + port + "");
                Thread.Sleep(500);
                pro.StartInfo.FileName = "cmd";
                pro.StartInfo.WorkingDirectory = "c:\\Windows\\System32\\";
                pro.Start();
            }
        }


        /// <summary>
        /// 关闭自己的mysql服务
        /// </summary>
        public static void close_mysqld()
        {
            bool f = false;
            int index = 0;
            while (index < 10 && f == false)
            {
                index++;
                try
                {
                    Process pro = new Process();
                    pro.StartInfo.UseShellExecute = false;
                    pro.StartInfo.CreateNoWindow = true;
                    pro.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    pro.StartInfo.RedirectStandardError = false;
                    pro.StartInfo.Arguments = string.Join(" ", "/C taskkill /f /im " + File_newname + "");
                    pro.StartInfo.FileName = "cmd";
                    pro.StartInfo.WorkingDirectory = "c:\\Windows\\System32\\";
                    pro.Start();
                    Thread.Sleep(1000);
                    File.SetAttributes(str_path + "\\bin\\" + File_newname, FileAttributes.Normal);
                    File.Delete(str_path + "\\bin\\" + File_newname);
                    f = true;
                }
                catch
                {
                }
            }
        }
        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <param name="connectionString">连接数据库字符串</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, string connectionString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        public static object GetSingle(string SQLString, int Times, string connectionString)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        object obj = cmd.ExecuteScalar();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        connection.Close();
                        throw e;
                    }
                }
            }
        }

        /// <summary>
        /// 执行一条计算查询结果语句，返回查询结果（object）。
        /// </summary>
        /// <param name="SQLString">计算查询结果语句</param>
        /// <returns>查询结果（object）</returns>
        public static object GetSingle(string SQLString, string connectionString, params MySqlParameter[] cmdParms)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                using (MySqlCommand cmd = new MySqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        object obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        if ((Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)))
                        {
                            return null;
                        }
                        else
                        {
                            return obj;
                        }
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {
                        throw e;
                    }
                }
            }
        }

        private static void PrepareCommand(MySqlCommand cmd, MySqlConnection conn, MySqlTransaction trans, string cmdText, MySqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {


                foreach (MySqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        
       /// <summary>
       /// 用于连接数据库的备份(慎用)
       /// </summary>
       /// <param name="str_root">root权限的数据库连接字符串</param>
       
        public static void bei_fen(string str_root)
        {
            using (MySqlConnection mysql_conn=new MySqlConnection(str_root))
            {
                using (MySqlCommand cmd=mysql_conn.CreateCommand())
                {
                    try
                    {
                        mysql_conn.Open();
                        string str_cmd = @"create table backup.agentip select * from appcommon.agentip;
                                            create table backup.api select * from appcommon.api;
                                            create table back.c_job_count select * from appcommon.c_job_count;
                                            create table back.category_collention select * from appcommon.category_collention;
                                            create table back.changeipflag select * from appcommon.changeipflag;
                                            create table back.changeipparameter select * from appcommon.changeipparameter;
                                            create table back.configinfo slect * from appcommon.configinfo;
                                            create table back.configlist select * from appcommon.configlist;
                                            create table back.list_url select * from appcommon.list_url;
                                            create table back.paping_flag select * from appcommon.paping_flag;
                                            create table back.scriptlog select * from appcommon.scriptlog;
                                            create table back.systemerror select * from appcommon.systemerror;
                                            create table back.systeminfo select * from appcommon.systeminfo;
                                            create table back.task_plan select * from appcommon.task_plan;
                                            create table back.task_planinfo select * from appcommon.task_planinfo;
                                            create table back.ua select * from appcommon.ua";
                        cmd.CommandText = str_cmd;
                        cmd.ExecuteNonQuery();
                    }
                    catch(MySql.Data.MySqlClient.MySqlException e)
                    {
                        
                        mysql_conn.Close();
                        

                    }
                    
                }
            }
            


        }

        /// <summary>
        /// 用于数据库的恢复（慎用）
        /// </summary>
        /// <param name="str_root">root权限的数据库连接字符串</param>
        public static void hui_fu(string str_root)
        {
            using (MySqlConnection mysql_conn = new MySqlConnection(str_root))
            {
                using (MySqlCommand cmd = mysql_conn.CreateCommand())
                {
                    try
                    {
                        mysql_conn.Open();
                        string str_cmd = @"
                                            drop database appcommon;
                                            create database appcommon;
                                            create table appcommon.agentip select * from backup.agentip;
                                            create table appcommon.api select * from backup.api;
                                            create table appcommon.c_job_count select * from backup.c_job_count;
                                            create table appcommon.category_collention select * from backup.category_collention;
                                            create table appcommon.changeipflag select * from backup.changeipflag;
                                            create table appcommon.changeipparameter select * from backup.changeipparameter;
                                            create table appcommon.configinfo slect * from backup.configinfo;
                                            create table appcommon.configlist select * from backup.configlist;
                                            create table appcommon.list_url select * from backup.list_url;
                                            create table appcommon.paping_flag select * from backup.paping_flag;
                                            create table appcommon.scriptlog select * from backup.scriptlog;
                                            create table appcommon.systemerror select * from backup.systemerror;
                                            create table appcommon.systeminfo select * from backup.systeminfo;
                                            create table appcommon.task_plan select * from backup.task_plan;
                                            create table appcommon.task_planinfo select * from backup.task_planinfo;
                                            create table appcommon.ua select * from back.ua";
                        cmd.CommandText = str_cmd;
                        cmd.ExecuteNonQuery();
                    }
                    catch (MySql.Data.MySqlClient.MySqlException e)
                    {

                        mysql_conn.Close();


                    }

                }
            }

        }

        
        /// <summary>
        /// 进行用户的验证或者创建如果用存在则不创建，如果不存在则在后台 创建一个与之相对应的数据库和权限
        /// </summary>
        /// <param name="user">用户名</param>
        /// <param name="password">密码</param>
        /// <returns></returns>
        public static string[] create_User(string user, string password = null)
        {
            string[] str_info = new string[2];
            //提取文件里面的端口数据，用来连接数据库
            string[] strInfo = File.ReadAllLines(@"" + str_path + "\\my.ini");
            int port = Convert.ToInt32(strInfo[1].Substring(7));

            //此处为非常重要的地方慎改
            string str_con = "server=127.0.0.1;database=mysql;uid=root;pwd=HappyGhost001!;pooling=true;port=" + port + ";Charset=utf8";
            MysqlHelper.MySqlConnString(str_con);
            string sql = string.Format("select * from user where User='{0}' ", user);
            DataTable dt = MysqlHelper.ExecuteDataTable(sql);
            if (dt == null || dt.Rows.Count <= 0)
            {
                //数据库中无该用户名---新建
                List<string> SQLStringList = new List<string>();
                //SQLStringList.Add(string.Format("drop user '{0}'@'localhost';", user));
                //SQLStringList.Add(string.Format("flush privileges;"));
                SQLStringList.Add(string.Format("create user '{0}'@'localhost' IDENTIFIED by '{1}';", user, password));
                SQLStringList.Add(string.Format("create database `{0}` DEFAULT CHARACTER SET utf8 COLLATE utf8_general_ci;", user));
                SQLStringList.Add(string.Format("grant all privileges on `{0}`.* to '{0}'@'localhost' identified by '{1}'; ", user, password));
                SQLStringList.Add(string.Format("flush privileges;"));
                int Flag= MysqlHelper.ExecuteSqlTran(SQLStringList);
                if(Flag==1)
                {
                    //执行成功
                    str_info[0] = user;
                    str_info[1] = password;
                }
                else
                {
                    str_info[0] = user;
                    str_info[1] = "创建失败";
                }
            }
            else
            {
                //修改密码
                List<string> SQLStringList = new List<string>();
                SQLStringList.Add(string.Format("SET PASSWORD FOR '{0}'@'localhost'= PASSWORD('{1}');", user,password));
                SQLStringList.Add(string.Format("flush privileges;"));
                int Flag = MysqlHelper.ExecuteSqlTran(SQLStringList);
                //执行成功
                str_info[0] = user;
                str_info[1] = password;
            }
            return str_info;
        }
        
        /// <summary>
        /// 测试数据库是否成连接
        /// 成功返回的mysql连接字符串否则返回（下标0为当前服务开启的pid,1为连接信息）
        /// </summary>
        /// <param name="uid">数据库用户名</param>
        /// <param name="pwd">数据库密码</param>
        /// <param name="databases">要连接的数据库名称</param>
        /// <returns></returns>
        public static string[] test_data(string uid,string pwd,string databases)
        {
            string[] return_str = new string[2];
            //不管数据库有没开启，我们都先将服务关闭一下，以免造成二次开启
           // close_mysqld();
            //run_Mysqld();
            string[] strInfo = File.ReadAllLines(@"" + str_path + "\\my.ini");
            //提取文件里面的端口数据，用来连接数据库
            int port = Convert.ToInt32(strInfo[1].Substring(7));
            //写好数据库的连接参数

            //server=服务器地址
            //databases=数据库名称 uid=用户名  pwd=密码(一定要填写正确) port=端口(一般情况下默认的是3306)
            //第一次查询，查询测试是否有这个用户名
           
            string except = "server=127.0.0.1;database="+databases+";uid="+uid+";pwd="+pwd+";pooling=true;port=" + port + ";charset=utf8;";
            return_str[1] = except;
            try
            {
                using (MySqlConnection con = new MySqlConnection(except))
                {
                    con.Open();
                }
            }
            catch (MySqlException e)
            {
                string str_message = e.Message;

                if (Regex.IsMatch(str_message, @"^Unknown database"))
                {
                    //return except = "2";
                    return_str[0] = "";
                    return_str[1] = "失败:数据库不存在";
                }
                else
                {
                    //except = "1";
                    return_str[0] = "";
                    return_str[1] = "失败:用户名或密码错误";
                }

            }
            string str= File_newname.Replace(".exe", "");
            Process []pros= Process.GetProcessesByName(str);
            if (pros.Length !=0 )
            {
                int pros_id = pros[0].Id;
                return_str[0] = pros_id.ToString();
            }
            
            return return_str;
            //close_mysqld();
            //return except;
        }


        /// <summary>
        /// 测试数据库(此重载不建议单独调用)
        /// </summary>

        private static string test_data()
        {

            

            string[] strInfo = File.ReadAllLines(@"" + str_path + "\\my.ini");
            //提取文件里面的端口数据，用来连接数据库
            int port = Convert.ToInt32(strInfo[1].Substring(7));
            //写好数据库的连接参数

            //server=服务器地址
            //databases=数据库名称 uid=用户名  pwd=密码(一定要填写正确) port=端口(一般情况下默认的是3306)

            //此处为非常重要的地方慎改
            string strcon = "server=127.0.0.1;uid=root;pwd=HappyGhost001!;database=mysql;pooling=true;CharSet=utf8;port=" + port + "";

            string str = "";
            using (MySqlConnection con = new MySqlConnection(strcon))
            {
                
               //如果数据库能够打开，则我们将能够将数据库连接成功的端口返回
                con.Open();

                return strcon;
            }
        }
       static string s = "";
        /// <summary>
        /// 更改数据库的端口(不建议单独调用此函数)
        /// </summary>
        public static string  updata_port()
        {
            string[] strInfo = File.ReadAllLines(@"" + str_path + "\\my.ini");
            #region 更改my.ini里面的数据(主要是更改里面的端口)
            //更改端口命令
            //FileStream fs = File.Open("", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            string[] myInfo = File.ReadAllLines(@""+str_path+"\\my.ini");

            string s1Port = myInfo[1];//找到需要更改client的端口
                                      //将要被替换的字符，找到要更改的端口
            int port1 = Convert.ToInt32(s1Port.Substring(7));
            //替换的数据，将端口加1
            //一直循环，直到检测到没有占用的端口为止
            int port2 = port1 + 1;
         
            s1Port = s1Port.Replace(port1.ToString(), port2.ToString());
       
            strInfo[1] = s1Port;//更改client的端口
            strInfo[7] = "";
            strInfo[8] = s1Port;//更改mysqld的端口


          
            //再重新覆盖掉原来的文件，重新生成里面的数据
            using (StreamWriter sw = new StreamWriter(@"" + str_path + "\\my.ini"))
            {
                for (int i = 0; i < strInfo.Length; i++)
                {
                    sw.WriteLine(strInfo[i]);
                }
            }

            #endregion

            #region 更改open.bat里面的数据（主要是更改里面的端口）
            //通过文件流的方式找到open.bat文件
            String[] fileInfo = File.ReadAllLines(@"" + str_path + "\\open.bat");
            //找到文件里面的第8行数据(端口数据)
            string openbat_Port = fileInfo[7];
           
            //更改第八行里面的端口数据
            fileInfo[7] = openbat_Port.Replace(port1.ToString(), port2.ToString());
            //再重新覆盖掉原来的文件，重新生成里面的数据
            using (StreamWriter sw = new StreamWriter(@"" + str_path + "\\open.bat"))
            {
                for (int i = 0; i < fileInfo.Length; i++)
                {
                    sw.WriteLine(fileInfo[i]);
                }
            }


            //再次调用run方法执行，是否能够执行成功，如果失败，则再次重新执行catch代码块
            //重新再次更改端口，并且生成数据

            #endregion

            #region 更改shut_down.bat里面的数据
            //shut_down.bat不用更改，因为里面不涉及到更改端口的命令，只有一条操作语句
            #endregion
            //下面的语句是可以看成是重新启动数据库
            //因为不重新启动库的话，数据当前更改的端口是无效的
            close_mysqld();
            run_Mysqld();
            //如果更改之后连接失败，则再次进行端口更改，直接能成功连接为止
         
            try {s = test_data();
                
            } catch (Exception esException){ updata_port(); }
            return s;
        }

       static int index = 0;
        /// <summary>
        /// 开始运行mysql数据库并且会自动开启数据库服务(当端口被占用时也会自动地去更改端口)
        /// </summary>
        public static string run()
        {
            string str_connection = "" ;
            try
            {
                //第一次测试数据库是否能够连接成功
                //测试数据库能够是否连接成功
                str_connection= test_data();
            }
            catch(Exception exception)
            {   //当数据库端口发生被抢占的时候我们需要执行这个代码块进行数据端口的修正
                //当端口发现被占用后直接需要将mysqld.exe服务关闭，不然端口更改完成的时候服务会再次重启
                //服务会冲突，数据库死锁（也不确定是不是这样叫），程序会卡死

                //pro.StartInfo.FileName=@"D:\mysql_simp\shut_down.bat";
                //pro.Start();
                //如有用户数据库中没有mysql服务则开启服务
               
                try
                {
                    if (index == 0)
                    {
                        //第一次报错的时候则判断为用户没有数据库服务，则我们自己开启一个数据库服务
                        if (i == 0)
                        {
                            //开启服务
                            run_Mysqld();
                        }

                        //检测端口是否被占用
                        //IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
                        //IPEndPoint[] ipendpoints = ipProperties.GetActiveTcpListeners();
                        //foreach (IPEndPoint item in ipendpoints)
                        //{
                        //    if (item.Port == port)
                        //    {

                        //        MessageBox.Show("端口已经被占用!!!");
                        //        updata_port();
                        //    }
                        //    i = 1;

                        //}

                        //再次测试是否连接成功....
                        index++;
                        str_connection= test_data();
                    }
                    
                }
                catch(Exception ee)
                {
                    //假如数据库服务已经开启了，还会报错，则是更改数据的端口
                 str_connection= updata_port();

                }
            }
            return str_connection;
        }
    }
    public class Desc_help
    {

        /// <summary>  
        /// 获取密钥  
        /// </summary>  
        private static string Key
        {
            get { return @")O[NB]6,YF}+efcaj{+oESb9d8>Z'e9M"; }
        }

        /// <summary>  
        /// 获取密钥向量  
        /// </summary>  
        private static string IV
        {
            get { return @"L+\~f4,Ir)b$=pkf"; }
        }

        /// <summary>  
        /// AES加密  
        /// </summary>  
        /// <param name="plainStr">明文字符串</param>  
        /// <returns>密文</returns>  
        public static string AESEncrypt(string plainStr)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(Key);
            byte[] bIV = Encoding.UTF8.GetBytes(IV);
            byte[] byteArray = Encoding.UTF8.GetBytes(plainStr);

            string encrypt = null;
            Rijndael aes = Rijndael.Create();
            using (MemoryStream mStream = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateEncryptor(bKey, bIV), CryptoStreamMode.Write))
                {
                    cStream.Write(byteArray, 0, byteArray.Length);
                    cStream.FlushFinalBlock();
                    encrypt = Convert.ToBase64String(mStream.ToArray());
                }
            }
            aes.Clear();
            return encrypt;
        }

        /// <summary>  
        /// AES加密  
        /// </summary>  
        /// <param name="plainStr">明文字符串</param>  
        /// <param name="returnNull">加密失败时是否返回 null，false 返回 String.Empty</param>  
        /// <returns>密文</returns>  
        public static string AESEncrypt(string plainStr, bool returnNull)
        {
            string encrypt = AESEncrypt(plainStr);
            return returnNull ? encrypt : (encrypt == null ? String.Empty : encrypt);
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        /// <param name="encryptStr">密文字符串</param>  
        /// <returns>明文</returns>  
        public static string AESDecrypt(string encryptStr)
        {
            byte[] bKey = Encoding.UTF8.GetBytes(Key);
            byte[] bIV = Encoding.UTF8.GetBytes(IV);
            byte[] byteArray = Convert.FromBase64String(encryptStr);

            string decrypt = null;
            Rijndael aes = Rijndael.Create();
            using (MemoryStream mStream = new MemoryStream())
            {
                using (CryptoStream cStream = new CryptoStream(mStream, aes.CreateDecryptor(bKey, bIV), CryptoStreamMode.Write))
                {
                    cStream.Write(byteArray, 0, byteArray.Length);
                    cStream.FlushFinalBlock();
                    decrypt = Encoding.UTF8.GetString(mStream.ToArray());
                }
            }
            aes.Clear();
            return decrypt;
        }

        /// <summary>  
        /// AES解密  
        /// </summary>  
        /// <param name="encryptStr">密文字符串</param>  
        /// <param name="returnNull">解密失败时是否返回 null，false 返回 String.Empty</param>  
        /// <returns>明文</returns>  
        public static string AESDecrypt(string encryptStr, bool returnNull)
        {
            string decrypt = AESDecrypt(encryptStr);
            return returnNull ? decrypt : (decrypt == null ? String.Empty : decrypt);
        }
    }
}
