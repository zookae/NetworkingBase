using UnityEngine;
using System.Collections;
//using System.Text;

//http://answers.unity3d.com/questions/145330/network-setup.html
public class NetworkClient : MonoBehaviour {
	
	//public Transform cubePrefab;
	public GameObject target;
	
	public enum MessType_ToClient { 
		  PlayerPairReady=0     //sent to each player in a pair once they've been matched up on the server
		, RelationsList         //sent to client b4 hearts clicking can happen
		, DomainDescription     //sent to client b4 anything can happen
		, DomainObjectsInitial  //sent to client b4 add/remove scene
		, DomainObjectsFinal    //sent to client after server does intersection on the pair player's object lists
		, NewDomainObjPair      //sent to client each "cycle" of swan
		, FirstToSelectRelation //send to the player in a player pair each time they select a relation faster than their mate
		, SwanReadyToMove       //server sends to client when both players have the swan at the end of the screen
		, GameOver              //server sends to client when the game is over
		, RelationMatchResult   //server sends to client to indicate whether the player pair agreed on the previous relation
		, YouWin				//server send to client if the game is over and they have the higher score in the player pair
        , UNTYPED               //a catch-all type 
	};
	public enum MessType_ToServer { 
		  DeviceUniqueIdentifier=0
        , SaveDBStr                 // client sends to server information to be saved to database
        , ReadDBStr                 // client sends to server information to be read from database
		, DomainObjectIDsDeleted    //client sends to server ids of story objects existing in db that client doesn't want
		, DomainObjectIDsNOTDeleted //client sends to server ids of story objects existing in db that client does want
		, DomainObjectNamesAdded    //client sends to server names of story objects (possibly existing in db) that client thinks belongs
		//, DomainObjectsRevised      //client sends to sever after done adding/removing domain objects
		, JustGotToSwanScreen       //client sends to server when done loading the heart/relation selection screen
		, SelectedRelation          //client sends to server when the relation selected for the current object pair has been selected
		, SwanAtEndOfScreen         //client sends to sever after the swan has gone off the right side of the sceen
		, PlayerHasNoLives   		//client sends to server when out of lives
		, PlayerScore	   		    //client sends whenever the swan is at the end of the screen
		, PlayAgain                 //client sends when they want to play again
	};
	
	static NetworkClient _instance;
	private static bool doneStart = false;
	
	
	/// <summary>
	/// Static instance of the NetworkClient.
	///
	/// When you want to access the client without a direct
	/// reference (which you do in mose cases), use NetworkClient.Instance and the required
	/// GameObject initialization will be done for you.
	/// </summary>
	public static NetworkClient Instance {
		get {
		  if (_instance == null) {
		    _instance = FindObjectOfType(typeof(NetworkClient)) as NetworkClient;
		
		    if (_instance != null) {
		      return _instance;
		    }
		
		    GameObject client = new GameObject("__Game Client Controller__");
		    _instance = client.AddComponent<NetworkClient>();
			_instance.Start();
		  }
		
		  return _instance;
		}
	}
	
	//both clients and the server start on the scene "sceneNetworking"; once this object is started, 
	//clients progress to the first real game scene and the server remains on the simple sceneNetworking
	void Start()
	{
        string[] args = System.Environment.GetCommandLineArgs();
        if (!doneStart && System.Array.IndexOf(args, "server") < 0)
		{
			DebugConsole.Log("NetworkClient.Start() start");
			DontDestroyOnLoad( target );
		
			//try to get the web parameters
			if( Application.isWebPlayer )
	        	Application.ExternalEval("var unity = unityObject.getObjectById(\"unityPlayer\");unity.SendMessage(\"" + name + "\", \"ReceiveURL\", document.URL);");
			
			//set up masterserver and facilitator options; if null, then the defaults are used
			if( NetworkServer.MASTERSERVER_HOSTNAME != null )
			{
				NetworkServer.MASTERSERVER = NetworkServer.ServerInfo.FromHostName( NetworkServer.MASTERSERVER_HOSTNAME, NetworkServer.MASTERSERVER_PORT );
				MasterServer.port = NetworkServer.MASTERSERVER.serverPort;
				MasterServer.ipAddress = NetworkServer.MASTERSERVER.serverIP;
			}
			else//use Unity defaults
				NetworkServer.MASTERSERVER = new NetworkServer.ServerInfo( MasterServer.ipAddress, MasterServer.port );
			
			if( NetworkServer.FACILITATOR_HOSTNAME != null )
			{
				NetworkServer.FACILITATOR = NetworkServer.ServerInfo.FromHostName( NetworkServer.FACILITATOR_HOSTNAME, NetworkServer.FACILITATOR_PORT );
				Network.natFacilitatorIP = NetworkServer.FACILITATOR.serverIP;
				Network.natFacilitatorPort = NetworkServer.FACILITATOR.serverPort;
			}
			else//use Unity defaults
				NetworkServer.FACILITATOR = new NetworkServer.ServerInfo( Network.natFacilitatorIP, Network.natFacilitatorPort );
			
		    MasterServer.RequestHostList( NetworkServer.SERVER_GAMETYPENAME );

            //don't load the next screen unless we're the client
            Application.LoadLevel(1);
            DebugConsole.Log("NetworkClient.Start() ... am client");

		    //DebugConsole.IsOpen = true;
//#if UNITY_WEBPLAYER		    
//            if( Application.isWebPlayer )
//            {
//            }
//            Application.LoadLevel(1);			
//#else
//#endif
			DebugConsole.Log("NetworkClient.Start() done");
		}
	}
	 
	
	//URL options
	//http://forum.unity3d.com/threads/4552-unityweb-access-url-parameters
	//http://answers.unity3d.com/questions/44853/pass-initialization-arguments-to-the-web-player.html
	//http://forum.unity3d.com/threads/85469-WebPlayer-get-url-parameters-into-Unity
	
