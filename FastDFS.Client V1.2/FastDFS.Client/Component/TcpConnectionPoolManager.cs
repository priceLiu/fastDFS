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

using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Xml;
using FastDFS.Client.Core;
using FastDFS.Client.Core.Pool;
using FastDFS.Client.Core.Pool.Support;
using log4net;
using ThreadContext = FastDFS.Client.Core.Threading.ThreadContext;

namespace FastDFS.Client.Component
{
    internal sealed class TcpConnectionPoolManager
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string AddressConfigItemName = "Address";
        private const string PoolSizeConfigItemName = "PoolSize";
        private const string PortCoutConfigItemName = "Port";
        private const string GroupNameCoutConfigItemName = "GroupName";


        private static IDictionary<string, IList<IPEndPoint>> _groupServer;
        internal static IDictionary<string, IList<IPEndPoint>> GroupServer
        {
            get
            {
                return _groupServer;
            }
        }


        private static IPEndPoint[] _trackerServers;
        internal static IPEndPoint[] TrackerServers
        {
            get { return _trackerServers; }
        }

        private static IPEndPoint[] _storageServers;

        internal static IPEndPoint[] _StorageServers
        {
            get { return _storageServers; }
        }


        /// <summary>
        ///����tracker���ӳ�.
        /// </summary>
        /// <param name="nodes">tracker������.</param>
        internal static void CreateTrackerServerPool(XmlNodeList nodes)
        {
            CreatePool(nodes, out _trackerServers, true, false);
        }

        /// <summary>
        /// ����storage���ӳ�
        /// </summary>
        /// <param name="nodes">storage������.</param>
        internal static void CreateStorageServerPool(XmlNodeList nodes)
        {
            CreatePool(nodes, out _storageServers, false, true);
        }

        /// <summary>
        /// �������ӳ�.
        /// </summary>
        /// <param name="nodes">���ӳ�������.</param>
        /// <param name="servers"></param>
        /// <param name="isTrackerPool">�������Ϊtrue���õ�tracker��Ψһkey.</param>
        /// <param name="isStoragePool">�������Ϊtrue���õ�storage��Ψһkey.</param>
        /// <remarks>
        /// isTrackerPool,isStoragePool����ͬʱΪtrue����false
        /// </remarks>
        internal static void CreatePool(XmlNodeList nodes, out IPEndPoint[] servers, bool isTrackerPool, bool isStoragePool)
        {
            servers = new IPEndPoint[nodes.Count];
            object ipAddress;
            object port;
            object poolSize;
            object groupName;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!ConfigReader.TryGetAttributeValue(nodes[i], AddressConfigItemName, out ipAddress) ||
                    !ConfigReader.TryGetAttributeValue(nodes[i], PortCoutConfigItemName, out port))
                    continue;

                if (!ConfigReader.TryGetAttributeValue(nodes[i], PoolSizeConfigItemName, out poolSize))
                    poolSize = 50;

                servers[i] = new IPEndPoint(IPAddress.Parse(ipAddress.ToString()),
                                            Int32.Parse(port.ToString()));

                //����ֻ�ж���tracker����,���湦����չ��ʱ�����ȥ��
                if (isTrackerPool && ConfigReader.TryGetAttributeValue(nodes[i], GroupNameCoutConfigItemName, out groupName))
                {
                    if (null == _groupServer) _groupServer = new Dictionary<string, IList<IPEndPoint>>();
                    if (_groupServer.ContainsKey(groupName.ToString()))
                    {
                        _groupServer[groupName.ToString()].Add(servers[i]);//modify by seapeak.xu 2009-12-01
                                                                            //groupNameд����_groupServer��tostring
                    }
                    else
                    {
                        IList<IPEndPoint> list = new List<IPEndPoint>();
                        list.Add(servers[i]);
                        _groupServer.Add(groupName.ToString(), list);
                    }
                }

                IObjectPool<TcpConnection> pool =
                    new ObjectPool<TcpConnection>(new TcpConnectionFactory<TcpConnection>(),
                                                  Int32.Parse(poolSize.ToString()));
                ThreadContext.SetData(
                    GetPoolKey(ipAddress.ToString(), Int32.Parse(port.ToString()), isTrackerPool, isStoragePool),
                    pool);
            }
        }

        /// <summary>
        /// �õ����ӳسص�Ψһkey
        /// </summary>
        /// <param name="ipAddress">���ӵ�IP��ַ.</param>
        /// <param name="port">���Ӷ���Ķ˿�.</param>
        /// <param name="isTrackerPool">�������Ϊtrue���õ�tracker��Ψһkey.</param>
        /// <param name="isStoragePool">�������Ϊtrue���õ�storage��Ψһkey.</param>
        /// <remarks>
        /// isTrackerPool,isStoragePool����ͬʱΪtrue����false
        /// </remarks>
        /// <returns></returns>
        internal static string GetPoolKey(string ipAddress, int port, bool isTrackerPool, bool isStoragePool)
        {
            string message;
            if (!isTrackerPool && !isStoragePool)
            {
                message = "�޷��õ��̳߳ص�Ψһkey,ԭ���Ǵ����tracker��storage��ʶ������ȷ,��Ϊfalse!";
                if (null != _logger)
                    _logger.Error(message);
                throw new Exception(message);
            }
            if (isTrackerPool && isStoragePool)
            {
                message = "�޷��õ��̳߳ص�Ψһkey,ԭ���Ǵ����tracker��storage��ʶ������ȷ,��Ϊtrue!";
                if (null != _logger)
                    _logger.Error(message);
                throw new Exception(message);
            }
            if (isTrackerPool && !isStoragePool)
                return String.Format("TrackerPool:{0}:{1}", ipAddress, port);
            if (!isTrackerPool && isStoragePool)
                return String.Format("StoragePool:{0}:{1}", ipAddress, port);

            message = "�޷��õ��̳߳ص�Ψһkey,ԭ���Ǵ����tracker��storage��ʶ������ȷ!";
            if (null != _logger)
                _logger.Error(message);
            throw new Exception(message);
        }

        /// <summary>
        /// �õ�ָ�������ӳ�.
        /// </summary>
        /// <param name="ipAddress">��Ҫ�õ����Ӷ����IP.</param>
        /// <param name="port">��Ҫ�õ����Ӷ���Ķ˿�.</param>
        /// <param name="isTrackerPool">�������Ϊtrue���õ�tracker��Ψһkey.</param>
        /// <param name="isStoragePool">�������Ϊtrue���õ�storage��Ψһkey.</param>
        /// <remarks>
        /// isTrackerPool,isStoragePool����ͬʱΪtrue����false
        /// </remarks>
        internal static IObjectPool<TcpConnection> GetPool(string ipAddress, int port, bool isTrackerPool,
                                                           bool isStoragePool)
        {
            object obj = ThreadContext.GetData(GetPoolKey(ipAddress, port, isTrackerPool, isStoragePool));
            string message = string.Empty;
            if (null == obj)
            {
                if (isTrackerPool) message = "�޷���ָ��key�Ķ������ȡ��tracker����!";
                if (isStoragePool) message = "�޷���ָ��key�Ķ������ȡ��storage����!";
                if (null != _logger)
                    _logger.Error(message);
                throw new Exception(message);
            }
            return (IObjectPool<TcpConnection>)obj;
        }

        /// <summary>
        /// ֹͣ�̳߳�.
        /// </summary>
        internal static void StopPool()
        {

            ThreadContext.FreeNamedDataSlot();
        }
    }
}