//
// Copyright 2017 Valve Corporation. All rights reserved. Subject to the following license:
// https://valvesoftware.github.io/steam-audio/license.html
//

using System;
using UnityEngine;

namespace Phonon
{

    //
    //	PhononMaterialValue
    //	Represents the values of a specific material.
    //

    [Serializable]
    public class PhononMaterialValue
    {

        //
        //	Constructor.
        //
        public PhononMaterialValue()
        {
        }

        //
        //	Constructor.
        //
        public PhononMaterialValue(float aLow, float aMid, float aHigh)
        {
            LowFreqAbsorption = aLow;
            MidFreqAbsorption = aMid;
            HighFreqAbsorption = aHigh;

            Scattering = 0.05f;

            LowFreqTransmission = .1f;
            MidFreqTransmission = .05f;
            HighFreqTransmission = .03f;
        }

        //
        // Constructor.
        //
        public PhononMaterialValue(float aLow, float aMid, float aHigh, float scattering, float tLow, float tMid, float tHigh)
        {
            LowFreqAbsorption = aLow;
            MidFreqAbsorption = aMid;
            HighFreqAbsorption = aHigh;

            Scattering = scattering;

            LowFreqTransmission = tLow;
            MidFreqTransmission = tMid;
            HighFreqTransmission = tHigh;
        }

        //
        // Copy constructor.
        //
        public PhononMaterialValue(PhononMaterialValue other)
        {
            CopyFrom(other);
        }

        //
        // Copies data from another object.
        //
        public void CopyFrom(PhononMaterialValue other)
        {
            LowFreqAbsorption = other.LowFreqAbsorption;
            MidFreqAbsorption = other.MidFreqAbsorption;
            HighFreqAbsorption = other.HighFreqAbsorption;

            Scattering = other.Scattering;

            LowFreqTransmission = other.LowFreqTransmission;
            MidFreqTransmission = other.MidFreqTransmission;
            HighFreqTransmission = other.HighFreqTransmission;
        }

        //
        // Data members.
        //

        // Absorption coefficients.
        [Range(0.0f, 1.0f)]
        public float LowFreqAbsorption;
        [Range(0.0f, 1.0f)]
        public float MidFreqAbsorption;
        [Range(0.0f, 1.0f)]
        public float HighFreqAbsorption;

        // Scattering coefficients.
        [Range(0.0f, 1.0f)]
        public float Scattering;

        // Scattering coefficients.
        [Range(0.0f, 1.0f)]
        public float LowFreqTransmission;
        [Range(0.0f, 1.0f)]
        public float MidFreqTransmission;
        [Range(0.0f, 1.0f)]
        public float HighFreqTransmission;
    }
}