	//CACHE problems
	//http://answers.unity3d.com/questions/37107/turn-off-invalidate-browser-cache.html
	//http://stackoverflow.com/questions/9114622/how-to-clear-cache-of-www-unity3d
	
    public string fullUrl; //from http://pastebin.com/QSf8bi2M
    public void ReceiveURL(string url) {
        // this will include the full URL, including url parameters etc.
        fullUrl = url;
    }	
	
	void OnConnectedToServer() 
	{
	    DebugConsole.Log("Connected to server");
	    DebugConsole.Log("network: " + Network.peerType);
		GameStateClient.Instance.isConnected = true;
		
		SendServerMess( MessType_ToServer.DeviceUniqueIdentifier, SystemInfo.deviceUniqueIdentifier );
	}
	 
	void OnDisconnectedFromServer(NetworkDisconnection info)
	{
	    if (info == NetworkDisconnection.LostConnection)
	       DebugConsole.LogError("Lost connection to the server");
	    else
	       DebugConsole.Log("Successfully disconnected from the server");
        GameStateClient.Instance.isConnected = false; //Network.peerType == NetworkPeerType.Disconnected
	}
	 
	void OnFailedToConnect(NetworkConnectionError error)
	{
	    DebugConsole.LogError("Could not connect to server: " + error);
		GameStateClient.Instance.isConnected = false;
	}
	 
	void OnFailedToConnectToMasterServer(NetworkConnectionError info)
	{
	    DebugConsole.LogError("Game Client Could not connect to master server: " + info);
		GameStateClient.Instance.isConnected = false;
	}
	 
	public void RefreshHostList()
	{
		MasterServer.ClearHostList();
	    MasterServer.RequestHostList( NetworkServer.SERVER_GAMETYPENAME );
	}
	
	public void Disconnect()
	{
		Network.Disconnect();
	}
	
	//returns false if couldn't connect
	public bool ConnectToFirstAvailable()
	{
		bool successful = false;
		HostData[] data = MasterServer.PollHostList();
	    // Go through all the hosts in the host list
	    foreach (HostData element in data)
	    {
	       //string name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
	       string hostInfo;
	       hostInfo = "[";
	       foreach (string host in element.ip)
	         hostInfo = hostInfo + host + ":" + element.port + " ";
	       hostInfo = hostInfo + "]";

	         // Connect to HostData struct, internally the correct method is used (GUID when using NAT).
	 
			DebugConsole.Log("Connecting..." + hostInfo);
			NetworkConnectionError error = Network.Connect(element);
			if (error != NetworkConnectionError.NoError)
			{
				DebugConsole.LogError("Failed to connect " + error);
				successful = false;
			}
			else
			{
				DebugConsole.Log("network: " + Network.peerType);
				successful = true;
			}
			DebugConsole.Log("Done attempting to connect");
			
	    }
		return successful;
	}

#if !EIL_PRODUCTION
	void OnGUI()
	{
        if( Network.peerType == NetworkPeerType.Disconnected )
		{
			Rect windowRectClient = new Rect (150, 20, 350, 250);
			windowRectClient = GUILayout.Window(3, windowRectClient, ClientConnectWindow, "Client Connect", GUILayout.Width(350) );
		}
	}
	
