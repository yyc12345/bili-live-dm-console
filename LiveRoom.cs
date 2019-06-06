using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using bili_live_dm_console.BiliDMLib;
using bili_live_dm_console.BilibiliDM_PluginFramework;

namespace bili_live_dm_console {
    public class LiveRoom {
        public LiveRoom(string id, bool isDebug, bool isActualRoomID) {
            main = new DanmakuLoader(isDebug);
            main.UrlID = id;
            this.URLID = id;
            this.isActualRoomId = isActualRoomID;
            this.isDebug = isDebug;
            //start
            ConsoleAssistance.WriteLine("[" + URLID + @"]初始化完毕!", ConsoleColor.Yellow);
            //binding event
            main.Disconnected += func_Disconnected;
            main.LogMessage += func_LogMessage;
            main.ReceivedDanmaku += func_ReceivedDanmaku;
            main.ReceivedRoomCount += func_ReceivedRoomCount;
            ConsoleAssistance.WriteLine("[" + URLID + @"]成功监测事件!", ConsoleColor.Yellow);

            //first connect
            firstConnect(id);

        }

        public string URLID;

        DanmakuLoader main;

        bool isDebug;
        bool isActualRoomId;
        int actualRoomID;
        uint userCount = 0;
        uint UserCount {
            get { return userCount; }
            set { userCount = value; }
        }
        public string VisualUserCount {
            get {
                return "直播间人数：" + userCount.ToString();
            }
        }

        public void Disconnect() {
            main.Disconnect();
        }

        #region connect and get actual room id

        private void firstConnect(string urlID) {
            Task.Run(() => {
                //init actual room id
                if (!isActualRoomId) actualRoomID = getActualRoomID(urlID);
                else {
                    try {
                        actualRoomID = int.Parse(urlID);
                    } catch {
                        actualRoomID = 0;
                    }
                }

                //connect
                connectToRoom();
            });
        }

        private async void connectToRoom() {
            if (actualRoomID > 0) {
                var connectresult = false;
                var trytime = 0;

                while (!connectresult) {
                    if (trytime > 5)
                        break;
                    else
                        trytime++;

                    await Task.Delay(1000);// 稍等一下
                    ConsoleAssistance.WriteLine("[" + URLID + @"]正在连接");
                    connectresult = await main.ConnectAsync(actualRoomID);

                    if (!connectresult && main.Error != null && isDebug)// 如果连接不成功并且出错了
                    {
                        ConsoleAssistance.WriteLine("[" + URLID + @"][debug] 出错信息：" + main.Error.ToString(), ConsoleColor.Red);
                    }
                }


                if (connectresult) {
                    ConsoleAssistance.WriteLine("[" + URLID + @"]弹幕机连接成功!", ConsoleColor.Yellow);
                } else {
                    ConsoleAssistance.WriteLine("[" + URLID + @"]连接失败", ConsoleColor.Red);
                    //Environment.Exit(1);
                    //stay
                }
            } else {
                ConsoleAssistance.WriteLine("[" + URLID + @"]ID非法", ConsoleColor.Red);
                //Environment.Exit(1);
                //stay
            }

        }

        private int getActualRoomID(string urlID) {

            var roomWebPageUrl = "https://api.live.bilibili.com/room/v1/Room/get_info?id=" + urlID;
            var wc = new WebClient();
            wc.Headers.Add("Accept: text/html");
            wc.Headers.Add("User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.110 Safari/537.36");
            wc.Headers.Add("Accept-Language: zh-CN,zh;q=0.8,en;q=0.6,ja;q=0.4");

            //发送HTTP请求获取真实房间号
            string pageHtml;
            bool result = true;
            int id = 0;

            try {
                pageHtml = wc.DownloadString(roomWebPageUrl);
                var jsonResult = JObject.Parse(pageHtml);
                id = int.Parse(jsonResult["data"]["room_id"].ToString());
            } catch (Exception e) {
                id = 0;
                result = false;
            }

            //WebClient wc = new WebClient();
            //wc.Credentials = CredentialCache.DefaultCredentials;//获取或设置用于向Internet资源的请求进行身份验证的网络凭据
            //Byte[] pageData = wc.DownloadData(@"https://live.bilibili.com/" + urlID); //从指定网站下载数据
            ////string pageHtml = Encoding.Default.GetString(pageData);  //如果获取网站页面采用的是GB2312，则使用这句            
            //string pageHtml = Encoding.UTF8.GetString(pageData); //如果获取网站页面采用的是UTF-8，则使用这句

            //get actual room id
            if (!result) {
                ConsoleAssistance.WriteLine("获取真实Room ID失败，将使用URL ID作为连接参数", ConsoleColor.Red);
                var ex = int.TryParse(urlID, out id);
                if (ex == false) {
                    Console.WriteLine("ID非法");
                    //Environment.Exit(1);
                    //stay
                }
            }

            return id;
        }

        #endregion


        #region event processor

        private void func_Disconnected(object sender, DisconnectEvtArgs e) {
            ConsoleAssistance.WriteLine("[" + URLID + @"]连接被意外断开！正在准备自动重连", ConsoleColor.Yellow);
            connectToRoom();
        }

        private void func_LogMessage(object sender, LogMessageArgs e) {
            ConsoleAssistance.WriteLine("[" + URLID + @"][log]" + e.message, ConsoleColor.Yellow);
        }
        private void func_ReceivedDanmaku(object sender, ReceivedDanmakuArgs e) {
            ProcDanmaku(e.Danmaku);
        }
        private void func_ReceivedRoomCount(object sender, ReceivedRoomCountArgs e) {
            UserCount = e.UserCount;
        }

        #endregion

        private void send_danmaku(string str) {
            OnNewMessage(new NewDanmakuEventArgs(int.Parse(this.URLID), "[" + URLID + "]" + str));
        }

        private void ProcDanmaku(DanmakuModel danmakuModel) {
            switch (danmakuModel.MsgType) {
                case MsgTypeEnum.Comment:
                    send_danmaku("收到弹幕:" + (danmakuModel.isAdmin ? "[管]" : "") + (danmakuModel.isVIP ? "[爷]" : "") +
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
                    switch (danmakuModel.UserGuardLevel) {
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

        #region event
        public event EventHandler<NewDanmakuEventArgs> NewMessage;
        protected void OnNewMessage(NewDanmakuEventArgs e) => NewMessage?.Invoke(this, e);
        #endregion

    }

    public class NewDanmakuEventArgs : EventArgs {
        public NewDanmakuEventArgs(int roomID, string msg) {
            RoomID = roomID;
            Msg = msg;
        }
        public int RoomID { get; private set; }
        public string Msg { get; private set; }
    }

}