//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using UnityEngine;

namespace Phonon
{
    //
    // CustomSpeakerLayout
    // Custom speaker layout.
    //

    [AddComponentMenu("Phonon/Custom Speaker Layout")]
    public class CustomSpeakerLayout : MonoBehaviour
    {
        public UnityEngine.Vector3[] speakerPositions;
    }
}
