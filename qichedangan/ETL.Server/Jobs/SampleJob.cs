using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using Common.Logging;
using Quartz;
using System.Threading.Tasks;
using Dapper;
using Newtonsoft.Json;
using qichedangan.Core;
using qichedangan.Data;

namespace qichedangan.Jobs
{
    /// <summary>
    /// A sample job that just prints info on console for demostration purposes.
    /// </summary>
    public class SampleJob : IJob
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(SampleJob));

        /// <summary>
        /// Called by the <see cref="IScheduler" /> when a <see cref="ITrigger" />
        /// fires that is associated with the <see cref="IJob" />.
        /// </summary>
        /// <remarks>
        /// The implementation may wish to set a  result object on the 
        /// JobExecutionContext before this method exits.  The result itself
        /// is meaningless to Quartz, but may be informative to 
        /// <see cref="IJobListener" />s or 
        /// <see cref="ITriggerListener" />s that are watching the job's 
        /// execution.
        /// </remarks>
        /// <param name="context">The execution context.</param>
        public void Execute(IJobExecutionContext context)
        {
            logger.Info("SampleJob running...");
            string companycode = ConfigurationManager.AppSettings["companycode"];
            string companypassword = ConfigurationManager.AppSettings["companypassword"];
            string accesstionUrl = ConfigurationManager.AppSettings["accesstionUrl"];
            string addrecordUrl = ConfigurationManager.AppSettings["addrecordUrl"];
            //身份验证
            var access = new Access() {companycode = companycode, companypassword = companypassword};
            var result = PostInfoAsync(accesstionUrl, JsonConvert.SerializeObject(access));

            if (result != null)
            {
                if (result.code == "1")
                {
                    string token = result.access_token;
                }
                else
                {
                    logger.Info(result.status);
                }
            }
            Thread.Sleep(TimeSpan.FromSeconds(5));
            logger.Info("SampleJob run finished.");
        }
        /// <summary>
        /// 查询维修记录表
        /// </summary>
        /// <returns></returns>
        public IList<ServiceM> GetServiceMList(string token)
        {
            string sqlConnString = ConfigurationManager.ConnectionStrings["CSConnect"].ConnectionString;
            var conn = new SqlConnection(sqlConnString);
            //返回信息
            string query = $@"select {token} as access_token ,
                                             registerNo as vehicleplatenumber,
                                             StoreName as companyname,
                                             VIN as vin,
                                             CONVERT(varchar(100), Intime, 112) as repairdate,
                                             InMileage as repairmileage,
                                             CONVERT(varchar(100), SettleDate, 112) as settledate,
                                             '' as faultdescription,
                                             BillNo as costlistcode
                                            from v_r_serviceM";
            return conn.Query<ServiceM>(query).ToList();
        }
        /// <summary>
        /// 查询维修记录表
        /// </summary>
        /// <returns></returns>
        public IList<ServiceD> GetServiceDList(string billno)
        {
            string sqlConnString = ConfigurationManager.ConnectionStrings["CSConnect"].ConnectionString;
            var conn = new SqlConnection(sqlConnString);
            //返回信息
            string query = $@"select ItemName as repairproject,isnull(WorkTimes,0) as workinghours from v_r_serviceD where BillNo=@billno";
            return conn.Query<ServiceD>(query, new {billno = billno }).ToList();
        }
        /// <summary>
        /// 查询维修记录表
        /// </summary>
        /// <returns></returns>
        public IList<ROutD> GetROutDList(string billno)
        {
            string sqlConnString = ConfigurationManager.ConnectionStrings["CSConnect"].ConnectionString;
            var conn = new SqlConnection(sqlConnString);
            //返回信息
            string query = $@"select GoodsNo as partscode,GoodsName  as partsname,
                                    convert(decimal(12,1), isnull(Qty,0)) as partsquantity from V_R_OutD; where BillNo=@billno";
            return conn.Query<ROutD>(query, new { billno = billno }).ToList();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="apiUrl"></param>
        /// <param name="jsonData"></param>
        /// <returns></returns>
        public ResultData PostInfoAsync(string apiUrl, string jsonData)
        {
            using (var client = new HttpClient())
            {
                if (!string.IsNullOrEmpty(apiUrl))
                {
                    client.BaseAddress = new Uri(apiUrl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
                else
                {
                    logger.Info("未配置 WebApi 地址。");
                    return null;
                }
                //参数列表(暂时不用)
                //HttpContent postContent = new FormUrlEncodedContent(formContent);
                var content = new StringContent(jsonData);
                HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;
                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        //WebApi返回的data
                        string data = response.Content.ReadAsStringAsync().Result;
                        var result = JsonConvert.DeserializeObject<ResultData>(data);
                        logger.Info(data);
                        return result;
                    }
                    catch (Exception e)
                    {
                        logger.Error("apiUrl：" + apiUrl + ", " + e);
                    }
                }
            }
            return null;
        }
    }
}