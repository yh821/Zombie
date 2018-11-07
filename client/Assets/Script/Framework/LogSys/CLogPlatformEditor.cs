using UnityEngine;
using System.Collections;
using System.IO;

public class CLogPlatformEditor : ILogPlatform
{
    private static FileStream mStream;
    private static StreamWriter mWriter;
    private static string mLogPath;

    public void OpenLogFile(string path)
    {
        //mLogPath = path;
        //string dirPath = Path.GetDirectoryName(path);
        //if (!Directory.Exists(dirPath))
        //    Directory.CreateDirectory(dirPath);

        //mStream = new FileStream(path, FileMode.Append);
        //mWriter = new StreamWriter(mStream);
    }

    public void CloseLogFile()
    {
        //if (mWriter != null)
        //{
        //    mWriter.Flush();
        //    mWriter.Close();
        //    mWriter.Dispose();
        //}
        //if (mStream != null)
        //{
        //    mStream.Close();
        //    mStream.Dispose();
        //}
        //mStream = null;
        //mWriter = null;
    }

    public void Log(string logStr)
    {
        //if (mStream == null || mWriter == null)
        //    OpenLogFile(mLogPath);

        //mWriter.WriteLine(logStr);
        //mWriter.Flush(); //
    }
	
	public void Flush()
	{
	}
}
