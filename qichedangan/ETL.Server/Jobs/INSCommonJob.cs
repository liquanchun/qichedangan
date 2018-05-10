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
    /// ����ϵͳͨ�ó�������
    /// </summary>
    public class INSCommonJob : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(INSCommonJob));
        /// <summary>
        /// ����������Ϣ����Insight�ӿ��г�ȡ���ݵ��������ݿ���
        /// </summary>
        /// <param name="context">The execution context.</param>
        public void Execute(IJobExecutionContext context)
        {
            logger.Info($"{context.JobDetail.Key.Name} running...");
            logger.Info($"��ʼִ������{context.JobDetail.Description}...");
            try
            {
                //Insight�ӿ��õ��Ĵ洢��������
                var procedure = context.JobDetail.JobDataMap["procedure"].ToString();
                //Ŀ�����
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
        /// ��webapi��������
        /// </summary>
        /// <param name="path"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static async Task GetOrderInfoAsync(string path, string tableName)
        {
            using (var client = new HttpClient())
            {
                //Insight WebApi��ַ
                string insUrl = ConfigurationManager.AppSettings["InsightApiURL"];
                if (!string.IsNullOrEmpty(insUrl))
                {
                    client.BaseAddress = new Uri(insUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                else
                {
                    logger.Info("δ����Insight WebApi ��ַ��");
                    return;
                }
                //�����б�(��ʱ����)
                HttpContent postContent = new FormUrlEncodedContent(new Dictionary<string, string>()
                {
                    //{"orderId", "2017082101442256885174"}
                });

                HttpResponseMessage response = await client.PostAsync(path, postContent);
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        //WebApi���ص�datatable json
                        string data = await response.Content.ReadAsStringAsync();
                        //��Json�ַ���ת��ΪDataTable
                        DataTable dataTable = JsonAndDateTable.JsonToDataTable(data);
                        int deleteCount = 0;
                        int dataTableCount = dataTable.Rows.Count;
                        //�������ݲ�ִ��ɾ���Ͳ���
                        if (dataTable!=null&& dataTable.Rows.Count > 0)
                        {
                            //������ProductTypeId��ִ�в���
                            if (dataTable.Columns.Contains("ProductTypeId"))
                            {
                                var  ls = new List<string>() ; //�����һ�������е�ֵ   
                                foreach (DataRow dr in dataTable.Rows)
                                {
                                    if (!ls.Contains(dr["ProductTypeId"].ToString()))
                                        ls.Add(dr["ProductTypeId"].ToString());
                                }
                                string typeId = "'" + string.Join("','", ls) + "'";
                                string deleteSql = $"delete from {tableName} where ProductTypeId in({typeId})  ";
                                //ȫ�����£���ɾ��
                                //deleteCount=SQLServerHepler.ExecuteNonQuery(BoardContext.Instance.Database.Connection.ConnectionString,
                                           //CommandType.Text, deleteSql, null);
                            }
                            //��������
                            //SQLServerHepler.SqlBulkCopyByDataTable(BoardContext.Instance.Database.Connection.ConnectionString, tableName, dataTable);
                        }
                        logger.Info($"ɾ��{deleteCount}��������{dataTableCount}��");
                     
                    }
                    catch (Exception e)
                    {
                        logger.Error("path��" + path + "   tableName:" + tableName + "  " + e);
                    }
                }
            }
        }
    }
}