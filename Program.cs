using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bili_live_dm_console.BiliDMLib;
using bili_live_dm_console.BilibiliDM_PluginFramework;

namespace bili_live_dm_console
{
    class Program
    {

        static bool isCanceled;
        static List<string> msgCache;
        static object lockMsgCache;
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

            //welcome title
            Console.Title = "bili-live-dm-console";
            ConsoleAssistance.WriteLine("Welcome you to use bili-live-dm-console!", ConsoleColor.Yellow);
            //output info
            ConsoleAssistance.WriteLine("bili-live-dm-console configuration", ConsoleColor.Yellow);
            ConsoleAssistance.WriteLine("Version:1.0.0", ConsoleColor.White);
            ConsoleAssistance.WriteLine("bililive_dm lib version:1.0.1.126", ConsoleColor.White);
            ConsoleAssistance.WriteLine("Workpath:" + Environment.CurrentDirectory, ConsoleColor.White);
            ConsoleAssistance.WriteLine("Platform:" + Enum.GetName(typeof(PlatformID), Environment.OSVersion.Platform), ConsoleColor.White);

            //output
            ConsoleAssistance.WriteLine(@"==========");
            ConsoleAssistance.WriteLine(@"Starting bili-live-dm-console...", ConsoleColor.Yellow);
            isCanceled = false;
            msgCache = new List<string>();
            lockMsgCache = new object();

            //start
            var main = new DanmakuLoader();
            ConsoleAssistance.WriteLine(@"初始化完毕!", ConsoleColor.Yellow);
            main.Disconnected += func_Disconnected;
            main.LogMessage += func_LogMessage;
            main.ReceivedDanmaku += func_ReceivedDanmaku;
            main.ReceivedRoomCount += func_ReceivedRoomCount;
            ConsoleAssistance.WriteLine(@"成功监测事件!", ConsoleColor.Yellow);

            //connect
            connectToRoom(args[0], main);

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

        private static async void connectToRoom(string parameter, DanmakuLoader main)
        {
            int roomId = 0;
            try
            {
                roomId = Convert.ToInt32(parameter);
            }
            catch (Exception)
            {
                ConsoleAssistance.WriteLine("请输入房间号,房间号是!数!字!", ConsoleColor.Red);
                Environment.Exit(1);
            }
            if (roomId > 0)
            {
                var connectresult = false;
                var trytime = 0;
                ConsoleAssistance.WriteLine("正在连接");

                connectresult = await main.ConnectAsync(roomId);

                if (!connectresult && main.Error != null)// 如果连接不成功并且出错了
                {
                    ConsoleAssistance.WriteLine("出错信息：" + main.Error.ToString(), ConsoleColor.Red);
                }

                while (!connectresult)
                {
                    if (trytime > 5)
                        break;
                    else
                        trytime++;

                    System.Threading.Thread.Sleep(1000);// 稍等一下
                    ConsoleAssistance.WriteLine("正在连接");
                    connectresult = await main.ConnectAsync(roomId);
                }


                if (connectresult)
                {
                    ConsoleAssistance.WriteLine(@"弹幕机连接成功!", ConsoleColor.Yellow);
                }
                else
                {
                    ConsoleAssistance.WriteLine("連接失敗", ConsoleColor.Red);
                    Environment.Exit(1);
                }
            }
            else
            {
                ConsoleAssistance.WriteLine("ID非法", ConsoleColor.Red);
                Environment.Exit(1);
            }

        }

        //event processor
        private static void func_Disconnected(object sender, DisconnectEvtArgs e)
        {
            ConsoleAssistance.WriteLine("链接被断开！", ConsoleColor.Yellow);
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

        private static void send_danmaku(string str)
        {
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
