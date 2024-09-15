// Portions are Copyright 2011-2013, SpockerDotNet LLC http://www.sdnllc.com/
// Version 0.1

/**
 * Original version of this Script can be found at:
 * 
 * 		https://github.com/prime31/P31UnityAddOns/blob/master/Scripts/Debugging/D.cs
 * 
 * 
 * 
 * We suggest that you use the Global Defines wizard from Prime31 to manage these
 * 
 * 		https://github.com/prime31/P31UnityAddOns/blob/master/Editor/GlobalDefinesWizard.cs
 * 
 * 
 * 
 * To turn on a feature, simply uncomment the #define you want to activate. 
 * To turn it off, simply recomment the line.
 * 
 * Note that a recompile is required before the affect will take place.
 * 
 **/

// NOTE: the use of #define below is no longer used in this way by Unity to conform to proper use of #define in C#
// use Scripting Define Symbols in the Edit->Preferences->Player tab to set them globally. Here are some pre-mage strings you can copy into the symbols area:

// DEBUG_LEVEL_LOG;DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE;DEBUG_TOHTML;DEBUG_TOFILE
// DEBUG_LEVEL_LOG;DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE;DEBUG_TOHTML
// DEBUG_LEVEL_LOG;DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE;DEBUG_TOFILE
// DEBUG_LEVEL_LOG;DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOHTML;DEBUG_TOFILE
// DEBUG_LEVEL_LOG;DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE
// DEBUG_LEVEL_LOG;DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOHTML
// DEBUG_LEVEL_LOG;DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOFILE

// DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE;DEBUG_TOHTML
// DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE;DEBUG_TOFILE
// DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE
// DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOHTML
// DEBUG_LEVEL_WARN;DEBUG_LEVEL_ERROR;DEBUG_TOFILE

// DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE;DEBUG_TOHTML
// DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE;DEBUG_TOFILE
// DEBUG_LEVEL_ERROR;DEBUG_TOCONSOLE
// DEBUG_LEVEL_ERROR;DEBUG_TOHTML
// DEBUG_LEVEL_ERROR;DEBUG_TOFILE

