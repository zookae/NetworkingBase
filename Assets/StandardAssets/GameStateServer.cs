using UnityEngine;
#if UNITY_WEBPLAYER
#else
using System.Collections;
using System.Collections.Generic;
using System;
#endif

public class GameStateServer : MonoBehaviour 
{
	#if UNITY_WEBPLAYER
	
	#else

	static GameStateServer _instance;
	private static bool doneStart = false;
	public static System.Random rand = new System.Random();
	
	// use Network.connections to see all connections
	private List<NetworkPlayer> playersWaitingToGo = new List<NetworkPlayer>();
	internal Dictionary<NetworkPlayer,RunningGameData> dPlayerToGamedata = new Dictionary<NetworkPlayer,RunningGameData>();
	
	//database stuff
	// This is the file path of the database file we want to use
	// Right now, it'll load espDB.sqlite3 in the project's root folder.
	// If one doesn't exist, it will be automatically created.
	public static String databaseName = "db.sqlite3";
    private string[] RAY_TABLE_NAMES = new string[] { "player", "topscore", "confidenceLookupMC", "savedata", "game", "gamedetail" };

    internal DBManipulation dbManip = new DBManipulation( databaseName, false, false );
	internal ValidationStrategy valStrat = null; //initialized in Start() so that we can use the momoized (database) version intelligently/safely
	
	/// <summary>
	/// Static instance of the GameStateServer.
	///
	/// When you want to access the client without a direct
	/// reference (which you do in mose cases), use GameStateServer.Instance and the required
	/// GameObject initialization will be done for you.
	/// </summary>
	public static GameStateServer Instance {
		get {
		  if (_instance == null) {
		    _instance = FindObjectOfType(typeof(GameStateServer)) as GameStateServer;
		
		    if (_instance != null) {
		      return _instance;
		    }
		
		    GameObject client = new GameObject("__GameStateServer__");
		    _instance = client.AddComponent<GameStateServer>();
			_instance.Start();
			DontDestroyOnLoad( _instance );
			DebugConsole.Log( "GameStateServer instance created." );
		  }
		
		  return _instance;
		}
	}
	
	//not used? or is it auto called by MonoBehavior?
	public static void Init()
	{
		GameStateServer gss = Instance;
	}
	
	//returns null only in error
	//NOTE you may want to put your "decide which game to give the player" logic here (in the case that the dictionary doesn't have a mapping for this player -> game)
	private RunningGameData getRunningGameData( NetworkPlayer player, NetworkClient.MessType_ToServer messType, string args )
	{
		RunningGameData rgd = null;
		if( dPlayerToGamedata.ContainsKey( player ) )
		{//they are in the dictionary
			rgd = dPlayerToGamedata[ player ];
		}
		else// if( messType != NetworkClient.MessType_ToServer.PlayAgain && SINGLEPLAYER_ONLY )
		{//not in dictionary dPlayerToGamedata, and not a play again message; assume 1 player?
			DebugConsole.LogError( "non-existant player (not in dPlayerToGamedata). Assuming 1 player" );
			
			int tmpI = playersWaitingToGo.IndexOf( player );
		    if( tmpI >= 0 ) //disconnected from wait room.
			{
				//remove from wait room
				playersWaitingToGo.RemoveAt( tmpI ); 
				
				//make single player data
				RunningGameData_1player rgd1p = new RunningGameData_1player( this, -1, player );
                rgd1p.dPlayerData.Add(player, new PlayerData()); 
				rgd = rgd1p;
				this.dPlayerToGamedata.Add( player, rgd1p );

//					NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.DomainDescription, ansPair.Value.ToString() );
//					NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.PlayerPairReady );
			}else{//PLAY AGAIN?
				DebugConsole.LogError( "player was not in dPlayerToGamedata nor playersWaitingToGo, so we're ignoring them" );
//				//this player was not in the wait room, but also not in dPlayerToGamedata. odd?
//				return null;
			}
			
		}
		return rgd; //always returns null, unless we already have a mapping from player to RunningGameData
	}
	
