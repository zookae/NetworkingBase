using UnityEngine;
using System.Collections;
using System;

class RunningGameData_1player: RunningGameData
{
#if UNITY_WEBPLAYER
#else
	//static readonly 
	
	public bool bHaveSavedScores = false;
	public int iOfPlayAgain = 0;

    private NetworkPlayer player;
	
	public RunningGameData_1player( GameStateServer inGSS, int gid, NetworkPlayer inPlayer ) : base( inGSS, gid )
	{
        player = inPlayer;
	}
	/*
	 * 1] how to select next obj pair for player:
	 *     dID <- domain is not done [ SELECT FROM storyDomain WHERE bUse = 1 AND bDone = 0 ]
	 * 
	 *     (AND
	 *       pair is not done [ SELECT FROM storyObjectPair WHERE domainid=dID AND bUse = 1 AND bDone = 0 ]
	 *       pair never seen by player [ SELECT FROM objectPairRelation_1player WHERE 
	 *      )
	 * 
	 * 2] insert player answer for pair
	 * 3] update counts and confidence
	 * 4] update pair completion
	 */
	
	override public void ResetVarsForNewGame()
	{
		//base.ResetVarsForNewGame();
		
	}
	
	
	
//	override public System.Collections.Generic.KeyValuePair<int/*domainId*/,System.Text.StringBuilder/*domainid|||domainName|||domainDescr*/> 
//		SelectDomainid()
//	{
//		System.Collections.Generic.KeyValuePair<int,System.Text.StringBuilder> ansPair = new System.Collections.Generic.KeyValuePair<int,System.Text.StringBuilder>();
//		//choose domainid for the solo player
//		//send the: (0) domainid, (1) domain name, (2) domain description, 
//		
//		//THIS GETS domain from THE N-1th answer, which has the 'npc' answers (ie chosen relation & millisecondsChoosing)
//		System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
//		sbSQL.Append( "SELECT vop.domainid, sd.domainName, sd.domainDescr, sd.numPairsLeft FROM" );
//		sbSQL.Append( " vwObjPair vop LEFT JOIN storyDomain sd on sd.domainid=vop.domainid" );
//		sbSQL.Append( " WHERE vop.bUse=1 AND vop.bDone=0 AND vop.bHasDeletedObj=0" );
//		sbSQL.Append( " AND ( vop.objpairid NOT IN ( SELECT vop1p.objpairid FROM vwObjPair1Player vop1p WHERE playerid=" );
//		sbSQL.Append( this.playerData.playerid ).Append( ") )" );
//		if( gss.hsObjpairidInPlay.Count > 0 )
//		{//remove all objpairid that are currently being ESPd (so we don't over-collect answers for something); at worst, 10 sec delay
//			int[] listObjPairInPlay = new int[ gss.hsObjpairidInPlay.Count ] ;
//			gss.hsObjpairidInPlay.CopyTo( listObjPairInPlay );
//			
//			sbSQL.Append( " AND vop.objpairid NOT IN ( " ).Append( listObjPairInPlay[0] );
//			for( int i=1; i<listObjPairInPlay.Length; i++ )
//				sbSQL.Append( ", " ).Append( listObjPairInPlay[i] );
//			sbSQL.Append( ") " );
//		}
//		sbSQL.Append( " AND sd.bUse=1 LIMIT 1" );
//		
//		System.Data.IDataReader res = gss.db.BasicQuery( sbSQL.ToString() ); 
//		if( res.Read() )
//		{//have some outstanding pairs this player can weigh in on, so use this domain
//			int tmpK = 0;
//			int domainid = res.GetInt32( tmpK++ );
//			string domainName = res.GetString( tmpK++ );
//			string domainDescr = res.GetString( tmpK++ );
//			int numPairsLeft = res.GetInt32( tmpK++ );
//	
//			System.Text.StringBuilder toSend = new System.Text.StringBuilder();
//			toSend.Append( domainid ).Append( "|||" ).Append( domainName ).Append( "|||" ).Append( domainDescr );
//			ansPair = new System.Collections.Generic.KeyValuePair<int,System.Text.StringBuilder>(domainid, toSend);
//		}
//		else
//		{//do not have 1player results to weigh in on. caller sends a game over
//			//ansPair = null;
//		}
//		//SELECT vop.domainid, sd.domainName, sd.domainDescr, sd.numPairsLeft FROM vwObjPair vop LEFT JOIN storyDomain sd on sd.domainid=vop.domainid WHERE vop.bUse=1 AND vop.bDone=0 AND vop.bHasDeletedObj=0 AND ( vop.objpairid NOT IN ( SELECT     vop1p.objpairid FROM vwObjPair1Player vop1p WHERE playerid=1) );
//		/*
//		sbSQL.Append( "SELECT vop1p.domainid, sd.domainName, sd.domainDescr, sd.numPairsLeft FROM" );
//		sbSQL.Append( " vwObjPair1Player vop1p LEFT JOIN storyDomain sd on sd.domainid=vop1p.domainid" );
//		sbSQL.Append( " WHERE vop1p.bDone=0 AND vop1p.bUse=1 AND vop1p.relationid IS NOT NULL" );
//		sbSQL.Append( " AND ( vop1p.objpairid NOT IN ( SELECT vop1p.objpairid FROM vwObjPair1Player WHERE playerid=" );
//		sbSQL.Append( this.playerData.playerid ).Append( ") )" );
//		if( gss.hsObjpairidInPlay.Count > 0 )
//		{//remove all objpairid that are currently being ESPd (so we don't over-collect answers for something); at worst, 10 sec delay
//			int[] listObjPairInPlay = new int[ gss.hsObjpairidInPlay.Count ] ;
//			gss.hsObjpairidInPlay.CopyTo( listObjPairInPlay );
//			
//			sbSQL.Append( " AND vop1p.objpairid NOT IN ( " ).Append( listObjPairInPlay[0] );
//			for( int i=1; i<listObjPairInPlay.Length; i++ )
//				sbSQL.Append( ", " ).Append( listObjPairInPlay[i] );
//			sbSQL.Append( ") " );
//		}
//		sbSQL.Append( " LIMIT 1" );
//		
//		System.Data.IDataReader res = gss.db.BasicQuery( sbSQL.ToString() ); 
//		if( res.Read() )
//		{//have some outstanding 1player results that this player can weigh in on, so use this domain
//			int tmpK = 0;
//			int domainid = res.GetInt32( tmpK++ );
//			string domainName = res.GetString( tmpK++ );
//			string domainDescr = res.GetString( tmpK++ );
//			int numPairsLeft = res.GetInt32( tmpK++ );
//	
//			System.Text.StringBuilder toSend = new System.Text.StringBuilder();
//			toSend.Append( domainid ).Append( "|||" ).Append( domainName ).Append( "|||" ).Append( domainDescr );
//			ansPair.Key = domainID;
//			ansPair.Value = toSend;
//		}
//		else
//		{//do not have 1player results to weigh in on, see if there is a domain that needs weighing in on 
//			//THIS FINDS A domain FOR WHICH THERE ARE NO ANSWERS (assumes subclasses will find obj pair with some answers, but no answers coming from calling player (pair))
//			sbSQL = new System.Text.StringBuilder();
//			sbSQL.Append( "SELECT vop.domainid, sd.domainName, sd.domainDescr, sd.numPairsLeft FROM" );
//			sbSQL.Append( " vwObjPair vop LEFT JOIN storyDomain sd on sd.domainid=vop.domainid " );
//			sbSQL.Append( " WHERE vop.bUse=1 AND vop.bDone=0 AND vop.bHasDeletedObj=0 " ); //numESPAgree=0 AND 
//			//this line does the excluding of objpairs where we have some answers already
//			sbSQL.Append( " AND NOT EXISTS (SELECT objpairrelid FROM objectPairRelation_1player WHERE vop.objpairid = objectPairRelation_1player.objpairid )" );
//			if( gss.hsObjpairidInPlay.Count > 0 )
//			{//remove all objpairid that are currently being ESPd
//				int[] listObjPairInPlay = new int[ gss.hsObjpairidInPlay.Count ] ;
//				gss.hsObjpairidInPlay.CopyTo( listObjPairInPlay );
//				
//				sbSQL.Append( " AND vop.objpairid NOT IN ( " ).Append( listObjPairInPlay[0] );
//				for( int i=1; i<listObjPairInPlay.Length; i++ )
//					sbSQL.Append( ", " ).Append( listObjPairInPlay[i] );
//				sbSQL.Append( ") " );
//			}
//			sbSQL.Append( " LIMIT 1" );
//			res = gss.db.BasicQuery( sbSQL.ToString() ); 
//			
//			if( res.Read() )
//			{
//				int tmpK = 0;
//				int domainid = res.GetInt32( tmpK++ );
//				string domainName = res.GetString( tmpK++ );
//				string domainDescr = res.GetString( tmpK++ );
//				int numPairsLeft = res.GetInt32( tmpK++ );
//		
//				System.Text.StringBuilder toSend = new System.Text.StringBuilder();
//				toSend.Append( domainid ).Append( "|||" ).Append( domainName ).Append( "|||" ).Append( domainDescr );
//				ansPair.Key = domainID;
//				ansPair.Value = toSend;
//			}
//			else
//			{
//			}
//			
//		}
//		*/
//		
//		//If doing add/remove domain objects, then
//		//SendPlayerPairDomObj( pair );
		
//		return ansPair;
//	}
	
