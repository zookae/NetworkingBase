using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class GameStateClient : MonoBehaviour 
{
//	public static int MAXIMUM_ROUNDS = 10;
//	public static int PLAYER_LIFE_WORTH = 100;
//	public static int SINGLE_PLAYER_DELAY_SEC = 10; //65 seemed too much... maybe 40?
//	public static int SPEED_COINDROP = 4;
//	public static int SPEED_FEATHERDROP = 4;
//	public static int SPEED_SWAN = 5;
//	public static int PLAYER_INIT_NUMLIVES = 4;
	
	static GameStateClient _instance;
	private static bool doneStart = false;
	
	public bool isConnected = false;
	
//	public enum StateEnum { Unspecified=0, Joining
//		, PlayerPairReady //sent to players when they have been paired up on the server side
//		, LocalRelationChosen 
//		, WaitingForDomObjPair //sent to server when client is waiting for a pair of dom obj
//		, ChoosingLocalRelation
//		, SwanReadyToMove //go ahead and move the swan
//		, GameOver
//	};
//	public long millisecChoosingRelation = 0;
//	
//	public StateEnum currState = StateEnum.Unspecified;
//	public bool bPlayerPairReady = false;
	
//	public string[] relationList = null; //holds relations sent from server
//	public SortedList<string,int> mRelationToID = null; //maps string relation to its id (from server)
//	
//	public int domainid = -1;
//	public string domainName = null;
//	public string domainDescr = null;
//	
//	public Dictionary<string /*tmpText*/,string/*relationid*/> mObjpairtextToObjpairid = new Dictionary<string, string>();
//	
//	public string[] domainObjects = null; //initially just the obj we want
//	public string[] strAgreedObjDeleted = null;
//	public string[] strAgreedObjNOTDeleted = null;
//	public string[] strAgreedObjAdded = null;

//	public string gameOverMessage = null;
//	
//	public SortedList<string /*storyObjectName*/,int/*storyObjectID*/> mStoryObjectToID = null; //maps string story object to its id (from server) for add/remove part of game
//	public Dictionary<string /*storyObjectID*/,string/*storyObjectName*/> mObjectidToObjectname = new Dictionary<string, string>();
//	
//	public Pair<string,string> pairDomainObj = null; //will hold the current pair from the server
//	public int idPairDomainObj = -1; //holds id of current pair of objects
//	public bool didLastRelationPairAgree = false; //true ONLY when the last was in agreement
//	public bool wasFirstToSelectRelation = false; //true when the player was first to select relation for the previous pair
//	
//	public bool bAmWinner = false;
//	
//	public bool heartClicked = false;
//	public int playerScore = 0;
//	public string swanName = null;
//	public bool tagroba = false;
	
//	public ArrayList deletedWords = new ArrayList();
	
//	public static string[] getAllValuesForKeys( SortedList<string,int> map, ArrayList lStringKeys )
//	{
//		if( map == null || lStringKeys == null ) return null;
//		
//		string[] ans = new string[ lStringKeys.Count ];
//		for( int i=0; i<lStringKeys.Count; i++ )
//		{
//			if( map.ContainsKey( (string)lStringKeys[i] ) )	ans[i] = map[ (string)lStringKeys[i] ].ToString();	
//		}
//		return ans;
//	}
	
	/// <summary>
	/// Static instance of the GameStateClient.
	///
	/// When you want to access the client without a direct
	/// reference (which you do in mose cases), use GameStateClient.Instance and the required
	/// GameObject initialization will be done for you.
	/// </summary>
	public static GameStateClient Instance {
		get {
		  if (_instance == null) {
		    _instance = FindObjectOfType(typeof(GameStateClient)) as GameStateClient;
		
		    if (_instance != null) {
		      return _instance;
		    }
		
		    GameObject client = new GameObject("__GameStateClient__");
		    _instance = client.AddComponent<GameStateClient>();
			_instance.Start();
			DontDestroyOnLoad( _instance );
		  }
		
		  return _instance;
		}
	}
	
	//a chance to reinitialize all your important variables for "play again" type behavior
	public void ReInit()
	{
		DebugConsole.Log( "GameStateClient.ReInit()" );
		doneStart = true;
		
		//isConnected = true;
		
//		
//		currState = StateEnum.Unspecified;
//		bPlayerPairReady = false;
//		
//		//relationList = null; //holds relations sent from server
//		//mRelationToID = null; //maps string relation to its id (from server)
//		
//		domainid = -1;
//		domainName = null;
//		domainDescr = null;
//		
//		//mObjpairtextToObjpairid = new Dictionary<string, string>();
//		
//		domainObjects = null; //initially just the obj we want
//		strAgreedObjDeleted = null;
//		strAgreedObjNOTDeleted = null;
//		strAgreedObjAdded = null;
//	
//		gameOverMessage = null;
//		
//		//mStoryObjectToID = null; //maps string story object to its id (from server) for add/remove part of game
//		//mObjectidToObjectname = new Dictionary<string, string>();
//		
//		pairDomainObj = null; //will hold the current pair from the server
//		idPairDomainObj = -1; //holds id of current pair of objects
//		didLastRelationPairAgree = false; //true ONLY when the last was in agreement
//		wasFirstToSelectRelation = false; //true when the player was first to select relation for the previous pair
//		
//		bAmWinner = false;		
//		heartClicked = false;
//		playerScore = 0;
//		swanName = null;
//		tagroba = false;
//		deletedWords = new ArrayList();
//		
//		millisecChoosingRelation = 0;
	}
	
	public void MessageFromServer( NetworkClient.MessType_ToClient messType, string[] args )
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		sb.Append( "Player " ).Append( Network.player ).Append( " GameStateClient.MessageFromServer: " ).Append( messType.ToString() );
		sb.Append( "\n" );

		if( args != null && args.Length > 0 ) 
			sb.Append( "args[] contains: " ).Append( NetworkClient.RayToString( args, "," ) );
		Debug.Log( sb.ToString() ); DebugConsole.Log( sb.ToString() );
		
		switch( messType )
		{
		case NetworkClient.MessType_ToClient.PlayerPairReady:
//			currState = StateEnum.PlayerPairReady;
//			bPlayerPairReady = true;
			break;
			
		case NetworkClient.MessType_ToClient.RelationsList: 
//			relationList = new string[ args.Length / 2 ];
//			mRelationToID = new SortedList<string, int>();
//			for( int i=0; i<args.Length; i = i + 2 )
//			{
//				mRelationToID.Add( args[i+1], Convert.ToInt32( args[i] ) );
//				relationList[ i / 2 ] = args[i+1];
//			}
			break;
			
		case NetworkClient.MessType_ToClient.DomainDescription:
			//toSend.Append( domainid ).Append( "|||" ).Append( domainName ).Append( "|||" ).Append( domainDescr );
//			if( args != null && args.Length > 0 )
//			{
//				string[] splitRay = Regex.Split( args[0],  Regex.Escape("|||") );
//				if( splitRay != null && splitRay.Length >= 2 && splitRay[ 1 ].Length > 0 )
//				{
//					domainid = Convert.ToInt32( splitRay[0] );
//					domainName = splitRay[1];
//					domainDescr = splitRay[2];
//				}else{
//					Debug.LogError( "Did not get domain description information!" );
//					NetworkClient.Instance.SendServerMess( NetworkClient.MessType_ToServer.PlayerHasNoLives );
//					DoGameOver( "Did not get domain description information!" );
//				}
//
//			}
//			else{
//				Debug.LogWarning( "GameStateClient.MessageFromServer: DomainDescription came back without ANY args." );
//				NetworkClient.Instance.SendServerMess( NetworkClient.MessType_ToServer.PlayerHasNoLives );
//				DoGameOver( "Did not get domain description information!" );
//			}
			break;
			
		case NetworkClient.MessType_ToClient.DomainObjectsInitial:
//			currState = StateEnum.PlayerPairReady;
//			domainObjects = new string[ args.Length / 2 ];//domainObjects = (string[])args.Clone();
//			mStoryObjectToID = new SortedList<string, int>();
//			for( int i=0; i<args.Length; i = i + 2 )
//			{
//				mStoryObjectToID.Add( args[i+1], Convert.ToInt32( args[i] ) );
//				mObjectidToObjectname.Add( args[i], args[i+1] );
//				domainObjects[ i / 2 ] = args[i+1];
//			}
//			Debug.Log( "GameStateClient.DomainObjectsInitial: " + NetworkClient.RayToString( args, "," ) );
			break;
			
		case NetworkClient.MessType_ToClient.DomainObjectsFinal:
			
//			if( args != null && args.Length > 0 )
//			{
//				Debug.Log( "GameStateClient.DomainObjectsFinal" );
//			    string[] tmpAgreed = args[0].Split( new char[] {'|'} );
//				if( tmpAgreed != null && tmpAgreed.Length > 0 && tmpAgreed[ 0 ].Length > 0 )
//				{
//					strAgreedObjDeleted = new string[ tmpAgreed.Length ];
//					for( int i=0; i<tmpAgreed.Length; i++ )//&& tmpAgreed[i] != null && tmpAgreed[i].Length > 0
//					{
//						if( mObjectidToObjectname.ContainsKey( tmpAgreed[ i ] ) )
//							strAgreedObjDeleted[ i ] = this.mObjectidToObjectname[ tmpAgreed[ i ] ];
//						else
//							Debug.LogWarning( "Didn't find deleted object id: " + tmpAgreed[i] );
//					}
//				}else
//					strAgreedObjDeleted = new string[ 0 ];
//				
//				
//				Debug.Log( "Got agreed deletes: " );
//				Debug.Log( NetworkClient.RayToString( strAgreedObjDeleted, "," ) );
//				
//				tmpAgreed = args[1].Split( new char[] {'|'} );
//				if( tmpAgreed != null && tmpAgreed.Length > 0 && tmpAgreed[ 0 ].Length > 0 )
//				{
//					strAgreedObjNOTDeleted = new string[ tmpAgreed.Length ];
//					for( int i=0; i<tmpAgreed.Length; i++ )
//					{
//						if( mObjectidToObjectname.ContainsKey( tmpAgreed[ i ] ) )
//							strAgreedObjNOTDeleted[ i ] = this.mObjectidToObjectname[ tmpAgreed[ i ] ];
//						else
//							Debug.LogWarning( "Didn't find NON deleted object id: " + tmpAgreed[i] );
//					}
//				}else
//					strAgreedObjNOTDeleted = new string[ 0 ];
//				
//				Debug.Log( "Got agreed NON deletes: " );
//				Debug.Log( NetworkClient.RayToString( strAgreedObjNOTDeleted, "," ) );
//				
//				strAgreedObjAdded = args[2].Split( new char[] {'|'} );
//				if( strAgreedObjAdded != null && strAgreedObjAdded.Length > 0 && strAgreedObjAdded[ 0 ].Length > 0 )
//				{}
//				else
//					strAgreedObjAdded = new string[ 0 ];
//				
//				Debug.Log( "Got agreed adds: " );
//				Debug.Log( NetworkClient.RayToString( strAgreedObjAdded, "," ) );
//			}
//			else{
//				Debug.LogWarning( "GameStateClient.MessageFromServer: Domain objects final came back without ANY args." );
//			}
			break;
			
		case NetworkClient.MessType_ToClient.NewDomainObjPair:
			
//			if( args != null && args.Length >=5 )
//			{
//				pairDomainObj = new Pair<string, string>( args[2], args[4] );
//				idPairDomainObj = Convert.ToInt32( args[0] );
//				currState = StateEnum.ChoosingLocalRelation;
//			}else{
//				Debug.LogWarning( "GameStateClient.MessageFromServer: NewDomainObjPair came back without ANY args." );
//			}
			break;
			
		case NetworkClient.MessType_ToClient.FirstToSelectRelation:
//			wasFirstToSelectRelation = true;
			break;
			
		case NetworkClient.MessType_ToClient.SwanReadyToMove:
//			millisecChoosingRelation = (System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond); //record start time of selection
//			currState = StateEnum.SwanReadyToMove;
			break;
			
		case NetworkClient.MessType_ToClient.RelationMatchResult:
//			if( args != null && args.Length > 0 )
//			{
//				didLastRelationPairAgree = args[ 0 ].Equals( "agree", StringComparison.OrdinalIgnoreCase );
//				Debug.Log( "args[ 0 ].Equals( \"agree\", StringComparison.OrdinalIgnoreCase ) == " + didLastRelationPairAgree );
//			}
//			else
//				Debug.LogWarning( "GameStateClient.MessageFromServer: RelationMatchResult came back without ANY args." );
			break;
			
		case NetworkClient.MessType_ToClient.GameOver:
//			if( args != null && args.Length > 0 && args[0] != null)
//			{
//				DoGameOver( args[0] );
//			}
//			else 
//			{
//				Debug.LogWarning( "GameStateClient.MessageFromServer: Got a game over message without any argument XD" );
//				DoGameOver( "" );
//			}
			break;
		
		case NetworkClient.MessType_ToClient.YouWin:
//			bAmWinner = true;
			break;
			
		default:
			DebugConsole.Log( "MessageFromServer: default" );
			break;
		}
	}
	
	private void DoGameOver( string gameOverReason )
	{
//		Debug.Log ( "Doing game over for following reason: " + gameOverReason );
//		gameOverMessage = (string)gameOverReason.Clone();
//		currState = StateEnum.GameOver;
//		bPlayerPairReady = false;
	}
	
	void Start()
	{
		if( !doneStart)
		{//put init stuff here.
			doneStart = true;
		}
	}	
	
	// Update is called once per frame
	//void Update () {}
}
