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
    /// ����ؽӿ�
    /// </summary>
    public interface IObjectPool<T>
        where T : new()
    {
        /// <summary>
        /// �õ�����.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        T GetObject(string ipAddress, int port);

        /// <summary>
        /// ��ʹ����ϵĶ��󷵻ص������.
        /// </summary>
        void ReturnObject(T target);

        /// <summary>
        /// �رն���ز��ͷų������е���Դ
        /// </summary>
        void Close();

        /// <summary>
        /// �õ���ǰ�����������ʹ�õĶ�����. 
        /// </summary>
        int NumActive { get; }        
        
        /// <summary>
        /// �õ���ǰ������п��õĶ�����
        /// </summary>
        int NumIdle { get; }

        /// <summary>
        /// ǿ�д���һ������
        /// </summary>
        /// <returns></returns>
        T RescueObject(string ipAddress,int port);
    }
}