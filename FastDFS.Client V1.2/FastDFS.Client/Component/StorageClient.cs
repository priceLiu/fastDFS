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
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using FastDFS.Client.Core.Pool;
using FastDFS.Client.Service;
using log4net;

namespace FastDFS.Client.Component
{

    public class StorageClient
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static TcpConnection GetStorageConnection(string groupName)
        {
            StorageServerInfo storageServerInfo = TrackerClient.GetStoreStorage(groupName);
            IObjectPool<TcpConnection> pool = TcpConnectionPoolManager.GetPool(storageServerInfo.IpAddress, storageServerInfo.Port, false, true);
            try
            {
                TcpConnection storageConnection = pool.GetObject(storageServerInfo.IpAddress, storageServerInfo.Port);
                storageConnection.Index = storageServerInfo.StorePathIndex;
                if (null != _logger)
                    _logger.InfoFormat("Storage����������Ϊ:{0}", pool.NumIdle);
                return storageConnection;
            }
            catch (Exception exc)
            {
                if (null != _logger)
                    _logger.WarnFormat("����Storage������ʱ�����쳣,�쳣��ϢΪ:{0}", exc.Message);
                throw;
            }
        }

        /// <summary>
        /// �ϴ��ļ�.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="localFileName">Name of the local file.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="extension">The extension.</param>
        /// <param name="metadatas">The metadatas.</param>
        /// <returns></returns>
        protected static string[] DoUpload(string groupName, string localFileName, byte[] buffer,
                                                     string extension, NameValuePair[] metadatas)
        {
            bool isGoing = false;
            DateTime begin = DateTime.Now;
            do
            {
                TimeSpan span = DateTime.Now.Subtract(begin);
                if ((int)span.TotalSeconds > FastDFSService.NetworkTimeout / 1000) //���������ӳٻ�δ�ɹ��ϴ�ͼƬ����ʧ��
                {
                    break;
                }
                TcpConnection storageConnection = GetStorageConnection(groupName);
                if (null != _logger)
                    _logger.InfoFormat("Storage��������IP��:{0}.�˿�Ϊ{1}", storageConnection.IpAddress, storageConnection.Port);
                long totalBytes = 0;
                try
                {
                    long length;
                    FileStream stream;

                    byte[] metadatasBuffer = metadatas == null
                                                 ? new byte[0]
                                                 : Encoding.GetEncoding(FastDFSService.Charset).GetBytes(
                                                       Util.PackMetadata(metadatas));

                    byte[] bufferSize = new byte[1 + 2*Protocol.TRACKER_PROTO_PKG_LEN_SIZE];

                    if (!string.IsNullOrEmpty(localFileName))
                    {
                        FileInfo fileInfo = new FileInfo(localFileName);
                        length = fileInfo.Exists ? fileInfo.Length : 0;
                        stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                    }
                    else
                    {
                        length = buffer.Length;
                        stream = null;
                    }

                    byte[] extensionBuffer = new byte[Protocol.FDFS_FILE_EXT_NAME_MAX_LEN];
                    Util.InitializeBuffer(extensionBuffer, 0);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        byte[] bs = Encoding.GetEncoding(FastDFSService.Charset).GetBytes(extension);
                        int ext_name_len = bs.Length;
                        if (ext_name_len > Protocol.FDFS_FILE_EXT_NAME_MAX_LEN)
                            ext_name_len = Protocol.FDFS_FILE_EXT_NAME_MAX_LEN;
                        Array.Copy(bs, 0, extensionBuffer, 0, ext_name_len);
                    }

                    Util.InitializeBuffer(bufferSize, 0);
                    bufferSize[0] = (byte) storageConnection.Index;
                    byte[] hexBuffer = Util.LongToBuffer(metadatasBuffer.Length);
                    Array.Copy(hexBuffer, 0, bufferSize, 1, hexBuffer.Length);
                    hexBuffer = Util.LongToBuffer(length);
                    Array.Copy(hexBuffer, 0, bufferSize, 1 + Protocol.TRACKER_PROTO_PKG_LEN_SIZE, hexBuffer.Length);

                    byte[] header = Util.PackHeader(Protocol.STORAGE_PROTO_CMD_UPLOAD_FILE,
                                                    1 + 2*Protocol.TRACKER_PROTO_PKG_LEN_SIZE +
                                                    Protocol.FDFS_FILE_EXT_NAME_MAX_LEN + metadatasBuffer.Length +
                                                    length,
                                                    0);

                    PackageInfo pkgInfo = null;
                    if (!storageConnection.Connected)
                    {
                        storageConnection.Connect();

                    }

                    NetworkStream outStream = storageConnection.GetStream();

                    outStream.Write(header, 0, header.Length);
                    outStream.Write(bufferSize, 0, bufferSize.Length);
                    outStream.Write(extensionBuffer, 0, extensionBuffer.Length);
                    outStream.Write(metadatasBuffer, 0, metadatasBuffer.Length);
                    if (stream != null)
                    {
                        int readBytes;
                        byte[] buff = new byte[128*1024];

                        while ((readBytes = Util.ReadInput(stream, buff, 0, buff.Length)) >= 0)
                        {
                            if (readBytes == 0) continue;
                            outStream.Write(buff, 0, readBytes);
                            totalBytes += readBytes;
                        }
                    }
                    else
                    {
                        outStream.Write(buffer, 0, buffer.Length);
                    }

                    pkgInfo = Util.RecvPackage(outStream, Protocol.STORAGE_PROTO_CMD_RESP, -1, "storage");

                    if (pkgInfo.ErrorNo != 0) return null;

                    if (pkgInfo.Body.Length <= Protocol.FDFS_GROUP_NAME_MAX_LEN)
                        throw new Exception(string.Format("_body length: {0} <= {1}",
                                                          pkgInfo.Body.Length, Protocol.FDFS_GROUP_NAME_MAX_LEN));

                    char[] chars = Util.ToCharArray(pkgInfo.Body);
                    string newGroupName = new string(chars, 0, Protocol.FDFS_GROUP_NAME_MAX_LEN).Trim();
                    string remoteFileName = new string(chars, Protocol.FDFS_GROUP_NAME_MAX_LEN,
                                                       pkgInfo.Body.Length - Protocol.FDFS_GROUP_NAME_MAX_LEN);
                    string[] results = new string[]
                                           {
                                               newGroupName, remoteFileName
                                           };
                    return results;
                }
                catch (Exception exc)
                {
                    try
                    {
                        storageConnection.GetStream().Close();
                    }
                    catch
                    {
                        
                    }
                    if (null != _logger)
                    {
                        _logger.Error(string.Format("�ϴ��ļ������쳣���쳣����:{0},��ϸ��Ϣ:{1}!", exc.InnerException.GetType(), exc));
                    }
                    storageConnection.Close();
                    isGoing = true;
                }
                finally
                {
                    storageConnection.Close(false, true);
                }
            } while (isGoing);
            return null;
        }



        /// <summary>
        /// �����ϴ��ļ�.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="filesCount">The files count.</param>
        /// <param name="localFileName">Name of the local file.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        protected static string[] DoBatchUpload(string groupName, IList<byte[]> filesBuffer, string[] filesExtension)
        {
            int filesCount = filesBuffer.Count;
            if (255 < filesCount)
            {
                if (null != _logger)
                {
                    _logger.ErrorFormat("�����ϴ��ļ�������Ϊ:{0}���������޶����ϴ��ļ������޶����ϴ��ļ���Ϊ255������ֶ���ϴ�.", filesCount);
                    throw new Exception("�ϴ��ļ����������������Ʒ�Χ��");
                }
            }
            if (null != _logger)
            {
                _logger.InfoFormat("��ʼ�����ϴ��ļ��������ϴ��ļ�������Ϊ:{0}.", filesCount);
            }
            TcpConnection storageConnection = GetStorageConnection(groupName);
            if (null != _logger)
                _logger.InfoFormat("Storage��������IP��:{0}.�˿�Ϊ{1}", storageConnection.IpAddress, storageConnection.Port);

            if (filesBuffer.Count != filesExtension.Length)
            {
                if (null != _logger)
                    _logger.ErrorFormat("�ϴ��ļ��������ϴ��ļ���չ�����ϴ��ļ����鳤��Ϊ{0},�ļ���չ�����鳤��Ϊ{1}��",
                        filesBuffer.Count, filesExtension.Length);
                throw new Exception("�ϴ��ļ�����ƥ�䡣");
            }

            //������չ��������
            byte[] filesExtensionBuffer = new byte[Protocol.FDFS_FILE_EXT_NAME_MAX_LEN * filesCount];
            byte[] fileExtensionBuffer = new byte[Protocol.FDFS_FILE_EXT_NAME_MAX_LEN];
            byte[] fileTempExtensionBuffer;
            for (int i = 0; i < filesExtension.Length; i++)
            {
                if (string.IsNullOrEmpty(filesExtension[i]))
                {
                    if (null != _logger)
                        _logger.Error("�ļ���չ��Ϊ�գ���ֹ�ļ��ϴ���");
                    throw new Exception("δ����ļ���չ����");
                }
                fileTempExtensionBuffer = Encoding.GetEncoding(FastDFSService.Charset).GetBytes(filesExtension[i]);
                int fileExtBufferLength = fileTempExtensionBuffer.Length;
                if (fileExtBufferLength > Protocol.FDFS_FILE_EXT_NAME_MAX_LEN) fileExtBufferLength = Protocol.FDFS_FILE_EXT_NAME_MAX_LEN;

                //Э�����
                Array.Copy(fileTempExtensionBuffer, 0, fileExtensionBuffer, 0, fileExtBufferLength);
                //�������紫��
                Array.Copy(fileExtensionBuffer, 0, filesExtensionBuffer, i * Protocol.FDFS_FILE_EXT_NAME_MAX_LEN, fileExtBufferLength);
            }

            //�����ļ�������
            long filesBufferLength = 0L;
            foreach (byte[] fileBuffer in filesBuffer)
            {
                filesBufferLength += fileBuffer.LongLength;
            }
          
            //�����ļ������ֽ���
            byte[] filesCountBuffer = Util.LongToBuffer(filesCount);

            //����ͷ��Э���
            // Protocol.TRACKER_PROTO_PKG_LEN_SIZE * (filesCount + 2) �����ļ��ĳ���+��չ���ֽ����ĳ���+�ļ��ֽ����ܹ��ĳ���
            byte[] headerBuffer = new byte[1 + Protocol.TRACKER_PROTO_PKG_LEN_SIZE * (filesCount + 2)];
            headerBuffer[0] = (byte)storageConnection.Index;//��һλΪstorage��index
            byte[] temp; 
            //ÿ8λ��ʾÿ���ļ����ֽ�������
            for (int i = 0; i < filesBuffer.Count; i++)
            {
                temp = Util.LongToBuffer(filesBuffer[i].LongLength);
                Array.Copy(temp, 0, headerBuffer, 1 + i * Protocol.TRACKER_PROTO_PKG_LEN_SIZE, temp.LongLength);
            }

            temp = Util.LongToBuffer(filesExtensionBuffer.LongLength);//��չ���ܹ��ĳ���
            Array.Copy(temp, 0, headerBuffer, 1 + filesBuffer.Count * Protocol.TRACKER_PROTO_PKG_LEN_SIZE, temp.LongLength);
            temp = Util.LongToBuffer(filesBufferLength);//�ļ��ֽ�������
            Array.Copy(temp, 0, headerBuffer, 1 + (filesBuffer.Count + 1) * Protocol.TRACKER_PROTO_PKG_LEN_SIZE, temp.LongLength);

            //����Э�鴫����
            byte[] protocalBuffer = Util.PackHeader(Protocol.STORAGE_PROTO_CMD_Batch_UPLOAD,
                //���ȹ��ɣ�һλstorage��index+ÿ���ļ����ֽڳ���+�ļ���չ�����ܳ���+�ļ��ֽڵ��ܳ���
                                               headerBuffer.Length +
                                               filesCountBuffer.Length
                                              + filesExtensionBuffer.Length + filesBufferLength,
                                               0);

            _logger.InfoFormat("�ϴ��ֽ���Ϊ��{0}", headerBuffer.Length +
                                               filesCountBuffer.Length
                                              + filesExtensionBuffer.Length + filesBufferLength);

            Stream outStream = storageConnection.GetStream();
            outStream.Write(protocalBuffer, 0, protocalBuffer.Length);
            outStream.Write(filesCountBuffer, 0, filesCountBuffer.Length);
            outStream.Write(headerBuffer, 0, headerBuffer.Length);
            outStream.Write(filesExtensionBuffer, 0, filesExtensionBuffer.Length);
            foreach (byte[] buffer in filesBuffer)
            {
                outStream.Write(buffer,0,buffer.Length);
            }

            Stream readStream;
            int fileNameBufferLength = Protocol.TRACKER_PROTO_PKG_LEN_SIZE + 128;//�ļ�������+�ļ�������
            byte[] tempBuffer;
            int tempReadSize = 0;
            int fileNameSize;
            byte[] tempFileNameBytes;
            char[] chars;
            int error;
            string[] filesName = new string[filesCount];
            for(int i = 0;i<filesCount;i++)
            {
                readStream = storageConnection.GetStream();
                tempBuffer = new byte[Protocol.TRACKER_PROTO_PKG_LEN_SIZE + 128];
                tempReadSize = readStream.Read(tempBuffer, 0, fileNameBufferLength);
                if(tempReadSize != fileNameBufferLength)
                {
                    if(tempReadSize == 10)//�ļ�δ������� ���ִ���
                    {
                        error = tempBuffer[Protocol.PROTO_HEADER_STATUS_INDEX];
                        if (null != _logger)
                            _logger.ErrorFormat("�ϴ��ļ��м䷢���쳣���ļ�λ��Ϊ:{0}.�����Ϊ:{1}", i + 1, error);
                        throw new Exception("�ϴ��ļ�����");
                    }
                    if (null != _logger)
                        _logger.ErrorFormat("�ϴ��ļ��м䷢���쳣���ļ�λ��Ϊ:{0}.δ�ܷ��ش���ţ�����header����Ϊ:{1}", i + 1, tempReadSize);
                    throw new Exception("�ϴ��ļ�����");
                }

                fileNameSize = (int)Util.BufferToLong(tempBuffer, 0);
                tempFileNameBytes = new byte[fileNameSize];
                Array.Copy(tempBuffer, Protocol.TRACKER_PROTO_PKG_LEN_SIZE, tempFileNameBytes, 0, fileNameSize);
                chars = Util.ToCharArray(tempFileNameBytes);
                filesName[i] = new string(chars, 0, fileNameSize).Trim('\0').Trim();

            }
            readStream = storageConnection.GetStream();
            tempBuffer = new byte[10];
            tempReadSize = readStream.Read(tempBuffer, 0, 10);
            if (10 != tempReadSize)
            {
                if (null != _logger)
                    _logger.ErrorFormat("�ļ��ϴ���ϣ������ļ�·���Ѿ�ȫ�����ؿͻ��ˡ����Ƿ���������������Ϣͷ���󡣴���header����Ϊ:{0}", tempReadSize);
            }

            error = tempBuffer[Protocol.PROTO_HEADER_STATUS_INDEX];
            if (0 != error)
            {
                if (null != _logger)
                    _logger.ErrorFormat("�ļ��ϴ���ϣ������ļ�·���Ѿ�ȫ�����ؿͻ��ˡ����Ƿ������������󡣴����:{0}",error);
            }

            return filesName;
        }
    }
}
