using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Phonon
{
    public class DirectSimulator
    {
        // Initializes settings for Direct Simulator.
        public void Initialize(AudioFormat audioFormat)
        {
            // Assumes Phonon Manager is not null.
            inputFormat = audioFormat;
            outputFormat = audioFormat;
        }

        // Initializes various Phonon API objects in a lazy fashion.
        // Safe to call this every frame.
        public void LazyInitialize(BinauralRenderer binauralRenderer, bool directBinauralEnabled, 
            RenderingSettings renderingSettings, EnvironmentalRenderer environmentalRenderer)
        {
            if (directBinauralEffect == IntPtr.Zero && outputFormat.channelLayout == ChannelLayout.Stereo
                && directBinauralEnabled && binauralRenderer.GetBinauralRenderer() != IntPtr.Zero)
            {
                // Create object based binaural effect for direct sound if the output format is stereo.
                if (PhononCore.iplCreateBinauralEffect(binauralRenderer.GetBinauralRenderer(), outputFormat, 
                    outputFormat, ref directBinauralEffect) != Error.None)
                {
                    Debug.Log("Unable to create binaural effect. Please check the log file for details.");
                    return;
                }
            }

            if (directSoundEffect == IntPtr.Zero && environmentalRenderer.GetEnvironmentalRenderer() != IntPtr.Zero)
            {
                if (PhononCore.iplCreateDirectSoundEffect(environmentalRenderer.GetEnvironmentalRenderer(), inputFormat,
                    outputFormat, ref directSoundEffect) != Error.None)
                {
                    Debug.Log("Unable to create direct sound effect. Please check the log file for details.");
                    return;
                }
            }

            if (directCustomPanningEffect == IntPtr.Zero  && outputFormat.channelLayout == ChannelLayout.Custom
                && !directBinauralEnabled && binauralRenderer.GetBinauralRenderer() != IntPtr.Zero)
            {
                // Panning effect for direct sound (used for rendering only for custom speaker layout, 
                // otherwise use default Unity panning)
                if (PhononCore.iplCreatePanningEffect(binauralRenderer.GetBinauralRenderer(), outputFormat, 
                    outputFormat, ref directCustomPanningEffect) != Error.None)
                {
                    Debug.Log("Unable to create custom panning effect. Please check the log file for details.");
                    return;
                }
            }
        }

        public void Destroy()
        {
            PhononCore.iplDestroyBinauralEffect(ref directBinauralEffect);
            directBinauralEffect = IntPtr.Zero;

            PhononCore.iplDestroyPanningEffect(ref directCustomPanningEffect);
            directCustomPanningEffect = IntPtr.Zero;

            PhononCore.iplDestroyDirectSoundEffect(ref directSoundEffect);
            directSoundEffect = IntPtr.Zero;
        }

        public void AudioFrameUpdate(float[] data, int channels, bool physicsBasedAttenuation, float directMixFraction, 
            bool directBinauralEnabled, bool applyAirAbsorption, HRTFInterpolation hrtfInterpolation, 
            OcclusionMode directOcclusionMode, OcclusionMethod directOcclusionMethod)
        {
            DirectSoundEffectOptions directSoundEffectOptions;
            directSoundEffectOptions.applyDistanceAttenuation = physicsBasedAttenuation ? Bool.True : Bool.False;        
            directSoundEffectOptions.applyAirAbsorption = applyAirAbsorption ? Bool.True : Bool.False;
            directSoundEffectOptions.occlusionMode = directOcclusionMode;

            AudioBuffer inputBuffer;
            inputBuffer.audioFormat = inputFormat;
            inputBuffer.numSamples = data.Length / channels;
            inputBuffer.deInterleavedBuffer = null;
            inputBuffer.interleavedBuffer = data;

            AudioBuffer outputBuffer;
            outputBuffer.audioFormat = outputFormat;
            outputBuffer.numSamples = data.Length / channels;
            outputBuffer.deInterleavedBuffer = null;
            outputBuffer.interleavedBuffer = data;

            for (int i = 0; i < data.Length; i++)
            {
                data[i] *= directMixFraction;
            }

            PhononCore.iplApplyDirectSoundEffect(directSoundEffect, inputBuffer, directSoundPath,
                directSoundEffectOptions, outputBuffer);

            Vector3 directDirection = directSoundPath.direction;
            if ((outputFormat.channelLayout == ChannelLayout.Stereo) && directBinauralEnabled)
            {
                // Apply binaural audio to direct sound.
                PhononCore.iplApplyBinauralEffect(directBinauralEffect, outputBuffer, directDirection,
                    hrtfInterpolation, outputBuffer);
            }
            else if (outputFormat.channelLayout == ChannelLayout.Custom)
            {
                // Apply panning fo custom speaker layout.
                PhononCore.iplApplyPanningEffect(directCustomPanningEffect, outputBuffer, directDirection,
                    outputBuffer);
            }
        }

        public void Flush()
        {
            PhononCore.iplFlushBinauralEffect(directBinauralEffect);
            PhononCore.iplFlushPanningEffect(directCustomPanningEffect);
            PhononCore.iplFlushDirectSoundEffect(directSoundEffect);
        }

        public void FrameUpdate(IntPtr envRenderer, Vector3 sourcePosition, Vector3 listenerPosition, 
            Vector3 listenerAhead, Vector3 listenerUp, float partialOcclusionRadius, OcclusionMode directOcclusionMode,
            OcclusionMethod directOcclusionMethod)
        {
            directSoundPath = PhononCore.iplGetDirectSoundPath(envRenderer, listenerPosition, listenerAhead, listenerUp, 
                sourcePosition, partialOcclusionRadius, directOcclusionMode, directOcclusionMethod);
        }

        AudioFormat inputFormat;
        AudioFormat outputFormat;

        DirectSoundPath directSoundPath;

        // Phonon API related variables.
        IntPtr directBinauralEffect = IntPtr.Zero;
        IntPtr directCustomPanningEffect = IntPtr.Zero;
        IntPtr directSoundEffect = IntPtr.Zero;
    }
}