// can no longer just comment these out (boo!)
//#define DEBUG_LEVEL_LOG
//#define DEBUG_LEVEL_WARN
//#define DEBUG_LEVEL_ERROR
//#define DEBUG_TOFILE
//#define DEBUG_TOCONSOLE
//#define DEBUG_TOHTML

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public static class D
{
	private static StreamWriter m_writer;
	private static StreamWriter m_html;

	private static DLogger m_logger;
	
	private static string m_logPath;
	private static string m_logName;
	
	private static int traceID;

	public static bool logGUI = true;
	public static bool logGUIMessage = true;
	public static bool logPhysics = true;
	public static bool logGraphics = true;
	public static bool logTerrain = true;
	public static bool logAudio = true;
	public static bool logNetworking = true;
	public static bool logNetworkServer = true;
	public static bool logNetworkClient = true;	
	public static bool logSystem = true;
	public static bool logGameMode = true;
	public static bool logCamera = true;
	public static bool logEvent = true;
	public static bool logInput = true;
	public static bool logReplay = true;
	public static bool logException = true;
	public static bool logContent = true;
	public static bool logUtility = true;
	public static bool logGameLogic = true;	
	public static bool logController = true;
	public static bool logHelm = true;
	public static bool logStructure = true;
	public static bool logFitting = true;
	public static bool logHeat = true;
	public static bool logPower = true;
	public static bool logDevice = true;
	public static bool logSocket = true;
	public static bool logModule = true;
	public static bool logWeapon = true;
	
	static D()
	{
		Debug.Log("Initialising DLogger in D.cs");
		
		GameObject _logger = GameObject.Find("DLogger");
		
		if ( _logger == null ) {
			Debug.Log("No initial DLogger GameObject. Adding DLogger GameObject to scene");
			_logger = new GameObject();
			_logger.name = "DLogger";
			_logger.AddComponent<DLogger>();
		}
		
		m_logger = _logger.GetComponent<DLogger>();
		m_logPath = "Logs";
		m_logName = "NoxLog";
				
		if ( m_logger.LoggerPath != "" ) 
		{
			m_logPath = m_logger.LoggerPath;
		}
		
		if ( m_logger.LoggerName != "" )
		{
			m_logName = m_logger.LoggerName;
		}
		
		Debug.Log("Logfile location: " + Path.Combine(Application.dataPath, m_logPath));

		CreateLogFile();
		CreateHtmlLogFile();

		Application.logMessageReceived += logCallback;
		//Application.RegisterLogCallback( logCallback );
	}
	
	public static void logCallback( string log, string stackTrace, LogType type )
	{
		// error gets a stack trace
		if( type == LogType.Error )
		{
			System.Console.WriteLine( log );
			System.Console.WriteLine( stackTrace );
		}
		else
		{
			System.Console.WriteLine( log );
		}
	}

	public static bool filterLog(string type)
	{
		bool logout = false;

    #if DEBUG_LEVEL_WARN
        if (type == "Warning")
        {
            return true;
        }
    #endif

    #if DEBUG_LEVEL_ERROR
        if (type == "Error")
        {
            return true;
        }
    #endif

        string filter = type.ToLower();
		
		switch(filter)
		{
			case "gui": if (logGUI) logout = true; break;
			case "guimessage": if (logGUIMessage) logout = true; break;
			case "physics": if (logPhysics) logout = true; break;
			case "graphics": if (logGraphics) logout = true; break;
			case "terrain": if (logTerrain) logout = true; break;
			case "audio": if (logAudio) logout = true; break;
			case "networking": if (logNetworking) logout = true; break;
			case "networkingserver": if (logNetworkServer) logout = true; break;
			case "networkingclient": if (logNetworkClient) logout = true; break;			
			case "system": if (logSystem) logout = true; break;
			case "gamemode": if (logGameMode) logout = true; break;
			case "camera": if (logCamera) logout = true; break;
			case "event": if (logEvent) logout = true; break;
			case "input": if (logInput) logout = true; break;
			case "replay": if (logReplay) logout = true; break;
			case "exception": if (logException) logout = true; break;
			case "content": if (logContent) logout = true; break;
			case "utility": if (logUtility) logout = true; break;
			case "gamelogic": if (logGameLogic) logout = true; break;			
			case "ai": if (logController) logout = true; break;
			case "helm": if (logHelm) logout = true; break;
			case "structure": if (logStructure) logout = true; break;
			case "fitting": if (logFitting) logout = true; break;
			case "heat": if (logHeat) logout = true; break;
			case "power": if (logPower) logout = true; break;
			case "device": if (logDevice) logout = true; break;
			case "socket": if (logSocket) logout = true; break;
			case "module": if (logModule) logout = true; break;			
			case "weapon": if (logWeapon) logout = true; break;
		}
		
		return logout;
	}	
	
	[System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
	[System.Diagnostics.Conditional( "DEBUG_LEVEL_WARN" )]
	[System.Diagnostics.Conditional( "DEBUG_LEVEL_ERROR" )]
	public static void log( object format, params object[] paramList )
	{
 //#if DEBUG_LEVEL_LOG || DEBUG_LEVEL_WARN || DEBUG_LEVEL_ERROR
        if( format is string ) {
			LogToConsole(string.Format( format as string, paramList ) );
			LogToFile(string.Format( format as string, paramList ) );
			LogToHtml("Log", string.Format( format as string, paramList ) );
		}
		else {
			LogToConsole( format);
			LogToFile( format );
			LogToHtml("Log", format);
		}
//#endif
    }

    [System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
    [System.Diagnostics.Conditional( "DEBUG_LEVEL_WARN" )]
    [System.Diagnostics.Conditional( "DEBUG_LEVEL_ERROR" )]
    public static void log( string type, object format, params object[] paramList )
	{
//#if DEBUG_LEVEL_LOG || DEBUG_LEVEL_WARN || DEBUG_LEVEL_ERROR
        if (filterLog(type))
		{	
			if( format is string ) {
				LogToConsole(string.Format( format as string, paramList ) );
				LogToFile(string.Format( format as string, paramList ) );
				LogToHtml(type, string.Format( format as string, paramList ) );
			}
			else {
				LogToConsole( format);
				LogToFile( format );
				LogToHtml( type, format );
			}
		}
//#endif
    }


    [System.Diagnostics.Conditional( "DEBUG_LEVEL_WARN" )]
	[System.Diagnostics.Conditional( "DEBUG_LEVEL_ERROR" )]
	public static void warn( object format, params object[] paramList )
	{
//#if DEBUG_LEVEL_WARN || DEBUG_LEVEL_ERROR
        if( format is string ) {
			LogToConsole(string.Format( format as string, paramList ) );
			LogToFile(string.Format( format as string, paramList ) );
			LogToHtml("Warning", string.Format( format as string, paramList ) );
		}
		else {
			LogToConsole( format);
			LogToFile( format );
			LogToHtml( "Warning", format );
		}
//#endif
    }

    [System.Diagnostics.Conditional( "DEBUG_LEVEL_ERROR" )]
	public static void error( object format, params object[] paramList )
	{
//#if DEBUG_LEVEL_ERROR
        if( format is string ) {
			LogToConsole(string.Format( format as string, paramList ) );
			LogToFile(string.Format( format as string, paramList ) );
			LogToHtml("Error", string.Format( format as string, paramList ) );
		}
		else {
			LogToConsole( format);
			LogToFile( format );
			LogToHtml( "Error", format );
		}
//#endif
    }
	
	[System.Diagnostics.Conditional( "UNITY_EDITOR" )]
	[System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
	public static void assert( bool condition )
	{
//#if UNITY_EDITOR || DEBUG_LEVEL_LOG
        assert( condition, string.Empty, true );
//#endif
    }

	
	[System.Diagnostics.Conditional( "UNITY_EDITOR" )]
	[System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
	public static void assert( bool condition, string assertString )
	{
//#if UNITY_EDITOR || DEBUG_LEVEL_LOG
        assert( condition, assertString, false );
//#endif
    }
	
	[System.Diagnostics.Conditional( "UNITY_EDITOR" )]
	[System.Diagnostics.Conditional( "DEBUG_LEVEL_LOG" )]
	public static void assert( bool condition, string assertString, bool pauseOnFail )
	{
//#if UNITY_EDITOR || DEBUG_LEVEL_LOG
        if( !condition )
		{
			Debug.LogError( "assert failed! " + assertString );
			
			if( pauseOnFail )
				Debug.Break();
		}
//#endif
    }

    [System.Diagnostics.Conditional( "DEBUG_TOFILE" )]
	private static void CreateLogFile() 
	{
//#if DEBUG_TOFILE
        Debug.Log("Creating Log File");
		
		try
		{
#if UNITY_EDITOR
            m_writer = new StreamWriter( "./Assets/" + m_logPath + "/" + m_logName + ".log", false );
#else
            m_writer = new StreamWriter( "./" + m_logPath + "/" + m_logName + ".log", false );
#endif
            m_writer.AutoFlush = true;
			LogToFile("Logger Active...");
		}
		catch (UnauthorizedAccessException e)
		{
			D.warn("System: {0}", "Permission not given to access file: " + e.Message);
		}		
		catch (System.IO.DirectoryNotFoundException e)
		{
			D.warn("System: {0}", "Could not find folder: " + e.Message);
		}
//#endif
    }

	[System.Diagnostics.Conditional( "DEBUG_TOFILE" )]
	private static void CloseLogFile() 
	{
		//#if DEBUG_TOFILE
		if (m_writer != null)
		{
			Debug.Log("Closing Log File");
			m_writer.Dispose();
			m_writer = null;
		}
//#endif
    }
		
	[System.Diagnostics.Conditional( "DEBUG_TOHTML" )]
	private static void CreateHtmlLogFile() 
	{
//#if DEBUG_TOHTML
        Debug.Log("Creating Html Log File");
		
		try
		{
#if UNITY_EDITOR
            m_html = new StreamWriter( "./Assets/" + m_logPath + "/" + m_logName + ".html", false );
#else
            m_html = new StreamWriter( "./" + m_logPath + "/" + m_logName + ".html", false );
#endif
            m_html.AutoFlush = true;
			m_html.WriteLine("<head>");
			m_html.WriteLine("<script language=\"javascript\" src=\"dlstyle\\dlogger.javascript\"></script>");
			m_html.WriteLine("<link rel=\"stylesheet\" type=\"text/css\" href=\"dlstyle\\dlogger.css\"/>");
			m_html.WriteLine("</head>");
			m_html.WriteLine("<div class=\"Header\">");
			if (logGUI) m_html.WriteLine("<input type=\"button\" value=\"GUI\" class=\"GUI Button\" onclick=\"hide_class('GUI')\"/>");
			if (logGUIMessage) m_html.WriteLine("<input type=\"button\" value=\"GUIMessage\" class=\"GUIMessage Button\" onclick=\"hide_class('GUIMessage')\"/>");
			if (logPhysics) m_html.WriteLine("<input type=\"button\" value=\"Physics\" class=\"Physics Button\" onclick=\"hide_class('Physics')\"/>");
			if (logGraphics) m_html.WriteLine("<input type=\"button\" value=\"Graphics\" class=\"Graphics Button\" onclick=\"hide_class('Graphics')\"/>");
			if (logTerrain) m_html.WriteLine("<input type=\"button\" value=\"Terrain\" class=\"Terrain Button\" onclick=\"hide_class('Terrain')\"/>");
			if (logAudio) m_html.WriteLine("<input type=\"button\" value=\"Audio\" class=\"Audio Button\" onclick=\"hide_class('Audio')\"/>");
			if (logNetworking) m_html.WriteLine("<input type=\"button\" value=\"Networking\" class=\"Networking Button\" onclick=\"hide_class('Networking')\"/>");
			if (logNetworkServer) m_html.WriteLine("<input type=\"button\" value=\"NetworkServer\" class=\"NetworkServer Button\" onclick=\"hide_class('NetworkServer')\"/>");
			if (logNetworkClient) m_html.WriteLine("<input type=\"button\" value=\"NetworkClient\" class=\"NetworkClient Button\" onclick=\"hide_class('NetworkClient')\"/>");		
			if (logSystem) m_html.WriteLine("<input type=\"button\" value=\"System\" class=\"System Button\" onclick=\"hide_class('System')\"/>");
			if (logEvent) m_html.WriteLine("<input type=\"button\" value=\"Event\" class=\"Event Button\" onclick=\"hide_class('Event')\"/>");
			if (logInput) m_html.WriteLine("<input type=\"button\" value=\"Input\" class=\"Input Button\" onclick=\"hide_class('Input')\"/>");
			if (logReplay) m_html.WriteLine("<input type=\"button\" value=\"Replay\" class=\"Replay Button\" onclick=\"hide_class('Replay')\"/>");
			if (logUtility) m_html.WriteLine("<input type=\"button\" value=\"Utility\" class=\"Utility Button\" onclick=\"hide_class('Utility')\"/>");
			m_html.WriteLine("<br />");
			m_html.WriteLine("<input type=\"button\" value=\"Error\" class=\"Error Button\" onclick=\"hide_class('Error')\"/>");
			m_html.WriteLine("<input type=\"button\" value=\"Assert\" class=\"Assert Button\" onclick=\"hide_class('Assert')\"/>");
			m_html.WriteLine("<input type=\"button\" value=\"Warning\" class=\"Warning Button\" onclick=\"hide_class('Warning')\"/>");
			m_html.WriteLine("<input type=\"button\" value=\"Log\" class=\"Log Button\" onclick=\"hide_class('Log')\"/>");
			m_html.WriteLine("<input type=\"button\" value=\"Exception\" class=\"Exception Button\" onclick=\"hide_class('Exception')\"/>");
			m_html.WriteLine("<br />");
			if (logGameMode) m_html.WriteLine("<input type=\"button\" value=\"GameMode\" class=\"GameMode Button\" onclick=\"hide_class('GameMode')\"/>");
			if (logCamera) m_html.WriteLine("<input type=\"button\" value=\"Camera\" class=\"Camera Button\" onclick=\"hide_class('Camera')\"/>");
			if (logContent) m_html.WriteLine("<input type=\"button\" value=\"Content\" class=\"Content Button\" onclick=\"hide_class('Content')\"/>");
			if (logGameLogic) m_html.WriteLine("<input type=\"button\" value=\"GameLogic\" class=\"GameLogic Button\" onclick=\"hide_class('GameLogic')\"/>");		
			if (logStructure) m_html.WriteLine("<input type=\"button\" value=\"Structure\" class=\"Structure Button\" onclick=\"hide_class('Structure')\"/>");
			if (logFitting) m_html.WriteLine("<input type=\"button\" value=\"Fitting\" class=\"Fitting Button\" onclick=\"hide_class('Fitting')\"/>");
			if (logController) m_html.WriteLine("<input type=\"button\" value=\"Controller\" class=\"Controller Button\" onclick=\"hide_class('Controller')\"/>");
			if (logHelm) m_html.WriteLine("<input type=\"button\" value=\"Helm\" class=\"Helm Button\" onclick=\"hide_class('Helm')\"/>");
			if (logDevice) m_html.WriteLine("<input type=\"button\" value=\"Device\" class=\"Device Button\" onclick=\"hide_class('Device')\"/>");
			if (logSocket) m_html.WriteLine("<input type=\"button\" value=\"Socket\" class=\"Socket Button\" onclick=\"hide_class('Socket')\"/>");
			if (logModule) m_html.WriteLine("<input type=\"button\" value=\"Module\" class=\"Module Button\" onclick=\"hide_class('Module')\"/>");
			if (logWeapon) m_html.WriteLine("<input type=\"button\" value=\"Weapon\" class=\"Weapon Button\" onclick=\"hide_class('Weapon')\"/>");
			if (logHeat) m_html.WriteLine("<input type=\"button\" value=\"Heat\" class=\"Heat Button\" onclick=\"hide_class('Heat')\"/>");  
			if (logPower) m_html.WriteLine("<input type=\"button\" value=\"Power\" class=\"Power Button\" onclick=\"hide_class('Power')\"/>");
			m_html.WriteLine("<br />");
			//m_html.WriteLine("<input type=\"button\" value=\"Enable All\" class=\"Enable All Button\" onclick=\"enable_all('LogLines')\"/>");
			//m_html.WriteLine("<input type=\"button\" value=\"Disable All\" class=\"Disable All Button\" onclick=\"disable_all('LogLines')\"/>");
			m_html.WriteLine("</div>");
			m_html.WriteLine("<br /><br /><br /><br /><br />");
			m_html.WriteLine();
			m_html.WriteLine("<h1 class=\"date\">" + System.DateTime.Now.ToString("MM/dd/yyy hh:mm:ss.fff") + " - OS: " + SystemInfo.operatingSystem + "</h1>");
			m_html.WriteLine("<p class=\"Usage\">Click on buttons above to toggle category visibility. Click on STACK buttons by individual log entries to toggle visibility of their stack traces.</p><br />");
			m_html.WriteLine("<div id=\"LogLines\">");
		}
		catch (UnauthorizedAccessException e)
		{
			D.warn("System: {0}", "Permission not given to access file: " + e.Message);
		}	
		catch (System.IO.DirectoryNotFoundException e)
		{
			D.warn("System: {0}", "Could not find folder: " + e.Message);
		}
//#endif
        }

    [System.Diagnostics.Conditional( "DEBUG_TOHTML" )]
        private static void CloseHtmlLogFile()
    {
//#if DEBUG_TOHTML
        Debug.Log("Closing Html Log File");
		m_html.Dispose();
		m_html = null;
//#endif
    }
	
	[System.Diagnostics.Conditional( "DEBUG_TOCONSOLE" )]
	private static void LogToConsole(object log)
    {
//#if DEBUG_TOCONSOLE
        Debug.Log(log);
//#endif
    }
	
	[System.Diagnostics.Conditional( "DEBUG_TOFILE" )]
	private static void LogToFile(object log)
    {
//#if DEBUG_TOFILE
        string _val = log as string;
		string _log = string.Format("{0} {1} {2}", System.DateTime.Now.ToFileTime(), "LOG", _val);
		m_writer.WriteLine(_log);
//#endif
    }

	[System.Diagnostics.Conditional( "DEBUG_TOHTML" )]
	private static void LogToHtml(string type, object log)
    {
//#if DEBUG_TOHTML
        if (filterLog(type))
		{
			m_html.Write("<p class=\"" + type + "\">");
			m_html.Write("<span class=\"Icon\"><img src=\"dlstyle\\{0}.png\" title=\"" + type + "\"></span><span class=\"Time\">{1}</span><a onclick=\"hide('trace{2}')\">STACK</a> ", type.ToLower(), System.DateTime.Now.ToString("MM/dd/yyy hh:mm:ss.fff"),traceID);
			string _val = log as string;
			string _log = string.Format(_val);
			m_html.Write(_log);
			m_html.Write("</p>");
			m_html.Write("\n<pre id=\"trace" + traceID + "\">" + StackTraceUtility.ExtractStackTrace() + "</pre>\n");
			traceID++;
		}
//#endif
    }
	
	public static void Quit() {

#if DEBUG_TOHTML
		if (m_html != null)
		{
			m_html.WriteLine("</div>");
			Debug.Log("DLogger is Shutting Down...");
			CloseHtmlLogFile();
		}
#endif

#if DEBUG_TOFILE
        CloseLogFile();
#endif
	}
}