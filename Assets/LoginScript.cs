using UnityEngine;
using System.Collections;

public class LoginScript : MonoBehaviour {

    void OnGUI() {        
            GUILayout.BeginArea( new Rect(0,0,150,800) );
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
        if (GUILayout.Button("Toggle debug"))
            DebugConsole.IsOpen = !DebugConsole.IsOpen;
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
