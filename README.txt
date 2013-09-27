How to use:
  Must have "sceneNetworking", can be named anything, but must be 0th scene   
  Must attach "networkClient.cs" and "networkServer.cs" and "DBSaveString.cs" to the networkCubePrefab
  Must add a "Network View" to the networkCubePrefab (click on the cube, then "component" menu -> misc -> network view)
    //as an aside, you can turn "state synchronization" on, which would allow for sharing network views of objects (auto synch obj movements, etc)
  MUST set "run in background" to true (checked) so that the server ... runs in the back ground (without focus)
  both client and server have their "Start()" run once; they set "DontDestroyOnLoad" so you can happily have one istance
  both client and server have a "Instance", which you use to get the singleton, and invoke methods
  any extras, you may want to throw into Start()
  when running server, it expects "server" as a command line arg; otherwise, it assumes it is a client, and loads scene 1

  when doing WEBPLAYER build, it will NOT compile while Assets/Plugins exists (because of dlls etc); rename or move folder to build
  it is essential that API compatibility level be set to .NET 2.0 (NOT subset) for db stuff to compile [file, build settings, player settings, "other settings"]
  You can access the Network Manager by selecting Edit->Project Settings->Network from the menu bar; increase the debug level to dig into networking issues

NetworkServer.cs
  masterserver ip/port; 
  facilitator ip/port
  game type name; game name; game comment
	public static string server_gameTypeName = "INCLabHeartESPGame_v1.1";
	public static string server_gameName = "Inc lab spatial relation esp game.";
  does some command line parsing for masterserver/facilitator
  use the following to change at compile time the masterserver and facilitator ip/port
	public static ServerInfo MASTERSERVER = MASTERSERVER_DOROTHY;
	public static ServerInfo FACILITATOR = FACILITATOR_DOROTHY;
  can only send primitive objects, String, or String[]; consider protobuf

NetworkClient.cs
  defines the message types to/from server (customize for your app)
	public enum MessType_ToClient
	public enum MessType_ToServer
  has handy OnDisconnect, OnConnect, etc
  has low level send message to client, send message to server
  
NetworkUtils.cs
  handy network testing, etc

==========================================================
all above was mostly game independent
below starts mostly game specific
==========================================================

GameStateServer
  MessageFromClient -- where messages bubble up; switch on message type
  MessageFromClient( NetworkPlayer player, NetworkClient.MessType_ToServer messType, string[] args )
	note -- you may find getRunningGameData(...) interesting and reusable

GameStateClient
  public void MessageFromServer( NetworkClient.MessType_ToClient messType, string[] args )


=============================================================
example use (from a client side js file):
=============================================================

if( GameStateClient.Instance.isConnected )
{
		NetworkClient.Instance.SendServerMess( NetworkClient.MessType_ToServer.JustGotToSwanScreen, (GameStateClient.Instance.playerScore+((1+playerLives)*GameStateClient.PLAYER_LIFE_WORTH)).ToString() );  

}

=============================================================
example use (from a server side cs file):
=============================================================

NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.GameOver, "Server quit." );