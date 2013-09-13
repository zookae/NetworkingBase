using UnityEngine;
using System.Collections;

public class MouseDrag : MonoBehaviour {

    private Vector3 screenPoint;
    private Vector3 offset;

    void OnMouseDown() {
        DebugConsole.Log("Mouse Down");
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
        offset = gameObject.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));
    }

    void OnMouseDrag() {
        DebugConsole.Log("Mouse Drag");
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = curPosition;

        //networkView.RPC("SendMovement", RPCMode.OthersBuffered, transform.position, transform.rotation);
    }

    void OnMouseExit() {
        DebugConsole.Log("Mouse Exit");
    }
}
