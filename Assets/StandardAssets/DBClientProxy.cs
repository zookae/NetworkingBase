using UnityEngine;
using System.Collections;

public class DBClientProxy : MonoBehaviour {


    public static DBClientProxy Instance { get; private set; }

// cf: http://clearcutgames.net/home/?p=437
    // Allow manipulation in editor and prevent duplicates
    void Awake () {
        // check for conflicting instances
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // destroy others that conflict
        }

        Instance = this; // save singleton instance

        DontDestroyOnLoad(gameObject); // ensure not destroyed b/t scenes
    }

    /// <summary>
    /// Save string in the database
    /// </summary>
    /// <param name="dbstr"></param>
    public void SaveToDB(string dbstr) {
        DebugConsole.Log("DBClientProxy:SaveToDB - saving to db the string: " + dbstr);
        NetworkClient.Instance.SendServerMess(NetworkClient.MessType_ToServer.SaveDBStr, dbstr);
    }
}
