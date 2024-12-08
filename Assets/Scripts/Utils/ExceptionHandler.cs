#if UNITY_EDITOR
#else
using UnityEngine;
using System;
using System.IO;

public class ExceptionHandler : MonoBehaviour
{
    //�Ƿ���Ϊ�쳣������
    public bool IsHandler = false;
    //�Ƿ��˳������쳣����ʱ
    public bool IsQuitWhenException = true;
    //�쳣��־����·�����ļ��У�
    private string LogPath;

    void Awake()
    {
        LogPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/"));

        //ע���쳣����ί��
        if (IsHandler)
        {
            Application.logMessageReceived += Handler;
        }
    }

    void OnDestory()
    {
        //���ע��
        Application.logMessageReceived -= Handler;
    }

    void Handler(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
        {
            string logPath = LogPath + "\\" + DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss") + ".log";
            //��ӡ��־
            if (Directory.Exists(LogPath))
            {
                File.AppendAllText(logPath, "[time]:" + DateTime.Now.ToString() + "\r\n");
                File.AppendAllText(logPath, "[type]:" + type.ToString() + "\r\n");
                File.AppendAllText(logPath, "[exception message]:" + logString + "\r\n");
                File.AppendAllText(logPath, "[stack trace]:" + stackTrace + "\r\n");
            }
            //�˳�����bug������������������
            if (IsQuitWhenException)
            {
                Application.Quit();
            }
        }
    }
}
#endif