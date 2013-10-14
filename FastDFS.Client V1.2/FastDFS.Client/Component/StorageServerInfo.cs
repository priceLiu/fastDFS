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

namespace FastDFS.Client.Component
{
    public class StorageServerInfo
    {
        private string _ipAddress;
        private int _port;
        private int _storePathIndex;

        public virtual int StorePathIndex
        {
            get { return _storePathIndex; }
            set { _storePathIndex = value; }
        }

        public virtual string IpAddress
        {
            get { return _ipAddress; }
            set{_ipAddress = value;}
        }

        public virtual int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public StorageServerInfo(string ipAddress,int port, int storePathIndex)
        {
            _ipAddress = ipAddress;
            _port = port;
            _storePathIndex = storePathIndex;
        }

        public StorageServerInfo(string ipAddress,int port, byte storePath)
        {
            _ipAddress = ipAddress;
            _port = port;
            _storePathIndex = storePath < 0 ? 256 + storePath : storePath;
        }
    }
}