	override public void SendWinnerMessage( )
	{
		//gss.hsObjpairidInPlay.Remove( this.objpairidUnderESP );
		//if( this.iNumSeen >= 4 && (this.iNumCorrect / ((float)this.iNumSeen)) > 0.5 )
		//{
			NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.YouWin );
		//}
	}
	
	override public void EndGame( string mess_p1, string mess_p2 )
	{
//		gss.hsObjpairidInPlay.Remove( this.objpairidUnderESP );
		if( gss.dPlayerToGamedata.ContainsKey( this.player ) )
		{
			SavePlayerScoreStats();
			
			//gss.dPlayerToGamedata.Remove( this.player );
			/* required before we were tracking playerids
			if( gss.dPlayerToSeenObjpairid.ContainsKey( this.player ) )
				gss.dPlayerToSeenObjpairid[ this.player ] = this.sbListSeenObjpairid;
			else
				gss.dPlayerToSeenObjpairid.Add( this.player, this.sbListSeenObjpairid );
			*/
			NetworkClient.Instance.SendClientMess( this.player, NetworkClient.MessType_ToClient.GameOver, mess_p1 );
		}
	}
	
	private void SavePlayerScoreStats()
	{
		if( /* this.iNumSeen > 0 && */ bHaveSavedScores == false )
		{//only save if the player has seen at least 1 relation, and has not reached 'end game'
            gss.dbManip.SavePlayerScoreStats(dPlayerData[player], this.iOfPlayAgain);
			
			bHaveSavedScores = true;
			iOfPlayAgain++; //update count of play again
		}
	}
	
	override public void PlayerDisconnected(NetworkPlayer player)
	{	
//		gss.hsObjpairidInPlay.Remove( this.objpairidUnderESP );
		SavePlayerScoreStats();
		//SendWinnerMessage();		
		
		//required technique before we were tracking playerid's
		//gss.dPlayerToSeenObjpairid.Remove( this.player );
		gss.dPlayerToGamedata.Remove( this.player );
	}
	