	void ClientConnectWindow( int windowID ) 
	{
		//GUILayout.BeginArea( new Rect(150,0,350,250) );
	    if (GUILayout.Button("Refresh Host List"))
	    {
	       MasterServer.ClearHostList();
           MasterServer.RequestHostList(NetworkServer.SERVER_GAMETYPENAME);
	    }
	    HostData[] data = MasterServer.PollHostList();
	    // Go through all the hosts in the host list
	    foreach (HostData element in data)
	    {
	       GUILayout.BeginHorizontal();  
	       string name = element.gameName + " " + element.connectedPlayers + " / " + element.playerLimit;
	       GUILayout.Label(name);    
	       GUILayout.Space(5);
	       string hostInfo;
	       hostInfo = "[";
	       foreach (string host in element.ip)
	         hostInfo = hostInfo + host + ":" + element.port + " ";
	       hostInfo = hostInfo + "]";
	       GUILayout.Label(hostInfo);    
	       GUILayout.Space(5);
	       GUILayout.Label(element.comment);
	       GUILayout.Space(5);
	       GUILayout.FlexibleSpace();
	       if (GUILayout.Button("Connect"))
	       {
	         // Connect to HostData struct, internally the correct method is used (GUID when using NAT).
	 
	         DebugConsole.Log("Connecting..." + hostInfo);
	         NetworkConnectionError error = Network.Connect(element);
	         if (error != NetworkConnectionError.NoError)
	         {
	          DebugConsole.LogError("Failed to connect " + error);
	         }
	         else
	         {
	          DebugConsole.Log("network: " + Network.peerType);
	         }
	       }
	       if (GUILayout.Button("Disconnect"))
	       {
	         // Connect to HostData struct, internally the correct method is used (GUID when using NAT).
	         Network.Disconnect();
	         DebugConsole.Log("network: " + Network.peerType);
	       }
	 
	       GUILayout.EndHorizontal();    
	    }
	 
	    if (GUILayout.Button("Send \"Hello World\""))
	    {
			//DebugConsole.Log("Views - ");
			//NetworkView[] networkViews = (NetworkView[])GetComponents(typeof(NetworkView));
	    	//foreach (NetworkView view in networkViews)
		    //    DebugConsole.Log("- " + view.viewID);
			DebugConsole.Log("GOO ");
			if( networkView == null )
			{ 
				DebugConsole.Log("networkView == null");
			}
			else
			{
				DebugConsole.Log("networkView != null" + networkView);
				
				DebugConsole.Log( "Sending messages." );
				DebugConsole.Log("networkView.viewID != null" + networkView.viewID);
				networkView.RPC("RPCTest", RPCMode.All, networkView.viewID, "Hello world");
				//SendServerMess( MessType_ToServer.Join, null );//0
				//SendServerMess( MessType_ToServer.Join, "one two" );//1
				//SendServerMess( MessType_ToServer.Join, "one", "two" );//2
				
			}
			
			if( target.networkView != null )
			{
				DebugConsole.Log("target.networkView != null");
				//target.networkView.RPC("RPCTest", RPCMode.All, target.networkView.viewID, "Hello world");
			}
			else
				DebugConsole.Log("target.networkView == null"+target.networkView);
			

	    }
		//GUILayout.EndArea();
	}
#endif
	
	public static string RayToString( string[] ray, string sep ) //","
	{
		if( ray != null )
		{
			if( ray.Length > 1 ) //more than one element
				return string.Join(sep,ray);
			else if( ray.Length == 1 )
				return ray[0];
			else 
				return "";
		}
		return "";
	}
	
	//from the server
	public void SendClientMess( NetworkPlayer player, NetworkClient.MessType_ToClient messType, params string[] args )
	{
		DebugConsole.LogWarning( "Sending message to player " + player + ": " + messType.ToString() );
		if( args != null && args.Length > 0 ) DebugConsole.Log( "args: " + RayToString(args,",") );
		if( Network.isServer ) 
		{
			//DebugConsole.Log( "SendClientMess( int messType )" );
			if( args != null && args.Length > 0 )
				networkView.RPC("FromServerWString", player, networkView.viewID, (int)messType, RayToString( args,"," ) );
			else
			{
				//DebugConsole.LogWarning( "null or zero length arguments to SendClientMess." );	
				networkView.RPC("FromServerNoArgs", player, networkView.viewID, (int)messType );
			}
			//DebugConsole.LogWarning( "Done sending message to client." );			
		}
		else{
			DebugConsole.LogWarning( "Did not send client mess, because I AM A CLIENT." );
		}
	}

