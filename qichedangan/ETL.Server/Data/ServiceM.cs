using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace qichedangan.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class ServiceM
    {
        /// <summary>
        /// 接口调用凭证
        /// </summary>
        public string access_token { get; set; }
        /// <summary>
        /// 车牌号码
        /// </summary>
        public string  vehicleplatenumber { get; set; }
        /// <summary>
        /// 维修企业名称
        /// </summary>
        public string companyname { get; set; }
        /// <summary>
        /// 车辆识别代码
        /// </summary>
        public string vin { get; set; }
        /// <summary>
        /// 送修日期
        /// </summary>
        public string repairdate { get; set; }
        /// <summary>
        /// 送修里程
        /// </summary>
        public string repairmileage { get; set; }
        /// <summary>
        /// 结算日期
        /// </summary>
        public string settledate { get; set; }
        /// <summary>
        /// 故障描述
        /// </summary>
        public string faultdescription { get; set; }
        /// <summary>
        /// 结算清单编号
        /// </summary>
        public string costlistcode { get; set; }
        
    }

    /// <summary>
    /// 
    /// </summary>
    public class ServiceD
    {
        /// <summary>
        /// 维修项目
        /// </summary>
        public string repairproject { get; set; }

        /// <summary>
        /// 维修工时
        /// </summary>
        public string workinghours { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ROutD
    {
        /// <summary>
        /// 配件编码
        /// </summary>
        public string partscode { get; set; }
        /// <summary>
        /// 配件名称
        /// </summary>
        public string partsname { get; set; }

        /// <summary>
        /// 配件数量
        /// </summary>
        public string partsquantity { get; set; }
    }
}
