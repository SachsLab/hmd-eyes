﻿using UnityEngine;
using UnityEditor;

using PupilLabs;

[CustomEditor(typeof(RequestController))]
public class RequestEditor : Editor
{
    private SerializedProperty ipProp;
    private SerializedProperty portProp;
    private SerializedProperty statusProb;
    private SerializedProperty isConnectingProb;

    public void OnEnable()
    {
        SerializedProperty requestProp = serializedObject.FindProperty("request");
        ipProp = requestProp.FindPropertyRelative("IP");
        portProp = requestProp.FindPropertyRelative("PORT");
        statusProb = requestProp.FindPropertyRelative("status");

        isConnectingProb = serializedObject.FindProperty("isConnecting");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        RequestController ctrl = serializedObject.targetObject as RequestController;
     
        DrawDefaultInspector();

        // request
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Connection", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Status",statusProb.stringValue);
        EditorGUILayout.PropertyField(ipProp,new GUIContent("IP"));
        EditorGUILayout.PropertyField(portProp,new GUIContent("PORT"));

        GUILayout.BeginHorizontal();
        
        string connectLabel = "Connect";
        GUI.enabled = !ctrl.IsConnected && Application.isPlaying;
        if (isConnectingProb.boolValue)
        {
            connectLabel = "Connecting ...";
            GUI.enabled = false;
        }
        if (GUILayout.Button(connectLabel))
        {
            ctrl.RunConnect();
        }

        GUI.enabled = ctrl.IsConnected;
        if (GUILayout.Button("Disconnect"))
        {
            ctrl.Disconnect();
        }

        GUI.enabled = true;
        GUILayout.EndHorizontal();
        
        serializedObject.ApplyModifiedProperties();
    }
}