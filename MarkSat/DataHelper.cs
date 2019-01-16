using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkSat
{
    class DataHelper
    {

        public static List<string> ReadWords(string filePath)
        {
            int totalCount = 0;
            return ReadWords(filePath, ref totalCount);
        }

        public static List<string> ReadWords(string filePath, ref int count)
        {
            List<string> wordsList = new List<string>();
            try
            {
                StreamReader sr = new StreamReader(filePath, Encoding.Default);
                String line;
                count = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    count++;
                    string word = line.ToString();
                    Console.WriteLine(word);
                    if (!wordsList.Contains(word))
                    {
                        wordsList.Add(word);
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return wordsList;
        }

        public static void WriteLog(string messgage)
        {
            try
            {
                string filePath = @"C:\Users\chent\Desktop\Code\MarkSat\MarkSat\bin\Debug\log.txt";
                FileStream fs = new FileStream(filePath, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                //开始写入
                sw.Write(messgage + "\r\n");
                //清空缓冲区
                sw.Flush();
                //关闭流
                sw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
            }

        }

        public static DataSet ImportData(string filePath)
        {
            try
            {
                DataSet ds = new DataSet();
                Workbook workbook = new Workbook(filePath);

                for (int index = 0; index < workbook.Worksheets.Count; index++)
                {
                    Cells cells = workbook.Worksheets[index].Cells;
                    // 有标题
                    DataTable dt = cells.ExportDataTable(0, 0, cells.MaxDataRow + 1, cells.MaxColumn + 1, true);
                    dt.TableName = workbook.Worksheets[index].Name;
                    ds.Tables.Add(dt);
                }

                return ds;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

    }
}
