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
	
	//static readonly 
//	public const bool SINGLEPLAYER_ONLY = true;
//	public const double MINIMUM_CONFIDENCE_RELATION = 0.85;
//	public const int MAXIMUM_ANSWERS_BEFORE_FAIL = 10;
	
	static GameStateServer _instance;
	private static bool doneStart = false;
	public static System.Random rand = new System.Random();
	
	// use Network.connections to see all connections
	private List<NetworkPlayer> playersWaitingToGo = new List<NetworkPlayer>();
	//private Dictionary<NetworkPlayer,Pair<NetworkPlayer, NetworkPlayer>> dPlayerToPlayerpair = new Dictionary<NetworkPlayer, Pair<NetworkPlayer, NetworkPlayer>>();
	//private Dictionary<Pair<NetworkPlayer, NetworkPlayer>,RunningGameData> dPlayerpairToGamedata = new Dictionary<Pair<NetworkPlayer, NetworkPlayer>,RunningGameData>();
	internal Dictionary<NetworkPlayer,RunningGameData> dPlayerToGamedata = new Dictionary<NetworkPlayer,RunningGameData>();
	
	//required technique before we were tracking playerid's
	//the following is only to be used for "play again" behavior
	//internal Dictionary<NetworkPlayer,System.Text.StringBuilder /*sbListSeenObjpairid*/> dPlayerToSeenObjpairid = new Dictionary<NetworkPlayer, System.Text.StringBuilder>();
	
//	internal Dictionary<int/*relationid*/,int/*relationClass*/> drelationidTOrelationClass = new Dictionary<int,int>();
//	internal HashSet<int> uniqueRelationClasses = null; //created after drelationidTOrelationClass is populated
	
//	internal HashSet<int> hsObjpairidInPlay = new HashSet<int>();
	
	//database stuff
	// This is the file path of the database file we want to use
	// Right now, it'll load espDB.sqlite3 in the project's root folder.
	// If one doesn't exist, it will be automatically created.
	public static String databaseName = "db.sqlite3";
 
	// This is the name of the table we want to use
	//public String tableName = "TestTable";

	internal DBAccess db = new DBAccess();
	internal ValidationStrategy valStrat = null; //initialized in Start() so that we can use the momoized (database) version intelligently/safely
	
//	private static string[] DEFAULT_DOMAIN_OBJECTS = { "D.O._1", "D.O._2", "D.O._3", "D.O._4", "D.O._5", "D.O._6", "D.O._7"};
	//private static string DEFAULT_NORELATION = "-1";
	
