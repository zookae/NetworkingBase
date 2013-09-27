using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game level data, NOT player level
/// </summary>
public abstract class RunningGameData
{
#if UNITY_WEBPLAYER
#else
	public GameStateServer gss = null;
//	public int roundNumber = -1;
//	public int objpairidUnderESP = -1;
//		
//	public string[] agreedStoryObjectIDsDeleted = null;
//	public string[] agreedStoryObjectIDsNOTDeleted = null;
//	public string[] agreedStoryObjectNamesAdded = null;
//	
//	//required technique before we were tracking playerid's
//	//public System.Text.StringBuilder sbListSeenObjpairid = new System.Text.StringBuilder(); //for the sql query
//	//public List<int> listSeenObjpairid = new List<int>();
//	public int domainID = -1;

    public int gameID;
    public Dictionary<NetworkPlayer, PlayerData> dPlayerData = new Dictionary<NetworkPlayer,PlayerData>();

    abstract public void SendWinnerMessage( );
	abstract public void EndGame( string mess_p1, string mess_p2 );
	//abstract public void SetUniqueDeviceID( NetworkPlayer player, string udid );		
    public void SetUniqueDeviceID(NetworkPlayer player, string udid) {
        Debug.Log("SetUniqueDeviceID for player " + player + " with udid " + udid);
        dPlayerData[player].uniqueDeviceID = udid;

        int playerid = gss.dbManip.getPlayerID(udid);
        if (playerid == -1) {//first time player; welcome!
            DebugConsole.Log("Got a first time player!");
            playerid = gss.dbManip.createPlayerID(udid);
            dPlayerData[player].isFirstTimePlayer = true;
        } else {//returning player
            DebugConsole.Log("We have a returning player");
            dPlayerData[player].isFirstTimePlayer = false;
        }
        dPlayerData[player].playerid = playerid;
    }
	
	abstract public void PlayerDisconnected(NetworkPlayer player);
	abstract public void ResetVarsForNewGame();

    public RunningGameData( GameStateServer inGSS, int gid )
	{
		this.gss = inGSS;
        this.gameID = gid;
	}
	
//	virtual public System.Collections.Generic.KeyValuePair<int/*domainId*/,System.Text.StringBuilder/*domainid|||domainName|||domainDescr*/> 
//		SelectDomainid()
//	{
//		System.Collections.Generic.KeyValuePair<int,System.Text.StringBuilder> ansPair = new System.Collections.Generic.KeyValuePair<int,System.Text.StringBuilder>();
		//choose domainid for the solo player
//		//send the: (0) domainid, (1) domain name, (2) domain description, 
//		System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
//		sbSQL.Append( "SELECT domainid, domainName, domainDescr, numPairsLeft FROM storyDomain WHERE bUse=1 AND bDone=0 LIMIT 1" );
//		System.Data.IDataReader res = gss.db.BasicQuery( sbSQL.ToString() ); 
//		
//		if( res.Read() )
//		{
//			int tmpK = 0;
//			int domainid = res.GetInt32( tmpK++ );
//			string domainName = res.GetString( tmpK++ );
//			string domainDescr = res.GetString( tmpK++ );
//			int numPairsLeft = res.GetInt32( tmpK++ );
//	
//			System.Text.StringBuilder toSend = new System.Text.StringBuilder();
//			toSend.Append( domainid ).Append( "|||" ).Append( domainName ).Append( "|||" ).Append( domainDescr );
//			ansPair = new System.Collections.Generic.KeyValuePair<int,System.Text.StringBuilder>(domainID, toSend);
//			//NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.DomainDescription, toSend.ToString() );
//			
//			//If doing add/remove domain objects, then
//			//SendPlayerPairDomObj( pair );
//		}
//		return ansPair;
//	}
	
