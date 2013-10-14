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

namespace FastDFS.Client.Core.Pool
{
    /// <summary>
    /// ����ػ�����
    /// </summary>
    public interface IPoolableObjectFactory<T>
    {
        /// <summary>
        /// ��������
        /// </summary>
        T CreateObject();

        /// <summary>
        /// ���ٶ���.
        /// </summary>
        void DestroyObject(T obj);

        /// <summary>
        /// ��鲢ȷ������İ�ȫ
        /// </summary>
        bool ValidateObject(T obj);

        /// <summary>
        /// ���������еĴ��ö���. 
        /// </summary>
        /// <param name="obj">The obj.</param>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        void ActivateObject(T obj, string ipAddress, int port);

        /// <summary>
        /// ж���ڴ�������ʹ�õĶ���.
        /// </summary>
        void PassivateObject(T obj);

    }
}