//	private string LookupGoldstandardForObjectpairid( string inObjectpairid )
//	{
//		Debug.Log( "LookupGoldstandardForObjectpairid" );
//		System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
//		
//		sbSQL.Append( "SELECT objpairid, relationid FROM relationGoldStandard WHERE " );
//		sbSQL.Append( " objpairid=" ).Append( inObjectpairid ); 
//		sbSQL.Append( " LIMIT 1" );
//
//		System.Data.IDataReader res = gss.db.BasicQuery( sbSQL.ToString() ); 
//		
//		System.Text.StringBuilder sb = new System.Text.StringBuilder();
//		while( res.Read() ){
//			int tmpI = 0;
//			if( sb.Length > 0 ) sb.Append( "," );
//			int objpairid = res.GetInt32( tmpI++ );
//			int relationid = res.GetInt32( tmpI++ ); 
//			
//			sb.Append( objpairid.ToString() ).Append( "," );
//			sb.Append( relationid.ToString() );
//			this.npcRelationid = relationid.ToString();
//		}
//		DebugConsole.Log( "Query result: " );
//		DebugConsole.Log( sb.ToString() );
//		return sb.ToString();
//	}
//	
	//ROWCOUNT alternative http://stackoverflow.com/questions/962032/how-do-i-count-the-number-of-rows-returned-in-my-sqlite-reader-in-c?rq=1
//	override public string MakeNextObjPairString(  )
//	{
////		Debug.Log( "1P MakeNextObjPairString 1" );
//		
//		//GRAB SINGLE PLAYER DATA NEEDING A MATCH
//		string ans = MakeNextObjPairStringFromSinglePlayer();
//		if( ans == null || ans.Length <= 0 )
//		{//THERE IS NO SINGLE PLAYER DATA TO GRAB, return the usual stuff
//			Debug.Log( "could not get from single player table. Trying to get from normal table" );
//			ans = base.MakeNextObjPairString();
//			npcRelationid = null;
//			this.npcMillisecChoose = 0;
//			//this.objpairrelid = null;
//			isObjpairidFromSinglePlayerTable = false;
//			//TODO: lookup a goldstandard id to compare theirs to
//			
//			this.LookupGoldstandardForObjectpairid( this.objpairidUnderESP.ToString() );
//			
//		}else{
//			Debug.Log( "Got from single player table. " + ans );
//		}
//		return ans; //return the single player stuff
//	}		
	
	//THIS GETS THE N-1th answer, which has the 'npc' answers (ie chosen relation & millisecondsChoosing)
	//equivalently, it attempts to work on object pairs for which we have already gathered partial data but not yet completed
	//thus, if it returns nothing, then there are no 'open' jobs that the player hasn't already weighed in on
	//there may still be object pairs that have no answers