	//THIS FINDS A OBJECT PAIR FOR WHICH THERE ARE NO ANSWERS (assumes subclasses will find obj pair with some answers, but no answers coming from calling player (pair))
	//note that this means there is no 'npc' data to go off of
//	virtual public string MakeNextObjPairString()
//	{
//		Debug.Log( "base. MakeNextObjPairString 1 " );
//		System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
//		
//		//THIS FINDS A OBJECT PAIR FOR WHICH THERE ARE NO ANSWERS (assumes subclasses will find obj pair with some answers, but no answers coming from calling player (pair))
//		//note that this means there is no 'npc' data to go off of
//		//m.objpairid, m.domainid, m.bUse, m.bDone, m.intConfidence, m.numESPAgree, m.numTimesSeen, m.bHasDeletedObj
//		sbSQL.Append( "SELECT objpairid, object1id, object2id, nameObj1, nameObj2 FROM vwObjPair WHERE " );
//		sbSQL.Append( " bUse=1 AND bDone=0 AND bHasDeletedObj=0 " ); //numESPAgree=0 AND 
//		sbSQL.Append( " AND domainid=" ).Append( this.domainID ); 
//		//AND not in objectPairRelation_1player.objpairid
//		//required before we were tracking playerids
//		//sbSQL.Append( " AND NOT EXISTS (SELECT objpairrelid FROM objectPairRelation_1player WHERE vwObjPair.objpairid = objectPairRelation_1player.objpairid)" );
//		//this line does the excluding of objpairs where we have some answers already
//		sbSQL.Append( " AND NOT EXISTS (SELECT objpairrelid FROM objectPairRelation_1player WHERE vwObjPair.objpairid = objectPairRelation_1player.objpairid" );
//		sbSQL.Append( " ) " );
//		
//		/* required technique before we were tracking playerid's
//		if( sbListSeenObjpairid.Length >  0 )
//		{//remove all objpairid this player pair has already seen (in case of disagreement, could come back; in case of agreement, we shouldnt see anyway)
//			sbSQL.Append( " AND objpairid NOT IN ( " ).Append( sbListSeenObjpairid );
//			sbSQL.Append( " ) " );
//		}
//		*/
//		
//		if( gss.hsObjpairidInPlay.Count > 0 )
//		{//remove all objpairid that are currently being ESPd
//			int[] listObjPairInPlay = new int[ gss.hsObjpairidInPlay.Count ] ;
//			gss.hsObjpairidInPlay.CopyTo( listObjPairInPlay );
//			
//			sbSQL.Append( " AND objpairid NOT IN ( " ).Append( listObjPairInPlay[0] );
//			for( int i=1; i<listObjPairInPlay.Length; i++ )
//				sbSQL.Append( ", " ).Append( listObjPairInPlay[i] );
//			sbSQL.Append( ") " );
//		}
//		//foreach( int iSeenObjectPairID in rgd.listSeenObjpairid )
//		//	sbSQL.Append( " AND objpairid!=" ).Append( iSeenObjectPairID );
//		
//		sbSQL.Append( " LIMIT 1" );
//		
//		DebugConsole.LogWarning( "Next object pair query: " );
//		DebugConsole.Log( sbSQL.ToString() );
//		
//		System.Data.IDataReader res = gss.db.BasicQuery( sbSQL.ToString() ); 
		
//		System.Text.StringBuilder sb = new System.Text.StringBuilder();
//		while( res.Read() ){
//			//SELECT objpairid, object1id, object2id, nameObj1, nameObj2 FROM vwObjPair
//			int tmpI = 0;
//			if( sb.Length > 0 ) sb.Append( "," );
//			int objpairid = res.GetInt32( tmpI++ ); 
//			//int domainid = res.GetInt32( tmpI++ );
//			//int numESPAgree = res.GetInt32( tmpI++ );
//			//int bHasDeletedObj = res.GetInt32( tmpI++ );
//			int object1id = res.GetInt32( tmpI++ );
//			int object2id = res.GetInt32( tmpI++ );
//			
//			string nameObj1 = res.GetString( tmpI++ );
//			string nameObj2 = res.GetString( tmpI++ );
//			
//			this.objpairidUnderESP = objpairid;
//			gss.hsObjpairidInPlay.Add( objpairid );
//			
//			sb.Append( objpairid ).Append( "," );
//			sb.Append( object1id.ToString() ).Append( "," ).Append( nameObj1 ).Append( "," );
//			sb.Append( object2id.ToString() ).Append( "," ).Append( nameObj2 );
//			
//			/* required technique before we were tracking playerid's
//			if( sbListSeenObjpairid.Length == 0 )
//				sbListSeenObjpairid.Append( objpairid.ToString() );
//			else
//				sbListSeenObjpairid.Append( ", " ).Append( objpairid.ToString() );
//			*/
//		}
//		DebugConsole.Log( "Query result: " );
//		DebugConsole.Log( sb.ToString() );
//		return sb.ToString();
//	}
//	abstract public void Mess_JustGotToSwanScreen( NetworkPlayer player, string[] args );
//	abstract public void SendFinalListAndFirstPairWhenReady( NetworkPlayer player );
//	abstract public void Mess_DomainObjectIDsDeleted( NetworkPlayer player, string[] args );
//	abstract public void Mess_DomainObjectIDsNOTDeleted( NetworkPlayer player, string[] args );
//	abstract public void Mess_DomainObjectNamesAdded( NetworkPlayer player, string[] args );
//	abstract public void Mess_SwanAtEndOfScreen( NetworkPlayer player, string[] args );
	abstract public void Mess_PlayerHasNoLives( NetworkPlayer player, string[] args );
//	abstract public void Mess_SelectedRelation( NetworkPlayer player, string[] args );
	

#endif
}

