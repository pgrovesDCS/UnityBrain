//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Phonon
{

    //
    // PhononSourceInspector
    // Custom inspector for PhononSource components.
    //

    [CustomEditor(typeof(PhononSource))]
    [CanEditMultipleObjects]
    public class PhononSourceInspector : Editor
    {
        //
        // Draws the inspector.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Direct Sound UX
            PhononGUI.SectionHeader("Direct Sound");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("directBinauralEnabled"));
            if (serializedObject.FindProperty("directBinauralEnabled").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("hrtfInterpolation"), new GUIContent("HRTF Interpolation"));
            }

            serializedObject.FindProperty("directOcclusionMode").enumValueIndex = 
                EditorGUILayout.Popup("Direct Sound Occlusion", 
                serializedObject.FindProperty("directOcclusionMode").enumValueIndex, optionsOcclusion);

            if (serializedObject.FindProperty("directOcclusionMode").enumValueIndex 
                != (int) OcclusionMode.NoOcclusion)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("directOcclusionMethod"));
                if (serializedObject.FindProperty("directOcclusionMethod").enumValueIndex == 
                    (int)OcclusionMethod.Partial)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("partialOcclusionRadius"), 
                        new GUIContent("Source Radius (meters)"));
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("physicsBasedAttenuation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("airAbsorption"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("directMixFraction"));

            // Indirect Sound UX
            PhononGUI.SectionHeader("Reflected Sound");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableReflections"));

            if (serializedObject.FindProperty("enableReflections").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("sourceSimulationType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("indirectMixFraction"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("indirectBinauralEnabled"));

                EditorGUILayout.HelpBox("Go to Window > Phonon > Simulation to update the global simulation " +
                    "settings.", MessageType.Info);
                if (serializedObject.FindProperty("indirectBinauralEnabled").boolValue)
                    EditorGUILayout.HelpBox("The binaural setting is ignored if Phonon Listener component is " +
                        "attached with mixing enabled.", MessageType.Info);

                PhononSource phononEffect = serializedObject.targetObject as PhononSource;
                if (phononEffect.sourceSimulationType == SourceSimulationType.BakedStaticSource)
                {
                    BakedSourceGUI();
                    serializedObject.FindProperty("bakedStatsFoldout").boolValue = 
                        EditorGUILayout.Foldout(serializedObject.FindProperty("bakedStatsFoldout").boolValue, 
                        "Baked Static Source Statistics");
                    if (phononEffect.bakedStatsFoldout)
                        BakedSourceStatsGUI();
                }
            }

            // Save changes.
            serializedObject.ApplyModifiedProperties();
        }

        public void BakedSourceGUI()
        {
            PhononGUI.SectionHeader("Baked Static Source Settings");

            PhononSource bakedSource = serializedObject.targetObject as PhononSource;
            GUI.enabled = !PhononBaker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uniqueIdentifier"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("bakingRadius"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAllProbeBoxes"));

            if (!serializedObject.FindProperty("useAllProbeBoxes").boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("probeBoxes"), true);

            bakedSource.uniqueIdentifier = bakedSource.uniqueIdentifier.Trim();
            if (bakedSource.uniqueIdentifier.Length == 0)
                EditorGUILayout.HelpBox("You must specify a unique identifier name.", MessageType.Warning);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Bake Effect"))
            {
                if (bakedSource.uniqueIdentifier.Length == 0)
                    Debug.LogError("You must specify a unique identifier name.");
                else
                {
                    bakedSource.BeginBake();
                }
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            DisplayProgressBarAndCancel();
        }

        public void BakedSourceStatsGUI()
        {
            PhononSource bakedSource = serializedObject.targetObject as PhononSource;
            GUI.enabled = !PhononBaker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
            bakedSource.UpdateBakedDataStatistics();
            for (int i = 0; i < bakedSource.bakedProbeNames.Count; ++i)
                EditorGUILayout.LabelField(bakedSource.bakedProbeNames[i], (bakedSource.bakedProbeDataSizes[i] / 1000.0f).ToString("0.0") + " KB");
            EditorGUILayout.LabelField("Total Size", (bakedSource.bakedDataSize / 1000.0f).ToString("0.0") + " KB");
            GUI.enabled = true;
        }

        void DisplayProgressBarAndCancel()
        {
            PhononSource bakedSource = serializedObject.targetObject as PhononSource;
            bakedSource.phononBaker.DrawProgressBar();
            Repaint();
        }

        string[] optionsOcclusion = new string[] { "Off", "On, No Transmission", "On, Frequency Independent Transmission", "On, Frequency Dependent Transmission" };
    }
}