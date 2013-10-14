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
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using log4net;

namespace FastDFS.Client.Component
{
    public sealed class Util
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// ��head����ת���ɶ�����
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="pkgLength"></param>
        /// <param name="errno"></param>
        /// <returns></returns>
        public static byte[] PackHeader(byte cmd, long pkgLength, byte errno)
        {
            byte[] headerBuffer = new byte[Protocol.TRACKER_PROTO_PKG_LEN_SIZE + 2];
            headerBuffer = InitializeBuffer(headerBuffer, 0);
            byte[] hexs = LongToBuffer(pkgLength);
            Array.Copy(hexs, 0, headerBuffer, 0, hexs.Length);
            headerBuffer[Protocol.PROTO_HEADER_CMD_INDEX] = cmd;
            headerBuffer[Protocol.PROTO_HEADER_STATUS_INDEX] = errno;
            return headerBuffer;
        }


        /// <summary>
        /// ���ܴӷ��������ؿͻ��˵���Ϣ.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="cmd">The CMD.</param>
        /// <param name="bodyLength">Length of the body.</param>
        /// <returns></returns>
        public static HeaderInfo RecvHeader(NetworkStream stream, byte cmd, long bodyLength, string source)
        {
            byte[] headerBuffer = new byte[Protocol.TRACKER_PROTO_PKG_LEN_SIZE + 2];
            int bytes;

            if (headerBuffer.Length != (bytes = ReadInput(stream, headerBuffer, 0, headerBuffer.Length)))
            {
                string message = string.Format("�������˷��͵İ���С��ͷ��С�����.����СΪ��{0},ͷ��СΪ��{1}", bytes, headerBuffer.Length);
                if (null != _logger)_logger.Error(message);
                throw new IOException(message);
            }

            if (cmd != headerBuffer[Protocol.PROTO_HEADER_CMD_INDEX])
            {
                string message = string.Format("��Դ��{0},���������յ��������Э��涨������Э��Ϊ:{1},����Э��Ϊ:{2}",
                                               source,headerBuffer[Protocol.PROTO_HEADER_CMD_INDEX], cmd);
                if (null != _logger)_logger.Error(message);
                throw new IOException(message);
            }

            if (0 != headerBuffer[Protocol.PROTO_HEADER_STATUS_INDEX])
            {
                return new HeaderInfo(headerBuffer[Protocol.PROTO_HEADER_STATUS_INDEX], 0);
            }

            long pkgLength = BufferToLong(headerBuffer, 0);
            if (0 > pkgLength)
            {
                string message = string.Format("���������͵����������Ȳ����Ϲ涨������������Ϊ:{0}", pkgLength);
                if (null != _logger)_logger.Error(message);
                throw new IOException(message);
            }

            if (0 <= bodyLength && pkgLength != bodyLength)
            {
                string message = string.Format("���������͵����������Ȳ�����Э��涨��������������Ϊ:{0},Э��涨����������Ϊ��{1}", pkgLength, bodyLength);
                if (null != _logger)_logger.Error(message);
                throw new IOException(message);
            }

            return new HeaderInfo(0, pkgLength);
        }

        /// <summary>
        /// ���մӷ��������ؿͻ��˵���Ϣ
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="cmd">The CMD.</param>
        /// <param name="bodyLength">Length of the body.</param>
        /// <returns></returns>
        public static PackageInfo RecvPackage(NetworkStream stream, byte cmd, long bodyLength, string source)
        {
            HeaderInfo header = RecvHeader(stream, cmd, bodyLength, source);
            if (header.ErrorNo != 0) return new PackageInfo(header.ErrorNo, null);

            byte[] body = new byte[(int) header.Length];
            int totalBytes = 0;
            int remainBytes = (int) header.Length;
            int bytes;

            while (totalBytes < header.Length)
            {
                if ((bytes = ReadInput(stream, body, totalBytes, remainBytes)) < 0) break;

                totalBytes += bytes;
                remainBytes -= bytes;
            }

            if (totalBytes != header.Length)
                throw new IOException(string.Format("Recv package size {0} != {1}", totalBytes, header.Length));

            return new PackageInfo(0, body);
        }


        /// <summary>
        /// ȡ��Ԫ����
        /// </summary>
        /// <param name="metadata">The meta_buff.</param>
        /// <returns></returns>
        public static NameValuePair[] SplitMetadata(string metadata)
        {
            return SplitMetadata(metadata, Protocol.FDFS_RECORD_SEPERATOR, Protocol.FDFS_FIELD_SEPERATOR);
        }

        /// <summary>
        /// ȡ��Ԫ����.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="recordSeperator">The record seperator.</param>
        /// <param name="filedSeperator">The filed seperator.</param>
        /// <returns></returns>
        public static NameValuePair[] SplitMetadata(string buffer,
                                                    string recordSeperator, string filedSeperator)
        {
            string[] cols;
            NameValuePair[] nameValuePairs;

            string[] rows = buffer.Split(new string[] {recordSeperator}, StringSplitOptions.RemoveEmptyEntries);
            nameValuePairs = new NameValuePair[rows.Length];
            for (int i = 0; i < rows.Length; i++)
            {
                cols = rows[i].Split(new string[] {filedSeperator}, 2, StringSplitOptions.RemoveEmptyEntries);
                nameValuePairs[i] = new NameValuePair(cols[0]);
                if (cols.Length == 2)
                {
                    nameValuePairs[i].Value = (cols[1]);
                }
            }

            return nameValuePairs;
        }

