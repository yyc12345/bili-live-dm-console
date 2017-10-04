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

        static List<LiveRoom> liveRoomList;
        static DanmakuRecord dmRecord;

        static bool isCanceled;
        static bool isDebug;
        static bool isRecord;

        static List<string> msgCache;
        static object lockMsgCache;

        static string selectedDanmaku = "";
        static bool selectedWithRegex = false;

        static void Main(string[] args)
        {
            //get settings
            isRecord = !args.Contains("-nr");
            isDebug = args.Contains("-debug");
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
            liveRoomList = new List<LiveRoom>();

            //start
            dmRecord = new DanmakuRecord(isRecord);
            //record start
            dmRecord.Record("");
            dmRecord.Record(DateTime.Now.ToString());
            ConsoleAssistance.WriteLine(@"弹幕机初始化完毕!", ConsoleColor.Yellow);

            while (true)
            {
                var result = Console.ReadKey(true);
                if (result.Key == ConsoleKey.Tab)
                {
                    isCanceled = true;
                    ConsoleAssistance.Write(@"bili-live-dm-console > ", ConsoleColor.Green);
                    var cache = Console.ReadLine();
                    ProcessCommand(cache);
                    lock (lockMsgCache)
                    {
                        foreach (var item in msgCache)
                        {
                            //select remained danmaku
                            if (selectedDanmaku != "")
                            {
                                if (selectedWithRegex)
                                {
                                    if (!Regex.IsMatch(item, selectedDanmaku)) continue;
                                }
                                else
                                {
                                    if (!item.Contains(selectedDanmaku)) continue;
                                }
                            }
                            ConsoleAssistance.WriteLine(item);
                        }

                        msgCache.Clear();
                    }

                    isCanceled = false;

                }//else pass
            }

        }

        static void func_NewDanmaku(object sender, NewDanmakuEventArgs e)
        {
            //record all
            dmRecord.Record(e.Msg);

            //select danmaku
            if (selectedDanmaku != "")
            {
                if (selectedWithRegex)
                {
                    if (!Regex.IsMatch(e.Msg, selectedDanmaku)) return;
                }
                else
                {
                    if (!e.Msg.Contains(selectedDanmaku)) return;
                }
            }

            //show
            lock (lockMsgCache)
            {
                if (isCanceled) msgCache.Add(e.Msg);
                else ConsoleAssistance.WriteLine(e.Msg);
            }
        }

        static void ProcessCommand(string str)
        {
            if (str == "") return;
            var result = CommandSplitter.SplitCommand(str);

            if (!result.IsFine)
            {
                ConsoleAssistance.WriteLine("无效的命令:" + result.CommandBugDescription, ConsoleColor.Magenta);
                return;
            }

            try
            {
                switch (result[0])
                {
                    case "ls":
                        ConsoleAssistance.WriteLine("当前所有侦测的直播间", ConsoleColor.Magenta);
                        foreach (var item in liveRoomList)
                        {
                            ConsoleAssistance.WriteLine(item.URLID + item.VisualUserCount, ConsoleColor.Magenta);
                        }
                        break;
                    case "select":
                        selectedDanmaku = result[1];
                        if (result[2] == "-re") selectedWithRegex = true;
                        else selectedWithRegex = false;
                        ConsoleAssistance.WriteLine("新的筛选规则已经应用", ConsoleColor.Magenta);
                        break;
                    case "c":
                    case "connect":
                        var newLiveRoom = new LiveRoom(result[1], isDebug, !result.Contain("-s"));
                        newLiveRoom.NewMessage += func_NewDanmaku;
                        liveRoomList.Add(newLiveRoom);
                        dmRecord.Record("Add [" + result[1] + "] into list");
                        ConsoleAssistance.WriteLine("已添加新的直播间", ConsoleColor.Magenta);
                        break;
                    case "dc":
                    case "disconnect":
                        foreach (var item in liveRoomList)
                        {
                            if (item.URLID == result[1])
                            {
                                item.Disconnect();
                                liveRoomList.Remove(item);
                                dmRecord.Record("Remove [" + result[1] + "] from list");
                                ConsoleAssistance.WriteLine("已移除直播间", ConsoleColor.Magenta);
                                break;
                            }
                        }
                        ConsoleAssistance.WriteLine("没有匹配的直播间可以移除", ConsoleColor.Magenta);
                        break;
                    case "exit":
                        foreach (var item in liveRoomList)
                        {
                            item.Disconnect();
                        }
                        dmRecord.Close();
                        ConsoleAssistance.WriteLine("退出成功，感谢使用", ConsoleColor.Magenta);
                        Environment.Exit(0);
                        break;
                    default:
                        ConsoleAssistance.WriteLine("无匹配的命令", ConsoleColor.Magenta);
                        break;
                }
            }
            catch (System.Exception)
            {
                ConsoleAssistance.WriteLine("命令不完整", ConsoleColor.Magenta);
            }



        }

    }

}
