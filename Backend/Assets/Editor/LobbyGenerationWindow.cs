using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LobbyGenerationWindow : EditorWindow
{
    string lobbyServer;

    [MenuItem("Custom Windows/Lobby Generation")]
    public static void ShowWindow()
    {
        GetWindow<LobbyGenerationWindow>("Lobby Generation");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Whole map generation");
        lobbyServer = EditorGUILayout.TextField("Server IP", lobbyServer);
        if (GUILayout.Button("Get Lobby Data from Server"))
        {

        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate new Lobby Data"))
        {

        }
    }
}
