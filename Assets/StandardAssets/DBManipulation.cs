using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DBManipulation  {

    //TODO: consider aspect oriented programming for open/close of db on each method
    //http://stackoverflow.com/questions/9886144/add-method-call-to-each-method-in-a-class
    //http://www.developerfusion.com/article/5307/aspect-oriented-programming-using-net/

    public static bool keepOpen = false;
    public static bool enableForeignKeys = false;
    private string dbFileName = null;

    private DBAccess db {
         set;        
         get;        
    }

    public DBManipulation(string inDBFileName, bool inKeepOpen, bool inEnableForeignKeys ) {
        db = new DBAccess();
        dbFileName = inDBFileName;
        keepOpen = inKeepOpen;
        enableForeignKeys = inEnableForeignKeys;
    }

    //manually called on exit, unless you make this class extend from MonoBehavior (and then it will be called for you automatically)
    public void OnApplicationQuit() {
        if (keepOpen) {
            db.CloseDB();
        }
    }

    private void openConnection() {
        if (!keepOpen) {
            db.OpenDB(this.dbFileName);

            db.QueryForeignKeys();
            DebugConsole.Log("Open connection done (with trying to enable foreign keys)");
            if( enableForeignKeys ) 
                db.TryEnableForeignKeys();
            db.QueryForeignKeys();
       }
    }
    private void closeConnection() {
        if (!keepOpen) {
            db.CloseDB();
        }
    }

    public void TryEnableForeignKeys() {
        openConnection();
        db.TryEnableForeignKeys();
        closeConnection();
    }

    //returns -1 if the player doesn't exist
    internal int getPlayerID(string udid) {
        openConnection();
        
        Debug.Log("getPlayerID");
        int playerid = -1;
        System.Text.StringBuilder sbSQLSelect = new System.Text.StringBuilder();

        sbSQLSelect.Append("SELECT playerid FROM player WHERE udid='").Append(udid).Append("'");

        System.Data.IDataReader res = db.BasicQuery(sbSQLSelect.ToString());

        if (res.Read()) {//player already exists; return the id
            playerid = res.GetInt32(0); //WELCOME BACK!
        }//else we don't have a player id and need to get one (return -1)
        
        closeConnection();
        return playerid;
    }

    //returns null if the player doesn't exist
    internal string getPlayerUDID( int playerid )
    {
        openConnection();

        Debug.Log( "getPlayerUDID" );
        string playerUDID = null;
        System.Text.StringBuilder sbSQLSelect = new System.Text.StringBuilder();

        sbSQLSelect.Append( "SELECT udid FROM player WHERE playerid=" ).Append( playerid );

        System.Data.IDataReader res = db.BasicQuery( sbSQLSelect.ToString() );

        if (res.Read())
        {//player already exists; return the udid
            playerUDID = res.GetString( 0 );
        }//else we don't have a player with that id 

        closeConnection();
        return playerUDID;
    }

    internal int createPlayerID(string udid) {
        Debug.Log("createPlayerID");
        openConnection();

        int playerid = -1;
        System.Text.StringBuilder sbSQLInsert = new System.Text.StringBuilder();
        sbSQLInsert.Append("INSERT OR IGNORE INTO player (udid) VALUES ( '").Append(udid).Append("' )");
        db.BasicQuery(sbSQLInsert.ToString()); //inserts

        System.Text.StringBuilder sbSQLSelect = new System.Text.StringBuilder();
        sbSQLSelect.Append("SELECT playerid FROM player WHERE udid='").Append(udid).Append("'");

        System.Data.IDataReader res = db.BasicQuery(sbSQLSelect.ToString()); //get the new playerid
        if (res.Read()) {//got it, as expected
            playerid = res.GetInt32(0); //FIRST TIME PLAYER
        } else {
            Debug.LogError("Failed to get playerid for udid just inserted. Very very odd, and evil.");
        }

        closeConnection();
        return playerid;
    }

    internal void dataDump(int gameid, int playerid, string data) {
        Debug.Log("dataDump");
        openConnection();

        System.Text.StringBuilder sbSQLInsert = new System.Text.StringBuilder();
        sbSQLInsert.Append("INSERT OR IGNORE INTO savedata (gameid, playerid, thedata) VALUES ( ").Append(gameid).Append(", ").Append(playerid).Append(", '").Append(data).Append("' )");
        db.BasicQuery(sbSQLInsert.ToString()); //inserts

        closeConnection();
    }

    //actually also checks views
    internal bool VerifyTableExistence( string[] allNames ) {
        openConnection();
        bool ans = true;

        List<string> tableNames = db.GetAllTableNames();
        DebugConsole.Log("ALL TABLE/VIEW NAMES: ");
        foreach (string name in tableNames) {
            DebugConsole.Log(name);
        }

        foreach (string name in allNames) {
            if (!tableNames.Contains(name)) {
                ans = false;
                break;
            }
        }

        closeConnection();
        return ans;
    }

    internal void SavePlayerScoreStats( PlayerData playerData, int iOfPlayAgain ) {
        System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
        sbSQL.Append("UPDATE OR IGNORE player SET gamesPlayed = gamesPlayed+1 WHERE playerid=").Append(playerData.playerid);
        db.BasicQuery(sbSQL.ToString());

        sbSQL.Remove(0, sbSQL.Length);
        sbSQL.Append("INSERT OR IGNORE INTO topscore(playerid,scoreValue,iOfPlayAgain) VALUES(");
        sbSQL.Append(playerData.playerid).Append(",");
        sbSQL.Append(playerData.playerScore).Append(",");
        sbSQL.Append(iOfPlayAgain).Append(")");
        db.BasicQuery(sbSQL.ToString());
    }

    //returns -1 if we don't have an answer in db
    internal int LookupMonteCarloResults_RequiredForAgreement(int choices, int trials, double confidence)
    {
        openConnection();
     	System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
		sbSQL.Append( "SELECT reqForAgreement FROM confidenceLookupMC WHERE" );
		sbSQL.Append( " choices=" ).Append( choices.ToString() );
		sbSQL.Append( " AND trials=" ).Append( trials.ToString() );
		sbSQL.Append( " AND intConfidence=" ).Append( ((int)confidence*100).ToString() );
		//sbSQL.Append( " AND reqForAgreement IS NOT NULL" );
		sbSQL.Append( " LIMIT 1" );
		
		//sbSQL.Append();
		//sbSQL.Append();
		
		System.Data.IDataReader res = this.db.BasicQuery( sbSQL.ToString() );
		
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		int reqForAgreement=-1;
		if( res.Read() ){ //TODO dispose of the readers http://stackoverflow.com/questions/744051/sqldatareader-dispose
			if( sb.Length > 0 ) sb.Append( "," );
			if( res.IsDBNull( 0 ) ) //means there is no minimum that will satisfy the given (choices,trials,confidence)
				reqForAgreement = -1; 
			else 
				reqForAgreement = res.GetInt32( 0 ); 
			


			sb.Append( reqForAgreement.ToString() );
		}
        closeConnection();
        return reqForAgreement;
    }

    internal void SaveMonteCarloResults_RequiredForAgreement(int choices, int trials, double confidence, int reqForAgreement) {
        openConnection();
        System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
        sbSQL = new System.Text.StringBuilder();
        sbSQL.Append("INSERT OR IGNORE INTO confidenceLookupMC(choices,trials,intConfidence,reqForAgreement) VALUES(");
        sbSQL.Append(choices.ToString()).Append(",");
        sbSQL.Append(trials.ToString()).Append(",");
        sbSQL.Append(((int)confidence * 100).ToString()).Append(",");
        sbSQL.Append(reqForAgreement == -1 ? "NULL" : reqForAgreement.ToString());
        sbSQL.Append(")");

        System.Data.IDataReader res2 = this.db.BasicQuery(sbSQL.ToString());
        closeConnection();
    }

    //returns -1 if we don't have an answer in db
    internal double LookupMonteCarloResults_ConfidenceOfOutcome(int choices, int trials, int biggestAnswer) {
        openConnection();

        System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
        sbSQL.Append("SELECT intConfidence FROM confidenceLookupMC WHERE");
        sbSQL.Append(" choices=").Append(choices.ToString());
        sbSQL.Append(" AND trials=").Append(trials.ToString());
        sbSQL.Append(" AND reqForAgreement=").Append(biggestAnswer);
        sbSQL.Append(" ORDER BY intConfidence DESC LIMIT 1");

        System.Data.IDataReader res = this.db.BasicQuery(sbSQL.ToString());

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        double doubConfidence = -1;
        if (res.Read()) { //TODO dispose of the readers http://stackoverflow.com/questions/744051/sqldatareader-dispose
            if (sb.Length > 0) sb.Append(",");
            if (res.IsDBNull(0)) //means there is no minimum that will satisfy the given (choices,trials,confidence)
                doubConfidence = -1;
            else
                doubConfidence = res.GetInt32(0) / 100.0;

            sb.Append(doubConfidence.ToString());
        }
        closeConnection();
        return doubConfidence;
    }

    internal void SaveMonteCarloResults_ConfidenceOfOutcome(int choices, int trials, int biggestAnswer, double doubConfidence) {
        openConnection();

        System.Text.StringBuilder sbSQL = new System.Text.StringBuilder();
        sbSQL = new System.Text.StringBuilder();
        sbSQL.Append("INSERT OR IGNORE INTO confidenceLookupMC(choices,trials,intConfidence,reqForAgreement) VALUES(");
        sbSQL.Append(choices.ToString()).Append(",");
        sbSQL.Append(trials.ToString()).Append(",");
        sbSQL.Append(((int)doubConfidence * 100).ToString()).Append(",");
        sbSQL.Append(biggestAnswer.ToString());
        sbSQL.Append(")");

        System.Data.IDataReader res2 = this.db.BasicQuery(sbSQL.ToString());

        closeConnection();
    }
}
