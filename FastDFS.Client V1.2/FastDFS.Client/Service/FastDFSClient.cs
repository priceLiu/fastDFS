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

using System.Collections.Generic;
using FastDFS.Client.Component;

namespace FastDFS.Client.Service
{
    /// <summary>
    /// DFS�Ŀͻ���
    /// </summary>
    public class FastDFSClient : StorageClient
    {
        private const string SPLIT_GROUP_NAME_AND_FILENAME_SEPERATOR = "/";

        /// <summary>
        ///�ϴ��ļ�.
        /// </summary>
        /// <param name="buffer">�ϴ��ļ��Ķ�������.</param>
        /// <param name="extension">�ϴ��ļ�����չ��.</param>
        /// <returns>�ļ����ڵ�·��</returns>
        public static string Upload(byte[] buffer, string extension)
        {
            return Upload(string.Empty, buffer, extension, null);
        }

        /// <summary>
        /// �ϴ��ļ�.
        /// </summary>
        /// <param name="buffer">�ϴ��ļ��Ķ�������.</param>
        /// <param name="extension">�ϴ��ļ�����չ��.</param>
        /// <param name="metadatas">�ļ�����չ����.</param>
        /// <returns>�ļ����ڵ�·��</returns>
        public static string Upload(byte[] buffer, string extension, NameValuePair[] metadatas)
        {
            return Upload(string.Empty, buffer, extension, metadatas);
        }

        /// <summary>
        /// �ϴ��ļ�.
        /// </summary>
        /// <param name="groupName">�ϴ�������������.</param>
        /// <param name="buffer">�ϴ��ļ��Ķ�������.</param>
        /// <param name="extension">�ϴ��ļ�����չ��.</param>
        /// <returns>�ļ����ڵ�·��</returns>
        public static string Upload(string groupName, byte[] buffer, string extension)
        {
            return Upload(groupName, buffer, extension, null);
        }

        /// <summary>
        /// �ϴ��ļ�.
        /// </summary>
        /// <param name="groupName">�ϴ�������������.</param>
        /// <param name="buffer">�ϴ��ļ��Ķ�������.</param>
        /// <param name="extension">�ϴ��ļ�����չ��.</param>
        /// <param name="metadatas">�ļ�����չ����.</param>
        /// <returns>�ļ����ڵ�·��</returns>
        public static string Upload(string groupName, byte[] buffer, string extension, NameValuePair[] metadatas)
        {
            string[] uploadPath = DoUpload(groupName, string.Empty, buffer, extension, metadatas);
            //return uploadPath != null ? Util.GetFilePath(string.Format("{0}{1}{2}", uploadPath[0], SPLIT_GROUP_NAME_AND_FILENAME_SEPERATOR, uploadPath[1])) : null;

            return uploadPath != null ? Util.GetFilePath(string.Format("{0}{1}", SPLIT_GROUP_NAME_AND_FILENAME_SEPERATOR, uploadPath[1])) : null;
        }

        /// <summary>
        /// �ϴ��ļ�.
        /// </summary>
        /// <param name="localFileName">�ϴ��ļ��ı���·��.</param>
        /// <param name="extension">�ϴ��ļ�����չ��.</param>
        /// <returns>�ļ����ڵ�·��</returns>
        public static string Upload(string localFileName, string extension)
        {
            return Upload(string.Empty, localFileName, extension, null);
        }

        /// <summary>
        /// �ϴ��ļ�.
        /// </summary>
        /// <param name="localFileName">�ϴ��ļ��ı���·��.</param>
        /// <param name="extension">�ϴ��ļ�����չ��.</param>
        /// <returns>�ļ����ڵ�·��</returns>
        public static string Upload(string localFileName, string extension, NameValuePair[] metadatas)
        {
            return Upload(string.Empty, localFileName, extension, metadatas);
        }

        /// <summary>
        /// �ϴ��ļ�.
        /// </summary>
        /// <param name="groupName">�ϴ�������������.</param>
        /// <param name="localFileName">�ϴ��ļ��ı���·��.</param>
        /// <param name="extension">�ϴ��ļ�����չ��.</param>
        /// <returns>�ļ����ڵ�·��</returns>
        public static string Upload(string groupName, string localFileName, string extension)
        {
            return Upload(groupName, localFileName, extension, null);
        }

        /// <summary>
        /// �ϴ��ļ�.
        /// </summary>
        /// <param name="groupName">�ϴ�������������.</param>
        /// <param name="localFileName">�ϴ��ļ��ı���·��.</param>
        /// <param name="extension">�ϴ��ļ�����չ��.</param>
        /// <param name="metadatas">�ϴ��ļ�����չ����.</param>
        /// <returns>�ļ����ڵ�·��</returns>
        public static string Upload(string groupName, string localFileName, string extension, NameValuePair[] metadatas)
        {
            string[] uploadPath = DoUpload(groupName, localFileName, null, extension, metadatas);
            //return uploadPath != null ? Util.GetFilePath(string.Format("{0}{1}{2}", uploadPath[0], SPLIT_GROUP_NAME_AND_FILENAME_SEPERATOR, uploadPath[1])) : null;
            return uploadPath != null ? Util.GetFilePath(string.Format("{0}{1}", SPLIT_GROUP_NAME_AND_FILENAME_SEPERATOR, uploadPath[1])) : null;
        }

        public static string[] BatchUpload(string groupName, IList<byte[]> filesBuffer,
                                                   string[] filesExtension)
        {
            return DoBatchUpload(groupName, filesBuffer, filesExtension);
        }


    }
}
