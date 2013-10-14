/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 5173.com                                                                                   *
* 5173DFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the 5173DFS .Net Client source kit.                                                     *
* Please visit the 5173DFS .Net Client Home Page http://code.google.com/p/5173dfs-client/ for more detail.      *
*                                                                                                               *
* Author:Seapeak.Xu/xvhfeng                                                                                     *
*                                                                                                               *
****************************************************************************************************************/

using System.Collections;

namespace FastDFS.Client.Core.Threading
{
    public sealed class ThreadContext
    {
        private static Hashtable _data = Hashtable.Synchronized(new Hashtable());

        public static object GetData(string name)
        {
            return _data[name];
        }
        public static void SetData(string name, object value)
        {
            _data.Add(name, value);
        }
        public static void FreeNamedDataSlot(string name)
        {
            _data.Remove(name);
        }
        public static void FreeNamedDataSlot()
        {
            _data.Clear();
        }
    }
}