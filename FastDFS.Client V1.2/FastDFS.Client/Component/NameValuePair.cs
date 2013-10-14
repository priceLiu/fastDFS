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
    /// <summary>
    /// Metedata�Ľṹ
    /// </summary>
    public class NameValuePair
    {
        private string _name;
        private string _value;

        public NameValuePair()
        {
        }

        /// <summary>
        /// ��ʼ��<see cref="NameValuePair"/> ����.
        /// </summary>
        /// <param name="name">Metedata������.</param>
        public NameValuePair(string name)
        {
            _name = name;
        }

        /// <summary>
        /// ��ʼ��<see cref="NameValuePair"/> ����.
        /// </summary>
        /// <param name="name">Mmetedata������.</param>
        /// <param name="value">Metedata��ֵ.</param>
        public NameValuePair(string name, string value)
        {
            _name = name;
            _value = value;
        }

        /// <summary>
        /// �õ���������Metedata������
        /// </summary>
        /// <value>Metedata������.</value>
        public virtual string Name
        {
            get { return _name; }

            set { _name = value; }
        }

        /// <summary>
        /// �õ���������Metedata��ֵ.
        /// </summary>
        /// <value>Metedata��ֵ.</value>
        public virtual string Value
        {
            get { return _value; }

            set { _value = value; }
        }
    }
}