#if UNITY_EDITOR
#else
using UnityEngine;
using System;
using System.IO;

public class ExceptionHandler : MonoBehaviour
{
    //是否作为异常处理者
    public bool IsHandler = false;
    //是否退出程序当异常发生时
    public bool IsQuitWhenException = true;
    //异常日志保存路径（文件夹）
    private string LogPath;

    void Awake()
    {
        LogPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));

        //注册异常处理委托
        if (IsHandler)
        {
            Application.logMessageReceived += Handler;
        }
    }

    void OnDestory()
    {
        //清除注册
        Application.logMessageReceived -= Handler;
    }

    void Handler(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            string logPath = LogPath + "\\" + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss") + ".log";
            //打印日志
            if (Directory.Exists(LogPath))
            {
                File.AppendAllText(logPath, "[time]:" + DateTime.Now.ToString() + "\r\n");
                File.AppendAllText(logPath, "[type]:" + type.ToString() + "\r\n");
                File.AppendAllText(logPath, "[exception message]:" + logString + "\r\n");
                File.AppendAllText(logPath, "[stack trace]:" + stackTrace + "\r\n");
            }
            //退出程序，bug反馈程序重启主程序
            if (IsQuitWhenException)
            {
                Application.Quit();
            }
        }
    }
}
#endif