    //assumes we're connected
    public static void SendClientPairMessage(Pair<NetworkPlayer, NetworkPlayer> pr, NetworkClient.MessType_ToClient messType, params string[] args) {
        //TODO call the no arg version if args is empty or null
        if (args == null || args.Length == 0 || args[0] == null || args[0].Length == 0) {
            NetworkClient.Instance.SendClientMess(pr.First, messType);
            NetworkClient.Instance.SendClientMess(pr.Second, messType);
        } else {
            NetworkClient.Instance.SendClientMess(pr.First, messType, args);
            NetworkClient.Instance.SendClientMess(pr.Second, messType, args);
        }
    }

	//from the client
	public void SendServerMess( NetworkClient.MessType_ToServer messType, string args )
	//public void SendServerMess( NetworkClient.MessType messType, string args )
	{
		DebugConsole.LogWarning( "Sending message to server: " + messType.ToString() );
		if( args != null && args.Length > 0 ) DebugConsole.Log( "args: " + args );
		
		if( Network.isClient ) 
		{
			
			//DebugConsole.LogWarning("network: " + Network.peerType);

			if( args != null && args.Length > 0 )
			{	
				//DebugConsole.Log( "  args - [" + string.Join(",",args) + "]" );
				//networkView.RPC("ToServerWString", RPCMode.Server, networkView.viewID, messType, string.Join(",",args) );
				networkView.RPC("ToServerWString", RPCMode.Server, networkView.viewID, (int)messType, args );
			}
			else
			{
				//DebugConsole.LogWarning( "null or zero length arguments to SendServerMess." );	
				networkView.RPC("ToServerNoArgs", RPCMode.Server, networkView.viewID, (int)messType );
			}
			//DebugConsole.LogWarning( "Done sending message to server." );			
		}
		else{
			DebugConsole.LogError( "Did not send server mess, because I AM THE SERVER." );
		}
	}

    /*
     * The only params that can be sent over rpc:
     * int, float, string, NetworkPlayer, NetworkViewID, Vector3, Quaternion
SEE ALSO http://answers.unity3d.com/questions/318593/using-rpc-to-send-a-list.html
     */

    [RPC]
	void ToServerNoArgs( NetworkViewID viewID , int messType, NetworkMessageInfo nmi )
	{
		//DebugConsole.LogWarning( "Called the no param version of the rpc." );
		this.ToServerWString( viewID, messType, "", nmi );
	}
	
	[RPC]
	void FromServerNoArgs( NetworkViewID viewID , int messType )//, NetworkMessageInfo nmi
	{
		//DebugConsole.LogWarning( "Called the no param version of the rpc." );
		this.FromServerWString( viewID, messType, "" );
	}
	
	[RPC]
	void ToServerWString( NetworkViewID viewID , int messType, string args , NetworkMessageInfo nmi )
	{
#if UNITY_WEBPLAYER
#else
		//DebugConsole.LogWarning( "Called the string param version of the rpc." );
		DebugConsole.LogWarning( "Message from client: " + nmi.sender );
        DebugConsole.LogWarning("Type: " + ((NetworkClient.MessType_ToServer)messType).ToString() + ". Args: " + args);
		if( Network.isClient ) DebugConsole.LogError( "Oh noz, ToServer invoked on the client." );
		//if( Network.isServer ) DebugConsole.LogWarning( "ToServer invoked on server." );
	
		//string[] rayArgs = null;
		if( args == null )
            DebugConsole.LogWarning("ToServerWString had null string arguments.");
		
		NetworkClient.MessType_ToServer castMess = (NetworkClient.MessType_ToServer) messType;
		
		GameStateServer.Instance.MessageFromClient( nmi.sender, castMess, args );
#endif
	}
	
	[RPC]
	void FromServerWString( NetworkViewID viewID , int messType, string args )//, NetworkMessageInfo nmi
	{
		//DebugConsole.LogWarning( "Called the string param version of the rpc." );
		if( Network.isServer ) DebugConsole.LogError( "Oh noz, FromServer invoked on the server (bad)." );
		//if( Network.isClient ) DebugConsole.LogWarning( "FromServer invoked on client (good)." );
	
		string[] rayArgs = null;
		if( args == null )
			DebugConsole.LogWarning( "FromServer had null string arguments." );
		else
		{
			//DebugConsole.Log( "args - " + args );
			rayArgs = args.Split( new char[] {','} );
		}
		
		NetworkClient.MessType_ToClient castMess = (NetworkClient.MessType_ToClient) messType;

		GameStateClient.Instance.MessageFromServer( castMess, rayArgs );
	}
	
