using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NetworkServer : MonoBehaviour {
	
	//DOCUMENTATION OF PROCESS: http://forum.unity3d.com/threads/153688-Helpful-information-for-anyone-using-Unity-Network-and-NAT-punchthrough
	//engineering options: http://answers.unity3d.com/questions/279493/rpc-custom-server-best-practice.html
	
	//full menu example http://subversion.assembla.com/svn/steamkart/trunk/unity/Assets/_Scripts/MainMenu.cs
	
	//see http://answers.unity3d.com/questions/145330/network-setup.html
	//and http://www.scribd.com/doc/38400039/M2H-Unity-Networking-Tutorial
	
	//server settings
	//http://docs.unity3d.com/Documentation/Components/net-MasterServer.html
	//set *HOSTNAME to null if you want to use Unity defaults (dont worry about the ports for default)
    public static string MASTERSERVER_HOSTNAME = null;//"dorothy.cc.gt.atl.ga.us";"127.0.0.1";
	public static int MASTERSERVER_PORT = 23466;
    public static string FACILITATOR_HOSTNAME = null;//"dorothy.cc.gt.atl.ga.us";"127.0.0.1"
	public static int FACILITATOR_PORT = 23468;
	
	//these two fields are set by NetworkServer.Start() and NetworkClient.Start()
	//consider reading from command line, or from URL params
	public static ServerInfo MASTERSERVER;// = MASTERSERVER_DEFAULTS;
	public static ServerInfo FACILITATOR;// = FACILITATOR_DEFAULTS;
	
	public static string SERVER_GAMETYPENAME = "EILab_networkingBase_v1";
	public static string SERVER_GAMENAME = "EI lab base networking game.";
	public static string SERVER_GAMECOMMENT = "Client server networking skeleton code";
	
	private static int SERVER_MAX_CONNECTIONS = 32;
	private static int SERVER_LISTEN_PORT = 50466;
	private static bool SERVER_USE_NAT = false; 
	
	public static bool isServer = false;
	
	public struct ServerInfo
	{
        //MasterServer.ipAddress = "xxx.xxx.xxx.xxx";
        //MasterServer.port = #####;
        //Network.natFacilitatorIP = ...
        //Network.natFacilitatorPort

		public string serverIP;
		public int serverPort;
		
		public ServerInfo( string ip, int port )
		{
			serverIP=ip;
			serverPort=port;
		}
		
		public ServerInfo( System.Net.IPAddress[] rayIps, int port )
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			
			foreach( System.Net.IPAddress ip in rayIps ) 
				sb.Append( ip ).Append( " " );
			Debug.Log( "host has ips: " + sb.ToString() );
			serverIP=rayIps[0].ToString();
			serverPort=port;			
			Debug.Log( "using " + serverIP );
		}
		
		public static ServerInfo FromHostName( string hostNameOrAddress, int port )
		{
			Debug.Log( "Creating ServerInfo for host " + hostNameOrAddress );
			return new ServerInfo( System.Net.Dns.GetHostAddresses( hostNameOrAddress ), port );
		}
	}
	
	static NetworkServer _instance;
	private static bool doneStart = false;
