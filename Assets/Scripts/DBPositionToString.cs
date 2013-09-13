using UnityEngine;
using System.Collections;

public class DBPositionToString : MonoBehaviour, IDBToString {

    public string DBString() {
        return transform.position.ToString();
    }

}
