//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using UnityEngine;

namespace Phonon
{
    //
    // CustomPhononSettings
    // Custom Phonon Settings.
    //

    [AddComponentMenu("Phonon/Custom Phonon Settings")]
    public class CustomPhononSettings : MonoBehaviour
    {
        // Simulation settings.
        public SceneType rayTracerOption = SceneType.Phonon;

        //Renderer settings.
        public ConvolutionOption convolutionOption;

        //OpenCL settings.
        [Range(0, 8)]
        public int numComputeUnits = 4;
    }
}
