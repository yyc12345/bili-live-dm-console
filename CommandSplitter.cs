using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bili_live_dm_console {

    public static class CommandSplitter {

        public static CommandSplitterResult SplitCommand(string command) {

            var result = new CommandSplitterResult();
            //==========================check 
            if (command == "" || command == string.Empty) {
                result.CommandBugDescription = "No command";
                return result;
            }
            int quotation = 0;
            foreach (var item in command) {
                if (item == '"') quotation++;
            }
            if (quotation % 2 != 0) {
                result.CommandBugDescription = "Lost quotation";
                return result;
            }


            //==========================split
            //make sure splitter can work. add a space
            command += " ";

            int quotationCount = 0;
            string cacheItem = string.Empty;
            foreach (var item in command) {
                if (item == '"') quotationCount++; //ignore quotation
                else if (item == ' ') {
                    //push data
                    if (quotationCount % 2 == 0) {
                        result.Add(cacheItem);
                        //clear string
                        cacheItem = string.Empty;
                    } else cacheItem += item; //save space
                } else cacheItem += item; //save word
            }

            result.IsFine = true;
            return result;
        }

    }

    public class CommandSplitterResult : IEnumerable {

        public CommandSplitterResult() {
            IsFine = false;
            CommandBugDescription = string.Empty;
            parameter = new List<string>();
        }

        public bool IsFine { get; set; }
        public string CommandBugDescription { get; set; }
        List<string> parameter;

        public int Count { get { return parameter.Count; } }

        /// <summary>
        /// indexer
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[int index] {
            get {
                return index >= Count ? string.Empty : parameter[index];
            }
        }

        /// <summary>
        /// iterator
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetEnumerator() {
            foreach (var item in parameter) {
                yield return item;
            }
        }

        public void Add(string newParameter) => parameter.Add(newParameter);

        public bool Contain(string searchedParameter) {
            return parameter.Contains(searchedParameter);
        }
    }

}
