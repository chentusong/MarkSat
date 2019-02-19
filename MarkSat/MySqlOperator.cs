using MarkSat;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class MySqlOperator
{
    public static String _connetStr = "server=localhost;user=root;password=091816;database=bluelightdb;";
    public static MySqlConnection _conn = null;
    public static List<string> _fieldList = new List<string>()
    {
        "Id","Definition","Translation","PolysemyIndex",
        "Phonetic","IsSAT","Examples","Audio","BNCIndex",
        "COCAIndex","Collins","IsGRE","IsOxford","IsTOEFL",
        "ElibIndex","IsForVocabularTest"
    };

    static MySqlOperator()
    {
        try
        {
            _conn = new MySqlConnection(_connetStr);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    public DataTable QueryDataTable(string sql)
    {
        try
        {
            DataTable dt = TableStructure();

            _conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, _conn);
            cmd.CommandType = CommandType.Text;
            MySqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                if (reader.HasRows)//有一行读一行
                {
                    DataRow drNew = dt.NewRow();
                    foreach (string colName in _fieldList)
                    {
                        drNew[colName] = reader[colName].ToString();
                    }
                    dt.Rows.Add(drNew);
                }
            }
            _conn.Close();

            return dt;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }

    public bool UpdateDataTable(string sql)
    {
        try
        {
            _conn.Open();
            MySqlCommand cmd = new MySqlCommand(sql, _conn);
            cmd.CommandType = CommandType.Text;
            int count = cmd.ExecuteNonQuery();//执行插入、删除、更改语句
            _conn.Close();
            return count > 0;
        }
        catch (Exception ex)
        {
            _conn.Close();
            DataHelper.WriteLog(sql);
            return false;
        }
    }

    public DataTable TableStructure()
    {
        DataTable dt = new DataTable();
        foreach (string colName in _fieldList)
        {
            dt.Columns.Add(colName);
        }
        return dt;
    }

}
