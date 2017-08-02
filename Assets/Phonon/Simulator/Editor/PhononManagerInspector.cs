//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

using System;
using System.Collections.Generic;

namespace Phonon
{

    //
    // PhononManagerInspector
    // Custom inspector for a PhononManager component.
    //

    [CustomEditor(typeof(PhononManager))]
    public class PhononManagerInspector : Editor
    {
        //
        // Draws the inspector.
        //
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Audio Engine
            GUI.enabled = !EditorApplication.isPlayingOrWillChangePlaymode;

            PhononGUI.SectionHeader("Audio Engine Integration");
            string[] engines = { "Unity Audio" };
            var audioEngineProperty = serializedObject.FindProperty("audioEngine");
            audioEngineProperty.enumValueIndex = EditorGUILayout.Popup("Audio Engine", 
                audioEngineProperty.enumValueIndex, engines);

            // Scene Settings
            PhononManager phononManager = ((PhononManager)target);
            PhononGUI.SectionHeader("Global Material Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("materialPreset"));
            if (serializedObject.FindProperty("materialPreset").enumValueIndex < 11)
            {
                PhononMaterialValue actualValue = phononManager.materialValue;
                actualValue.CopyFrom(PhononMaterialPresetList.PresetValue(
                    serializedObject.FindProperty("materialPreset").enumValueIndex));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("materialValue"));
            }

            PhononGUI.SectionHeader("Scene Export");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(" ");

            if (GUILayout.Button("Export to OBJ"))
                phononManager.ExportScene(true);
            if (GUILayout.Button("Pre-Export Scene"))
                phononManager.ExportScene(false);

            EditorGUILayout.EndHorizontal();

