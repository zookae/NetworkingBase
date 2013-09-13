using UnityEngine;
using System.Collections;

public class DBSaveString : MonoBehaviour {

    public IDBToString dbStr;

    public void SendStringToServer() {
        DebugConsole.Log("sending server DBStr to save : " + dbStr.DBString());
        NetworkClient.Instance.SendServerMess(NetworkClient.MessType_ToServer.SaveDBStr, dbStr.DBString());
    }
}
