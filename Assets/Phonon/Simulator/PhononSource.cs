//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;

namespace Phonon
{
    //
    // SourceSimulationType
    // Various simulation options for a PhononSource.
    //
    public enum SourceSimulationType
    {
        Realtime,
        BakedStaticSource,
        BakedStaticListener
    }

    //
    // PhononSource
    // Enables physics-based modeling for any object with AudioSource component.
    //
    [AddComponentMenu("Phonon/Phonon Source")]
    public class PhononSource : MonoBehaviour
    {
        private void Awake()
        {
            Initialize();
            LazyInitialize();
        }

        private void OnEnable()
        {
            StartCoroutine(EndOfFrameUpdate());
        }

        private void OnDisable()
        {
            directSimulator.Flush();
            indirectSimulator.Flush();
        }

        private void OnDestroy()
        {
            Destroy();
        }

        private void Initialize()
        {
            initialized = false;
            destroying = false;
            errorLogged = false;

            phononManager = FindObjectOfType<PhononManager>();
            if (phononManager == null)
            {
                Debug.LogError("Phonon Manager Settings object not found in the scene! Click Window > Phonon");
                return;
            }

            bool initializeRenderer = true;
            phononManager.Initialize(initializeRenderer);
            phononContainer = phononManager.PhononManagerContainer();
            phononContainer.Initialize(initializeRenderer, phononManager);

            directSimulator.Initialize(phononManager.AudioFormat());
            indirectSimulator.Initialize(phononManager.AudioFormat(), phononManager.SimulationSettings());
        }

        private void LazyInitialize()
        {
            if (phononManager != null && phononContainer != null)
            {
                directSimulator.LazyInitialize(phononContainer.BinauralRenderer(), directBinauralEnabled,
                    phononManager.RenderingSettings(), phononContainer.EnvironmentalRenderer());

                indirectSimulator.LazyInitialize(phononContainer.BinauralRenderer(), enableReflections,
                    indirectBinauralEnabled, phononManager.RenderingSettings(), true, sourceSimulationType,
                    uniqueIdentifier, phononManager.PhononStaticListener(), ReverbSimulationType.RealtimeReverb,
                    phononContainer.EnvironmentalRenderer());
            }
        }

        private void Destroy()
        {
            mutex.WaitOne();
            destroying = true;

            directSimulator.Destroy();
            indirectSimulator.Destroy();

            if (phononContainer != null)
            {
                phononContainer.Destroy();
                phononContainer = null;
            }

            mutex.ReleaseMutex();
        }

        //
        // Coroutine to update source and listener position and orientation at frame end.
        // Done this way to ensure correct update in VR setup.
        //
        private IEnumerator EndOfFrameUpdate()
        {
            while (true)
            {
                LazyInitialize();

                if (!errorLogged && phononManager != null && phononContainer != null 
                    && phononContainer.Scene().GetScene() == IntPtr.Zero
                    && ((directOcclusionMode != OcclusionMode.NoOcclusion) || enableReflections))
                {
                    Debug.LogError("Scene not found. Make sure to pre-export the scene.");
                    errorLogged = true;
                }

                if (phononManager != null && !errorLogged) //Output silence in case if scene is missing and required.
                {
                    UpdateRelativeDirection();
                    directSimulator.FrameUpdate(phononContainer.EnvironmentalRenderer().GetEnvironmentalRenderer(),
                        sourcePosition, listenerPosition, listenerAhead, listenerUp, partialOcclusionRadius, 
                        directOcclusionMode, directOcclusionMethod);
                    indirectSimulator.FrameUpdate(true, sourceSimulationType, ReverbSimulationType.RealtimeReverb,
                        phononManager.PhononStaticListener(), phononManager.PhononListener());

                    initialized = true;
                }

                yield return waitForEndOfFrame;   // Must yield after updating the relative direction.
            }
        }

        //
        // Updates the direction of the source relative to the listener.
        // Wait until the end of the frame to update the position to get latest information.
        //
        private void UpdateRelativeDirection()
        {
            AudioListener listener = phononManager.AudioListener();
            if (listener == null) return;

            sourcePosition = Common.ConvertVector(transform.position);
            listenerPosition = Common.ConvertVector(listener.transform.position);
            listenerAhead = Common.ConvertVector(listener.transform.forward);
            listenerUp = Common.ConvertVector(listener.transform.up);
        }

        //
        // Applies propagation effects to dry audio.
        //
        void OnAudioFilterRead(float[] data, int channels)
        {
            mutex.WaitOne();

            if (data == null)
            {
                mutex.ReleaseMutex();
                return;
            }

            if (!initialized || destroying)
            {
                mutex.ReleaseMutex();
                Array.Clear(data, 0, data.Length);
                return;
            }

            //data is copied, must be used before directSimulator which modifies the data.
            float[] wetData = indirectSimulator.AudioFrameUpdate(data, channels, sourcePosition, listenerPosition, 
                                listenerAhead, listenerUp, enableReflections, indirectMixFraction, 
                                indirectBinauralEnabled, phononManager.PhononListener());

            directSimulator.AudioFrameUpdate(data, channels, physicsBasedAttenuation, directMixFraction, 
                directBinauralEnabled, airAbsorption, hrtfInterpolation, directOcclusionMode, directOcclusionMethod);

            if (wetData != null && wetData.Length != 0)
            {
                for (int i = 0; i < data.Length; ++i)
                {
                    data[i] += wetData[i];
                }
            }

            mutex.ReleaseMutex();
        }