	//networkView.RPC("RPCTest", RPCMode.All, networkView.viewID, "Hello world");
	[RPC]
	void RPCTest( NetworkViewID viewID , string mess, NetworkMessageInfo nmi )
	{
		DebugConsole.Log( "viewID " + viewID );
		DebugConsole.Log( "Client: RPCTest! " + mess );
		DebugConsole.Log( "NetworkMessageInfo: " + nmi );
		Debug.Log( "Client: RPCTest!" + mess );
	}

    public void SendAllSpawnBox( Vector3 location, NetworkViewID nvid ) {
        DebugConsole.Log("SendAllSpawnBox called with viewID: " + nvid);
        networkView.RPC("SpawnBox", RPCMode.AllBuffered, nvid, location );
        DebugConsole.Log("SendAllSpawnBox done");
    }

    public void SendAllSpawnBoxSync(Vector3 location, NetworkViewID nvid) {
        DebugConsole.Log("SendAllSpawnBoxSync called with viewID: " + nvid);
        networkView.RPC("SpawnBoxSync", RPCMode.AllBuffered, nvid, location);
        DebugConsole.Log("SendAllSpawnBoxSync done");
    }

    public void SendAllScoreSync(Vector3 location, NetworkViewID nvid) {
        DebugConsole.Log("SendAllScoreSync called with viewID: " + nvid);
        networkView.RPC("ScoreSync", RPCMode.AllBuffered, nvid, location);
        DebugConsole.Log("SendAllScoreSync done");
    }
	
	//http://docs.unity3d.com/Documentation/ScriptReference/Network.AllocateViewID.html?from=NetworkView
	[RPC]
	void SpawnBox(NetworkViewID viewID, Vector3 location) 
	{
        DebugConsole.Log("NetworkClient.[RPC]SpawnBox called.");

	    // Instantate the prefab locally        
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = location;
        go.transform.rotation = Quaternion.identity;

        go.AddComponent<NetworkView>(); // attach network view to object - must be present to set viewID
        go.GetComponent<NetworkView>().viewID = viewID;
        NetworkViewID vID = go.GetComponent<NetworkView>().viewID;
        DebugConsole.Log("SpawnBox called with viewID : " + vID);

        go.AddComponent<Rigidbody>();
        go.GetComponent<Rigidbody>().useGravity = false;

        go.AddComponent<NetworkMouseDrag>();
        
		DebugConsole.Log( "SpawnBox RPC done." );
	}

    //http://www.palladiumgames.net/tutorials/unity-networking-tutorial/
    /// <summary>
    /// Spawn a simple cube with the associated network viewID at a given location in the world.
    /// Synchronize state using Unity's statesynchronization (requires an owner)
    /// </summary>
    /// <param name="viewID"></param>
    /// <param name="location"></param>
    [RPC]
    public void SpawnBoxSync(NetworkViewID viewID, Vector3 location) {
        DebugConsole.Log("NetworkClient.SpawnBoxSync called.");

        // Instantate the prefab locally        
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = location;
        go.transform.rotation = Quaternion.identity;

        go.AddComponent<NetworkView>(); // attach network view to object - must be present to set viewID
        go.GetComponent<NetworkView>().viewID = viewID;
        go.GetComponent<NetworkView>().stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed; // use state sync
        go.GetComponent<NetworkView>().observed = go.transform; // follow transform
        NetworkViewID vID = go.GetComponent<NetworkView>().viewID;
        DebugConsole.Log("SpawnBoxSync called with viewID : " + vID);

        go.AddComponent<Rigidbody>();
        go.GetComponent<Rigidbody>().useGravity = false;

        go.AddComponent<MouseDrag>();
        DebugConsole.Log("SpawnBoxSync RPC done.");
    }

    [RPC]
    public void ScoreSync(NetworkViewID viewID, Vector3 location) {
        DebugConsole.Log("NetworkClient.ScoreSync called.");

        // Instantate the prefab locally        
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.transform.position = location;
        go.transform.rotation = Quaternion.identity;

        go.AddComponent<NetworkView>(); // attach network view to object - must be present to set viewID
        go.GetComponent<NetworkView>().viewID = viewID;
        go.GetComponent<NetworkView>().stateSynchronization = NetworkStateSynchronization.ReliableDeltaCompressed; // use state sync
        
        NetworkViewID vID = go.GetComponent<NetworkView>().viewID;
        DebugConsole.Log("ScoreSync called with viewID : " + vID);

        go.AddComponent<Rigidbody>();
        go.GetComponent<Rigidbody>().useGravity = false;

        go.AddComponent<NetworkScore>();
        go.GetComponent<NetworkView>().observed = go.GetComponent<NetworkScore>(); // follow script
        DebugConsole.Log("ScoreSync RPC done.");
    }
}
