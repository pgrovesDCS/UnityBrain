//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using UnityEngine;

namespace Phonon
{

    //
    // PhononMaterial
    // A component representing a material that can be set to a preset or a custom value.
    //

    [AddComponentMenu("Phonon/Phonon Material")]
    public class PhononMaterial : MonoBehaviour
    {
        //
        // Data members.
        //

        // Name of the current preset.
        public PhononMaterialPreset Preset;

        // Current values of the material.
        public PhononMaterialValue Value;
    }
}