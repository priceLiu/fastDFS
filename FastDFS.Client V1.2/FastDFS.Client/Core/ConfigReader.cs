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
using System.Xml;

namespace FastDFS.Client.Core
{
    /// <summary>
    /// �����ļ���ȡ��
    /// </summary>
    public static class ConfigReader
    {
        /// <summary>
        /// ����xml�ļ�
        /// </summary>
        /// <param name="path">����ָ���ļ���·��.</param>
        /// <returns></returns>
        /// <remarks>��·��Ϊ����Ӧ�ó����Ŀ¼������·�������ǲ�������ͷ��·������</remarks>
        /// <example>
        /// XmlDocument doc = ConfigReader.LoadXml("config\\FastDFS.config");
        /// </example>
        public static XmlDocument LoadXml(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(GetFullPath(path));
            return doc;
        }

        /// <summary>
        /// �õ������ļ�������·��
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string GetFullPath(string path)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            if (basePath.ToLower().IndexOf("\\bin") > 0)
            {
                basePath = basePath.Substring(0, basePath.ToLower().IndexOf("\\bin"));
                basePath = string.Format("{0}\\", basePath);
            }
            return string.Format("{0}{1}", basePath, path);
        }

        /// <summary>
        /// ����ָ���ļ��õ�ָ����ǩ�б�
        /// </summary>
        /// <param _name="doc">�����ļ�</param>
        /// <param _name="tagName">��ǩ����</param>
        /// <returns></returns>
        public static XmlNodeList Analyze(XmlDocument doc, string tagName)
        {
            //doc.DocumentElement.SelectNodes("")
            return null == doc ? null : doc.GetElementsByTagName(tagName);
        }

        /// <summary>
        /// Analyzes the single.
        /// </summary>
        /// <param _name="doc">The doc.</param>
        /// <param _name="tagName">Name of the tag.</param>
        /// <returns></returns>
        public static XmlNode AnalyzeSingle(XmlDocument doc, string tagName)
        {
            XmlNodeList nodes = Analyze(doc, tagName);
            if (null == nodes || 0 == nodes.Count) return null;
            return nodes[0];
        }

        /// <summary>
        /// �õ���ǩ���ƶ�����ֵ
        /// </summary>
        /// <typeparam _name="T"></typeparam>
        /// <param _name="node">The node.</param>
        /// <param _name="attributeName">Name of the attribute.</param>
        /// <param _name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetAttributeValue(XmlNode node, string attributeName, out object value)
        {
            if (null == node || string.IsNullOrEmpty(attributeName))
            {
                value = null;
                return false;
            }
            if (!HasAttribute(node, attributeName))
            {
                value = null;
                return false;
            }

            XmlNode attribute = node.Attributes.GetNamedItem(attributeName);
            if (null == attribute)
            {
                value = null;
                return false;
            }
            try
            {
                value = attribute.Value.Trim();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Tries the get attribute value.
        /// </summary>
        /// <typeparam _name="T"></typeparam>
        /// <param _name="doc">The doc.</param>
        /// <param _name="tagName">Name of the tag.</param>
        /// <param _name="attributeName">Name of the attribute.</param>
        /// <param _name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetAttributeValue(XmlDocument doc, string tagName, string attributeName, out object value)
        {
            if (null == doc)
            {
                value = null;
                return false;
            }

            XmlNodeList nodes = Analyze(doc, tagName);
            if (null == nodes || 0 == nodes.Count)
            {
                value = null;
                return false;
            }

            if (!TryGetAttributeValue(nodes[0], attributeName, out value))
            {
                value = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// �ڵ��Ƿ����ֵ
        /// </summary>
        /// <param _name="node"></param>
        /// <returns></returns>
        public static bool HasValue(XmlNode node)
        {
            return null != node && !string.IsNullOrEmpty(node.InnerText);
        }

        /// <summary>
        /// Determines whether the specified doc has value.
        /// </summary>
        /// <param _name="doc">The doc.</param>
        /// <param _name="tagName">Name of the tag.</param>
        /// <returns>
        /// 	<c>true</c> if the specified doc has value; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasValue(XmlDocument doc, string tagName)
        {
            XmlNode node = AnalyzeSingle(doc, tagName);
            return HasValue(node);
        }

        /// <summary>
        /// �ڵ��Ƿ��������
        /// </summary>
        /// <param _name="node"></param>
        /// <returns></returns>
        public static bool HasAttributes(XmlNode node)
        {
            return null == node ? false : null != node.Attributes && 0 != node.Attributes.Count;
        }

        public static bool HasAttributes(XmlDocument doc, string tagName)
        {
            XmlNode node = AnalyzeSingle(doc, tagName);
            return HasAttributes(node);
        }

        /// <summary>
        /// �ڵ��Ƿ����ָ�����Ƶ�����
        /// </summary>
        /// <param _name="node"></param>
        /// <param _name="attributeName"></param>
        /// <returns></returns>
        public static bool HasAttribute(XmlNode node, string attributeName)
        {
            if (!HasAttributes(node)) return false;
            return null != node.Attributes.GetNamedItem(attributeName);
        }

        public static bool HasAttribute(XmlDocument doc, string tagName, string attributeName)
        {
            XmlNode node = AnalyzeSingle(doc, tagName);
            return HasAttribute(node, attributeName);
        }

        /// <summary>
        /// ��ͼ�õ��ڵ��ֵ
        /// </summary>
        /// <param _name="node"></param>
        /// <param _name="value"></param>
        /// <returns></returns>
        public static bool TryGetNodeValue(XmlNode node, out object value)
        {
            if (!HasValue(node))
            {
                value = null;
                return false;
            }

            try
            {
                value =  node.InnerText.Trim();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Tries the get node value.
        /// </summary>
        /// <typeparam _name="T"></typeparam>
        /// <param _name="doc">The doc.</param>
        /// <param _name="tagName">Name of the tag.</param>
        /// <param _name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetNodeValue(XmlDocument doc, string tagName, out object value)
        {
            if (!HasValue(doc, tagName))
            {
                value = null;
                return false;
            }

            try
            {
                value = AnalyzeSingle(doc, tagName).InnerText.Trim();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}