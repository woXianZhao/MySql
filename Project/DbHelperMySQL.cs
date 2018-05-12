﻿using System;
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Configuration;
using System.Data.Common;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace Project
{
    public class MysqlHelper
    {
        /// <summary>
        /// 数据库连接串
        /// </summary>
        public static string ConnString = "";
        /// <summary>
        /// 数据库连接
        /// </summary>
        private static MySqlConnection Conn;
        /// <summary>
        /// 数据库连接
        /// </summary>
        private static MySqlDataReader reader;
        /// <summary>
        /// 错误信息
        /// </summary>
        public static string ErrorString = "";
        /// <summary>
        /// 超时（秒）
        /// </summary>
        private static int TimeOut = 100;

        /// <summary>
        /// 初始化数据库链接
        /// </summary>
        /// <param name="connString">数据库链接</param>
        public static void MySqlConnString(string connString)
        {
            ConnString = connString;
            ConnTo();
        }
        /// <summary>
        /// 去掉SQL中的特殊字符
        /// </summary>
        /// <param name="value">字符串</param>
        /// <returns></returns>
        public static string ReplaceSql(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            value = value.Replace("\\", "\\\\");
            value = value.Replace("'", "''");
            value = value.Replace("\"", "\\\"");
            value = value.Replace("%", "\\%");
            return value;
        }
        /// <summary>
        /// 执行多条SQL语句，实现数据库事务。
        /// </summary>
        /// <param name="SQLStringList">多条SQL语句</param>		
        public static int ExecuteSqlTran(List<String> SQLStringList)
        {
            if (Conn == null || Conn.State != ConnectionState.Open)
                ConnTo();
            MySql.Data.MySqlClient.MySqlCommand cmd = new MySql.Data.MySqlClient.MySqlCommand();
            cmd.Connection = Conn;
            cmd.CommandType = CommandType.Text;
            cmd.CommandTimeout = TimeOut;
            MySqlTransaction tx = Conn.BeginTransaction();
            cmd.Transaction = tx;
            try
            {
                int count = 0;
                for (int n = 0; n < SQLStringList.Count; n++)
                {
                    string strsql = SQLStringList[n];
                    if (strsql.Trim().Length > 1)
                    {
                        cmd.CommandText = strsql;
                        count += cmd.ExecuteNonQuery();
                    }
                }
                tx.Commit();
                return count;
            }
            catch(Exception ex)
            {
                tx.Rollback();
                return 0;
            }
            //using (MySqlConnection conn = new MySqlConnection(ConnString))
            //{
            //    conn.Open();
               
            //}
        }
        public static DataTable ExecuteDataTable(string SqlString)
        {
            return ExecuteDataTable(SqlString, null);
        }

        /// <summary>
        /// 执行sql返回DataTable
        /// </summary>
        /// <param name="SqlString">SQL语句</param>
        /// <param name="parms">Sql参数</param>
        /// <returns>DataTable</returns>
        public static DataTable ExecuteDataTable(string SqlString, MySqlParameter[] parms)
        {
            if (Conn == null || Conn.State != ConnectionState.Open)
                ConnTo();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = Conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlString;
                cmd.CommandTimeout = TimeOut;
                if (parms != null)
                    foreach (MySqlParameter pram in parms)
                        cmd.Parameters.Add(pram);
                DataTable dt = new DataTable();
                try
                {
                    if (reader != null && !reader.IsClosed)
                        reader.Close();
                    reader = cmd.ExecuteReader();
                    dt.Load(reader);
                }
                catch
                {
                    if (reader != null && !reader.IsClosed)
                        reader.Close();
                    reader = cmd.ExecuteReader();
                    dt = Read(ref reader);
                }
                return dt;
            }
            catch (Exception e)
            {
                AddError(e.Message, SqlString);
                return null;
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
            }
        }

        /// <summary>
        /// 读取所有数据
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private static DataTable Read(ref MySqlDataReader reader)
        {
            DataTable dt = new DataTable();
            bool frist = true;
            while (reader.Read())
            {
                if (frist)
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        string s = reader.GetName(i);
                        //var type = reader[0].GetType();
                        dt.Columns.Add(s, Type.GetType("System.String"));
                    }
                    frist = false;
                }
                DataRow dr = dt.NewRow();
                for (int i = 0; i < reader.FieldCount; i++)
                    dr[i] = reader.GetString(i);
                dt.Rows.Add(dr);
            }
            return dt;
        }

        /// <summary>
        /// 返回第一行--无参数
        /// </summary>
        /// <param name="SqlString"></param>
        /// <returns></returns>
        public static DataRow ExecuteDataTableRow(string SqlString)
        {
            return ExecuteDataTableRow(SqlString, null);
        }

        /// <summary>
        /// 返回第一行
        /// </summary>
        /// <param name="SqlString"></param>
        /// <returns></returns>
        public static DataRow ExecuteDataTableRow(string SqlString, MySqlParameter[] parms)
        {
            if (Conn == null || Conn.State != ConnectionState.Open)
                ConnTo();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = Conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlString;
                cmd.CommandTimeout = TimeOut;
                if (parms != null)
                    foreach (MySqlParameter pram in parms)
                        cmd.Parameters.Add(pram);
                DataTable dt = new DataTable();
                try
                {
                    if (reader != null && !reader.IsClosed)
                        reader.Close();
                    reader = cmd.ExecuteReader();
                    dt.Load(reader);
                }
                catch
                {
                    if (reader != null && !reader.IsClosed)
                        reader.Close();
                    reader = cmd.ExecuteReader();
                    dt = Read(ref reader);
                }
                if (dt.Rows.Count > 0)
                    return dt.Rows[0];
            }
            catch (Exception e)
            {
                AddError(e.Message, SqlString);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
            }
            return null;
        }

        public static string ExecuteFirst(string SqlString)
        {
            return ExecuteFirst(SqlString, null);
        }

        /// <summary>
        /// 返回第一个值
        /// </summary>
        /// <param name="SqlString"></param>
        /// <returns></returns>
        public static string ExecuteFirst(string SqlString, MySqlParameter[] parms)
        {
            if (Conn == null || Conn.State != ConnectionState.Open)
                ConnTo();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = Conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlString;
                cmd.CommandTimeout = TimeOut;
                if (parms != null)
                    foreach (MySqlParameter pram in parms)
                        cmd.Parameters.Add(pram);
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                reader = cmd.ExecuteReader();
                string xx = "";
                if (reader.Read())
                    xx = reader[0].ToString();
                return xx;
            }
            catch (Exception e)
            {
                AddError(e.Message, SqlString);
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
            }
            return null;
        }

        public static long ExecuteInsertId(string SqlString)
        {
            return ExecuteInsertId(SqlString, null);
        }

        /// <summary>
        /// 返回最后一个值
        /// </summary>
        /// <param name="SqlString"></param>
        /// <returns></returns>
        public static long ExecuteInsertId(string SqlString, MySqlParameter[] parms)
        {
            if (Conn == null || Conn.State != ConnectionState.Open)
                ConnTo();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = Conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlString;
                cmd.CommandTimeout = TimeOut;
                if (parms != null)
                    foreach (MySqlParameter pram in parms)
                        cmd.Parameters.Add(pram);
                cmd.ExecuteNonQuery();
                return cmd.LastInsertedId;
            }
            catch (Exception e)
            {
                AddError(e.Message, SqlString);
            }
            return 0;
        }
        /// <summary>
        /// 执行无返回SQL语句
        /// </summary>
        /// <param name="SqlString">SQL语句</param>
        ///<returns>是否执行成功</returns>
        public static bool ExecuteNonQuery(string SqlString)
        {
            return ExecuteNonQuery(SqlString, null);
        }

        /// <summary>
        /// 执行无返回SQL语句
        /// </summary>
        /// <param name="SqlString">SQL语句</param>
        /// <param name="parms">Sql参数</param>
        ///<returns>是否执行成功</returns>
        public static bool ExecuteNonQuery(string SqlString, MySqlParameter[] parms)
        {
            if (Conn == null || Conn.State != ConnectionState.Open)
                ConnTo();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = Conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlString;
                cmd.CommandTimeout = TimeOut;
                if (parms != null)
                    foreach (MySqlParameter pram in parms)
                        cmd.Parameters.Add(pram);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception e)
            {
                AddError(e.Message, SqlString);
                return false;
            }
        }

        /// <summary>
        /// 查询是否存在
        /// </summary>
        /// <param name="SqlString">SQL语句</param>
        /// <returns>是否存在</returns>
        public static bool ExecuteExists(string SqlString)
        {
            return ExecuteExists(SqlString, null);
        }

        /// <summary>
        /// 查询是否存在
        /// </summary>
        /// <param name="SqlString">SQL语句</param>
        /// <param name="parms">SQL参数</param>
        /// <returns>是否存在</returns>
        public static bool ExecuteExists(string SqlString, MySqlParameter[] parms)
        {
            if (Conn == null || Conn.State != ConnectionState.Open)
                ConnTo();
            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = Conn;
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = SqlString;
                cmd.CommandTimeout = TimeOut;
                if (parms != null)
                    foreach (MySqlParameter pram in parms)
                        cmd.Parameters.Add(pram);
                if (reader != null && !reader.IsClosed)
                    reader.Close();
                reader = cmd.ExecuteReader();
                if (reader.Read())
                    return true;
                return false;
            }
            catch (Exception e)
            {
                AddError(e.Message, SqlString);
                return false;
            }
            finally
            {
                if (reader != null && !reader.IsClosed)
                    reader.Close();
            }
        }
        /// <summary>
        /// 连接数据库
        /// </summary>
        private static void ConnTo()
        {
            Close();
            try
            {
                Conn = new MySqlConnection(ConnString);
                Conn.Open();
            }
            catch (Exception e)
            {
                AddError(e.Message, ConnString);
            }
        }
        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="sql"></param>
        private static void AddError(string message, string sql)
        {
            ErrorString += "数据库连接错误：" + message + "\r\nSQL语句：" + sql + "\r\n";
            if (!string.IsNullOrEmpty(ErrorString) && ErrorString.Length > 1000)
                ErrorString = "";
        }

        /// <summary>
        /// 关闭数据库链接
        /// </summary>
        public static void Close()
        {
            if (Conn != null && Conn.State == ConnectionState.Open)
            {
                lock(Conn)
                {
                    Conn.Close();
                    Conn = null;
                }
            }
            else
                Conn = null;
            GC.Collect();
        }

    }
}
