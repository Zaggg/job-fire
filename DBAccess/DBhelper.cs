using HW.DatabaseAccess.DatabaseStore.DbWrap;
using SubDis.Service.Entities;
using SubDis.Service.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubDis.Service.DBAccess
{
    public class DBhelper : DALBase
    {
        protected IDBInterface Sqlhelper = null;
        private static DBhelper root = new DBhelper();

        private DBhelper()
        {
            Sqlhelper = base.objDBWrap;
        }
        public static DBhelper GetInstance()
        {
            if (root == null)
            {
                object o = new object();
                lock (o)
                {
                    if (root == null)
                    {
                        root = new DBhelper();
                    }
                }
            }
            return root;
        }

        public DataTable GetTasks()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                DataTable dt = new DataTable();
                sb.Append(string.Format(@"use yourdatabase
                SELECT PMS_Supplier_Name
                      ,Url_Address
                      ,Time_Length
                      ,Time_Point
                      ,Status
                      ,Remarks
                      ,Counter
                  FROM PMS_ARI_Subscribtion_Distribution_Setting"));
                string _Error = "";
                dt = Sqlhelper.GetDataTableBySql(sb.ToString(), out _Error);

                return dt;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 更新任务表状态
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public bool UpdateTaskTable(string ID,bool status)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                bool flag = false;
                sb.Append(string.Format(@"use yourdatabase
                    UPDATE PMS_ARI_Subscribtion_Distribution_Setting
                       SET [Status] = '" + (status == true ? "1" : "0") + @"'
                     WHERE ID = '" + ID + "'"));

                string _Error = "";
                flag = Sqlhelper.ExecuteSQLNonQuery(sb.ToString(), out _Error);

                return flag;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 删除临时任务
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool DeleteTask(string ID)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                bool flag = false;
                sb.Append(string.Format(@"use yourdatabase
                DELETE FROM PMS_ARI_Subscribtion_Distribution_Setting
                  WHERE ID = '" + ID + "'"));

                string _Error = "";
                flag = Sqlhelper.ExecuteSQLNonQuery(sb.ToString(), out _Error);

                return flag;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public bool WriteInCommand(CommandLog log)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                bool flag = false;
                sb.Append(string.Format(@"use yourdatabase
                INSERT INTO [HWProd3].[dbo].[PMS_ARI_Subscribtion_Distribution_Command]
               ([PMS_Supplier_Name]
               ,[Url_Address]
               ,[Create_Time]
               ,[Status]
               ,[Remarks])
                VALUES
               ('" + log.PMSNAME +
                   "','" + log.URLADDRESS +
                   "','" + log.CREATETIME +
                   "'," + log.STATUS +
                   ",N'" + log.REMARKS + "')"));

                string _Error = "";
                flag = Sqlhelper.ExecuteSQLNonQuery(sb.ToString(), out _Error);

                return flag;
            }
           catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 服务启动时初始化所有任务状态
        /// </summary>
        /// <returns></returns>
        public bool initializeTask()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                bool flag = false;
                sb.Append(string.Format(@"use yourdatabase
                 update PMS_ARI_Subscribtion_Distribution_Setting
                 set Status = 0 "));

                string _Error = "";
                flag = Sqlhelper.ExecuteSQLNonQuery(sb.ToString(), out _Error);

                return flag;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
