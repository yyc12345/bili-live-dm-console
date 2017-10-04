using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace bili_live_dm_console {

    public class FilePathBuilder {

        private Stack<string> pathStack;
        private PlatformID OSType;

        public FilePathBuilder(string defaultPath, PlatformID os) {
            pathStack = new Stack<string>();

            if (defaultPath == string.Empty) throw new ArgumentException();
            OSType = os;

            switch (os) {
                case PlatformID.Win32NT:
                    foreach (var item in defaultPath.Split('\\')) {
                        if (item != string.Empty && item != "") pathStack.Push(item);
                    }
                    break;
                case PlatformID.Unix:
                    bool isFirst = true;
                    foreach (var item in defaultPath.Split('/')) {
                        if (item != string.Empty)
                            pathStack.Push(item);
                        else {
                            if (isFirst) {
                                pathStack.Push(item);
                                isFirst = false;
                            }
                        }
                    }
                    break;
                default:
                    throw new ArgumentException();
            }
        }

        /// <summary>
        /// Backtracking to previous path
        /// </summary>
        public void Backtracking() {
            switch (OSType) {
                case PlatformID.Win32NT:
                    if (pathStack.Count <= 1) return;
                    break;
                case PlatformID.Unix:
                    if (pathStack.Count <= 1) return;
                    break;
                default:
                    return;
            }

            pathStack.Pop();
        }

        public FilePathBuilder Enter(string name) {
            pathStack.Push(name);
            return this;
        }

        public FilePathBuilder Enter(List<string> name) {
            foreach (var item in name) {
                pathStack.Push(item);
            }
            return this;
        }

        /// <summary>
        /// get the path without slash
        /// </summary>
        public string Path {
            get {
                if (pathStack.Count == 0) return "";
                else {
                    //reserve the list because the ground of stack is the top of stack
                    List<string> GetPathList() {
                        var cache = pathStack.ToList();
                        cache.Reverse();
                        return cache;
                    }

                    switch (OSType) {
                        case PlatformID.Win32NT:
                            return string.Join(@"\", GetPathList());
                        case PlatformID.Unix:
                            return string.Join(@"/", GetPathList());
                        default:
                            return "";
                    }
                }
            }
        }
    }

}
