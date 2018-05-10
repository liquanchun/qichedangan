using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;

namespace qichedangan.Data
{
    /// <summary>
    /// 
    /// </summary>
    public class CSSoftContext : DbContext
    {
        private static CSSoftContext _instance;

        /// <summary>
        /// 
        /// </summary>
        public static CSSoftContext Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new CSSoftContext();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public CSSoftContext(): base("name=CSConnect")
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }
    }
}
