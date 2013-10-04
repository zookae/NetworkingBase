using UnityEngine;
#if UNITY_WEBPLAYER
#else
using System.Collections;
using System.Collections.Generic;
using Mono.Data.Sqlite;
using System.Data;
#endif

//explanation of dbAccess.js http://wiki.unity3d.com/index.php?title=SQLite
//on getting required dlls: http://answers.unity3d.com/questions/188334/unity-sql-database.html
//suggestion that dlls come from Unity release: http://forum.unity3d.com/threads/7866-Unity-and-Sqlite

//on required build settings in unity: http://docs.unity3d.com/Documentation/Manual/class-PlayerSettings.html
//on build alternatives: http://answers.unity3d.com/questions/134307/sqlite-runs-perfect-in-editor-doesnt-get-included.html
//managed vs unmanaged dlls and unity indy/pro: http://forum.unity3d.com/threads/56013-Can-I-use-DLLs-in-Unity-free
//  search for managed dll's that are .net 2.0 compliant can be used in indy, unmanaged dll's require pro

//suggestion to view sqlite database with external browser prog:
//  http://sqlitebrowser.sourceforge.net/index.html

/*  c# class for accessing SQLite objects.  
     To use it, you need to make sure you COPY Mono.Data.SQLiteClient.dll from wherever it lives in your Unity directory
     to your project's Assets folder
     Originally created by dklompmaker in 2009, 
     http://forum.unity3d.com/threads/28500-SQLite-Class-Easier-Database-Stuff    
     Modified 2011 by Alan Chatham
     Modified 2013 by SMLU to run in c#           */

/*
 * Just want to thank everyone for your help. I finally got the sqlite db working. The only thing I was missing was to download Mono.Data.SqliteClient.dll and System.Data.dll and put them in the Assets folder of my project. Oh, and the file name matching the classname. And if you don't have the sqlite3.dll in your C:\Program Files\Unity\Editor directory, you'll have to put one there,as well.
C:\Program Files (x86)\Unity\Editor\Data\Mono\lib\mono\2.0
*/

public class DBAccess //: MonoBehaviour 
{
#if UNITY_WEBPLAYER
#else
	// Use this for initialization
	//void Start () {}
	
	// Update is called once per frame
	//void Update () {}
	
	// variables for basic query access
    private string connection;
    private IDbConnection dbcon;
    private IDbCommand dbcmd;
    private IDataReader reader;

    public void OpenDB(string p){
	    connection = "URI=file:" + p; // we set the connection to our database
	    dbcon = new SqliteConnection(connection);
	    dbcon.Open();
    }
 
	//SMLU added
	public IDbTransaction TransactionBegin()
	{
		dbcmd = dbcon.CreateCommand(); // create empty command
		IDbTransaction trans = dbcon.BeginTransaction();
		//http://msdn.microsoft.com/en-us/library/system.data.idbtransaction.commit.aspx
		// Must assign both transaction object and connection 
        // to Command object for a pending local transaction
		dbcmd.Connection = dbcon;
		dbcmd.Transaction = trans;
		
		return trans;
	}
	
	//SMLU
	//ONLY use this once a transaction has begun; only do INSERT or UPDATE (or DELETE?)
	//see http://msdn.microsoft.com/en-us/library/system.data.idbtransaction.rollback.aspx
	public IDataReader TransactionExecute( string sqlCommand )
	{
		Debug.Log( "DBAccess.TransactionExecute: " + sqlCommand );
		dbcmd.CommandText = sqlCommand;
		//dbcmd.ExecuteNonQuery();
		return reader = dbcmd.ExecuteReader();
	}
	
	//SMLU
	public void TransactionEnd( IDbTransaction trans )
	{
		trans.Commit();
	}
	
    public IDataReader BasicQuery(string q){ // run a baic Sqlite query
		Debug.Log( "DBAccess.BasicQuery: " + q );
        dbcmd = dbcon.CreateCommand(); // create empty command
        dbcmd.CommandText = q; // fill the command
        reader = dbcmd.ExecuteReader(); // execute command which returns a reader
        return reader; // return the reader
    }
 
    // This returns a 2 dimensional ArrayList with all the
    //  data from the table requested
    public ArrayList ReadFullTable(string tableName){
        string query;
        query = "SELECT * FROM " + tableName;	
        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = query; 
        reader = dbcmd.ExecuteReader();
        var readArray = new ArrayList();
        while(reader.Read()){ 
            var lineArray = new ArrayList();
            for (int i = 0; i < reader.FieldCount; i++)
                lineArray.Add(reader.GetValue(i)); // This reads the entries in a row
            readArray.Add(lineArray); // This makes an array of all the rows
        }
        return readArray; // return matches
    }
 
