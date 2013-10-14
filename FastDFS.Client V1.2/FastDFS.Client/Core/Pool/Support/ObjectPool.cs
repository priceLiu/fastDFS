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
using System.Reflection;
using System.Threading;
using FastDFS.Client.Component;
using FastDFS.Client.Service;
using log4net;

namespace FastDFS.Client.Core.Pool.Support
{
    /// <summary>
    ///  �����
    /// </summary>
    public class ObjectPool<T> : IObjectPool<T> where T : TcpConnection, new()
    {
        private readonly IPoolableObjectFactory<T> _factory;
        private IList<T> _busy = new List<T>();
        private bool _closed;
        private IList<T> _free = new List<T>();
        private static object locker = new object();
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ObjectPool(IPoolableObjectFactory<T> factory, int size)
        {
            if (null == factory)
            {
                if (null != _logger)
                    _logger.Error("���������ʱ�����쳣������ػ���������Ϊ��");
                throw new ArgumentNullException("factory", "���󴴽���������Ϊ�գ�");
            }
            _factory = factory;
            InitItems(size);

            if (null != _logger)
                _logger.InfoFormat("������Ѿ�����������س���Ϊ��{0}", size);
        }

        #region IObjectPool Members

        /// <summary>
        /// Gets the object.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public T GetObject(string ipAddress,int port)
        {
            return DoGetObject(ipAddress,port);
        }

        /// <summary>
        /// ��ʹ����ϵĶ��󷵻ص������.
        /// </summary>
        public void ReturnObject(T target)
        {
            DoReturnObject(target);
        }

        /// <summary>
        /// �رն���ز��ͷų������е���Դ
        /// </summary>
        public void Close()
        {
            DoClose();
        }

        /// <summary>
        /// �õ���ǰ�����������ʹ�õĶ�����. 
        /// </summary>
        public int NumActive
        {
            get { return _busy.Count; }
        }

        /// <summary>
        /// �õ���ǰ������п��õĶ�����
        /// </summary>
        public int NumIdle
        {
            get { return _free.Count; }
        }

        #endregion

        protected void InitItems(int initialInstances)
        {
            if (initialInstances <= 0)
            {
                if (null != _logger)
                    _logger.Error("ʵ�����������ʱ�����쳣������س��Ȳ���Ϊ��");
                throw new ArgumentException("����س��Ȳ���Ϊ�գ�", "initialInstances");
            }
            for (int i = 0; i < initialInstances; ++i)
            {
                _free.Add(_factory.CreateObject());
            }
        }

        protected T DoGetObject(string ipAddress,int port)
        {
            bool isLock = false;
            try 
            {
                if(_closed)
                {
                    if (null != _logger)
                        _logger.Warn("�Ӷ�����л�ȡ����ʱ�����쳣��������Ѿ��رգ��޷�ȡ�ö��󣬶�������д���һ�������Ӷ���");
                    return RescueObject(ipAddress, port);
                }
                if (!Monitor.TryEnter(locker, FastDFSService.MonitorTimeout))
                {
                    if (null != _logger)
                        _logger.Warn("��������������޷�ȡ�ö��󣬶�������д���һ�������Ӷ���");
                    return RescueObject(ipAddress, port);
                }
                isLock = true;
                while (_free.Count > 0)
                {
                    int i = _free.Count - 1;
                    T o = _free[i];
                    _free.RemoveAt(i);
                    _factory.ActivateObject(o,ipAddress,port);
                    if (!_factory.ValidateObject(o)) continue;

                    _busy.Add(o);
                    if (null != _logger)
                        _logger.InfoFormat("���ӳ�״̬�����ڿ��ж��󳤶�Ϊ:{0},æµ���󳤶�Ϊ{1}.", NumIdle, NumActive);
                    return o;
                }

                if (null != _logger)
                    _logger.InfoFormat("�Ӷ�����л�ȡ����ʱ�����쳣���������û�п��ö���!���ڿ��ж��󳤶�Ϊ:{0},æµ���󳤶�Ϊ{1}.", NumIdle, NumActive);
               return RescueObject(ipAddress,port);
            }
            catch(Exception exc)
            {
                if (null != _logger)
                    _logger.ErrorFormat("�Ӷ�����л�ȡ����ʱ�����쳣��{0}.",exc.Message);
                return RescueObject(ipAddress, port);
            }
            finally
            {
                try
                {
                    if(isLock)  Monitor.Exit(locker);
                }
                catch(Exception exc)
                {
                    if (isLock)
                    {
                        if (null != _logger)
                            _logger.ErrorFormat("��������ͷŶ�����ʱ�����쳣��{0}.", exc.Message);
                    }
                    else
                    {
                        if (null != _logger)
                            _logger.ErrorFormat("��δ��ȡ���Ķ������ͷ���ʱ�����쳣��{0}.", exc.Message);
                    }
                }
            }
        }

        protected bool DoReturnObject(T target)
        {
            if (_closed)
            {
                _factory.DestroyObject(target);
                if (null != _logger) _logger.Info("���ӳ��Ѿ��رգ��Żض����ͷţ�");
                return true;
            }
           if(null != target && FastDFSService.BatchId != target.BatchId)
           {
               _factory.DestroyObject(target);
               if (null != _logger) _logger.InfoFormat("�˶������ڸ����ӳأ������ӵ���������Ϊ{0},�����ɶ��������Ϊ{1}��", target.BatchId, FastDFSService.BatchId);
               return true;
           }
            lock (locker)
            {
                if (_busy.Contains(target))
                {
                    if (null != _logger) _logger.Info("���Ӷ���ʹ����ϣ�׼���Ż����ӳأ�");
                    _busy.Remove(target);
                    _factory.PassivateObject(target);
                    if (target.Connected)
                    {
                        _free.Add(target);
                    }
                    else//�����쳣�����Ӳ�Ҫ�������ӳ�,����һ�������ӷ������ӳ�
                    {
                        _free.Add(_factory.CreateObject());
                        if(null != _logger)
                        {
                            _logger.Fatal("���ӷ����쳣����������������һ�������ӷ������ӳأ�");
                        }
                    }
                    if (null != _logger) _logger.Info("���Ӷ���ʹ����ϣ��Ż����ӳأ�");
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// �رն����
        /// </summary>
        private void DoClose()
        {
            _free = new List<T>();
            _closed = true;
        }

        /// <summary>
        /// ǿ�д���һ������
        /// </summary>
        /// <returns></returns>
        public T RescueObject(string ipAddress, int port)
        {
            return DoRescueObject(ipAddress, port);
        }

        protected T DoRescueObject(string ipAddress, int port)
        {
            T obj = _factory.CreateObject();
            _factory.ActivateObject(obj, ipAddress, port);
            obj.IsFromPool = false;
            return obj;
        }
    }
}