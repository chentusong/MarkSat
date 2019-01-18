using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkSat
{
    class Program
    {
        static void Main(string[] args)
        {
            UpdateCOCAIndexNull();
            UpdateCOCA60000();
        }


        #region 标注单词类型

        /// <summary>
        /// 更新单词信息
        /// </summary>
        public static void UpdateEnglishWords()
        {
            try
            {
                int totalCount = 0;
                string filePath = Environment.CurrentDirectory + "\\sat.txt";
                List<string> wordsList = DataHelper.ReadWords(filePath, ref totalCount);

                MySqlOperator sqlOperator = new MySqlOperator();

                foreach (string word in wordsList)
                {
                    MarkSat(sqlOperator, word);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 标注SAT
        /// </summary>
        public static void MarkSat(MySqlOperator sqlOperator, string word)
        {
            try
            {
                string sql = string.Format("select * from elibenglishwords where id = '{0}'", word);
                DataTable dt = sqlOperator.QueryDataTable(sql);
                if (dt != null)// 标注一词多义
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        sql = string.Format("update elibenglishwords set issat = 1 where id = '{0}' and PolySemyIndex = {1}",
                                   dt.Rows[i]["id"].ToString(),
                                   dt.Rows[i]["polySemyIndex"].ToString());

                        sqlOperator.UpdateDataTable(sql);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion


        /// <summary>
        /// 1. 找出存在于词库中但未标注为sat的单词，并返回词频  
        /// 2. 不在词库中的数据 
        /// </summary>
        public static void UpdateFrequencyWords()
        {
            try
            {
                int totalCount = 0;
                int noSatCount = 0;
                int satCount = 0;
                int notExistCount = 0;
                int oneWordMoreExplainCount = 0;

                string filePath = Environment.CurrentDirectory + "\\sat - Llist3.txt";
                List<string> wordsList = DataHelper.ReadWords(filePath, ref totalCount);

                MySqlOperator sqlOperator = new MySqlOperator();
                List<string> fieldList = MySqlOperator._fieldList;
                DataTable dtNoSat = sqlOperator.TableStructure();
                DataTable dtSat = sqlOperator.TableStructure();
                DataTable dtNotExist = sqlOperator.TableStructure();

                // 将单词类型标注为sat
                for (int i = 0; i < wordsList.Count; i++)
                {
                    string word = wordsList[i];
                    MarkSat(sqlOperator, word);
                }

                for (int i = 0; i < wordsList.Count; i++)
                {
                    string word = wordsList[i];

                    // 查找存在，未标注sat的单词
                    string sql = string.Format("select * from elibenglishwords where id = '{0}' and issat = 0 ", word);
                    DataTable dt = sqlOperator.QueryDataTable(sql);
                    if (dt != null && dt.Rows.Count > 0)
                    {
                        noSatCount++;

                        foreach (DataRow dr in dt.Rows)
                        {
                            oneWordMoreExplainCount++;

                            DataRow drNew = dtNoSat.NewRow();
                            foreach (string colName in fieldList)
                            {
                                drNew[colName] = dr[colName].ToString();
                            }
                            dtNoSat.Rows.Add(drNew);
                        }
                    }

                    // 查找存在，已标注sat的单词
                    sql = string.Format("select * from elibenglishwords where id = '{0}' and issat = 1 ", word);
                    DataTable dt1 = sqlOperator.QueryDataTable(sql);
                    if (dt1 != null && dt1.Rows.Count > 0)
                    {
                        satCount++;

                        foreach (DataRow dr in dt1.Rows)
                        {
                            oneWordMoreExplainCount++;

                            DataRow drNew = dtSat.NewRow();
                            foreach (string colName in fieldList)
                            {
                                drNew[colName] = dr[colName].ToString();
                            }
                            dtSat.Rows.Add(drNew);
                        }
                    }

                    sql = string.Format("select * from elibenglishwords where id = '{0}'", word);
                    DataTable dt2 = sqlOperator.QueryDataTable(sql);
                    if (dt2 == null || dt2.Rows.Count == 0)
                    {
                        notExistCount++;

                        DataRow drNew = dtNotExist.NewRow();
                        drNew["id"] = word;
                        dtNotExist.Rows.Add(drNew);
                    }
                }

                string message = string.Format(@"
去重前单词数量共: {0}
去重后单词数量共: {1}, 
  单词库中不存在: {2},
    单词库中存在: {3},
       标注为sat: {4}，
     未标注未sat: {5},
      一词多义共: {6}",
        totalCount,
        wordsList.Count,
        notExistCount,
        satCount + noSatCount,
        satCount,
        noSatCount,
        oneWordMoreExplainCount
        );

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region 填充|撤销  bncindex、cocaindex字段

        /// <summary>
        /// 填充词频
        /// </summary>
        /// <param name="notNullColumn"></param>
        /// <param name="nullColumn"></param>
        public static void UpdateIndex(string notNullColumn, string nullColumn)
        {
            try
            {
                string sql = string.Format(@"
select id , PolySemyIndex, {0} , {1}  
  FROM elibenglishwords 
 where issat = 1
   and {0} is null
   and {1} is not null",
        nullColumn, notNullColumn);

                MySqlOperator sqlOperator = new MySqlOperator();

                DataTable dt = sqlOperator.QueryDataTable(sql);

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow dr = dt.Rows[i];

                    sql = string.Format("update elibenglishwords set {2} = {3} where id = '{0}' and  PolySemyIndex = {1} ",
                            dr["id"].ToString(),
                            dr["PolySemyIndex"].ToString(),
                            nullColumn,
                            dr[notNullColumn].ToString());

                    sqlOperator.UpdateDataTable(sql);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 撤销填充词频
        /// </summary>
        public static void ReverseUpdateIndex()
        {
            try
            {
                string filePath = Environment.CurrentDirectory + "\\sat - reverse.txt";
                List<string> wordsList = DataHelper.ReadWords(filePath);

                MySqlOperator sqlOperator = new MySqlOperator();

                foreach (string word in wordsList)
                {
                    string sql = string.Format("select * from elibenglishwords where id = '{0}' ", word);
                    DataTable dt = sqlOperator.QueryDataTable(sql);
                    if (dt != null)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            DataRow dr = dt.Rows[i];

                            sql = string.Format("update elibenglishwords set cocaIndex = null  where id = '{0}' and  PolySemyIndex = {1} ",
                                    dr["id"].ToString(),
                                    dr["PolySemyIndex"].ToString()
                                    );

                            sqlOperator.UpdateDataTable(sql);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        /// <summary>
        /// 填充elibIndex，elibIndex = !string.isnullorempty(cocaindex)? cocaindex : nbcindex
        /// </summary>
        public static void UpdateElibIndex()
        {
            try
            {
                MySqlOperator sqlOperator = new MySqlOperator();
                string sql = "select * from elibenglishwords ";
                DataTable dt = sqlOperator.QueryDataTable(sql);

                int failCount = 0;

                if (dt != null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        string cocaindex = dt.Rows[i]["cocaindex"].ToString();
                        string bncindex = dt.Rows[i]["bncindex"].ToString();
                        string updateValue = ""; // 更新值
                        if (!string.IsNullOrEmpty(cocaindex))//优先使用cocaindex
                        {
                            updateValue = cocaindex;
                        }
                        else if (!string.IsNullOrEmpty(bncindex))// 再使用bncindex
                        {
                            updateValue = bncindex;
                        }
                        else
                        {
                            updateValue = "null";
                            //continue;
                        }

                        string word = dt.Rows[i]["id"].ToString().Replace("'", "\\'");
                        string updateSql = string.Format("update elibenglishwords set elibindex = {0} where id = '{1}' and PolySemyIndex = {2}",
                                     updateValue,
                                     word,
                                     dt.Rows[i]["polySemyIndex"].ToString());

                        bool success = sqlOperator.UpdateDataTable(updateSql);
                        if (!success)
                        {
                            failCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        /// <summary>
        /// 更新单词库词频为COCA60000
        /// </summary>
        public static void UpdateCOCA60000()
        {
            try
            {
                string filePath = Environment.CurrentDirectory + "\\COCA60000.xlsx";
                DataTable dtSource = DataHelper.ImportData(filePath).Tables[0];

                string sql = string.Empty;
                MySqlOperator sqlOperator = new MySqlOperator();

                List<string> replaceList = new List<string>()
                {
                    "(",")","（","）","[","]"
                };

                for (int index = 0; index < dtSource.Rows.Count; index++)
                {
                    string[] words = dtSource.Rows[index][2].ToString().ToLower().Trim().Split(
                        new string[] { "/" },
                        StringSplitOptions.RemoveEmptyEntries);
                    string frequency = dtSource.Rows[index][0].ToString();

                    foreach (string temp in words)
                    {
                        string word = temp.Replace("'", "\\'");
                        foreach (string rep in replaceList)
                        {
                            word = word.Replace(rep, "");
                        }

                        sql = string.Format(@"update elibenglishwords set cocaIndex = {0} , elibIndex = {0} where id = '{1}' ",
                                               frequency, word);

                        bool success = sqlOperator.UpdateDataTable(sql);
                        if (!success)
                        {
                            DataHelper.WriteLog(dtSource.Rows[index][2].ToString().ToLower());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 字段cocaIndex设置为空
        /// </summary>
        public static void UpdateCOCAIndexNull()
        {
            try
            {
                MySqlOperator sqlOperator = new MySqlOperator();
                string sql = string.Format(@"update elibenglishwords set cocaIndex = null ");
                bool success = sqlOperator.UpdateDataTable(sql);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