//	private string MakeNextObjPairStringFromSinglePlayer()
//	{
//		Debug.Log( "MakeNextObjPairStringFromSinglePlayer" );
//		System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
//		
//		//THIS GETS THE N-1th answer, which has the 'npc' answers (ie chosen relation & millisecondsChoosing)
//		sbSQL.Append( "SELECT objpairid, object1id, object2id, nameObj1, nameObj2, relationid, millisecChoose FROM vwObjPair1Player WHERE " );
//		sbSQL.Append( " bUse= 1" );
//		sbSQL.Append( " AND bDone= 0" );
//		sbSQL.Append( " AND domainid=" ).Append( this.domainID ); 
//		sbSQL.Append( " AND relationid IS NOT NULL " );
//		sbSQL.Append( " AND ( objpairid NOT IN ( SELECT objpairid FROM vwObjPair1Player WHERE playerid = " ).Append( this.playerData.playerid ).Append( " ) )" );
//		//select * from vwObjPair1Player WHERE domainid=1 AND bDone=0 AND bUse=1 AND ( objpairid NOT IN (select objpairid FROM vwObjPair1Player WHERE playerid=1) );
//
//		/* WAS NEEDED BEFORE WE WERE TRACKING UNIQUE DEVICE ID'S
//		sbSQL.Append( " AND playerid != " ).Append( this.playerData.playerid );
//		if( sbListSeenObjpairid.Length > 0 )
//		{//remove all objpairid this player pair has already seen (in case of disagreement, could come back; in case of agreement, we shouldnt see anyway)
//			sbSQL.Append( " AND objpairid NOT IN ( " ).Append( sbListSeenObjpairid );
//			sbSQL.Append( " ) " );
//		}
//		*/
//		
//		if( gss.hsObjpairidInPlay.Count > 0 )
//		{//remove all objpairid that are currently being ESPd (so we don't over-collect answers for something); at worst, 10 sec delay
//			int[] listObjPairInPlay = new int[ gss.hsObjpairidInPlay.Count ] ;
//			gss.hsObjpairidInPlay.CopyTo( listObjPairInPlay );
//			
//			sbSQL.Append( " AND objpairid NOT IN ( " ).Append( listObjPairInPlay[0] );
//			for( int i=1; i<listObjPairInPlay.Length; i++ )
//				sbSQL.Append( ", " ).Append( listObjPairInPlay[i] );
//			sbSQL.Append( ") " );
//		}
//		//required technique before we were tracking playerid's
//		//foreach( int iSeenObjectPairID in rgd.listSeenObjpairid )
//		//	sbSQL.Append( " AND objpairid!=" ).Append( iSeenObjectPairID );
//		
//		sbSQL.Append( " LIMIT 1" ); //ORDER BY lastModified DESC 
//		
//		DebugConsole.LogWarning( "MakeNextObjPairStringFromSinglePlayer query: " );
//		DebugConsole.Log( sbSQL.ToString() );
//		
//		System.Data.IDataReader res = gss.db.BasicQuery( sbSQL.ToString() ); 
//		
//		System.Text.StringBuilder sb = new System.Text.StringBuilder();
//		if( res.Read() ){
//			// objpairid, object1id, object2id, nameObj1, nameObj2, relationid, millisecChoose FROM vwObjPair1Player
//			int tmpI = 0;
//			if( sb.Length > 0 ) sb.Append( "," );
//			int objpairid = res.GetInt32( tmpI++ ); 
//			int object1id = res.GetInt32( tmpI++ );
//			int object2id = res.GetInt32( tmpI++ );
//			
//			string nameObj1 = res.GetString( tmpI++ );
//			string nameObj2 = res.GetString( tmpI++ );
//			
//			this.npcRelationid = res.IsDBNull( tmpI ) ? null : res.GetInt32( tmpI ).ToString(); //int relationid = res.GetInt32( tmpI++ ); 
//			tmpI++;
//			this.npcMillisecChoose = res.IsDBNull( tmpI ) ? int.MaxValue : res.GetInt32( tmpI );
//			tmpI++;
//			
//			this.objpairidUnderESP = objpairid;
//			gss.hsObjpairidInPlay.Add( objpairid );
//			this.isObjpairidFromSinglePlayerTable = true;
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
//		
//		DebugConsole.Log( "Query result: " );
//		DebugConsole.Log( sb.ToString() );
//		return sb.ToString();
//	}
	