        /// <summary>
        /// ��Ԫ���ݺ�ת����Э��涨��ʽ
        /// </summary>
        /// <param name="nameValuePair">The name value pair.</param>
        /// <returns></returns>
        public static string PackMetadata(NameValuePair[] nameValuePair)
        {
            if (nameValuePair.Length == 0)
            {
                return "";
            }

            StringBuilder sb = new StringBuilder(32*nameValuePair.Length);
            sb.Append(nameValuePair[0].Name).Append(Protocol.FDFS_FIELD_SEPERATOR).Append(nameValuePair[0].Value);
            for (int i = 1; i < nameValuePair.Length; i++)
            {
                sb.Append(Protocol.FDFS_RECORD_SEPERATOR);
                sb.Append(nameValuePair[i].Name).Append(Protocol.FDFS_FIELD_SEPERATOR).Append(nameValuePair[i].Value);
            }

            return sb.ToString();
        }

        /// <summary>
        /// ��long��������ת���ɶ�����
        /// </summary>
        /// <param name="n">The n.</param>
        /// <returns></returns>
        public static byte[] LongToBuffer(long n)
        {
            byte[] buffer = new byte[8];
            buffer[0] = (byte) ((n >> 56) & 0xFF);
            buffer[1] = (byte) ((n >> 48) & 0xFF);
            buffer[2] = (byte) ((n >> 40) & 0xFF);
            buffer[3] = (byte) ((n >> 32) & 0xFF);
            buffer[4] = (byte) ((n >> 24) & 0xFF);
            buffer[5] = (byte) ((n >> 16) & 0xFF);
            buffer[6] = (byte) ((n >> 8) & 0xFF);
            buffer[7] = (byte) (n & 0xFF);

            return buffer;
        }

        /// <summary>
        /// ��������ת����long��������
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <returns></returns>
        public static long BufferToLong(byte[] buffer, int offset)
        {
            return (((long) (buffer[offset] >= 0 ? buffer[offset] : 256 + buffer[offset])) << 56) |
                   (((long) (buffer[offset + 1] >= 0 ? buffer[offset + 1] : 256 + buffer[offset + 1])) << 48) |
                   (((long) (buffer[offset + 2] >= 0 ? buffer[offset + 2] : 256 + buffer[offset + 2])) << 40) |
                   (((long) (buffer[offset + 3] >= 0 ? buffer[offset + 3] : 256 + buffer[offset + 3])) << 32) |
                   (((long) (buffer[offset + 4] >= 0 ? buffer[offset + 4] : 256 + buffer[offset + 4])) << 24) |
                   (((long) (buffer[offset + 5] >= 0 ? buffer[offset + 5] : 256 + buffer[offset + 5])) << 16) |
                   (((long) (buffer[offset + 6] >= 0 ? buffer[offset + 6] : 256 + buffer[offset + 6])) << 8) |
                   ((buffer[offset + 7] >= 0 ? buffer[offset + 7] : 256 + buffer[offset + 7]));
        }


        /// <summary>
        /// Fills the array with an specific value from an specific index to an specific index.
        /// </summary>
        /// <param _name="array">The array to be filled.</param>
        /// <param _name="fromindex">The first index to be filled.</param>
        /// <param _name="toindex">The last index to be filled.</param>
        /// <param _name="val">The value to fill the array with.</param>
        public static void Fill(Array array, Int32 fromindex, Int32 toindex, Object val)
        {
            Object temp = val;
            Type elementtype = array.GetType().GetElementType();
            if (elementtype != val.GetType())
                temp = Convert.ChangeType(val, elementtype);
            if (array.Length == 0)
                throw (new NullReferenceException());
            if (fromindex > toindex)
                throw (new ArgumentException());
            if ((fromindex < 0) || (array).Length < toindex)
                throw (new IndexOutOfRangeException());
            for (int index = (fromindex > 0) ? fromindex-- : fromindex; index < toindex; index++)
                array.SetValue(temp, index);
        }

        /// <summary>
        /// Fills the array with an specific value.
        /// </summary>
        /// <param _name="array">The array to be filled.</param>
        /// <param _name="val">The value to fill the array with.</param>
        public static void Fill(Array array, Object val)
        {
            Fill(array, 0, array.Length, val);
        }

        public static int ReadInput(Stream sourceStream, byte[] target, int start, int count)
        {
            if (target.Length == 0)
                return 0;

            byte[] receiver = new byte[target.Length];

            int bytesRead = sourceStream.Read(receiver, start, count);

            if (bytesRead == 0)
                return -1;

            for (int i = start; i < start + bytesRead; i++)
                target[i] = receiver[i];

            return bytesRead;
        }

        public static byte[] InitializeBuffer(byte[] buffer,byte by)
        {
            for (long i = 0; i < buffer.LongLength; i++)
            {
                buffer[i] = by;
            }
            return buffer;

        }

        /// <summary>
        /// Converts an array of bytes to an array of chars
        /// </summary>
        /// <param _name="byteArray">The array of bytes to convert</param>
        /// <returns>The new array of chars</returns>
        public static char[] ToCharArray(byte[] byteArray)
        {
            return Encoding.UTF8.GetChars(byteArray);
        }

        public static string GetFilePath(string fileName)
        {
            int index = fileName.IndexOf("/M00/");//�����ʶ��
            return index > -1 ? fileName.Remove(0, index + 4) : fileName;
        }

    }
}