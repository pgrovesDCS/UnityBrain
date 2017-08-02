//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using System;

namespace Phonon
{
    public class EnvironmentalRenderer
    {
        public void Create(Environment environment, RenderingSettings renderingSettings, SimulationSettings simulationSettings, GlobalContext globalContext)
        {
            ambisonicsFormat.channelLayoutType = ChannelLayoutType.Ambisonics;
            ambisonicsFormat.ambisonicsOrder = simulationSettings.ambisonicsOrder;
            ambisonicsFormat.numSpeakers = (ambisonicsFormat.ambisonicsOrder + 1) * (ambisonicsFormat.ambisonicsOrder + 1);
            ambisonicsFormat.ambisonicsOrdering = AmbisonicsOrdering.ACN;
            ambisonicsFormat.ambisonicsNormalization = AmbisonicsNormalization.N3D;
            ambisonicsFormat.channelOrder = ChannelOrder.Deinterleaved;

            var error = PhononCore.iplCreateEnvironmentalRenderer(globalContext, environment.GetEnvironment(),
                renderingSettings, ambisonicsFormat, IntPtr.Zero, IntPtr.Zero, ref environmentalRenderer);
            if (error != Error.None)
            {
                throw new Exception("Unable to create environment renderer [" + error.ToString() + "]");
            }
        }

        public IntPtr GetEnvironmentalRenderer()
        {
            return environmentalRenderer;
        }

        public void Destroy()
        {
            PhononCore.iplDestroyEnvironmentalRenderer(ref environmentalRenderer);
        }

        AudioFormat ambisonicsFormat;
        IntPtr environmentalRenderer = IntPtr.Zero;
    }
}