	//THIS IS THE KEY METHOD OF THIS CLASS
	public void MessageFromClient( NetworkPlayer player, NetworkClient.MessType_ToServer messType, string args )
	{
		DebugConsole.LogWarning( "GameStateServer.MessageFromClient " + player + ": " + messType.ToString() );
		DebugConsole.Log( "args: " );
		if( args == null || args.Length == 0 )
			DebugConsole.Log( "<null or zero length>" );
		else
            DebugConsole.Log( args );
		
		//if( args != null && args.Length > 0 ) DebugConsole.Log( NetworkClient.RayToString( args ) );
		RunningGameData rgd = getRunningGameData( player, messType, args );		
		if( rgd == null )
			return; //this is ok.			
		
		DebugConsole.Log( "Hitting big message demux." );
		
		switch( messType )
		{
            //TODO case NetworkClient.MessType_ToServer.ReadDBStr:  "udid" arg
            case NetworkClient.MessType_ToServer.ReadDBStr:
                if( String.Compare( "udid", args, true ) == 0 )
                    NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.UNTYPED, dbManip.getPlayerUDID( rgd.dPlayerData[ player ].playerid ) ); 
                break;
            case NetworkClient.MessType_ToServer.SaveDBStr:
                DebugConsole.Log("client requested save string : " + args);
                dbManip.dataDump(rgd.gameID, rgd.dPlayerData[player].playerid, args);
                DebugConsole.Log("dumped string info : " + rgd.gameID + ", " + rgd.dPlayerData[player].playerid + ", " + args);
                break;
		case NetworkClient.MessType_ToServer.DomainObjectIDsDeleted:
//			rgd.Mess_DomainObjectIDsDeleted( player, args );
//			rgd.SendFinalListAndFirstPairWhenReady( player );
			break;
			
		case NetworkClient.MessType_ToServer.DomainObjectIDsNOTDeleted:
//			rgd.Mess_DomainObjectIDsNOTDeleted( player, args );
//			rgd.SendFinalListAndFirstPairWhenReady( player );
			break;
			
		case NetworkClient.MessType_ToServer.DomainObjectNamesAdded:
//			rgd.Mess_DomainObjectNamesAdded( player, args );
//			rgd.SendFinalListAndFirstPairWhenReady( player );
			break;
			
		case NetworkClient.MessType_ToServer.JustGotToSwanScreen: //has playerScore arg; for when we're not doing add/remove objects screen
//			rgd.Mess_JustGotToSwanScreen( player, args );
			break;
			
		case NetworkClient.MessType_ToServer.SelectedRelation:
			//DebugConsole.Log( "MessageFromClient: MessType.SelectedRelation" );
//			rgd.Mess_SelectedRelation( player, args );
			break;
			
		case NetworkClient.MessType_ToServer.SwanAtEndOfScreen: //has playerScore, millisec args
//			rgd.Mess_SwanAtEndOfScreen( player, args );
			break;
			
		case NetworkClient.MessType_ToServer.PlayerHasNoLives: //has playerScore arg
//			rgd.Mess_PlayerHasNoLives( player, args );
			break;
			
		case NetworkClient.MessType_ToServer.PlayAgain: //HANDLED IN getRunningGameData
			//PlayerConnected( player );
//			NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.RelationsList, strListRelations );
			
			//select, send the domain
//			System.Collections.Generic.KeyValuePair<int,System.Text.StringBuilder> ansPair = rgd.SelectDomainid();
//			if( ansPair.Equals( default(KeyValuePair<int,System.Text.StringBuilder>) ) )
//			{	//no more domains to work on! 
//				rgd.EndGame( "No more domains to play!", null );
//			}else{ 
//				rgd.ResetVarsForNewGame();
//				rgd.domainID = ansPair.Key;
//				NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.DomainDescription, ansPair.Value.ToString() );
//				NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.PlayerPairReady );
//			}
			break;
		case NetworkClient.MessType_ToServer.DeviceUniqueIdentifier:  //may be HANDLED IN getRunningGameData
			rgd.SetUniqueDeviceID( player, args ); //comes from NetworkClient.OnConnectedToServer
            DebugConsole.Log( "Got a UDID of: " + args );
			break;
		default:
			DebugConsole.Log( "MessageFromClient: default" );
			break;
		}
	}
	
	//always sends relation list; handles wait room && pair up logic
	//autmotatically invoked
	public void PlayerConnected(NetworkPlayer player)
	{
		DebugConsole.LogWarning("Have a single player waiting to begin: player" + player);
		playersWaitingToGo.Add( player );
//			NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.RelationsList, strListRelations );
	}
	
	public void PlayerDisconnected(NetworkPlayer player)
	{
		//clean up waiting pool
		int tmpI = playersWaitingToGo.IndexOf( player );

		if( tmpI >= 0 ) //disconnected from wait room.
			playersWaitingToGo.RemoveAt( tmpI );
		else
		{//disconnected from active game.
			//clean up active pool
			if( dPlayerToGamedata.ContainsKey( player ) )
			{
				RunningGameData rgd = dPlayerToGamedata[ player ];
				rgd.PlayerDisconnected( player );
			}else{
				Debug.LogWarning( "Player disconnected, however they were not in the waiting pool nor do I have running game data for them." );
			}
		}
		DebugConsole.LogWarning("Number players in waiting: " + playersWaitingToGo.Count );
	}
	
	//triggered when you close the server (any way you quit, besides kill -9)
	void OnApplicationQuit()
	{

        dbManip.OnApplicationQuit();
		while( playersWaitingToGo.Count > 0 )
		{
			NetworkPlayer player = playersWaitingToGo[ playersWaitingToGo.Count - 1 ];
			playersWaitingToGo.RemoveAt( playersWaitingToGo.Count - 1 );
			NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.GameOver, "Server quit." );
		}
		
		NetworkPlayer[] tmpRay = new NetworkPlayer[ dPlayerToGamedata.Keys.Count ];
		dPlayerToGamedata.Keys.CopyTo( tmpRay, 0 );
		
		foreach( var key in tmpRay )
		{
			dPlayerToGamedata.Remove( key );
			NetworkClient.Instance.SendClientMess( key, NetworkClient.MessType_ToClient.GameOver, "Server quit." );
		}
	}
	
	
	//TODO we could make the list of tableNames to check for be an array, so that this method is a bit more "agnostic" to implementation
	void Start()
	{
		if( !doneStart )
		{//put init stuff here
			DebugConsole.Log("GameStateServer.Start()");
            
			dbManip.TryEnableForeignKeys(); //VERY IMPORTANT to ensure foreign keys are enforced
			
			if( dbManip.VerifyTableExistence( RAY_TABLE_NAMES ) ) 
			{
				//choose which Validation strategy you want to use
				valStrat = new ValidationStrategyMemoized( dbManip );
			}else{ //probably don't have a propperly set up db. 
				//TODO give them an easy way to set one up.
				DebugConsole.LogError( "Did not find expected table(s) in db. Make sure it is set up!" );
			}
			
			doneStart = true;
		}
	}	
#endif
	
}