            // Simulation Settings
            PhononGUI.SectionHeader("Simulation Settings");
            EditorGUILayout.PropertyField(serializedObject.FindProperty("simulationPreset"));
            if (serializedObject.FindProperty("simulationPreset").enumValueIndex < 3)
            {
                SimulationSettingsValue actualValue = phononManager.simulationValue;
                actualValue.CopyFrom(SimulationSettingsPresetList.PresetValue(
                    serializedObject.FindProperty("simulationPreset").enumValueIndex));
            }
            else
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("simulationValue"));
                if (Application.isEditor && EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    SimulationSettingsValue actualValue = phononManager.simulationValue;
                    IntPtr environment = phononManager.PhononManagerContainer().Environment().GetEnvironment();
                    if (environment != IntPtr.Zero)
                        PhononCore.iplSetNumBounces(environment, actualValue.RealtimeBounces);
                }
            }

            // Fold Out for Advanced Settings
            PhononGUI.SectionHeader("Advanced Options");
            serializedObject.FindProperty("showLoadTimeOptions").boolValue = 
            EditorGUILayout.Foldout(serializedObject.FindProperty("showLoadTimeOptions").boolValue, 
                "Per Frame Query Optimization");

            if (phononManager.showLoadTimeOptions)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("updateComponents"));
            }

            serializedObject.FindProperty("showMassBakingOptions").boolValue = 
                EditorGUILayout.Foldout(serializedObject.FindProperty("showMassBakingOptions").boolValue, 
                "Consolidated Baking Options");
            if (phononManager.showMassBakingOptions)
            {
                bool noSettingMessage = false;
                noSettingMessage = ProbeGenerationGUI() || noSettingMessage;
                noSettingMessage = BakeAllGUI() || noSettingMessage;
                noSettingMessage = BakedSourcesGUI(phononManager) || noSettingMessage;
                noSettingMessage =  BakedStaticListenerNodeGUI(phononManager) || noSettingMessage;
                noSettingMessage = BakedReverbGUI(phononManager) || noSettingMessage;

                if (!noSettingMessage)
                    EditorGUILayout.LabelField("Scene does not contain any baking related components.");
            }

            GUI.enabled = true;
            EditorGUILayout.HelpBox("Do not manually add Phonon Manager component. Click Window > Phonon.", 
                MessageType.Info);

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }

        public bool ProbeGenerationGUI()
        {
            ProbeBox[] probeBoxes = GameObject.FindObjectsOfType<ProbeBox>();
            if (probeBoxes.Length > 0)
                PhononGUI.SectionHeader("Probe Generation");
            else
                return false;

            GUI.enabled = !PhononBaker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
            foreach (ProbeBox probeBox in probeBoxes)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(probeBox.name);
                if (GUILayout.Button("Generate Probe", GUILayout.Width(200.0f)))
                {
                    probeBox.GenerateProbes();
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
                EditorGUILayout.EndHorizontal();
            }
            GUI.enabled = true;

            return true;
        }

        public bool BakeAllGUI()
        {
            bool hasBakeComponents = GameObject.FindObjectsOfType<ProbeBox>().Length > 0;

            if (hasBakeComponents)
                PhononGUI.SectionHeader("Bake All");
            else
                return false;

            GUI.enabled = !PhononBaker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Select All"))
            {
                SelectForBakeEffect(true);
            }

            if (GUILayout.Button("Select None"))
            {
                SelectForBakeEffect(false);
            }

            if (GUILayout.Button("Bake", GUILayout.Width(200.0f)))
            {
                BakeSelected();
            }

            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            DisplayProgressBarAndCancel();

            return true;
        }

        void DisplayProgressBarAndCancel()
        {
            PhononManager phononManager = serializedObject.targetObject as PhononManager;
            phononManager.phononBaker.DrawProgressBar();
            Repaint();
        }

        public void BakeSelected()
        {
            List<GameObject> gameObjects = new List<GameObject>();
            List<BakingMode> bakingModes = new List<BakingMode>();
            List<string> identifers = new List<string>();
            List<Sphere> influenceSpheres = new List<Sphere>();
            List<ProbeBox[]> probeBoxes = new List<ProbeBox[]>();

            PhononSource[] bakedSources = GameObject.FindObjectsOfType<PhononSource>();
            foreach (PhononSource bakedSource in bakedSources)
            {
                if (bakedSource.enableReflections && bakedSource.uniqueIdentifier.Length != 0 && 
                    bakedSource.sourceSimulationType == SourceSimulationType.BakedStaticSource && 
                    bakedSource.bakeToggle)
                {
                    gameObjects.Add(bakedSource.gameObject);
                    bakingModes.Add(BakingMode.StaticSource);
                    identifers.Add(bakedSource.uniqueIdentifier);

                    Sphere bakeSphere;
                    Vector3 sphereCenter = Common.ConvertVector(bakedSource.transform.position);
                    bakeSphere.centerx = sphereCenter.x;
                    bakeSphere.centery = sphereCenter.y;
                    bakeSphere.centerz = sphereCenter.z;
                    bakeSphere.radius = bakedSource.bakingRadius;
                    influenceSpheres.Add(bakeSphere);

                    if (bakedSource.useAllProbeBoxes)
                        probeBoxes.Add(FindObjectsOfType<ProbeBox>() as ProbeBox[]);
                    else
                        probeBoxes.Add(bakedSource.probeBoxes);
                }
            }

            BakedStaticListenerNode[] bakedStaticNodes = GameObject.FindObjectsOfType<BakedStaticListenerNode>();
            foreach (BakedStaticListenerNode bakedStaticNode in bakedStaticNodes)
            {
                if (bakedStaticNode.uniqueIdentifier.Length != 0 && bakedStaticNode.bakeToggle)
                {
                    gameObjects.Add(bakedStaticNode.gameObject);
                    bakingModes.Add(BakingMode.StaticListener);
                    identifers.Add(bakedStaticNode.uniqueIdentifier);

                    Sphere bakeSphere;
                    Vector3 sphereCenter = Common.ConvertVector(bakedStaticNode.transform.position);
                    bakeSphere.centerx = sphereCenter.x;
                    bakeSphere.centery = sphereCenter.y;
                    bakeSphere.centerz = sphereCenter.z;
                    bakeSphere.radius = bakedStaticNode.bakingRadius;
                    influenceSpheres.Add(bakeSphere);

                    if (bakedStaticNode.useAllProbeBoxes)
                        probeBoxes.Add(FindObjectsOfType<ProbeBox>() as ProbeBox[]);
                    else
                        probeBoxes.Add(bakedStaticNode.probeBoxes);
                }
            }

            PhononListener bakedReverb = GameObject.FindObjectOfType<PhononListener>();
            if (!(bakedReverb == null || !bakedReverb.enableReverb
                || bakedReverb.reverbSimulationType != ReverbSimulationType.BakedReverb) && 
                bakedReverb.bakeToggle)
            {
                gameObjects.Add(bakedReverb.gameObject);
                bakingModes.Add(BakingMode.Reverb);
                identifers.Add("__reverb__");
                influenceSpheres.Add(new Sphere());

                if (bakedReverb.useAllProbeBoxes)
                    probeBoxes.Add(FindObjectsOfType<ProbeBox>() as ProbeBox[]);
                else
                    probeBoxes.Add(bakedReverb.probeBoxes);
            }

            if (gameObjects.Count > 0)
            {
                PhononManager phononManager = serializedObject.targetObject as PhononManager;
                phononManager.phononBaker.BeginBake(gameObjects.ToArray(), bakingModes.ToArray(), 
                    identifers.ToArray(), influenceSpheres.ToArray(), probeBoxes.ToArray());
            }
            else
            {
                Debug.LogWarning("No game object selected for baking.");
            }
        }

        public void SelectForBakeEffect(bool select)
        {
            PhononSource[] bakedSources = GameObject.FindObjectsOfType<PhononSource>();
            foreach (PhononSource bakedSource in bakedSources)
            {
                if (bakedSource.enableReflections && bakedSource.uniqueIdentifier.Length != 0
                    && bakedSource.sourceSimulationType == SourceSimulationType.BakedStaticSource)
                {
                    bakedSource.bakeToggle = select;
                }
            }

            BakedStaticListenerNode[] bakedStaticNodes = GameObject.FindObjectsOfType<BakedStaticListenerNode>();
            foreach (BakedStaticListenerNode bakedStaticNode in bakedStaticNodes)
            {
                if (bakedStaticNode.uniqueIdentifier.Length != 0)
                {
                    bakedStaticNode.bakeToggle = select;
                }
            }

            PhononListener bakedReverb = GameObject.FindObjectOfType<PhononListener>();
            if (!(bakedReverb == null || !bakedReverb.enableReverb
                || bakedReverb.reverbSimulationType != ReverbSimulationType.BakedReverb))
            {
                bakedReverb.bakeToggle = select;
            }

        }

        public bool BakedSourcesGUI(PhononManager phononManager)
        {
            PhononSource[] bakedSources = GameObject.FindObjectsOfType<PhononSource>();

            bool showBakedSources = false;
            foreach (PhononSource bakedSource in bakedSources)
                if (bakedSource.enableReflections && bakedSource.uniqueIdentifier.Length != 0
                    && bakedSource.sourceSimulationType == SourceSimulationType.BakedStaticSource)
                {
                    showBakedSources = true;
                    break;
                }

            if (showBakedSources)
                PhononGUI.SectionHeader("Baked Sources");
            else
                return false;

            foreach (PhononSource bakedSource in bakedSources)
            {
                if (!bakedSource.enableReflections || bakedSource.uniqueIdentifier.Length == 0
                    || bakedSource.sourceSimulationType != SourceSimulationType.BakedStaticSource)
                    continue;

                GUI.enabled = !PhononBaker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
                EditorGUILayout.BeginHorizontal();

                bakedSource.UpdateBakedDataStatistics();
                bool previousValue = bakedSource.bakeToggle;
                bool newValue = GUILayout.Toggle(bakedSource.bakeToggle, " " + bakedSource.uniqueIdentifier);
                if (previousValue != newValue)
                {
                    Undo.RecordObject(bakedSource, "Toggled " + bakedSource.uniqueIdentifier + 
                        " in Phonon Manager");
                    bakedSource.bakeToggle = newValue;
                }

                EditorGUILayout.LabelField((bakedSource.bakedDataSize / 1000.0f).ToString("0.0") + " KB");
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            return true;
        }

        public bool BakedStaticListenerNodeGUI(PhononManager phononManager)
        {
            BakedStaticListenerNode[] bakedStaticNodes = GameObject.FindObjectsOfType<BakedStaticListenerNode>();

            bool showBakedStaticListenerNodes = false;
            foreach (BakedStaticListenerNode bakedStaticNode in bakedStaticNodes)
                if (bakedStaticNode.uniqueIdentifier.Length != 0)
                {
                    showBakedStaticListenerNodes = true;
                    break;
                }

            if (showBakedStaticListenerNodes)
                PhononGUI.SectionHeader("Baked Static Listener Nodes");
            else
                return false;

            foreach (BakedStaticListenerNode bakedStaticNode in bakedStaticNodes)
            {
                if (bakedStaticNode.uniqueIdentifier.Length == 0)
                    continue;

                GUI.enabled = !PhononBaker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
                EditorGUILayout.BeginHorizontal();
                bakedStaticNode.UpdateBakedDataStatistics();

                bool previousValue = bakedStaticNode.bakeToggle;
                bool newValue = GUILayout.Toggle(bakedStaticNode.bakeToggle, " " + 
                    bakedStaticNode.uniqueIdentifier);
                if (previousValue != newValue)
                {
                    Undo.RecordObject(bakedStaticNode, "Toggled " + bakedStaticNode.uniqueIdentifier +
                        " in Phonon Manager");
                    bakedStaticNode.bakeToggle = newValue;
                }
                EditorGUILayout.LabelField((bakedStaticNode.bakedDataSize / 1000.0f).ToString("0.0") + " KB");
                EditorGUILayout.EndHorizontal();
                GUI.enabled = true;
            }

            return true;
        }

        public bool BakedReverbGUI(PhononManager phononManager)
        {
            PhononListener bakedReverb = GameObject.FindObjectOfType<PhononListener>();
            if (bakedReverb == null || !bakedReverb.enableReverb
                || bakedReverb.reverbSimulationType != ReverbSimulationType.BakedReverb)
                return false;

            PhononGUI.SectionHeader("Bake Reverb");

            GUI.enabled = !PhononBaker.IsBakeActive() && !EditorApplication.isPlayingOrWillChangePlaymode;
            EditorGUILayout.BeginHorizontal();
            bakedReverb.UpdateBakedDataStatistics();

            bool previousValues = bakedReverb.bakeToggle;
            bool newValue = GUILayout.Toggle(bakedReverb.bakeToggle, " reverb");
            if (previousValues != newValue)
            {
                Undo.RecordObject(bakedReverb, "Toggled reverb in Phonon Manager");
                bakedReverb.bakeToggle = newValue;
            }

            EditorGUILayout.LabelField((bakedReverb.bakedDataSize / 1000.0f).ToString("0.0") + " KB");
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;

            return true;
        }

    }
}