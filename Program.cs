using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using bili_live_dm_console.BiliDMLib;
using bili_live_dm_console.BilibiliDM_PluginFramework;

namespace bili_live_dm_console
{
    class Program
    {

        static DanmakuLoader main;
        static DanmakuRecord dmRecord;

        static bool isCanceled;
        static bool isDebug;
        static bool isRecord;
        static bool isActualRoomId;
        static List<string> msgCache;
        static object lockMsgCache;
        static int actualRoomID;
        static uint userCount = 0;
        static uint UserCount
        {
            get { return userCount; }
            set
            {
                userCount = value;
                //update people
                Console.Title = "bili-live-dm-console - " + VisualUserCount;
            }
        }
        static string VisualUserCount
        {
            get
            {
                return "直播间人数：" + userCount.ToString();
            }
        }

        static void Main(string[] args)
        {
            //get settings
            isRecord = !args.Contains("-nr");
            isDebug = args.Contains("-debug");
            isActualRoomId = !args.Contains("-s");
            isCanceled = false;

            //welcome title
            Console.Title = "bili-live-dm-console";
            ConsoleAssistance.WriteLine("Welcome you to use bili-live-dm-console!", ConsoleColor.Yellow);
            //output debug
            if (isDebug)
            {
                ConsoleAssistance.WriteLine("bili-live-dm-console configuration", ConsoleColor.Yellow);
                ConsoleAssistance.WriteLine("Version:1.0.0", ConsoleColor.White);
                ConsoleAssistance.WriteLine("bililive_dm lib version:1.0.1.126", ConsoleColor.White);
                ConsoleAssistance.WriteLine("Workpath:" + Environment.CurrentDirectory, ConsoleColor.White);
                ConsoleAssistance.WriteLine("Platform:" + Enum.GetName(typeof(PlatformID), Environment.OSVersion.Platform), ConsoleColor.White);
            }

            //initialize
            ConsoleAssistance.WriteLine(@"==========");
            ConsoleAssistance.WriteLine("正在启动...", ConsoleColor.Yellow);
            msgCache = new List<string>();
            lockMsgCache = new object();

            //start
            main = new DanmakuLoader(isDebug);
            dmRecord = new DanmakuRecord(isRecord);
            //record start
            dmRecord.Record("");
            dmRecord.Record(DateTime.Now.ToString());
            ConsoleAssistance.WriteLine(@"初始化完毕!", ConsoleColor.Yellow);
            //binding event
            main.Disconnected += func_Disconnected;
            main.LogMessage += func_LogMessage;
            main.ReceivedDanmaku += func_ReceivedDanmaku;
            main.ReceivedRoomCount += func_ReceivedRoomCount;
            ConsoleAssistance.WriteLine(@"成功监测事件!", ConsoleColor.Yellow);

            //first connect
            firstConnect(args[0]);

            while (true)
            {
                var result = Console.ReadKey(true);
                if (result.Key == ConsoleKey.Tab)
                {
                    isCanceled = true;
                    ConsoleAssistance.Write(@"bili-live-dm-console > ", ConsoleColor.Green);
                    var cache = Console.ReadLine();
                    //mainClient.ProcessCommand(cache);
                    lock (lockMsgCache)
                    {
                        foreach (var item in msgCache)
                        {
                            ConsoleAssistance.WriteLine(item);
                        }

                        msgCache.Clear();
                    }

                    isCanceled = false;

                }//else pass
            }

        }

        #region connect and get actual room id

        private static void firstConnect(string urlID)
        {
            Task.Run(() =>
            {
                //init actual room id
                if (!isActualRoomId) actualRoomID = getActualRoomID(urlID);

                //connect
                connectToRoom();
            });
        }

        private static async void connectToRoom()
        {
            if (actualRoomID > 0)
            {
                var connectresult = false;
                var trytime = 0;

                while (!connectresult)
                {
                    if (trytime > 5)
                        break;
                    else
                        trytime++;

                    System.Threading.Thread.Sleep(1000);// 稍等一下
                    ConsoleAssistance.WriteLine("正在连接");
                    connectresult = await main.ConnectAsync(actualRoomID);

                    if (!connectresult && main.Error != null && isDebug)// 如果连接不成功并且出错了
                    {
                        ConsoleAssistance.WriteLine("[debug] 出错信息：" + main.Error.ToString(),ConsoleColor.Red);
                    }
                }


                if (connectresult)
                {
                    ConsoleAssistance.WriteLine("弹幕机连接成功!", ConsoleColor.Yellow);
                }
                else
                {
                    ConsoleAssistance.WriteLine("连接失败", ConsoleColor.Red);
                    Environment.Exit(1);
                }
            }
            else
            {
                ConsoleAssistance.WriteLine("ID非法", ConsoleColor.Red);
                Environment.Exit(1);
            }

        }

