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
using System.Xml;
using FastDFS.Client.Component;
using FastDFS.Client.Core;
using log4net;

namespace FastDFS.Client.Service
{
    /// <summary>
    /// FastDFS�ͻ��˷���
    /// </summary>
    public sealed class FastDFSService
    {
        private const string CharsetConfigItemName = "Charset";
        private const string DefaultConfigFilePath = @"config\FastDFS.config";
        private const string NetworkTimeoutConfigItemName = "NetworkTimeout";
        private const string StorageServerConfigItemName = "StorageServer";
        private const string TrackerServerConfigItemName = "TrackerServer";
        private const string MonitorTimeoutConfigItemName = "MonitorTimeout";
        private static int _monitorTimeout = 100;
        private static string _charset = "ISO8859-1";
        private static int _networkTimeout = 30;
        private static string _batchId = string.Empty;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// ��������
        /// </summary>
        internal static string BatchId
        {
            get
            {
                return _batchId;
            }
        }

        /// <summary>
        /// �õ������������ӳ�ʱ.
        /// </summary>
        /// <value>The network timeout.</value>
        public static int NetworkTimeout
        {
            get { return _networkTimeout; }
            set { _networkTimeout = value; }
        }

        /// <summary>
        /// �õ����������ļ�����.
        /// </summary>
        /// <value>The charset.</value>
        public static string Charset
        {
            get { return _charset; }
            set { _charset = value; }
        }

        public static int MonitorTimeout
        {
            get { return _monitorTimeout; }
            set { _monitorTimeout = value; }
        }

        /// <summary>
        /// ʹ��Ĭ��·���µ������ļ�����FastDFS�ͻ��˷���.
        /// </summary>
        /// <remarks>
        /// Ĭ�������ļ�Ϊ"config\FastDFS.config".
        /// </remarks>
        public static void Start()
        {
            if (null != _logger)
                _logger.Info("FastDFSʹ��Ĭ�������ļ�������Ĭ�������ļ�·��Ϊconfig/FastDFS.config!");
            Start(DefaultConfigFilePath);
        }

        /// <summary>
        /// ʹ��ָ�������ļ�����FastDFS�ͻ��˷���.
        /// </summary>
        /// <param name="configFile">FastDFS�����ļ�.</param>
        public static void Start(string configFile)
        {
            XmlDocument doc = ConfigReader.LoadXml(configFile);
            object network_timeout;
            if (ConfigReader.TryGetNodeValue(doc, NetworkTimeoutConfigItemName, out network_timeout))
                _networkTimeout = int.Parse(network_timeout.ToString());
            object charset;
            if (ConfigReader.TryGetNodeValue(doc, CharsetConfigItemName, out charset))
                _charset = charset.ToString();
            object monitorTimeout;
            if (ConfigReader.TryGetNodeValue(doc, MonitorTimeoutConfigItemName, out monitorTimeout))
                _monitorTimeout = int.Parse(network_timeout.ToString());
            _batchId = DateTime.Now.Ticks.ToString();

            if (null != _logger)
                _logger.InfoFormat("ֹͣFastDFS�ͻ��˷��񣬵�ǰ����������Ϊ{0}!", _batchId);
            TcpConnectionPoolManager.CreateTrackerServerPool(ConfigReader.Analyze(doc, TrackerServerConfigItemName)); //����tracker���ӳ�
            TcpConnectionPoolManager.CreateStorageServerPool(ConfigReader.Analyze(doc, StorageServerConfigItemName)); //����storage���ӳ�
            if (null != _logger)
                _logger.Info("FastDFS�ͻ��˷��������ɹ�!");
        }


        /// <summary>
        /// ֹͣ���ӳ�.
        /// </summary>
        public static void Stop()
        {
            TcpConnectionPoolManager.StopPool();
            if (null != _logger)
                _logger.InfoFormat("ֹͣFastDFS�ͻ��˷��񣬵�ǰ����������Ϊ{0}!",_batchId);
            _batchId = string.Empty;
            if (null != _logger)
                _logger.Info("ֹͣFastDFS�ͻ��˷��񣬲�������ӳ�!");
        }

        /// <summary>
        /// ʹ��Ĭ�������ļ�����FastDFS
        /// </summary>
        public static void Reset()
        {
            Reset(DefaultConfigFilePath);
        }
        /// <summary>
        /// ָ�������ļ�����FastDFS
        /// </summary>
        /// <param name="config">FastDFS�������ļ�</param>
        public static void Reset(string config)
        {
            if (null != _logger)
                _logger.Info("��ʼ�������ӳ�!");
            Stop();
            Start(config);
            if (null != _logger)
                _logger.Info("�������ӳ����!");
        }
    }
}