﻿/*************************************************************************************************************
 * 
 * Copyright (C) 2010 Seapeak.Xu / xvhfeng
 * FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,  
 * which may be found in the FastDFS .Net Client source kit.
 * Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail.
 * 
*************************************************************************************************************/

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// 有关程序集的常规信息通过下列属性集
// 控制。更改这些属性值可修改
// 与程序集关联的信息。
[assembly: AssemblyTitle("5173DFS.Client")]
[assembly: AssemblyDescription("The 5173DFS .Net Client")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("5173.com")]
[assembly: AssemblyProduct("5173DFS.Client")]
[assembly: AssemblyCopyright("Copyright (C) 2010 5173.com")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// 将 ComVisible 设置为 false 使此程序集中的类型
// 对 COM 组件不可见。如果需要从 COM 访问此程序集中的类型，
// 则将该类型上的 ComVisible 属性设置为 true。
[assembly: ComVisible(false)]

// 如果此项目向 COM 公开，则下列 GUID 用于类型库的 ID
[assembly: Guid("7a7da6a0-df35-4a36-ba2c-729be92022ef")]

// 程序集的版本信息由下面四个值组成:
//
//      主版本
//      次版本 
//      内部版本号
//      修订号
//
// 可以指定所有这些值，也可以使用“修订号”和“内部版本号”的默认值，
// 方法是按如下所示使用“*”:
[assembly: AssemblyVersion("1.0.2.1")]
[assembly: AssemblyFileVersion("1.0.2.1")]


/*
 * v1.0.0.0 fastdfs上传文件
 * v1.0.1.0 因为服务器增加目录服务，更改返回的路径截取方式
 * v1.0.1.1 更改客户端侦测异常信息的来源
 * v1.0.2.0 更改客户端超时引起的图片串行问题
 * v1.0.2.1 增加上传文件时，循环中超时机制
 * */
