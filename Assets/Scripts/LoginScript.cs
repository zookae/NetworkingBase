using UnityEngine;
using System.Collections;

//imagine this is your actual client side game -- this script shows how you might connect etc
public class LoginScript : MonoBehaviour {

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
        if (GUILayout.Button("Spawn network box")) {
            //NetworkViewID viewID = Network.AllocateViewID();
            //networkView.RPC("SpawnBox", RPCMode.AllBuffered, viewID, transform.position);

            NetworkViewID nvid = Network.AllocateViewID();
            DebugConsole.Log("Calling 'SendAllSpawnBox' with network view ID: " + nvid);
            NetworkClient.Instance.SendAllSpawnBox( Vector3.zero, nvid );
            
            //consider doing something like http://docs.unity3d.com/Documentation/Components/net-NetworkInstantiate.html
            //Network.Instantiate(GameObject.CreatePrimitive(PrimitiveType.Cube), Vector3.zero, Quaternion.identity, 0);
        }
        if( GUILayout.Button( "Spawn local box" ) )
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = Vector3.zero;
            go.transform.rotation = Quaternion.identity;
            go.AddComponent<Rigidbody>();
            go.GetComponent<Rigidbody>().useGravity = false;
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
