using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using SubDis.Service.NetAccess;
using SubDis.Service.DBAccess;
using SubDis.Service.Log;
using System.Configuration;
using SubDis.Service.Entities;
using SubDis.Service.SerializeTools;
using System.Threading;
using Topshelf;
using SubDis.Service.Infos;
using SubDis.Service.Proxies;
using System.Collections.Concurrent;
using SubDis.Service.Tools;
//using SubDis.Service.Model;
using CacheAPIModel = SubDis.CacheAPI.Common.Model;

namespace SubDis.Service
{
    public partial class SubDisService : ServiceBase//, ServiceControl
    {
        public int counter;

        /// <summary>
        /// 记录定点计划,false为未执行,true为已执行过
        /// </summary>
        static ConcurrentDictionary<string, bool> TaskLog = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 临时任务计划 缓存在本地而不更新数据库
        /// </summary>
        static ConcurrentDictionary<string, int> TempTaskLog = new ConcurrentDictionary<string, int>();

        /// <summary>
        /// 定时任务时间缓存
        /// </summary>
        static ConcurrentDictionary<string, DateTime> TimeRuningTaskCache = new ConcurrentDictionary<string, DateTime>();

        /// <summary>
        /// 任务执行状态缓存,true为正在执行
        /// </summary>
        static ConcurrentDictionary<string, bool> TaskStatusCache = new ConcurrentDictionary<string, bool>();

        /// <summary>
        /// 线程锁
        /// </summary>
        static object _lock = new object();

        /// <summary>
        /// 延迟更新任务表计时起点缓存
        /// </summary>
        static DateTime? updateStartPoint;

        /// <summary>
        /// 缓存定时更新
        /// </summary>
        static DateTime? UpdateTaskCacheTimeInRange;

        public SubDisService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            System.Timers.Timer _timer = new System.Timers.Timer();
            _timer.Interval = int.Parse(PrimaryInfo.ServiceFrequency);//60000;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(DistributionDelegate);
            _timer.AutoReset = true;
            _timer.Enabled = true;
            InitializeTask();
            Task.Run(()=>LogHelper.GetInstance().WriteNormal("Subscribe & Distribution Service Start..."));
            //设置线程数
            ThreadPool.SetMaxThreads(ThreadPoolTools.GetMaxThreadsNum(), ThreadPoolTools.maxThreadWorker);
        }

        protected override void OnStop()
        {
            //this._timer.Enabled = false;
            Task.Run(()=>LogHelper.GetInstance().WriteNormal("Subscribe & Distribution Service Stop..."));
        }

        /// <summary>
        /// 调试用
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns></returns>
        public bool Start(HostControl hostControl)
        {
            System.Timers.Timer _timer = new System.Timers.Timer();
            _timer.Interval = int.Parse(PrimaryInfo.ServiceFrequency);//30000;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(DistributionDelegate);
            _timer.AutoReset = true;
            _timer.Enabled = true;
            InitializeTask();
            LogHelper.GetInstance().WriteNormal("Subscribe & Distribution Service Start...");
            return true;
        }
        /// <summary>
        /// 调试用
        /// </summary>
        /// <param name="hostControl"></param>
        /// <returns></returns>
        public bool Stop(HostControl hostControl)
        {
            //this._timer.Enabled = false;
            LogHelper.GetInstance().WriteNormal("Subscribe & Distribution Service Stop...");
            throw new NotImplementedException();
        }