//	private string strListRelations = null; //the string to send to clients that lists relationid, relation name for all relations
	
	//don't use; instead use the two atomic operations
	/*
	private int getOrCreatePlayerID( string udid )
	{
		Debug.Log( "getOrCreatePlayerID" );
		int playerid = -1;
		System.Text.StringBuilder sbSQLSelect = new System.Text.StringBuilder();
		
		sbSQLSelect.Append( "SELECT playerid FROM player WHERE udid=" ).Append( udid );

		System.Data.IDataReader res = db.BasicQuery( sbSQLSelect.ToString() ); 
		
		if( res.Read() )
		{//player already exists; return the id
			playerid = res.GetInt32( 0 ); //WELCOME BACK!
		}else{//else we don't have a player id and need to get one		
			System.Text.StringBuilder sbSQLInsert = new System.Text.StringBuilder();
			sbSQLInsert.Append( "INSERT OR IGNORE INTO player (udid) VALUES ( '" ).Append( udid ).Append( "' )" );
			db.BasicQuery( sbSQLInsert.ToString() ); //inserts
			
			res = db.BasicQuery( sbSQLSelect.ToString() ); //get the new playerid
			if( res.Read() )
			{//got it, as expected
				playerid = res.GetInt32( 0 ); //FIRST TIME PLAYER
			}else{
				Debug.LogError( "Failed to get playerid for udid just inserted. Very very odd, and evil." );
			}
		}
		return playerid;
	}
	*/
	
	//returns -1 if the player doesn't exist
	internal int getPlayerID( string udid )
	{
		Debug.Log( "getPlayerID" );
		int playerid = -1;
		System.Text.StringBuilder sbSQLSelect = new System.Text.StringBuilder();
		
		sbSQLSelect.Append( "SELECT playerid FROM player WHERE udid='" ).Append( udid ).Append("'");

		System.Data.IDataReader res = db.BasicQuery( sbSQLSelect.ToString() ); 
		
		if( res.Read() )
		{//player already exists; return the id
			playerid = res.GetInt32( 0 ); //WELCOME BACK!
		}//else we don't have a player id and need to get one (return -1)
		return playerid;
	}
	
	internal int createPlayerID( string udid )
	{
		Debug.Log( "createPlayerID" );
		int playerid = -1;
		System.Text.StringBuilder sbSQLInsert = new System.Text.StringBuilder();
		sbSQLInsert.Append( "INSERT OR IGNORE INTO player (udid) VALUES ( '" ).Append( udid ).Append( "' )" );
		db.BasicQuery( sbSQLInsert.ToString() ); //inserts
		
		System.Text.StringBuilder sbSQLSelect = new System.Text.StringBuilder();		
		sbSQLSelect.Append( "SELECT playerid FROM player WHERE udid='" ).Append( udid ).Append("'");
		
		System.Data.IDataReader res = db.BasicQuery( sbSQLSelect.ToString() ); //get the new playerid
		if( res.Read() )
		{//got it, as expected
			playerid = res.GetInt32( 0 ); //FIRST TIME PLAYER
		}else{
			Debug.LogError( "Failed to get playerid for udid just inserted. Very very odd, and evil." );
		}
		return playerid;
	}

	
	//public enum StateEnum { Unspecified=0, Joining, PlayerPairReady, LocalRelationChosen };
	//public StateEnum currState = StateEnum.Unspecified;
	
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
	private RunningGameData getRunningGameData( NetworkPlayer player, NetworkClient.MessType_ToServer messType, string[] args )
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
				RunningGameData_1player rgd1p = new RunningGameData_1player( this, player ); 
				rgd = rgd1p;
				this.dPlayerToGamedata.Add( player, rgd1p );

