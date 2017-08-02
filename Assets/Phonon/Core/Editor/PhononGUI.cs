//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using UnityEngine;
using UnityEditor;

namespace Phonon
{
    //
    // GUI helper class for Phonon.
    //
    public static class PhononGUI
    {
        public static void SectionHeader(string title)
        {
#if UNITY_5
            GUIStyle headingStyle = new GUIStyle(EditorStyles.helpBox);
#else
            GUIStyle headingStyle = new GUIStyle(EditorStyles.largeLabel);
#endif
            headingStyle.font = EditorStyles.boldLabel.font;
            headingStyle.fontStyle = FontStyle.Bold;
    
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(title, headingStyle);
        }
    }
}