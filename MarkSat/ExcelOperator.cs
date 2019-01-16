using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ExcelOperator
{
    public static bool ExportDataToNewFile(DataTable data, string newFilePath)
    {
        try
        {
            if (data == null)
            {
                return false;
            }

            if (newFilePath == "")
            {
                return false;
            }

            Workbook book = new Workbook(); //创建工作簿
            Worksheet sheet = book.Worksheets[0]; //创建工作表
            Cells cells = sheet.Cells; //单元格

            int Colnum = data.Columns.Count;//表格列数 
            int Rownum = data.Rows.Count;//表格行数 
                                         //生成行 列名行 
            for (int i = 0; i < Colnum; i++)
            {
                cells[0, i].PutValue(data.Columns[i].ColumnName); //添加表头
            }

            //生成数据行 
            for (int i = 0; i < Rownum; i++)
            {
                for (int k = 0; k < Colnum; k++)
                {
                    cells[1 + i, k].PutValue(data.Rows[i][k].ToString()); //添加数据
                }
            }

            sheet.AutoFitColumns(); //自适应宽

            book.Save(newFilePath); //保存
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }


    public static bool ExportDataFromOldFile(DataTable data, string filePath)
    {
        try
        {
            if (data == null)
            {
                return false;
            }

            if (filePath == "")
            {
                return false;
            }

            Workbook book = new Workbook(filePath); //创建工作簿
            Worksheet sheet = book.Worksheets.Add(data.TableName); //创建工作表
            Cells cells = sheet.Cells; //单元格

            int Colnum = data.Columns.Count;//表格列数 
            int Rownum = data.Rows.Count;//表格行数 
                                         //生成行 列名行 
            for (int i = 0; i < Colnum; i++)
            {
                cells[0, i].PutValue(data.Columns[i].ColumnName); //添加表头
            }

            //生成数据行 
            for (int i = 0; i < Rownum; i++)
            {
                for (int k = 0; k < Colnum; k++)
                {
                    cells[1 + i, k].PutValue(data.Rows[i][k].ToString()); //添加数据
                }
            }

            sheet.AutoFitColumns(); //自适应宽

            book.Save(filePath); //保存
        }
        catch (Exception e)
        {
            return false;
        }

        return true;
    }


    public static DataSet ImportData(string filePath, DataSet ds)
    {
        try
        {
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


public enum WordType
{
    Basic基础,
    Core核心,
    Tough进阶,
    USWords,
    ToneAndPointOfViewWords,
    WordsInQuestionsStem
}
