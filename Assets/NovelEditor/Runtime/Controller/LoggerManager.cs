using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LoggerManager
{
    private static LoggerManager instance = null;
    private static readonly object padlock = new object();

    public static LoggerManager Instance
    {
        get
        {
            if (instance == null)
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new LoggerManager();
                    }
                }
            }
            return instance;
        }
    }

    List<Tuple<string, string>> logHistory = new List<Tuple<string, string>>();

    public void ClearLog()
    {
        logHistory.Clear();
    }

    public void AddLog(string name, string message)
    {
        // êÊì™Ç…ì¸ÇÈ
        logHistory.Insert(0, new Tuple<string, string>(name, message));
    }

    public List<Tuple<string, string>> GetLog()
    {
        return logHistory;
    }
}