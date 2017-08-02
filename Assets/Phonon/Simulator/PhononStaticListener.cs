//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using UnityEngine;
using System;
using System.Threading;
using System.Runtime.InteropServices;

namespace Phonon
{
    //
    // Phonon Static Listener
    // Represents a component to update the latest node for baked static listener.
    //

    [AddComponentMenu("Phonon/Phonon Static Listener")]
    public class PhononStaticListener : MonoBehaviour
    {
        public BakedStaticListenerNode currentStaticListenerNode;
    }
}
