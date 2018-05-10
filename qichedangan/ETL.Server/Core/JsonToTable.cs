using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Text.RegularExpressions;
using System.Collections;
using System.Text;

namespace qichedangan.Core
{
    /// <summary>
    /// JsonAndDateTable 的摘要说明
    /// </summary>
    public static class JsonAndDateTable
    {
      

        /// <summary>
        /// 根据Json返回DateTable,JSON数据格式如:
        /// {table:[{column1:1,column2:2,column3:3},{column1:1,column2:2,column3:3}]}
        /// </summary>
        /// <param name="strJson">Json字符串</param>
        /// <returns></returns>
        public static DataTable JsonToDataTable(string strJson)
        {
            //取出表名
            //Regex rg = new Regex(@"(?<={)[^:]+(?=:/[)", RegexOptions.IgnoreCase);
            string strName = "mytable"; //rg.Match(strJson).Value;
            DataTable tb = null;
            //去除表名
            strJson = strJson.Substring(strJson.IndexOf("[") + 1);
            strJson = strJson.Substring(0, strJson.IndexOf("]"));

            //获取数据
            Regex rg = new Regex(@"(?<={)[^}]+(?=})");
            MatchCollection mc = rg.Matches(strJson);
            for (int i = 0; i < mc.Count; i++)
            {
                string strRow = mc[i].Value;
                string[] strRows = strRow.Split(',');

                //创建表
                if (tb == null)
                {
                    tb = new DataTable();
                    tb.TableName = strName;
                    foreach (string str in strRows)
                    {
                        DataColumn dc = new DataColumn();
                        string[] strCell = str.Split(':');
                        dc.ColumnName = strCell[0].ToString().Replace("\"","");
                        tb.Columns.Add(dc);
                    }
                    tb.AcceptChanges();
                }

                //增加内容
                DataRow dr = tb.NewRow();
                for (int r = 0; r < strRows.Length; r++)
                {
                    int start = strRows[r].IndexOf(':') + 1;
                    if (start >= strRows[r].Length)
                    {
                        start = strRows[r].Length - 1;
                    }
                    string cellVal = strRows[r].Substring(start).Trim().Replace("，", ",");
                    cellVal = cellVal.Replace("\"", "");
                    if (cellVal == "false" || cellVal == "true")
                    {
                        dr[r] = cellVal == "false" ? 0 : 1;
                    }
                    else
                    {
                        dr[r] = cellVal;
                    }
                }
                tb.Rows.Add(dr);
                tb.AcceptChanges();
            }
            return tb;
        }
    }
}

