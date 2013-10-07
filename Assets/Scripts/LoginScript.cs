using UnityEngine;
using System.Collections;
using System;

//imagine this is your actual client side game -- this script shows how you might connect etc
public class LoginScript : MonoBehaviour {
    
    //IObserver<NetworkMailbox<NetworkClient.MessType_ToClient>.Envelope<NetworkClient.MessType_ToClient>>
    private SillyObserver messObserver = null;
    public class SillyObserver : IObserver<NetworkMailbox<NetworkClient.MessType_ToClient>.Envelope>
    {
        public IDisposable disposable{ get; set;}
        public virtual void OnNext( NetworkMailbox<NetworkClient.MessType_ToClient>.Envelope value )
        {
            DebugConsole.Log( "SillyObserver notified of message: " + value.ToString() );
            this.Unsubscribe();
        }

        public void Unsubscribe()
        {
            if (disposable != null) 
                disposable.Dispose(); 
        }
    }

    void OnGUI() {        
            GUILayout.BeginArea( new Rect(0,0,200,800) );
            Rect windowRectConfig = new Rect(20, 20, 150, 500);
            windowRectConfig = GUILayout.Window(0, windowRectConfig, ConfigWindow, "Config", GUILayout.Width(100));
            GUILayout.EndArea(); 
    }

    void ConfigWindow(int windowID) {
        //DebugConsole.IsOpen = true;
        if (GUILayout.Button("Quit")) {
            DebugConsole.Log("quit from login screen");
            Application.Quit();
        }
        if (GUILayout.Button("Connect To Server")) {
            if (NetworkClient.Instance.ConnectToFirstAvailable()) {
                DebugConsole.Log("I connected !");
                //Application.LoadLevel(2);
            }
        }
        if (GUILayout.Button("RPC - Spawn network box")) {
            //NetworkViewID viewID = Network.AllocateViewID();
            //networkView.RPC("SpawnBox", RPCMode.AllBuffered, viewID, transform.position);

            NetworkViewID nvid = Network.AllocateViewID();
            DebugConsole.Log("Calling 'SendAllSpawnBox' with network view ID: " + nvid);
            NetworkClient.Instance.SendAllSpawnBox( Vector3.zero, nvid );
            
            //consider doing something like http://docs.unity3d.com/Documentation/Components/net-NetworkInstantiate.html
            //Network.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), Vector3.zero, Quaternion.identity, 0);
        }
        if (GUILayout.Button("State Synchronization - Spawn network box")) {
            NetworkViewID nvid = Network.AllocateViewID();
            DebugConsole.Log("Calling 'SendAllSpawnBoxSync' with network view ID: " + nvid);
            NetworkClient.Instance.SendAllSpawnBoxSync(Vector3.zero, nvid);
            //consider doing something like http://docs.unity3d.com/Documentation/Components/net-NetworkInstantiate.html
            //Network.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), Vector3.zero, Quaternion.identity, 0);
        }
        if (GUILayout.Button("State Synchronization - sync global variable")) {
            NetworkViewID nvid = Network.AllocateViewID();
            DebugConsole.Log("Calling 'SendAllScoreSync' with network view ID: " + nvid);
            NetworkClient.Instance.SendAllScoreSync(Vector3.zero, nvid);
            //consider doing something like http://docs.unity3d.com/Documentation/Components/net-NetworkInstantiate.html
            //Network.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), Vector3.zero, Quaternion.identity, 0);
        }
        if( GUILayout.Button( "Spawn DB box" ) )
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.AddComponent<Rigidbody>();
            go.GetComponent<Rigidbody>().useGravity = false;

            //go.gameObject.tag = "DB_cube";
            go.name = "DB_cube";

            go.AddComponent<DBPositionToString>(); // attach component to enable DB string representation of position

            go.AddComponent<DBSaveString>(); // attach component to enable send message to server
            go.GetComponent<DBSaveString>().dbStr = go.GetComponent<DBPositionToString>(); // connect sender w/string-maker
        }
        if (GUILayout.Button("DB writing - position")) {
            DebugConsole.Log("searching for DB_cube");
            GameObject go = GameObject.Find("DB_cube");
            //GameObject[] goes = GameObject.FindGameObjectsWithTag("DB_cube"); // find DB cube from scene
            //foreach (GameObject go in goes) {
                DebugConsole.Log("sending DB_cube string for position: " + go.transform.position);
                go.GetComponent<DBSaveString>().SendStringToServer();
            //}

            // get all objects tagged saveable
            // invoke their DBstring construction
        }
        if (GUILayout.Button("DB writing - save all")) {
            DebugConsole.Log("searching for DB_cube");
            DBSaveString[] goes = GameObject.FindObjectsOfType(typeof(DBSaveString)) as DBSaveString[]; // find DB cube from scene
            foreach (DBSaveString go in goes) {
                //DebugConsole.Log("sending DBSave string for position: " + go.transform.position);
                DebugConsole.Log("sending DBSave string for position: " + go.dbStr.DBString());
                go.SendStringToServer();
            }

            // get all objects tagged saveable
            // invoke their DBstring construction
        }
        if (GUILayout.Button("DB writing - singleton-style")) {
            DebugConsole.Log("searching for DB_cube");
            GameObject go = GameObject.Find("DB_cube");
            DebugConsole.Log("sending DB_cube string for position: " + go.transform.position);
            DebugConsole.Log("DBClientProxy instance is: " + DBClientProxy.Instance.ToString());
            DBClientProxy.Instance.SaveToDB(go.transform.position.ToString());
        }
        if (GUILayout.Button("DB reading - udid")){
            DebugConsole.Log("DB reading - udid... start.");
            if (messObserver == null)
                messObserver = new SillyObserver();
            messObserver.disposable = GameStateClient.Instance.mailbox.Subscribe( messObserver, NetworkClient.MessType_ToClient.UNTYPED );
            NetworkClient.Instance.SendServerMess(NetworkClient.MessType_ToServer.ReadDBStr, "udid"); //not a blocking call
            DebugConsole.Log("DB reading - udid... done.");
        }
        if (GUILayout.Button("Toggle debug")) {
            DebugConsole.IsOpen = !DebugConsole.IsOpen;
        }
        if (GUILayout.Button("Network Info")) {
            //DebugConsole.IsOpen = true;
            DebugConsole.LogWarning("network: " + Network.peerType);
            if (networkView == null) {
                DebugConsole.Log("networkView == null");
                if (NetworkClient.Instance.target != null) {
                    DebugConsole.Log("Got target from client controller.");
                    DebugConsole.Log("networkView: " + NetworkClient.Instance.target.networkView);
                    DebugConsole.Log("networkView.viewID: " + NetworkClient.Instance.target.networkView.viewID);
                }
            } else {
                DebugConsole.Log("networkView != null" + networkView);
                /*
                if( networkView.viewID == null )
                {
                    DebugConsole.Log("networkView.viewID == null");
                }
                else
                {
                    DebugConsole.Log("networkView.viewID != null" + networkView.viewID);
                }

                DebugConsole.Log("networkView.group != null" + networkView.group);
                */
            }

        }
    }
}
