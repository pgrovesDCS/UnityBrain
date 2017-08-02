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
    // PhononMixerInspector
    // Custom inspector for the PhononMixer component.
    //
    [CustomEditor(typeof(PhononListener))]
    public class PhononListenerInspector : Editor
    {
        //
        // Draws the inspector.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = !serializedObject.FindProperty("enableReverb").boolValue;
            PhononGUI.SectionHeader("Mixer Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("acceleratedMixing"));
            GUI.enabled = true;

            PhononGUI.SectionHeader("Rendering Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("indirectBinauralEnabled"));

            if (serializedObject.FindProperty("acceleratedMixing").boolValue && 
                serializedObject.FindProperty("indirectBinauralEnabled").boolValue)
                EditorGUILayout.HelpBox("The binaural settings on Phonon Source will be ignored.", 
                    MessageType.Info);

            GUI.enabled = !serializedObject.FindProperty("acceleratedMixing").boolValue;
            PhononGUI.SectionHeader("Reverb Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("enableReverb"));

            if (serializedObject.FindProperty("enableReverb").boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("reverbSimulationType"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("dryMixFraction"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("reverbMixFraction"));

                PhononListener phononListener = serializedObject.targetObject as PhononListener;
                if (phononListener.reverbSimulationType == ReverbSimulationType.BakedReverb)
                {
                    BakedReverbGUI();
                    serializedObject.FindProperty("bakedStatsFoldout").boolValue =
                        EditorGUILayout.Foldout(serializedObject.FindProperty("bakedStatsFoldout").boolValue,
                        "Baked Reverb Statistics");
                    if (phononListener.bakedStatsFoldout)
                        BakedReverbStatsGUI();
                }
            }
            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

        //
        // GUI for BakedReverb
        //
        public void BakedReverbGUI()
        {
            PhononGUI.SectionHeader("Baked Reverb Settings");

            PhononListener bakedReverb = serializedObject.targetObject as PhononListener;
            GUI.enabled = !PhononBaker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("useAllProbeBoxes"));
            if (!serializedObject.FindProperty("useAllProbeBoxes").boolValue)
                EditorGUILayout.PropertyField(serializedObject.FindProperty("probeBoxes"), true);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");
            if (GUILayout.Button("Bake Reverb"))
            {
                bakedReverb.BeginBake();
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            DisplayProgressBarAndCancel();
        }
        public void BakedReverbStatsGUI()
        {
            PhononListener bakedReverb = serializedObject.targetObject as PhononListener;
            GUI.enabled = !PhononBaker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
            bakedReverb.UpdateBakedDataStatistics();
            for (int i = 0; i < bakedReverb.bakedProbeNames.Count; ++i)
                EditorGUILayout.LabelField(bakedReverb.bakedProbeNames[i], 
                    (bakedReverb.bakedProbeDataSizes[i] / 1000.0f).ToString("0.0") + " KB");
            EditorGUILayout.LabelField("Total Size", 
                (bakedReverb.bakedDataSize / 1000.0f).ToString("0.0") + " KB");
            GUI.enabled = true;
        }

        void DisplayProgressBarAndCancel()
        {
            PhononListener bakedReverb = serializedObject.targetObject as PhononListener;
            bakedReverb.phononBaker.DrawProgressBar();
            Repaint();
        }
    }
}