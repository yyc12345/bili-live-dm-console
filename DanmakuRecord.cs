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
    public class DanmakuRecord
    {
        public DanmakuRecord(bool isRecord)
        {
            this.isRecord = isRecord;
            if (isRecord)
            {
                var path = new FilePathBuilder(Environment.CurrentDirectory, Environment.OSVersion.Platform);
                var cache = DateTime.Now;
                var file = path.Enter(cache.Year.ToString() + cache.Month.ToString() + cache.Day.ToString() + "_record.txt").Path;
                if (System.IO.File.Exists(file)) sw = new StreamWriter(file, true, Encoding.UTF8);
                else sw = new StreamWriter(file, false, Encoding.UTF8);
            }
        }

        StreamWriter sw;
        bool isRecord;

        public void Record(string str)
        {
            if (isRecord)
                sw.WriteLineAsync(str);
        }

        public void Close(){
            sw.Close();
            sw.Dispose();
        }
    }
}