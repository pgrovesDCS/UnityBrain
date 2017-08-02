//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using UnityEditor;
using UnityEngine;

namespace Phonon
{

    //
    // PhononGeometryInspector
    // Custom inspector for the AcousticGeometry class.
    //

    [CustomEditor(typeof(PhononGeometry))]
    public class PhononGeometryInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            PhononGeometry geometry = serializedObject.targetObject as PhononGeometry;

            EditorGUILayout.Space();
            bool toggleValue = serializedObject.FindProperty("exportAllChildren").boolValue;
            if (geometry.transform.childCount != 0)
                serializedObject.FindProperty("exportAllChildren").boolValue = GUILayout.Toggle(toggleValue, " Export All Children");

            PhononGUI.SectionHeader("Geometry Statistics");
            EditorGUILayout.LabelField("Vertices", geometry.GetNumVertices().ToString());
            EditorGUILayout.LabelField("Triangles", geometry.GetNumTriangles().ToString());

            if (geometry.gameObject.GetComponent<Terrain>() != null)
            {
                PhononGUI.SectionHeader("Terrain Export Settings");
                EditorGUILayout.PropertyField(serializedObject.FindProperty("TerrainSimplificationLevel"));
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }
    }
}