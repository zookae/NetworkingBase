using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;

public class GameStateClient : MonoBehaviour 
{
	static GameStateClient _instance;
	private static bool doneStart = false;
	
	public bool isConnected = false;

    public NetworkMailbox<NetworkClient.MessType_ToClient> mailbox = new NetworkMailbox<NetworkClient.MessType_ToClient>();
	
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

	}
	
	public void MessageFromServer( NetworkClient.MessType_ToClient messType, string[] args )
	{
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		sb.Append( "Player " ).Append( Network.player ).Append( " GameStateClient.MessageFromServer: " ).Append( messType.ToString() );
		sb.Append( "\n" );

		if( args != null && args.Length > 0 ) 
			sb.Append( "args[] contains: " ).Append( NetworkClient.RayToString( args, "," ) );
		Debug.Log( sb.ToString() ); DebugConsole.Log( sb.ToString() );
        /*
         * NOTE! any message that needs instant reaction should have a "case ..." here; 
         * otherwise, the "case default" is to throw the message in a queue 
         * (the queue we're using is a NetworkMailbox; by default the queue length is unbounded!)
         */
        switch ( messType )
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
			DebugConsole.Log( "MessageFromServer: default (adding to mailbox)" );
            mailbox.AddMessage( messType, args );
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
