using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using Common.Logging;
using Quartz;
using qichedangan.Data;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Script.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using qichedangan.Core;

namespace qichedangan.Jobs
{
    /// <summary>
    /// 看板系统通用抽数程序
    /// </summary>
    public class INSCommonJob : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(INSCommonJob));
        /// <summary>
        /// 根据配置信息，从Insight接口中抽取数据到看板数据库中
        /// </summary>
        /// <param name="context">The execution context.</param>
        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"{context.JobDetail.Key.Name} running...");
            logger.Info($"开始执行任务{context.JobDetail.Description}...");
            try
            {
                //Insight接口用到的存储过程名称
                var procedure = context.JobDetail.JobDataMap["procedure"].ToString();
                //目标表名
                var table = context.JobDetail.JobDataMap["totable"].ToString();
                GetOrderInfoAsync($"api/insight/ysg/{procedure}", table).Wait();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
            logger.Info($"{context.JobDetail.Key.Name} run finished.");
        }

        /// <summary>
        /// 从webapi程序数据
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static async Task GetOrderInfoAsync(string path, string tableName)
        {
            using (var client = new HttpClient())
            {
                //Insight WebApi地址
                string insUrl = ConfigurationManager.AppSettings["InsightApiURL"];
                if (!string.IsNullOrEmpty(insUrl))
                {
                    client.BaseAddress = new Uri(insUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                else
                {
                    logger.Info("未配置Insight WebApi 地址。");
                    return;
                }
                //参数列表(暂时不用)
                HttpContent postContent = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    //{"orderId", "2017082101442256885174"}
                });

                HttpResponseMessage response = await client.PostAsync(path, postContent);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        //WebApi返回的datatable json
                        string data = await response.Content.ReadAsStringAsync();
                        //将Json字符串转换为DataTable
                        DataTable dataTable = JsonAndDateTable.JsonToDataTable(data);
                        int deleteCount = 0;
                        int dataTableCount = dataTable.Rows.Count;
                        //存在数据才执行删除和插入
                        if (dataTable!=null&& dataTable.Rows.Count > 0)
                        {
                            //存在以ProductTypeId才执行插入
                            if (dataTable.Columns.Contains("ProductTypeId"))
                            {
                                var  ls = new List<string>() ; //存放你一整列所有的值   
                                foreach (DataRow dr in dataTable.Rows)
                                {
                                    if (!ls.Contains(dr["ProductTypeId"].ToString()))
                                        ls.Add(dr["ProductTypeId"].ToString());
                                }
                                string typeId = "'" + string.Join("','", ls) + "'";
                                string deleteSql = $"delete from {tableName} where ProductTypeId in({typeId})  ";
                                //全量更新，先删除
                                //deleteCount=SQLServerHepler.ExecuteNonQuery(BoardContext.Instance.Database.Connection.ConnectionString,
                                           //CommandType.Text, deleteSql, null);
                            }
                            //批量导入
                            //SQLServerHepler.SqlBulkCopyByDataTable(BoardContext.Instance.Database.Connection.ConnectionString, tableName, dataTable);
                        }
                        logger.Info($"删除{deleteCount}条，插入{dataTableCount}条");
                     
                    }
                    catch (Exception e)
                    {
                        logger.Error("path：" + path + "   tableName:" + tableName + "  " + e);
                    }
                }
            }
        }
    }
}