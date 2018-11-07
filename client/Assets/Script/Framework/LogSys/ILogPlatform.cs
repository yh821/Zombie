using UnityEngine;
using System.Collections;

public interface ILogPlatform 
{
    void OpenLogFile(string path);
    void CloseLogFile();
    void Log(string logStr);
	
	void Flush();
}