//	override public void Mess_JustGotToSwanScreen( NetworkPlayer player, string[] args )
//	{
//		//Debug.Log( "1P Mess_JustGotToSwanScreen a" );
//		//RunningGameData_1player rgd = this;
//		playerData.isAtSwanScreen = true;
//		//Debug.Log( "1P Mess_JustGotToSwanScreen a1" );
//		if( args != null && args.Length >=1 )
//		{
//			playerData.playerScore = Convert.ToInt32( args[0] );
//		}else{
//			DebugConsole.LogWarning( "GameStateClient.MessageFromServer(1p): Mess_JustGotToSwanScreen did not come with a score." );
//		}
//		
//		//send clients the first pair, update round number, inform them they're ready to move
//		//Debug.Log( "1P Mess_JustGotToSwanScreen b" );
//		roundNumber = 0;
//		string nextObjPairString = this.MakeNextObjPairString( );
//		//Debug.Log( "1P Mess_JustGotToSwanScreen c" );
//		if( nextObjPairString.Length > 0 )
//		{
//			NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.NewDomainObjPair, nextObjPairString );
//			NetworkClient.Instance.SendClientMess( player, NetworkClient.MessType_ToClient.SwanReadyToMove );
//		}
//		else
//		{//Arrived at swan screen, but now there's no more pairs
//			SendWinnerMessage( );
//			EndGame( "No more object pairs for this domain!", null );
//		}
//		//Debug.Log( "1P Mess_JustGotToSwanScreen d" );
//	}
//	
//	//depricated
//	override public void SendFinalListAndFirstPairWhenReady( NetworkPlayer player )
//	{
//		RunningGameData_1player rgd = this;
//		if( rgd.agreedStoryObjectIDsDeleted != null
//			&& rgd.agreedStoryObjectIDsNOTDeleted != null
//			&& rgd.agreedStoryObjectNamesAdded != null )
//		{
//		
//			//send clients the final list
//			System.Text.StringBuilder sb = new System.Text.StringBuilder();
//			sb.Append( NetworkClient.RayToString( rgd.agreedStoryObjectIDsDeleted, "|" ) ).Append( "," );
//			sb.Append( NetworkClient.RayToString( rgd.agreedStoryObjectIDsNOTDeleted, "|" ) ).Append( "," );
//			sb.Append( NetworkClient.RayToString( rgd.agreedStoryObjectNamesAdded, "|" ) );
//			NetworkClient.Instance.SendClientMess( rgd.player, NetworkClient.MessType_ToClient.DomainObjectsFinal, sb.ToString() );
//			
//			//send clients the first pair, update round number, inform them they're ready to move
//			rgd.roundNumber = 0;
//			string nextObjPairString = MakeNextObjPairString( );
//			if( nextObjPairString.Length > 0 )
//			{
//				NetworkClient.Instance.SendClientMess( rgd.player, NetworkClient.MessType_ToClient.NewDomainObjPair, nextObjPairString );
//				NetworkClient.Instance.SendClientMess( rgd.player, NetworkClient.MessType_ToClient.SwanReadyToMove );
//			}
//			else
//			{
//				SendWinnerMessage( );
//				EndGame( "No more object pairs for this domain!", null );
//			}
//		}
//	}
//	
//	override public void Mess_DomainObjectIDsDeleted( NetworkPlayer player, string[] args )
//	{
//		RunningGameData_1player rgd = this;
//		
//		rgd.playerData.storyObjectIDsDeleted = args;
//		rgd.playerData.state = GameStateClient.StateEnum.WaitingForDomObjPair;
//		
//		if( playerData.storyObjectIDsDeleted != null )
//		{
//			//collect up the intersection
//			//TODO
//			DebugConsole.LogError( "RunningGameData_1player.Mess_DomainObjectIDsDeleted: STUB" );
//		}		
//	}
//	
//	override public void Mess_DomainObjectIDsNOTDeleted( NetworkPlayer player, string[] args )
//	{
//		RunningGameData_1player rgd = this;
//	
//		rgd.playerData.storyObjectIDsNOTDeleted = args;
//		rgd.playerData.state = GameStateClient.StateEnum.WaitingForDomObjPair;
//		
//		if( rgd.playerData.storyObjectIDsNOTDeleted != null )
//		{
//			//collect up the intersection
//			//TODO
//			DebugConsole.LogError( "RunningGameData_1player.Mess_DomainObjectIDsNOTDeleted: STUB" );
//		}
//
//				
//	}
//	
//	override public void Mess_DomainObjectNamesAdded( NetworkPlayer player, string[] args )
//	{
//
//		RunningGameData_1player rgd = this;
//		
//		rgd.playerData.storyObjectNamesAdded = args;
//		rgd.playerData.state = GameStateClient.StateEnum.WaitingForDomObjPair;
//		
//		if( rgd.playerData.storyObjectNamesAdded != null )
//		{
//			//collect up the intersection
//			//TODO
//			DebugConsole.LogError( "RunningGameData_1player.Mess_DomainObjectNamesAdded: STUB" );
//		}
//
//	}
//		
//	override public void Mess_SelectedRelation( NetworkPlayer player, string[] args )
//	{
//		RunningGameData_1player rgd = this;
//		
//		playerData.selectedRelationID = args[1];//objpairid+ "," + relationid+","+GameStateClient.Instance.millisecChoosing;
//		playerData.millisecChoosingRelation = System.Int32.Parse( args[2] );
//		
//		this.insertResultInto1PlayerTable();
//		this.updateSOPCountsAndStatus(); //update confidence and done status
//		
//		gss.hsObjpairidInPlay.Remove( objpairidUnderESP ); 
//		
//		if( rgd.isObjpairidFromSinglePlayerTable ){ //if we have something to compare to
//			if( rgd.npcRelationid == null )
//				NetworkClient.Instance.SendClientMess( this.player, NetworkClient.MessType_ToClient.FirstToSelectRelation );
//			else if( playerData.millisecChoosingRelation < rgd.npcMillisecChoose )
//				NetworkClient.Instance.SendClientMess( this.player, NetworkClient.MessType_ToClient.FirstToSelectRelation );
//		}
//		else if( GameStateServer.rand.Next( 100 ) >= 50 )
//		{//50-50 chance of getting it first against computer (ie no previous answer)
//			NetworkClient.Instance.SendClientMess( this.player, NetworkClient.MessType_ToClient.FirstToSelectRelation );
//		}
//		
//		if( this.npcRelationid == null )
//		{ //have no goldstandard, nor n-1 result; randomly assign winning
//			if( GameStateServer.rand.Next(100) <= 32 )// 1 in 3 wins!
//			{
//				this.iNumCorrect++;
//				bRelationMatchResultAgree = true;
//			}
//		}
//		else
//		{ //have an npcRelation (ie n-1 or goldstandard)
//			int iplayerRelID = Convert.ToInt32( playerData.selectedRelationID );
//			int inpcRelID = Convert.ToInt32( npcRelationid );
//			int iplayerRelClassID = gss.drelationidTOrelationClass[ iplayerRelID ] ;
//			int inpcRelClassID = gss.drelationidTOrelationClass[ inpcRelID ];
//			
//			if( iplayerRelClassID == inpcRelClassID )
//			{//class agreement: n-1/goldstandard are in the same class as user answer
//				//EXACT MATCH: else if( pdPlayer1.selectedRelationID.Equals( pdPlayer2.selectedRelationID, StringComparison.OrdinalIgnoreCase ) )
//				//CLASS MATCH: else if( gss.drelationidTOrelationClass[ Convert.ToInt32( playerData.selectedRelationID ) ]
//				//	     == gss.drelationidTOrelationClass[ Convert.ToInt32( npcRelationid ) ] )
//				this.iNumCorrect++;
//				bRelationMatchResultAgree = true;
//				this.iRelationClassAgree++;
//			}//else n-1/goldstandard not in the same class
//			
//			if( iplayerRelID == inpcRelID )
//			{//exact agreement
//				this.iRelationIDAgree++;
//			}
//		}
//
//	}
//	
//	override public void Mess_SwanAtEndOfScreen( NetworkPlayer player, string[] args )
//	{
//		RunningGameData_1player rgd = this;
//		rgd.playerData.isSwanAtEnd = true;
//		
//		//TODO(?) rgd.state = GameStateClient.StateEnum.WaitingForDomObjPair;
//		
//		if( args != null && args.Length >=1 )
//		{
//			rgd.playerData.playerScore = Convert.ToInt32( args[0] );
//		}else{
//			Debug.LogWarning( "GameStateClient(1p).MessageFromServer: Mess_SwanAtEndOfScreen did not come with a score." );
//		}
//		
//		//grade the submitted pairs, and inform clients about (dis)agreement
//		//if( this.isObjpairidFromSinglePlayerTable == true )
//		//{
//		//}
//		
//		if( this.playerData.selectedRelationID == null )
//		{//user did not choose a relation; null never has agreement (send disagree), but record the no-answer so we don't show this to player again
//			playerData.millisecChoosingRelation = -1;
//			if( args != null && args.Length >= 2 ){
//				try{ 
//					playerData.millisecChoosingRelation = Convert.ToInt32( args[1] );
//				}catch(OverflowException oe){
//					playerData.millisecChoosingRelation = int.MaxValue;
//				}
//			}
//			
//			this.insertResultInto1PlayerTable();
//		}
//
//		NetworkClient.Instance.SendClientMess( this.player, NetworkClient.MessType_ToClient.RelationMatchResult, this.bRelationMatchResultAgree ? "agree" : "disagree" );
//		
//		gss.hsObjpairidInPlay.Remove( objpairidUnderESP ); //have to do this because we never were sent a relation for this object pair by this player
//		
//		//update state variables
//		rgd.roundNumber++;
//		playerData.isSwanAtEnd = false;
//		playerData.selectedRelationID = null;
//		this.bRelationMatchResultAgree = false;
//			
//		if( rgd.roundNumber < GameStateClient.MAXIMUM_ROUNDS ) //TODO different game rules? ie lives instead of max rules?
//		{//send the next relation
//			string nextObjPairString = MakeNextObjPairString();
//			if( nextObjPairString.Length > 0 )
//			{
//				NetworkClient.Instance.SendClientMess( this.player, NetworkClient.MessType_ToClient.NewDomainObjPair, nextObjPairString );
//				
//				//tell the player to go ahead.
//				NetworkClient.Instance.SendClientMess( this.player, NetworkClient.MessType_ToClient.SwanReadyToMove );
//			}else{
//				if( (this.iNumCorrect / ((float)this.iNumSeen)) > 0.5 )
//				{
//					SendWinnerMessage( );
//				}
//
//				EndGame( "No more object pairs for this domain!", null );
//			}
//		}
//		else{ //max rounds exceeded
//			SendWinnerMessage( );
//			EndGame( "Maximum number of rounds reached.", null );
//		}					
//			
//	}
	
	/*
	private void tmpDeleteThisMethod()
	{
					//System.Data.IDbTransaction transaction = gss.db.TransactionBegin();
			try
    			{
					System.Data.IDataReader reader = null;
					//update relation table
					//TODO use parameterized query for faster, safer inserts
					//http://stackoverflow.com/questions/904796/how-do-i-get-around-the-problem-in-sqlite-and-c/926251#926251
					System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
					sbSQL.Append( "INSERT OR FAIL INTO objectPairRelation (domainid, objpairid, relationid_p1, relationid_p2) VALUES (" );
					sbSQL.Append( rgd.domainID ).Append( ", " );
					sbSQL.Append( rgd.objpairidUnderESP ).Append( ", " );
					sbSQL.Append( playerData.selectedRelationID == null ? "NULL" : playerData.selectedRelationID ).Append( ", " ); //TODO verify
					sbSQL.Append( this.npcRelationid == null ? "NULL" : this.npcRelationid ); //TODO verify
					sbSQL.Append( " )" );
					
					//reader = gss.db.TransactionExecute( sbSQL.ToString() );
					reader = gss.db.BasicQuery( sbSQL.ToString() );
					
					//delete from single player table
					sbSQL.Remove( 0, sbSQL.Length );
					sbSQL.Append( "DELETE FROM objectPairRelation_1player WHERE objpairrelid=" ).Append( this.objpairrelid );
					
					//reader = gss.db.TransactionExecute( sbSQL.ToString() );
					reader = gss.db.BasicQuery( sbSQL.ToString() );
					
					//gss.db.TransactionEnd( transaction );
				}
		        catch (Exception ex)
		        {
		            Console.WriteLine("Commit Exception Type (1p) (to storyObject, during handling deleted objects): {0}", ex.GetType());
		            Console.WriteLine("  Message: {0}", ex.Message);
		
		            // Attempt to roll back the transaction. 
		            try
		            {
		                //transaction.Rollback();
		            }
		            catch (Exception ex2)
		            {
		                // This catch block will handle any errors that may have occurred 
		                // on the server that would cause the rollback to fail, such as 
		                // a closed connection.
		                Console.WriteLine("(1p) Rollback Exception Type: {0}", ex2.GetType());
		                Console.WriteLine("  Message: {0}", ex2.Message);
		            }
				}

	}
	*/
	
