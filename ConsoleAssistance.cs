using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bili_live_dm_console {

    public static class ConsoleAssistance {

        /// <summary>
        /// the update of console.writeline()
        /// </summary>
        /// <param name="str"></param>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        /// <param name="previousForeground"></param>
        /// <param name="previousBackground"></param>
        public static void WriteLine(string str, ConsoleColor foreground = ConsoleColor.White,
            ConsoleColor background = ConsoleColor.Black) {

            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;

            Console.WriteLine(str);

            //restore
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// the update of console.write()
        /// </summary>
        /// <param name="str"></param>
        /// <param name="foreground"></param>
        /// <param name="background"></param>
        /// <param name="previousForeground"></param>
        /// <param name="previousBackground"></param>
        public static void Write(string str, ConsoleColor foreground = ConsoleColor.White,
            ConsoleColor background = ConsoleColor.Black) {

            Console.BackgroundColor = background;
            Console.ForegroundColor = foreground;

            Console.Write(str);

            //restore
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }


    }

}