//				if( messType == NetworkClient.MessType_ToServer.DeviceUniqueIdentifier )
//				{
//					rgd.SetUniqueDeviceID( player, args[0] ); //comes from NetworkClient.OnConnectedToServer
//				}
//				
//				//send relations
//				//NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.RelationsList, strListRelations );			
//				
//				//select, send the domain
//				System.Collections.Generic.KeyValuePair<int,System.Text.StringBuilder> ansPair = rgd.SelectDomainid();
//				if( ansPair.Equals( default(KeyValuePair<int,System.Text.StringBuilder>) ) )
//				{	//no more domains to work on! 
//					rgd.EndGame( "No more domains to play!", null );
//				}else{ 
//					rgd.domainID = ansPair.Key;
//					NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.DomainDescription, ansPair.Value.ToString() );
//					NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.PlayerPairReady );
//				}
			}else{//PLAY AGAIN?
//				DebugConsole.LogError( "player was not in dPlayerToGamedata nor playersWaitingToGo, so we're ignoring them" );
//				//this player was not in the wait room, but also not in dPlayerToGamedata. odd?
//				return null;
			}
			
		}
		return rgd; //always returns null, unless we already have a mapping from player to RunningGameData
	}
	
	//THIS IS THE KEY METHOD OF THIS CLASS
	public void MessageFromClient( NetworkPlayer player, NetworkClient.MessType_ToServer messType, string[] args )
	{
		DebugConsole.LogWarning( "GameStateServer.MessageFromClient " + player + ": " + messType.ToString() );
		DebugConsole.Log( "args: " );
		if( args == null || args.Length == 0 )
			DebugConsole.Log( "<null or zero length>" );
		else
			foreach( string tmpA in args ) 
				if( tmpA == null )
					DebugConsole.Log( "<null>" );
				else if( tmpA.Length == 0 )
					DebugConsole.Log( "\"\"" );
				else
					DebugConsole.Log( tmpA );
		
		//if( args != null && args.Length > 0 ) DebugConsole.Log( NetworkClient.RayToString( args ) );
		RunningGameData rgd = getRunningGameData( player, messType, args );		
		if( rgd == null )
			return; //this is ok.			
		
		DebugConsole.Log( "Hitting big message demux." );
		
		switch( messType )
		{
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
			rgd.SetUniqueDeviceID( player, args[0] ); //comes from NetworkClient.OnConnectedToServer
            DebugConsole.Log( "Got a UDID of: " + args[0] );
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
//		if( SINGLEPLAYER_ONLY )
//		{//we are only doing 1player games
			DebugConsole.LogWarning("Have a single player waiting to begin: player" + player);
			playersWaitingToGo.Add( player );
//			NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.RelationsList, strListRelations );
//		}
//		else
//		{//allow for 2player games
//			if( playersWaitingToGo.Count >= 1 && playersWaitingToGo.Count % 2 != 0 && playersWaitingToGo[ 0 ] != player )
//			{//have a pair ready to go
//				NetworkPlayer p1 = playersWaitingToGo[ 0 ];
//				playersWaitingToGo.RemoveAt( 0 );
//				Pair<NetworkPlayer,NetworkPlayer> pair = new Pair<NetworkPlayer, NetworkPlayer>(p1,player);
//				
//				DebugConsole.LogWarning("Have a pair ready to go for players: " + p1 + " and " + player);
//				
//				RunningGameData_2player rgd2p = new RunningGameData_2player( this, pair );
//				
//				/* required technique before we were tracking playerid's
//				if( this.dPlayerToSeenObjpairid.ContainsKey( player ) )
//					rgd2p.sbListSeenObjpairid.Append( dPlayerToSeenObjpairid[ player ] );
//				if( this.dPlayerToSeenObjpairid.ContainsKey( p1 ) )
//					rgd2p.sbListSeenObjpairid.Append( dPlayerToSeenObjpairid[ p1 ] );
//				*/
//				
//				dPlayerToGamedata.Add( p1, rgd2p );
//				dPlayerToGamedata.Add( player, rgd2p );
//							
//				rgd2p.pairPlayers = pair;
//				rgd2p.dPlayerData[ pair.First ].state = GameStateClient.StateEnum.PlayerPairReady;
//				rgd2p.dPlayerData[ pair.Second ].state = GameStateClient.StateEnum.PlayerPairReady;
//				
//				//relations
//				SendClientPairMessage( pair, NetworkClient.MessType_ToClient.RelationsList, strListRelations );			
//				
//				//TODO choose domainid for the pair
//				//TODO send the pair: (1) domain name, (2) domain description, 
//				System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
//				sbSQL.Append( "SELECT domainid, domainName, domainDescr, numPairsLeft FROM storyDomain WHERE bUse=1 AND bDone=0 LIMIT 1" );
//				System.Data.IDataReader res = db.BasicQuery( sbSQL.ToString() ); 
//				res.Read();
//				
//				int tmpI = 0;
//				int domainid = res.GetInt32( tmpI++ );
//				string domainName = res.GetString( tmpI++ );
//				string domainDescr = res.GetString( tmpI++ );
//				int numPairsLeft = res.GetInt32( tmpI++ );
//	
//				rgd2p.domainID = domainid;
//				System.Text.StringBuilder toSend = new System.Text.StringBuilder();
//				toSend.Append( domainid ).Append( "|||" ).Append( domainName ).Append( "|||" ).Append( domainDescr );
//				SendClientPairMessage( pair, NetworkClient.MessType_ToClient.DomainDescription, toSend.ToString() );
//				
//				
//				//If doing add/remove domain objects, then
//				//SendPlayerPairDomObj( pair );
//				
//				
//				//ready
//				SendClientPairMessage( pair, NetworkClient.MessType_ToClient.PlayerPairReady );
//				
//				//ADDED TO SKIP THE ADD/REMOVE OBJ SCREEN
//				//send clients the first pair, update round number, inform them they're ready to move
//				//rgd.roundNumber = 0;
//				//string nextObjPairString = MakeNextObjPairString( rgd );
//				//if( nextObjPairString.Length > 0 )
//				//{
//				//	SendClientPairMessage( pair, NetworkClient.MessType_ToClient.NewDomainObjPair, nextObjPairString );
//					//SendClientPairMessage( pair, NetworkClient.MessType_ToClient.SwanReadyToMove );
//				//}else
//				//	EndGame( pair );
//	
//			}
			
//		}
	}
	
	//assumes we're connected
	public static void SendClientPairMessage( Pair<NetworkPlayer,NetworkPlayer> pr, NetworkClient.MessType_ToClient messType, params string[] args )
	{
		
		//TODO call the no arg version if args is empty or null
		if( args == null || args.Length == 0 || args[0] == null || args[0].Length == 0 )
		{
			NetworkClient.Instance.SendClientMess( pr.First, messType );
			NetworkClient.Instance.SendClientMess( pr.Second, messType );
		}
		else
		{
			NetworkClient.Instance.SendClientMess( pr.First, messType, args );
			NetworkClient.Instance.SendClientMess( pr.Second, messType, args );
		}
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
	
//	private void printRelationsTable()
//	{
//		System.Data.IDataReader res = db.BasicQuery( "SELECT objpairrelid, objpairid, relationid_p1, relationid_p2, lastModified FROM objectPairRelation" );
//		DebugConsole.Log( "objpairrelid | objpairid | relationid_p1 | relationid_p2 | lastModified" );
//		while( res.Read() ){ 
//			int tmpI=0;
//			int objpairrelid = res.GetInt32( tmpI++ ); 
//			int objpairid = res.GetInt32( tmpI++ ); 
//			int relationid_p1 = res.GetInt32( tmpI++ ); 
//			int relationid_p2 = res.GetInt32( tmpI++ ); 
//			string lastModified = res.GetString( tmpI++ ); 
//			DebugConsole.Log( objpairrelid + " | " + objpairid + " | " + relationid_p1 + " | " + relationid_p1 + " | " + lastModified );
//		}
//	}
	
	//triggered when you close the server (any way you quit, besides kill -9)
	void OnApplicationQuit()
	{
		db.CloseDB();
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
			db.OpenDB(databaseName);
			
			List<string> tableNames = db.GetAllTableNames();
			DebugConsole.Log( "TABLE NAMES: " );
			foreach( string name in tableNames )	DebugConsole.Log( name );
			
			List<string> pragResults = db.TryEnableForeignKeys(); //VERY IMPORTANT to ensure foreign keys are enforced
			
			
			if( 
					tableNames.Contains( "player" )
				&& tableNames.Contains( "topscore" )			
				//				tableNames.Contains( "relation" ) //relationid|relationName|relationDescr
//				&& tableNames.Contains( "storyDomain" ) 
//				&& tableNames.Contains( "storyObject" ) 
//				&& tableNames.Contains( "storyObjectPair" ) 
//				&& tableNames.Contains( "objectPairRelation" ) 
//				&& tableNames.Contains( "objectPairRelation_1player" ) 
//				&& tableNames.Contains( "relationGoldStandard" ) 
//				&& tableNames.Contains( "confidenceLookupMC" )			
				) 
			{
				//setup reading Game specific stuff from database
//				System.Text.StringBuilder sb = new System.Text.StringBuilder();
//				System.Data.IDataReader res = db.BasicQuery( "SELECT relationid, relationClass, relationName FROM relation" );
//				while( res.Read() ){ 
//					if( sb.Length > 0 ) sb.Append( "," );
//					int relationid = res.GetInt32( 0 ); 
//					int relationClass = res.GetInt32( 1 );
//					string relationName = res.GetString( 2 );
//					sb.Append( relationid.ToString() ).Append( "," ).Append( relationName );
//					DebugConsole.Log( relationid.ToString() + " | " + relationName );
//					drelationidTOrelationClass.Add( relationid, relationClass );
//				}
//				strListRelations = sb.ToString();
				
				//choose which Validation strategy you want to use
				valStrat = new ValidationStrategyMemoized( db );
				
//				Dictionary<int,int>.ValueCollection relationClasses = drelationidTOrelationClass.Values;
//				uniqueRelationClasses = new HashSet<int>( drelationidTOrelationClass.Values );
				
			}else{ //probably don't have a propperly set up db. 
				//TODO give them an easy way to set one up.
				DebugConsole.LogError( "Did not find expected table(s) in db. Make sure it is set up!" );
			}
			
			doneStart = true;
		}
	}	
	
	
	// Update is called once per frame
	//void Update () {}
	
#endif
	
}