        private void DistributionDelegate(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                //设置线程数
                //ThreadPool.SetMaxThreads(ThreadPoolTools.GetMaxThreadsNum(), ThreadPoolTools.maxThreadWorker);

            //var dt = DBhelper.GetInstance().GetTasks();//old
                if (UpdateTaskCacheTimeInRange == null)
                    UpdateTaskCacheTimeInRange = DateTime.Now;
                else if((DateTime.Now-(DateTime)UpdateTaskCacheTimeInRange).TotalSeconds >= int.Parse(PrimaryInfo.AutoUpdateCacheTimeRange))
                {
                    ApiHelper.GetInstance().UpdateTask();
                    UpdateTaskCacheTimeInRange = DateTime.Now;
                }
                    

            var _response = ApiHelper.GetInstance().GetTasks();

            //if (dt == null && dt.Rows.Count == 0)
            if (_response == null || _response.CACHEREADER == null)
            {
                DBhelper.GetInstance().WriteInCommand(GetCommandLog("缓存访问失败"));

                if (updateStartPoint == null)
                    updateStartPoint = DateTime.Now;

                //延时更新任务表
                delayUpdateTableCahce();

                Task.Run(()=>LogHelper.GetInstance().WriteNormal(DateTime.Now.ToString("o"), "缓存访问失败"));
                
                return;
            }

            foreach (var cacheReader in _response.CACHEREADER)
            {
                if (cacheReader.STATUS == "0" && !(TaskStatusCache.ContainsKey(cacheReader.ID)?TaskStatusCache[cacheReader.ID]:false))
                {
                    //获取定点信息
                    if (!string.IsNullOrEmpty(cacheReader.TIME_POINT) && (cacheReader.TIME_POINT != "0"))
                    {
                        //获取当前时间点
                        if (DateTime.Now.Hour.ToString() == cacheReader.TIME_POINT)
                        {
                           //判断是否临时任务
                            if (!string.IsNullOrEmpty(cacheReader.COUNTER) && (cacheReader.TIME_POINT != "0"))
                            {
                                //当在定点时,不包含pms
                                if (!TempTaskLog.ContainsKey(cacheReader.ID))
                                {
                                    TempTaskLog.TryAdd(cacheReader.ID, 0);
                                    //ThreadPool.QueueUserWorkItem(new WaitCallback(RunTask), dt.Rows[i]);
                                }
                                ////当在定点时,包含且执行状态为false
                                //if (TempTaskLog[dt.Rows[i]["ID"].ToString()] == 0)
                                //{

                                //}

                                //若已包含记录直接执行
                                //if (TempTaskLog.ContainsKey(dt.Rows[i]["ID"].ToString()))
                                //{
                                if (TempTaskLog[cacheReader.ID] < int.Parse(cacheReader.COUNTER))
                                {
                                    JudgeIfIntoQueue(cacheReader);
                                }
                                else
                                {
                                    //次数执行完毕删除任务 备份语句
                                    RemoveTask(cacheReader.ID);
                                    //ApiHelper.GetInstance().UpdateTask();
                                    int temp_Value;
                                    TempTaskLog.TryRemove(cacheReader.ID, out temp_Value);
                                    //下一步改成调用接口
                                }
                                //}
                                //else
                                //{
                                //    //TempTaskLog.Add(dt.Rows[i]["ID"].ToString(), 0);
                                //    //ThreadPool.QueueUserWorkItem(new WaitCallback(RunTask), dt.Rows[i]);
                                //}
                            }
                            else
                            {
                                if (!TaskLog.ContainsKey(cacheReader.ID))
                                {
                                    TaskLog.TryAdd(cacheReader.ID, false);
                                    //ThreadPool.QueueUserWorkItem(new WaitCallback(RunTask), dt.Rows[i]);
                                }

                                if (!TaskLog[cacheReader.ID])
                                {
                                    JudgeIfIntoQueue(cacheReader);
                                    TaskLog[cacheReader.ID] = true;
                                }
                                //else
                                //{
                                //    //次数执行完毕删除任务 备份语句
                                //    //RemoveTask(dt.Rows[i]["ID"].ToString());
                                //    //下一步改成调用接口
                                //}

                                ////获取当前时间点
                                //if (DateTime.Now.Hour.ToString() == dt.Rows[i]["Time_Point"].ToString())
                                //{
                                //    //当在定点时,不包含pms或包含且执行状态为false
                                //    if (!TaskLog.ContainsKey(dt.Rows[i]["ID"].ToString()) || TaskLog[dt.Rows[i]["ID"].ToString()] == false)
                                //    {
                                //        //****************
                                //        ThreadPool.QueueUserWorkItem(new WaitCallback(RunTask), dt.Rows[i]);
                                //    }
                                //}
                                //else
                                //{   //不在该定点,当不包含pms或者包含且执行状态为true,将执行状态设为false
                                //    if (TaskLog.ContainsKey(dt.Rows[i]["ID"].ToString()) && TaskLog[dt.Rows[i]["ID"].ToString()] == true)
                                //    {
                                //        TaskLog[dt.Rows[i]["ID"].ToString()] = false;
                                //    }
                                //}
                            }
                            
                        }
                        else
                        {   //不在该定点,当不包含pms或者包含且执行状态为true,将执行状态设为false,以便下次执行
                            //if (TaskLog.ContainsKey(dt.Rows[i]["ID"].ToString()) && TaskLog[dt.Rows[i]["ID"].ToString()] == true)
                            //{
                            //    TaskLog[dt.Rows[i]["ID"].ToString()] = false;
                            //}
                            if (TaskLog.ContainsKey(cacheReader.ID) && TaskLog[cacheReader.ID] == true)
                            {
                                TaskLog[cacheReader.ID] = false;
                            }
                        }

                        //是否是有次数限制的临时任务
                        
                    }
                    //获取定时任务
                    if (!string.IsNullOrEmpty(cacheReader.TIME_LENGTH) && ((cacheReader.TIME_LENGTH) != "0"))
                    {
                        if (!TimeRuningTaskCache.ContainsKey(cacheReader.ID))
                        {
                            TimeRuningTaskCache.TryAdd(cacheReader.ID, DateTime.Now);
                           
                        }

                        int totalSeconds = (int)(DateTime.Now - TimeRuningTaskCache[cacheReader.ID]).TotalSeconds;
                        if (totalSeconds >= int.Parse(cacheReader.TIME_LENGTH))
                        {
                        
                            //是否是有次数限制的临时任务
                            if (!string.IsNullOrEmpty(cacheReader.COUNTER) && ((cacheReader.COUNTER) != "0"))
                            {
                                //若不包含记录
                                if (!TempTaskLog.ContainsKey(cacheReader.ID))
                                {
                                    TempTaskLog.TryAdd(cacheReader.ID, 0);
                                }

                                if (TempTaskLog[cacheReader.ID] < int.Parse(cacheReader.COUNTER))
                                {
                                    JudgeIfIntoQueue(cacheReader);
                                    //更新定时任务缓存
                                    TimeRuningTaskCache.TryAdd(cacheReader.ID, DateTime.Now);
                                }
                                else
                                {
                                    //次数执行完毕删除任务 备份语句
                                    RemoveTask(cacheReader.ID);

                                    int temp_Value;
                                    TempTaskLog.TryRemove(cacheReader.ID, out temp_Value);
                                    //下一步改成调用接口
                                }


                                //TempTaskLog.Add(dt.Rows[i]["ID"].ToString(), 0);
                                //ThreadPool.QueueUserWorkItem(new WaitCallback(RunTask), dt.Rows[i]);

                            }
                            else
                            {

                                //if (!TimeRuningTaskCache.ContainsKey(dt.Rows[i]["ID"].ToString()))
                                //{
                                //    TimeRuningTaskCache.TryAdd(dt.Rows[i]["ID"].ToString(), DateTime.Now);
                                //    //ThreadPool.QueueUserWorkItem(new WaitCallback(RunTask), dt.Rows[i]);
                                //}

                                //if (!TimeRuningTaskCache[dt.Rows[i]["ID"].ToString()])
                                //{
                                JudgeIfIntoQueue(cacheReader);
                                    //更新定时任务缓存
                                TimeRuningTaskCache.TryAdd(cacheReader.ID, DateTime.Now);
                                //TaskLog[dt.Rows[i]["ID"].ToString()] = true;
                                //}

                            }
                        }
                        else
                        {
                            if (TaskLog.ContainsKey(cacheReader.ID) && TaskLog[cacheReader.ID] == true)
                            {
                                TaskLog[cacheReader.ID] = false;
                            }
                        }
                        
                    }
                }
                else if (cacheReader.STATUS == "1" || (TaskStatusCache.ContainsKey(cacheReader.ID) ? TaskStatusCache[cacheReader.ID] : true))
                {
                    Task.Run(() => LogHelper.GetInstance().WriteSystemInfo("TASK_NOT_RUN PMS_SUPPLIER_NAME:" + cacheReader.PMS_SUPPLIER_NAME + " TableStatus:" + cacheReader.STATUS + " CacheStatus:" + (TaskStatusCache.ContainsKey(cacheReader.ID) ? TaskStatusCache[cacheReader.ID].ToString() : "not exist")));
                    continue;
                }
                
            }
            }
            catch(Exception ex)
            {
                Task.Run(()=>LogHelper.GetInstance().WriteErrors("", ex));
            }
            
        }

        /// <summary>
        /// 任务进入线程池队列
        /// </summary>
        /// <param name="cacheReader"></param>
        public void JudgeIfIntoQueue(CacheAPIModel.CacheReader cacheReader)
        {
            if (!(TaskStatusCache.ContainsKey(cacheReader.ID) ? TaskStatusCache[cacheReader.ID] : false))
            {
                lock (_lock)
                {
                    if (!(TaskStatusCache.ContainsKey(cacheReader.ID) ? TaskStatusCache[cacheReader.ID] : false))
                    {
                        if (!TaskStatusCache.ContainsKey(cacheReader.ID))
                            TaskStatusCache.TryAdd(cacheReader.ID, true);
                        else
                            TaskStatusCache[cacheReader.ID] = true;
                        Task.Run(() => LogHelper.GetInstance().WriteSystemInfo("TASK_CACHE_CHANGED PMS_SUPPLIER_NAME:" + cacheReader.PMS_SUPPLIER_NAME + " TableStatus:" + cacheReader.STATUS + " CacheStatus:" + (TaskStatusCache.ContainsKey(cacheReader.ID) ? TaskStatusCache[cacheReader.ID].ToString() : "not exist")));
                        //更新数据库状态
                        //UpdateTask(cacheReader.ID, true);
                        //ApiHelper.GetInstance().UpdateTask();
                        //runtask  *********
                        ThreadPool.QueueUserWorkItem(new WaitCallback(RunTask), cacheReader);
                    }
                }
            }
        }

        /// <summary>
        /// 更新任务表及缓存
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="flag"></param>
        public void UpdateTask(string ID,bool flag)
        {
            DBhelper.GetInstance().UpdateTaskTable(ID, flag);
            ApiHelper.GetInstance().UpdateTask();
        }

        /// <summary>
        /// 移除临时任务 更新缓存
        /// </summary>
        /// <param name="ID"></param>
        public void RemoveTask(string ID)
        {
            DBhelper.GetInstance().DeleteTask(ID);
            ApiHelper.GetInstance().UpdateTask();
        }

        /// <summary>
        /// 初始化任务表 更新缓存
        /// </summary>
        public void InitializeTask()
        {
            //初始化表格
            //DBhelper.GetInstance().initializeTask();
            //更新缓存
            ApiHelper.GetInstance().UpdateTask();
        }
        //public void CheckTimePoint(DataRow row)
        //{
        //    //获取当前时间点
        //    if (DateTime.Now.Hour.ToString() == dt.Rows[i]["Time_Point"].ToString())
        //    {
        //        //当在定点时,不包含pms或包含且执行状态为false
        //        if (!TaskLog.ContainsKey(dt.Rows[i]["ID"].ToString()) || TaskLog[dt.Rows[i]["ID"].ToString()] == false)
        //        {
        //            //****************
        //            ThreadPool.QueueUserWorkItem(new WaitCallback(RunTask), dt.Rows[i]);
        //        }
        //    }
        //    else
        //    {   //不在该定点,当不包含pms或者包含且执行状态为true,将执行状态设为false
        //        if (TaskLog.ContainsKey(dt.Rows[i]["ID"].ToString()) && TaskLog[dt.Rows[i]["ID"].ToString()] == true)
        //        {
        //            TaskLog[dt.Rows[i]["ID"].ToString()] = false;
        //        }
        //    }
        //}


        public void RunTask(object _CacheReader)
        {
            CacheAPIModel.CacheReader cacheReader = (CacheAPIModel.CacheReader)_CacheReader;
            StringBuilder logCache = new StringBuilder();
            
            try
            {
                //var t_request = PrimaryInfo.CreateRequest();
                    //var request = XMLSerializeHelper.GetInstance().SerializeObject(PrimaryInfo.CreateRequest());
                //LogHelper.GetInstance().WriteNormal(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, t_request, "RQ:");
                    //var rsString = AccessNet2GetResponse.GetInstance().XmlWebService(request, row["Url_Address"].ToString());
                //AdapterProxy getResponse = new AdapterProxy(GetUrl(row["Url_Address"].ToString()), GetFuncName(row["Url_Address"].ToString()));
                //ARIAdapter getResponse = new ARIAdapter(GetUrl(row["Url_Address"].ToString()));
                    //getResponse.setURL(row["Url_Address"].ToString());
                var t_response = EZGetResponse(cacheReader,ref logCache);
                //var t_response = getResponse.BoTao_ARI_Send(t_request);
                if (!string.IsNullOrEmpty(t_response))
                    LogHelper.GetInstance().WriteInCache(t_response, ref logCache, "\r\n" + DateTime.Now.ToString("o") + " " + "PMS_SUPPLIER_NAME: " + cacheReader.PMS_SUPPLIER_NAME + " url:" + cacheReader.URL_ADDRESS + "\r\nRS:");
                else
                    throw new Exception(cacheReader .PMS_SUPPLIER_NAME + "请求返回为空");
                //BasicRS response = (BasicRS)XMLSerializeHelper.GetInstance().DeserializeObject(rsString, typeof(BasicRS));

                //记录结果
                DBhelper.GetInstance().WriteInCommand(GetCommandResultLog(cacheReader, t_response));
                //DBhelper.GetInstance().WriteInCommand(GetCommandLog(row, response.FLAG));

                //更新临时任务次数记录
                if (TempTaskLog.ContainsKey(cacheReader.ID))
                {
                    TempTaskLog[cacheReader.ID]++;

                    //减少数据库访问
                    lock (_lock)
                    {
                        if (TempTaskLog.ContainsKey(cacheReader.ID))
                        {
                            if (TempTaskLog[cacheReader.ID] >= int.Parse(cacheReader.COUNTER))
                            {
                                RemoveTask(cacheReader.ID);
                                int temp_Value;
                                TempTaskLog.TryRemove(cacheReader.ID, out temp_Value);
                            }
                        }
                    }
                }
                //定时任务执行完后更新时间缓存
                if (TimeRuningTaskCache.ContainsKey(cacheReader.ID))
                {
                    TimeRuningTaskCache[cacheReader.ID] = DateTime.Now;
                }

                if (TaskStatusCache.ContainsKey(cacheReader.ID))
                    TaskStatusCache[cacheReader.ID] = false;
                //UpdateTask(cacheReader.ID, false);

                if(logCache.Length !=0)
                    LogHelper.GetInstance().WriteNormal(logCache.ToString());
                
                //更新缓存接口 **************
            }
            catch(Exception ex)
            {
                if (TaskStatusCache.ContainsKey(cacheReader.ID))
                    TaskStatusCache[cacheReader.ID] = false;
                //try
                //{
                //    UpdateTask(cacheReader.ID, false);
                //}
                //catch(Exception ex2)
                //{
                //    LogHelper.GetInstance().WriteInCache(ex2.Message, ref logCache,"PMS: " + cacheReader.PMS_SUPPLIER_NAME+" url" + cacheReader.PMS_SUPPLIER_NAME + "\r\nRQ:");
                //}

                if (logCache.Length != 0)
                    LogHelper.GetInstance().WriteNormal(logCache.ToString());
                LogHelper.GetInstance().WriteErrors("", ex);
            }
        }

        public string EZGetResponse(CacheAPIModel.CacheReader cacheReader, ref StringBuilder logCache)
        {
            try
            {
                var t_request = PrimaryInfo.CreateRequest();
                var jsonRequest = JsonSerializeHelper.GetInstance().Convert(PrimaryInfo.CreateJsonRequest(cacheReader.PmsCode, cacheReader.HotelGroupCode));

                LogHelper.GetInstance().WriteInCache(t_request, ref logCache, DateTime.Now.ToString("o") + "\r\n xmlRQ:");
                LogHelper.GetInstance().WriteInCache(jsonRequest, ref logCache, DateTime.Now.ToString("o") + "\r\n jsonRQ:");

                ARIAdapter getResponse = new ARIAdapter(GetUrl(cacheReader.URL_ADDRESS));
                //超时时间
                getResponse.Timeout = int.Parse(PrimaryInfo.RequestTimeOut);//600000;
                Result t_response = null;
                string jsonResponse = null;
                switch (GetFuncName(cacheReader.URL_ADDRESS))
                {
                    case "ARI_Send":
                        t_response = getResponse.ARI_Send(t_request); break;
                    case "DynamicARI_Send":
                        t_response = getResponse.DynamicARI_Send(t_request); break;
                    case "Information_Landing":
                        t_response = getResponse.Information_Landing(t_request); break;
                    case "Invoke":
                        jsonResponse = getResponse.Invoke(jsonRequest, "");break;
                    case "Rate":
                        jsonResponse = getResponse.Invoke(jsonRequest, "Rate"); break;
                    case "Inventory":
                        jsonResponse = getResponse.Invoke(jsonRequest, "Inventory"); break;
                    default:
                        jsonResponse = getResponse.Invoke(jsonRequest, GetFuncName(cacheReader.URL_ADDRESS)); break;
                }
                if (!string.IsNullOrEmpty(jsonResponse))
                    return jsonResponse;
                else
                    return XMLSerializeHelper.GetInstance().SerializeObject(t_response);
            }
            catch(Exception ex)
            {
                try
                {
                     //记录结果
                    DBhelper.GetInstance().WriteInCommand(GetCommandLog(cacheReader, ex.Message));
                }
                catch(Exception ex2)
                {
                    LogHelper.GetInstance().WriteInCache(ex2.Message, ref logCache, "\r\nDB:");
                    //DBhelper.GetInstance().WriteInCommand(GetCommandLog(cacheReader, ex2.Message));
                }
                throw ex;
            }
        }

        public string GetFuncName(string url)
        {
            if (url.EndsWith(".asmx") || url.EndsWith(".asmx/"))
            {
                return "";
            }
            else
            {
                return url.Substring(url.LastIndexOf("/") + 1);
            }
        }

        public string GetUrl(string link)
        {
            if (link.EndsWith(".asmx") || link.EndsWith(".asmx/"))
            {
                return link;
            }
            else
            {
                //var url = link.Substring(0, link.LastIndexOf("/"));
                return link.Substring(0, link.LastIndexOf("/"));
            }
            
        }

        public CommandLog GetCommandResultLog(CacheAPIModel.CacheReader cacheReader, string apiRS)
        {
            if(apiRS.Contains("Flag"))
            {
                var response = JsonSerializeHelper.GetInstance().ReverseInfo<TriggerDisResponse>(apiRS);
                CommandLog m_command = new CommandLog();
                m_command.CREATETIME = DateTime.Now;
                m_command.PMSNAME = cacheReader.PMS_SUPPLIER_NAME;
                m_command.URLADDRESS = cacheReader.URL_ADDRESS;
                m_command.STATUS = (response.Flag == true ? 1 : 0);
                if (!string.IsNullOrEmpty(response.Remark))
                    m_command.REMARKS = response.Remark;
                else
                    m_command.REMARKS = cacheReader.REMARK;

                return m_command;
            }
            else
            {
                var response = (Result)XMLSerializeHelper.GetInstance().DeserializeObject(apiRS,typeof(Result));
                CommandLog m_command = new CommandLog();
                m_command.CREATETIME = DateTime.Now;
                m_command.PMSNAME = cacheReader.PMS_SUPPLIER_NAME;
                m_command.URLADDRESS = cacheReader.URL_ADDRESS;
                m_command.STATUS = (response.ret == true ? 1 : 0);
                if (!string.IsNullOrEmpty(response.mes))
                    m_command.REMARKS = response.mes;
                else
                    m_command.REMARKS = cacheReader.REMARK;

                return m_command;
            }
        }

        /// <summary>
        /// 异常log
        /// </summary>
        /// <param name="cacheReader"></param>
        /// <param name="infos"></param>
        /// <returns></returns>
        public CommandLog GetCommandLog(CacheAPIModel.CacheReader cacheReader, string infos)
        {
            CommandLog m_command = new CommandLog();
            m_command.CREATETIME = DateTime.Now;
            m_command.PMSNAME = cacheReader.PMS_SUPPLIER_NAME;
            m_command.URLADDRESS = cacheReader.URL_ADDRESS;
            m_command.STATUS = 0;
            m_command.REMARKS = infos;
          
            return m_command;
        }

        public CommandLog GetCommandLog(string infos)
        {
            CommandLog m_command = new CommandLog();
            m_command.CREATETIME = DateTime.Now;
            m_command.PMSNAME = "";
            m_command.URLADDRESS = "";
            m_command.STATUS = 0;
            m_command.REMARKS = infos;

            return m_command;
        }

        public void delayUpdateTableCahce()
        {
            if (updateStartPoint != null && (DateTime.Now - (DateTime)updateStartPoint).TotalMinutes >= int.Parse(PrimaryInfo.DelayUpdateCacheTimeDuration))
            {
                ApiHelper.GetInstance().UpdateTask();
                updateStartPoint = null;
            }
        }
    }
}