//	private void insertResultInto1PlayerTable()
//	{
//		RunningGameData_1player rgd = this;
//		this.iNumSeen++;
//		System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
//		sbSQL.Append( "INSERT OR FAIL INTO objectPairRelation_1player (domainid, objpairid, relationid, playerid, millisecChoose) VALUES (" );
//		sbSQL.Append( rgd.domainID ).Append( ", " );
//		sbSQL.Append( rgd.objpairidUnderESP ).Append( ", " );
//		sbSQL.Append( playerData.selectedRelationID == null ? "NULL" : playerData.selectedRelationID ).Append(", ");
//		sbSQL.Append( playerData.playerid ).Append( ", " );
//		sbSQL.Append( playerData.millisecChoosingRelation );
//		sbSQL.Append( " )" );
//		System.Data.IDataReader res = gss.db.BasicQuery( sbSQL.ToString() );
//	}
//	
//	//objpairCountsTable: objpairid, relationClass, numVotesInClass 
//	//storyObjectPair: numTimesSeen, numESPAgree, intConfidence
//	private void updateSOPCountsAndStatus()
//	{
//		//select * from ( SELECT rel1.relationClass as relationClass, count(relationClass) as cntRC, cntTot FROM objectPairRelation_1player opr1p LEFT JOIN relation rel1 on opr1p.relationid = rel1.relationid   left join ( SELECT objpairid , count(*) as cntTot FROM objectPairRelation_1player WHERE relationid IS NOT NULL AND objpairid = 2) as t  WHERE opr1p.objpairid = 2 GROUP BY relationClass ORDER BY count(relationClass) DESC) ;
//		//relationClass  cntRC       cntTot
//		RunningGameData_1player rgd = this;
//		
//		//finds the relation class with the most number of votes for a given object pair
//		System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
//		sbSQL.Append( "SELECT relationClass, cntRC, cntTot FROM" );
//		sbSQL.Append( " ( SELECT rel1.relationClass as relationClass, count(relationClass) as cntRC, cntTot FROM objectPairRelation_1player opr1p" );
//		sbSQL.Append( "   LEFT JOIN relation rel1 ON opr1p.relationid = rel1.relationid" );
//		sbSQL.Append( "   LEFT JOIN ( SELECT objpairid, count(*) as cntTot FROM objectPairRelation_1player WHERE relationid IS NOT NULL AND objpairid = " );
//		sbSQL.Append( rgd.objpairidUnderESP ).Append( ") as t" );
//		sbSQL.Append( " WHERE opr1p.objpairid = " ).Append( rgd.objpairidUnderESP ); 
//		sbSQL.Append( " GROUP BY relationClass ORDER BY count(relationClass) DESC LIMIT 1) " );
//		System.Data.IDataReader res = gss.db.BasicQuery( sbSQL.ToString() );
//		
//		System.Text.StringBuilder sb = new System.Text.StringBuilder();
//		if( res.Read() ){
//			//relationClass  cntRC       cntTot
//			int tmpI = 0;
//			if( sb.Length > 0 ) sb.Append( "," );
//			int relationClass = res.GetInt32( tmpI++ ); 
//			int cntRC = res.GetInt32( tmpI++ );
//			int cntTot = res.GetInt32( tmpI++ );
//			
//			sb.Append( relationClass.ToString() ).Append( "," ).Append( cntRC.ToString() ).Append( "," ).Append( cntTot.ToString() );
//			
//			if( cntTot >= 2 )
//			{//can only do confidence stuff if we have 2 or more non-null answers
//				double curConf = gss.valStrat.CurrentConfidence( gss.uniqueRelationClasses.Count , cntTot, cntRC );
//				DebugConsole.Log( "curConf: " + curConf );				
//				
//				sbSQL = new System.Text.StringBuilder();
//				sbSQL.Append( "UPDATE OR IGNORE storyObjectPair SET" );
//				if( cntTot >= GameStateServer.MAXIMUM_ANSWERS_BEFORE_FAIL )//make sure we don't beat our heads against the wall (quit after N times)
//				{	
//					DebugConsole.Log( "Reached MAXIMUM_ANSWERS_BEFORE_FAIL; flagging as done." );
//					sbSQL.Append( " bDone=1, " );
//				}
//				else if( curConf >= GameStateServer.MINIMUM_CONFIDENCE_RELATION )
//				{
//					DebugConsole.Log( "Reached MINIMUM_CONFIDENCE_RELATION; flagging as done." );
//					sbSQL.Append( " bDone=1, " );
//				}
//				else//mark it done if above confidence, zero otherwise
//					sbSQL.Append( " bDone=0, " );
//				sbSQL.Append( " numESPAgree=" ).Append( cntRC ).Append( ", " );
//				sbSQL.Append( " intConfidence=" ).Append( ( (int)Math.Floor( 100 * curConf ) ).ToString() ).Append( ", " );
//				sbSQL.Append( " iRelationClass=" ).Append( relationClass );
//				sbSQL.Append( " WHERE objpairid=" ).Append( this.objpairidUnderESP );
//				gss.db.BasicQuery( sbSQL.ToString() );
//				
//				//if( gss.valStrat.IsConfident( gss.drelationidTOrelationClass.Count, cntTot, cntRC, GameStateServer.MINIMUM_CONFIDENCE_RELATION ) )
//				//{	}
//				//else
//				//{//how many more are needed?
//				//	//gss.valStrat.RequiredForAgreement(
//				//}
//			}
//		}
//		
//		DebugConsole.Log( "updateSOPCountsAndStatus Query result: " );
//		DebugConsole.Log( sb.ToString() );
//		
//		
//	}	
	
	override public void Mess_PlayerHasNoLives( NetworkPlayer player, string[] args )
	{
//		if( (this.iNumCorrect / ((float)this.iNumSeen)) > 0.5 )
//		{
//			SendWinnerMessage( );
//		}
		
		EndGame( "You are out of lives.", null );
	}	
#endif
}