#if UNITY_WEBPLAYER
#else
	
	/// <summary>
	/// Static instance of the NetworkServer.
	///
	/// When you want to access the server without a direct
	/// reference (which you do in mose cases), use NetworkServer.Instance and the required
	/// GameObject initialization will be done for you.
	/// </summary>
	public static NetworkServer Instance {
		
		get {
			
		  if (_instance == null) {
		    _instance = FindObjectOfType(typeof(NetworkServer)) as NetworkServer;

		    if (_instance != null) {
		      return _instance;
		    }
		
		    GameObject server = new GameObject("__Game Server__");
		    _instance = server.AddComponent<NetworkServer>();
			_instance.Start();
			
		  }

		  return _instance;
		}
		
	}
	
	// Use this for initialization
	void Start () 
	{
        string[] args = System.Environment.GetCommandLineArgs();
        if (!doneStart && System.Array.IndexOf(args, "server") > 0) 
		{
			//Application.targetFrameRate = 1;
			DebugConsole.Log("NetworkServer.Start() start");
#if !EIL_PRODUCTION
			DebugConsole.IsOpen = true;
#endif
			
			processCommandLineArgs();
			
			if( NetworkServer.MASTERSERVER_HOSTNAME != null )
			{
				NetworkServer.MASTERSERVER = ServerInfo.FromHostName( MASTERSERVER_HOSTNAME, MASTERSERVER_PORT );
				MasterServer.port = NetworkServer.MASTERSERVER.serverPort;
				MasterServer.ipAddress = NetworkServer.MASTERSERVER.serverIP;
			}
			else//use Unity defaults
				NetworkServer.MASTERSERVER = new ServerInfo( MasterServer.ipAddress, MasterServer.port );
			
			if( NetworkServer.FACILITATOR_HOSTNAME != null )
			{
				NetworkServer.FACILITATOR = ServerInfo.FromHostName( FACILITATOR_HOSTNAME, FACILITATOR_PORT );
				Network.natFacilitatorIP = NetworkServer.FACILITATOR.serverIP;
				Network.natFacilitatorPort = NetworkServer.FACILITATOR.serverPort;
			}
			else//use Unity defaults
				NetworkServer.FACILITATOR = new ServerInfo( Network.natFacilitatorIP, Network.natFacilitatorPort );

            DebugConsole.Log("Connection Test initiated.");
			TestConnection();
			
			GameStateServer gss = GameStateServer.Instance;
			
			Debug.Log("Using master Server at " + MasterServer.ipAddress + ":" + MasterServer.port );
			Debug.Log("Using NAT Facilitator Info:" + Network.natFacilitatorIP );
			
			
			doneStart = true;
			DebugConsole.Log("NetworkServer.Start() done");
		}
		else
			DebugConsole.Log("NetworkServer.Start() already called, so skipping.");
	}
	
	//service options
	//http://superuser.com/questions/445345/how-to-monitor-a-windows-process-and-send-an-alert-when-it-crashes-or-closes
	//zumwalt http://forum.unity3d.com/threads/75710-Master-Server-for-Service-on-Windows-Servers
	//http://stackoverflow.com/questions/7764088/net-console-application-as-windows-service
	//https://groups.google.com/forum/#!topic/microsoft.public.dotnet.languages.csharp/TUXp6lRxy6Q
	
	//masterServer
	private static void processCommandLineArgs()
	{
		string[] args = System.Environment.GetCommandLineArgs();
		
		//if( System.Array.IndexOf( args, "server" ) >= 0 )
		//{
		//	NetworkServer.isServer = true;
		//}
		if( args.Length > 0 && args[0] != null )
		{
			string firstArg = args[0].ToString().ToLower();
			if( firstArg.IndexOf( '?' ) >= 0 || firstArg.IndexOf( "help" ) >=0 )
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendLine( "all command line parameters are optional: " );
				sb.Append( "[-masterserver|ms <hostnameOrAddress>:<port>] " );
				sb.Append( "[-facilitator|fa <hostnameOrAddress>:<port>] " );
				sb.Append( "[server] " );
				Debug.Log( sb.ToString() );
			}
		}
		
		for( int i=0; i < args.Length; i++ )
		{
			if( args[i] != null )
			{
				switch( args[i].ToLower() ) 
				{
				case "-ms":
				case "-masterserver": //eg -masterserver dorothy.cc.gt.atl.ga.us:25000
				{
					if( i+1 >= args.Length || args[i+1].ToString().IndexOf(':') < 0 ){ Debug.LogError( "Expecting hostNameOrAddr:Port after -masterserver" ); break; }
					string[] splitAddr = args[i+1].ToString().Split(':');
					NetworkServer.MASTERSERVER_HOSTNAME = splitAddr[0];
					NetworkServer.MASTERSERVER_PORT = int.Parse( splitAddr[1] );
					//NetworkServer.MASTERSERVER = ServerInfo.FromHostName( splitAddr[0], int.Parse( splitAddr[1] ) );
				}
					break;
				case "-fa":
				case "-facilitator":   //eg -facilitator 127.0.0.1:55555
				{
					if( i+1 >= args.Length || args[i+1].ToString().IndexOf(':') < 0 ){ Debug.LogError( "Expecting hostNameOrAddr:Port after -facilitator" ); break; }
					string[] splitAddr = args[i+1].ToString().Split(':');
					//NetworkServer.FACILITATOR = ServerInfo.FromHostName( splitAddr[0], int.Parse( splitAddr[1] ) );
					NetworkServer.FACILITATOR_HOSTNAME = splitAddr[0];
					NetworkServer.FACILITATOR_PORT = int.Parse( splitAddr[1] );
				}
					break;
				case "server":
					NetworkServer.isServer = true;
					break;
				default:
					break;
				}
			}
		}

	}
	
	// Update is called once per frame
	//void Update () {}
	
	void OnGUI()
	{
        if (isServer) //&& Network.peerType == NetworkPeerType.Disconnected
		{
			//GUILayout.BeginArea( new Rect(0,0,120,800) );
			Rect windowRectServerConfig = new Rect (20, 20, 120, 50);
			windowRectServerConfig = GUILayout.Window(0, windowRectServerConfig, ServerConfigWindow, "ServerConfig", GUILayout.Width(100) );
			
			Rect windowRectNatStatus = new Rect (500, 20, 300, 250);
			windowRectNatStatus = GUILayout.Window(1, windowRectNatStatus, NatStatusWindow, "NatStatus", GUILayout.Width(300) );
		}	
	}
	
	void ServerConfigWindow( int windowID )
	{
		//DebugConsole.IsOpen = true;
		if (GUILayout.Button ("Quit"))
			Application.Quit();
		if (GUILayout.Button ("Start Server"))
			NetworkServer.Instance.StartServer();
		if (GUILayout.Button ("Stop Server"))
			NetworkServer.Instance.StopServer();
		if (GUILayout.Button ("Test Connection"))
			NetworkServer.Instance.TestConnection();
		if ( GUILayout.Button( "Toggle debug" ) )
			DebugConsole.IsOpen = !DebugConsole.IsOpen;
		if (GUILayout.Button ("Network Info"))
		{
			//DebugConsole.IsOpen = true;
			DebugConsole.LogWarning("network: " + Network.peerType);
			if( networkView == null )
			{ 
				DebugConsole.Log("networkView == null");
				if( NetworkClient.Instance.target != null )
				{
					DebugConsole.Log( "Got target from client controller." );
					DebugConsole.Log( "networkView: " + NetworkClient.Instance.target.networkView );
					DebugConsole.Log( "networkView.viewID: " + NetworkClient.Instance.target.networkView.viewID );
				}
			}
			else
			{
				DebugConsole.Log("networkView != null" + networkView);
				/*
				if( networkView.viewID == null )
				{
					DebugConsole.Log("networkView.viewID == null");
				}
				else
				{
					DebugConsole.Log("networkView.viewID != null" + networkView.viewID);
				}

				DebugConsole.Log("networkView.group != null" + networkView.group);
				*/
			}
			
		}		
	}
	
	void NatStatusWindow( int windowID )
	{
		//GUILayout.BeginArea( new Rect(500,0,300,400) );
		GUILayout.BeginVertical(/*"NetworkServer"*/ ); //, GUILayout.MaxWidth(300), GUILayout.MinWidth(100) 
		GUILayout.Label("Current Status: " + NetworkUtils.ConnectionTester.testStatus);
		GUILayout.Label("Test result : " + NetworkUtils.ConnectionTester.testMessage);
		GUILayout.Label(NetworkUtils.ConnectionTester.shouldEnableNatMessage);
		GUILayout.EndVertical();
		//GUILayout.EndArea();
		if ( !NetworkUtils.ConnectionTester.isDoneTesting() )
			NetworkUtils.ConnectionTester.TestConnection( SERVER_LISTEN_PORT, false ) ;	
	}
	
	void OnPlayerConnected(NetworkPlayer player)
	{
	    DebugConsole.Log("Player " + player + " connected from " + player.ipAddress + ":" + player.port);
		DebugConsole.Log("viewID: " + networkView.viewID);
		DebugConsole.Log("group: " + networkView.group);
		//networkView.RPC("RPCTest", player, networkView.viewID, "Hello world");
		GameStateServer.Instance.PlayerConnected( player );
	}
	
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		DebugConsole.LogWarning("The following player disconnected, cleaning up: " + player);
	    GameStateServer.Instance.PlayerDisconnected( player );
		Network.RemoveRPCs(player);
	    Network.DestroyPlayerObjects(player);		
	}

	void OnServerInitialized()
	{
	    DebugConsole.Log("NetworkServer.OnServerInitialized()");
		DebugConsole.Log("network: " + Network.peerType);
		MasterServer.RegisterHost( SERVER_GAMETYPENAME, SERVER_GAMENAME, SERVER_GAMECOMMENT );
		Debug.Log( "Game server registered with master server" );
	}
	
	void OnFailedToConnectToMasterServer( NetworkConnectionError info ) 
	{
    	DebugConsole.Log("Game Server Could not connect to master server: "+ info);
	}
	
	//http://stackoverflow.com/questions/1678555/password-encryption-decryption-code-in-net
	//http://answers.unity3d.com/questions/46752/unity-3-sending-email-with-c.html
	//http://answers.unity3d.com/questions/433283/how-to-send-email-with-c.html
	void SendEmailToAdmin()
	{
	}
	
	void OnMasterServerEvent( MasterServerEvent mse )
	{
		switch (mse) {
	    case MasterServerEvent.HostListReceived:
			DebugConsole.Log( "NetworkServer.OnMasterServerEvent( mse ): HostListReceived" );
			break;
		case MasterServerEvent.RegistrationFailedGameName:
			DebugConsole.LogError( "NetworkServer.OnMasterServerEvent( mse ): RegistrationFailedGameName" );
			break;
		case MasterServerEvent.RegistrationFailedGameType:
			DebugConsole.LogError( "NetworkServer.OnMasterServerEvent( mse ): RegistrationFailedGameType" );
			break;
		case MasterServerEvent.RegistrationFailedNoServer:
			DebugConsole.LogError( "NetworkServer.OnMasterServerEvent( mse ): RegistrationFailedNoServer" );
			break;
		case MasterServerEvent.RegistrationSucceeded:
			DebugConsole.Log( "NetworkServer.OnMasterServerEvent( mse ): RegistrationSucceeded" );
			//NetworkViewID viewID = Network.AllocateViewID();
			//DebugConsole.Log( "ViewID:" + networkView.viewID );
			//DebugConsole.Log( "group:" + networkView.group );
			//DebugConsole.Log("Sending spawnbox from server.");
			//networkView.RPC( "SpawnBox", RPCMode.AllBuffered, viewID, transform.position);
			break;
		default:
			break;
		}
	}
	
	void OnNetworkInstantiate (NetworkMessageInfo info) 
	{
	    NetworkView[] networkViews = (NetworkView[])GetComponents(typeof(NetworkView));
	    DebugConsole.Log("New prefab network instantiated with views - ");
	    foreach (NetworkView view in networkViews)
	        DebugConsole.Log("- " + view.viewID);
	}
	
	public void TestConnection()
	{
		DebugConsole.Log("NetworkServer.TestConnection()");
		NetworkUtils.ConnectionTester.TestConnection( SERVER_LISTEN_PORT, true );
		
		//if( GUILayout.Button ("Test Connection") )
		//{
		//	GUILayout.Label("Current Status: " + testStatus);
		//    GUILayout.Label("Test result : " + testMessage);
		//    GUILayout.Label(shouldEnableNatMessage);
		//    if (!doneTesting)
		//        TestConnection();
		//}
	}
	
	//won't actually start server until connection test is done
	public void StartServer()
	{
		//the EASY way, which has drawbacks. //http://docs.unity3d.com/Documentation/Components/net-MasterServer.html
		//server_use_NAT = !Network.HavePublicAddress(); // Use NAT punchthrough if no public IP present
		
		//the HARD way, which can be slower but more robust. //http://docs.unity3d.com/Documentation/ScriptReference/Network.TestConnection.html
		DebugConsole.Log("NetworkServer.StartServer()");
		
		if( NetworkUtils.ConnectionTester.isDoneTesting() )
		{
			SERVER_USE_NAT = NetworkUtils.ConnectionTester.useNat;
			NetworkConnectionError error = Network.InitializeServer( SERVER_MAX_CONNECTIONS, SERVER_LISTEN_PORT, SERVER_USE_NAT);
			if( error != NetworkConnectionError.NoError )
			{//something bad has happened
				Debug.LogError( "Error initializing server: " + error.ToString() );
			}
			//MasterServer.RegisterHost( server_gameTypeName, server_gameName, server_gameComment );
			DebugConsole.Log("Server initialized.");
			
			
		}
		else
		{
			DebugConsole.Log("Not done testing connection.");
		}	
	}
	
	public void StopServer()
	{
		//if (GUILayout.Button ("Disconnect")) {
			DebugConsole.Log("NetworkServer.StopServer(); disconnecting and unregistering host.");
	        Network.Disconnect();
	        MasterServer.UnregisterHost();
	    //}
	}
	
	void OnApplicationQuit()
	{
		StopServer();
	}
#endif
}
