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
using System.Reflection;
using FastDFS.Client.Core.Pool;
using FastDFS.Client.Service;
using log4net;

namespace FastDFS.Client.Component
{
    /// <summary>
    /// ���ӳش�������
    /// </summary>
    public class TcpConnectionFactory<T> : IPoolableObjectFactory<T>
        where T : TcpConnection, new()
    {

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// ��������
        /// </summary>
        public T CreateObject()
        {
            T obj = Activator.CreateInstance<T>();
            obj.IsFromPool = true;
            obj.BatchId = FastDFSService.BatchId;
            return obj;
        }

        /// <summary>
        /// ���ٶ���.
        /// </summary>
        public void DestroyObject(T obj)
        {
            if (obj.Connected)
            {
                obj.GetStream().Close();
                obj.Close();
            }
            if (obj is IDisposable)
            {
                ((IDisposable)obj).Dispose();
            }
        }

        /// <summary>
        /// ��鲢ȷ������İ�ȫ
        /// </summary>
        public bool ValidateObject(T obj)
        {
            return obj.Connected;
        }

        /// <summary>
        /// ���������д��ö���. 
        /// </summary>
        public void ActivateObject(T obj,string ipAdderess,int port)
        {
            try
            {
                if (obj.Connected) return;
                obj.IpAddress = ipAdderess;
                obj.Port = port;
                obj.ReceiveTimeout = FastDFSService.NetworkTimeout;
                obj.Connect(); 
            }
            catch(Exception exc)
            {
                //if (null != _logger)_logger.WarnFormat("���ӳؼ������ʱ�����쳣,�쳣��ϢΪ:{0}",exc.Message);
            }
        }

        /// <summary>
        /// ж���ڴ�������ʹ�õĶ���.
        /// </summary>
        public void PassivateObject(T obj)
        {
            //if (obj.Connected) obj.Close();
        }
    }
}