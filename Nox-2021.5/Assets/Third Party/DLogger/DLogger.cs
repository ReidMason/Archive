using UnityEngine;
using System.Collections;

public class DLogger : MonoBehaviour 
{
	public string LoggerPath;
	public string LoggerName;
	
	// Use this for initialization
	void Awake() 
	{
		Debug.Log("DLogger is Active...\nCheck <Project Folder>\\Logs for file logs and dlstyle subfolder for HTML formatting. Check <Project Folder>\\Assets\\Custom Assets\\Third Party\\DLogger\\D.cs for settings.");
		D.log("LOG Test");
		D.warn("WARN Test");
		D.error("ERROR Test");
        enabled = false;
	}
	
	// Update is called once per frame
	void Update () 
	{
		D.warn("DLogger Update -- should not happen -- ");
	}
	
	void OnApplicationQuit() 
	{
		D.Quit();
	}
}