        public void BeginBake()
        {
            Sphere bakeSphere;
            Vector3 sphereCenter = Common.ConvertVector(gameObject.transform.position);
            bakeSphere.centerx = sphereCenter.x;
            bakeSphere.centery = sphereCenter.y;
            bakeSphere.centerz = sphereCenter.z;
            bakeSphere.radius = bakingRadius;

            GameObject[] bakeObjects = { gameObject };
            BakingMode[] bakingModes = { BakingMode.StaticSource };
            string[] bakeStrings = { uniqueIdentifier };
            Sphere[] bakeSpheres = { bakeSphere };

            ProbeBox[][] bakeProbeBoxes;
            bakeProbeBoxes = new ProbeBox[1][];

            if (useAllProbeBoxes)
                bakeProbeBoxes[0] = FindObjectsOfType<ProbeBox>() as ProbeBox[];
            else
                bakeProbeBoxes[0] = probeBoxes;

            phononBaker.BeginBake(bakeObjects, bakingModes, bakeStrings, bakeSpheres, bakeProbeBoxes);
        }

        public void EndBake()
        {
            phononBaker.EndBake();
        }

        void OnDrawGizmosSelected()
        {
            if (sourceSimulationType == SourceSimulationType.BakedStaticSource)
            {
                Color oldColor = Gizmos.color;
                Matrix4x4 oldMatrix = Gizmos.matrix;

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(gameObject.transform.position, bakingRadius);

                Gizmos.color = Color.magenta;
                ProbeBox[] drawProbeBoxes = probeBoxes;
                if (useAllProbeBoxes)
                    drawProbeBoxes = FindObjectsOfType<ProbeBox>() as ProbeBox[];

                if (drawProbeBoxes != null)
                {
                    foreach (ProbeBox probeBox in drawProbeBoxes)
                    {
                        if (probeBox == null)
                            continue;

                        Gizmos.matrix = probeBox.transform.localToWorldMatrix;
                        Gizmos.DrawWireCube(new UnityEngine.Vector3(0, 0, 0), new UnityEngine.Vector3(1, 1, 1));
                    }
                }

                Gizmos.matrix = oldMatrix;
                Gizmos.color = oldColor;
            }
        }

        public void UpdateBakedDataStatistics()
        {
            ProbeBox[] statProbeBoxes = probeBoxes;
            if (useAllProbeBoxes)
                statProbeBoxes = FindObjectsOfType<ProbeBox>() as ProbeBox[];

            if (statProbeBoxes == null)
                return;

            int dataSize = 0;
            List<string> probeNames = new List<string>();
            List<int> probeDataSizes = new List<int>();
            foreach (ProbeBox probeBox in statProbeBoxes)
            {
                if (probeBox == null || uniqueIdentifier.Length == 0)
                    continue;

                int probeDataSize = 0;
                probeNames.Add(probeBox.name);

                for (int i = 0; i < probeBox.probeDataName.Count; ++i)
                {
                    if (uniqueIdentifier == probeBox.probeDataName[i])
                    {
                        probeDataSize = probeBox.probeDataNameSizes[i];
                        dataSize += probeDataSize;
                    }
                }

                probeDataSizes.Add(probeDataSize);
            }

            bakedDataSize = dataSize;
            bakedProbeNames = probeNames;
            bakedProbeDataSizes = probeDataSizes;
        }

        // Public fields - direct sound.
        public bool directBinauralEnabled = true;
        public HRTFInterpolation hrtfInterpolation;
        public OcclusionMode directOcclusionMode;
        public OcclusionMethod directOcclusionMethod;
        [Range(.1f, 32f)]
        public float partialOcclusionRadius = 1.0f;
        public bool physicsBasedAttenuation = false;
        public bool airAbsorption = false;
        [Range(.0f, 1.0f)]
        public float directMixFraction = 1.0f;

        // Public fields - indirect sound.
        public bool enableReflections = false;
        public SourceSimulationType sourceSimulationType;
        [Range(.0f, 10.0f)]
        public float indirectMixFraction = 1.0f;
        public bool indirectBinauralEnabled = false;

        // Public fields - indirect baking.
        public string uniqueIdentifier = "";
        [Range(1f, 1024f)]
        public float bakingRadius = 16f;
        public bool useAllProbeBoxes = false;
        public ProbeBox[] probeBoxes = null;
        public PhononBaker phononBaker = new PhononBaker();

        // Public stored fields - baking.
        public List<string> bakedProbeNames = new List<string>();
        public List<int> bakedProbeDataSizes = new List<int>();
        public int bakedDataSize = 0;
        public bool bakedStatsFoldout = false;
        public bool bakeToggle = false;

        // Private fields.
        PhononManager phononManager = null;
        PhononManagerContainer phononContainer = null;

        AudioFormat inputFormat;
        AudioFormat outputFormat;
        AudioFormat ambisonicsFormat;

        Vector3 sourcePosition;
        Vector3 listenerPosition;
        Vector3 listenerAhead;
        Vector3 listenerUp;

        Mutex mutex = new Mutex();

        bool initialized = false;
        bool destroying = false;
        bool errorLogged = false;

        DirectSimulator directSimulator = new DirectSimulator();
        IndirectSimulator indirectSimulator = new IndirectSimulator();

        WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    }
}