        private static int getActualRoomID(string urlID)
        {
            WebClient wc = new WebClient();
            wc.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
            Byte[] pageData = wc.DownloadData(@"http://live.bilibili.com/" + urlID); //从指定网站下载数据
            //string pageHtml = Encoding.Default.GetString(pageData);  //如果获取网站页面采用的是GB2312，则使用这句            
            string pageHtml = Encoding.UTF8.GetString(pageData); //如果获取网站页面采用的是UTF-8，则使用这句

            //get actual room id
            var result = Regex.Match(pageHtml, @"var ROOMID = \d+;");
            if (!result.Success)
            {
                ConsoleAssistance.WriteLine("获取真实Room ID失败，将使用URL ID作为连接参数", ConsoleColor.Red);
                var ex = int.TryParse(urlID, out int rs);
                if (ex == false)
                {
                    Console.WriteLine("ID非法");
                    Environment.Exit(1);
                }
                return rs;
            }

            //process string
            var cache = result.Value.Replace("var ROOMID = ", "");
            cache = cache.Replace(";", "");
            var ex2 = int.TryParse(cache, out int rs2);
            if (ex2 == false)
            {
                Console.WriteLine("ID非法");
                Environment.Exit(1);
            }
            if (isDebug)
                ConsoleAssistance.WriteLine("[debug] get actual room id successfully. url id：" + urlID +
                 " room id:" + rs2,ConsoleColor.Red);
            return rs2;

        }

        #endregion


        #region event processor

        private static void func_Disconnected(object sender, DisconnectEvtArgs e)
        {
            ConsoleAssistance.WriteLine("链接被断开！正在准备自动重连", ConsoleColor.Yellow);
            connectToRoom();
        }

        private static void func_LogMessage(object sender, LogMessageArgs e)
        {
            ConsoleAssistance.WriteLine("[log]" + e.message, ConsoleColor.Yellow);
        }
        private static void func_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e)
        {
            ProcDanmaku(e.Danmaku);
        }
        private static void func_ReceivedRoomCount(object sender, ReceivedRoomCountArgs e)
        {
            UserCount = e.UserCount;
        }

        #endregion

        private static void send_danmaku(string str)
        {
            dmRecord.Record(str);
            lock (lockMsgCache)
            {
                if (isCanceled) msgCache.Add(str);
                else ConsoleAssistance.WriteLine(str);
            }

        }

        private static void ProcDanmaku(DanmakuModel danmakuModel)
        {
            switch (danmakuModel.MsgType)
            {
                case MsgTypeEnum.Comment:
                    send_danmaku("收到彈幕:" + (danmakuModel.isAdmin ? "[管]" : "") + (danmakuModel.isVIP ? "[爷]" : "") +
                            danmakuModel.UserName + " 說: " + danmakuModel.CommentText);
                    break;
                case MsgTypeEnum.GiftTop:
                    //don't process message
                    break;
                case MsgTypeEnum.GiftSend:
                    send_danmaku("收到道具:" + danmakuModel.UserName + " 赠送的: " + danmakuModel.GiftName + " x " +
                                    danmakuModel.GiftCount);
                    break;
                case MsgTypeEnum.GuardBuy:
                    send_danmaku("上船:" + danmakuModel.UserName + " 购买了 " + danmakuModel.GiftName + " x " + danmakuModel.GiftCount);
                    break;
                case MsgTypeEnum.Welcome:
                    send_danmaku("欢迎老爷" + (danmakuModel.isAdmin ? "和管理" : "") + ": " + danmakuModel.UserName + " 进入直播间");
                    break;
                case MsgTypeEnum.WelcomeGuard:
                    string guard_text = string.Empty;
                    switch (danmakuModel.UserGuardLevel)
                    {
                        case 1:
                            guard_text = "总督";
                            break;
                        case 2:
                            guard_text = "提督";
                            break;
                        case 3:
                            guard_text = "舰长";
                            break;
                    }
                    send_danmaku("欢迎" + guard_text + ": " + danmakuModel.UserName + " 进入直播间");
                    break;
            }
        }


    }

}