    // This function deletes all the data in the given table.  Forever.  WATCH OUT! Use sparingly, if at all
    public void DeleteTableContents(string tableName){
	    string query;
	    query = "DELETE FROM " + tableName;
	    dbcmd = dbcon.CreateCommand();
	    dbcmd.CommandText = query; 
	    reader = dbcmd.ExecuteReader();
    }
 
				    /*
    // Let's make sure we've got a table to work with as well!
    var tableName = TableName;
    var columnNames = new Array("firstName","lastName");
    var columnValues = new Array("text","text");
    try {db.CreateTable(tableName,columnNames,columnValues);
    }
    catch(e){// Do nothing - our table was already created
        //- we don't care about the error, we just don't want to see it
  */
    public void CreateTable(string name, ArrayList col, ArrayList colType){ // Create a table, name, column array, column type array
        string query;
        query  = "CREATE TABLE " + name + "(" + col[0] + " " + colType[0];
        for(var i=1; i<col.Count; i++){
            query += ", " + col[i] + " " + colType[i];
        }
        query += ")";
        dbcmd = dbcon.CreateCommand(); // create empty command
        dbcmd.CommandText = query; // fill the command
        reader = dbcmd.ExecuteReader(); // execute command which returns a reader
 
    }
 
    public void InsertIntoSingle(string tableName, string colName, string value){ // single insert 
        string query;
        query = "INSERT INTO " + tableName + "(" + colName + ") " + "VALUES (" + value + ")";
        dbcmd = dbcon.CreateCommand(); // create empty command
        dbcmd.CommandText = query; // fill the command
        reader = dbcmd.ExecuteReader(); // execute command which returns a reader
    }
 
    public void InsertIntoSpecific(string tableName, ArrayList col, ArrayList values){ // Specific insert with col and values
        string query;
        query = "INSERT INTO " + tableName + "(" + col[0];
        for(var i=1; i<col.Count; i++){
            query += ", " + col[i];
        }
        query += ") VALUES (" + values[0];
        for(int i=1; i<values.Count; i++){
            query += ", " + values[i];
        }
        query += ")";
        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = query; 
        reader = dbcmd.ExecuteReader();
    }
 
    public void InsertInto(string tableName, ArrayList values){ // basic Insert with just values
        string  query;
        query = "INSERT INTO " + tableName + " VALUES (" + values[0];
        for(var i=1; i<values.Count; i++){
            query += ", " + values[i];
        }
        query += ")";
        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = query; 
        reader = dbcmd.ExecuteReader(); 
    }
 
    // This function reads a single column
    //  wCol is the WHERE column, wPar is the operator you want to use to compare with, 
    //  and wValue is the value you want to compare against.
    //  Ex. - SingleSelectWhere("puppies", "breed", "earType", "=", "floppy")
    //  returns an array of matches from the command: SELECT breed FROM puppies WHERE earType = floppy;
    public ArrayList SingleSelectWhere(string tableName, string itemToSelect, string wCol, string wPar, string wValue){ // Selects a single Item
        string query;
        query = "SELECT " + itemToSelect + " FROM " + tableName + " WHERE " + wCol + wPar + wValue;	
        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = query; 
        reader = dbcmd.ExecuteReader();
        var readArray = new ArrayList();
        while(reader.Read()){ 
            readArray.Add(reader.GetString(0)); // Fill array with all matches
        }
        return readArray; // return matches
    }
	
	//added by SMLU
	public List<string> TryEnableForeignKeys(){
		string query = "PRAGMA foreign_keys = ON";
		dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = query; 
        reader = dbcmd.ExecuteReader();
        List<string> readArray = new List<string>();
        while(reader.Read()){ 
            readArray.Add(reader.GetString(0)); // Fill array with all matches
        }
        return readArray; // return matches
	}

    //added by SMLU
    public int QueryForeignKeys() {
        string query = "PRAGMA foreign_keys";
        dbcmd = dbcon.CreateCommand();
        dbcmd.CommandText = query;
        reader = dbcmd.ExecuteReader();
        int ans = 0;
        while (reader.Read()) {
            if (!reader.IsDBNull(0)) {
                ans = reader.GetInt32(0);                
            } else
                DebugConsole.Log("PRAGMA foreign_keys returned null");
        }
        //DebugConsole.Log("PRAGMA foreign_keys: " + ans);
        return ans; // return matches
    }	

	//added by SMLU
	public List<string> GetAllTableNames()
	{
		string query = "select tbl_name from sqlite_master where type='table' OR type='view'";
		reader = this.BasicQuery( query );
		List<string> readArray = new List<string>();
        while(reader.Read()){ 
            readArray.Add( reader.GetString(0) ); // Fill array with all matches
        }
        return readArray; 
	}
 
    public void CloseDB(){
        reader.Close(); // clean everything up
        reader = null; 
        dbcmd.Dispose(); 
        dbcmd = null; 
        dbcon.Close(); 
        dbcon = null; 
    }
#endif
}
