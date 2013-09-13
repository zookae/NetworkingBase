using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NetworkView))]
public class NetworkScore : MonoBehaviour {

    public int currentScore;

    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        int score = 0;

        DebugConsole.Log("score: " + score);
        DebugConsole.Log("currentScore: " + currentScore);

        if (stream.isWriting) {
            score = currentScore;
            stream.Serialize(ref score);
        } else {
            stream.Serialize(ref score);
            currentScore = score;
        }

        DebugConsole.Log("score: " + score);
        DebugConsole.Log("currentScore: " + currentScore);

    }

    void OnMouseDown() {
        DebugConsole.Log("current score: " + currentScore);
        currentScore += 1; // or score?
    }
}
