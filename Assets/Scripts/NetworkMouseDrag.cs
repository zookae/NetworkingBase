using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))] // require NetworkView exists for use of RPC
public class NetworkMouseDrag : MonoBehaviour {

    private Vector3 screenPoint;
    private Vector3 offset;

    void OnMouseDown() {
        DebugConsole.Log("On Mouse Down: ");
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseDrag() {
        DebugConsole.Log("On Mouse Drag: ");
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = curPosition;

        networkView.RPC("SendMovement", RPCMode.OthersBuffered, transform.position, transform.rotation);
    }

    void OnMouseExit() {
        DebugConsole.Log("On Mouse Exit: ");
    }

    [RPC]
    void SendMovement(Vector3 position1, Quaternion rotation1) {
        //DebugConsole.Log("NetworkMouseDrag.[RPC]SendMovement to viewID : " + gameObject.GetComponent<NetworkView>().viewID);
        DebugConsole.Log("NetworkMouseDrag.[RPC]SendMovement to viewID");
        transform.position = position1;
        transform.rotation = rotation1;
    }
}
