using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LSL;
using Assets.Scripts;

public class LSLInterface : MonoBehaviour {

    private liblsl.StreamInfo[] results;
    private liblsl.StreamInlet values;    

    
    // This allows us to display some status text to the user.   
    public Text LSLStatus;

    // The Electrode Template will be duplicated for each electrode.
    //  It requires a PointLight object and two AudioSource's.
    //  The AudioSources must be named Melodic and Percussive.
    public GameObject electrodeTemplate;

    // The electrodeParent is the parent object the electrodes will be placed in.
    // This allows for scaling and translation of the electrodes independent of their local coords.
    public GameObject electrodeParent;

    // A list of electrodes we've attached to the electrodeParent object.
    Assets.Scripts.Electrodes elist;

    void Start () {

        elist = new Electrodes(electrodeParent, electrodeTemplate, Instantiate);
        LSLStatus.text = "Waiting for Alpha/Beta stream...";        
    }

    // Update is called once per frame
    void Update()
    {
        // If values is null we don't have an LSL stream.
        if (values != null)
        {            
            float[] sample = new float[32];
            double ts = values.pull_sample(sample, 0.0);

            // If ts is 0 we have not received data yet.
            if (ts != 0)
            {
                LSLStatus.text = "";
                // Iterate over each electrode and do stuff...
                foreach (Electrodes.ElectrodeData e in elist.electrode)
                {
                    // Get a reference to our Point Light object for this electrode.
                    Light l = e.light;
                    AudioSource mel = e.melodic;
                    AudioSource per = e.percussive;

                    // Map the alpha/beta from [0, 1] to some hue.                                
                    float h = (1.0f - sample[e.electrodeIndex]) * (240.0f / 360.0f);

                    // Conver the HSV into an RGB value.
                    Color c = Color.HSVToRGB(h, 1.0f, 0.5f);

                    // Set the intensity so we get a nice effect.
                    l.intensity = 25;

                    // And assign the actual color.
                    l.color = c;

                    // Map the "melodic" volume from [0, 0.5] => [0, 1.0]
                    float mVol = 2.0f * sample[e.electrodeIndex];
                    if (sample[e.electrodeIndex] >= .5)
                        mel.volume = 0;
                    else
                        mel.volume = mVol;
                    
                    // Play the clip if it's not looping.
                    if (!mel.isPlaying)
                        mel.Play();

                    // Map the "percussive" volume from [0.5, 1.0] => [1.0,0]
                    float pVol = 2.0f * sample[e.electrodeIndex] - 1.0f;
                    if (sample[e.electrodeIndex] <= .5)
                        per.volume = 0;
                    else
                        per.volume = pVol;
                    // Play the clip if it's not looping.
                    if (!per.isPlaying)
                        per.Play();
                }
            }
        }
        else
        {
            // Check for our gNautilus stream. Timeout of .1 seconds keeps us from hanging the UI while waiting.
            results = liblsl.resolve_stream("name", "g.Nautilus", 1, .1d);

            if (results.Length == 0)
            {
                LSLStatus.text = "Unable to resolve a g.Nautilus stream!";
            }
            else
            {
                LSLStatus.text = "LSL g.Nautilus stream found, waiting for data stream to begin...";
                // Open and inlet stream to the first and only LSL stream we resolved.
                values = new liblsl.StreamInlet(results[0]);
            }
        }
    }    
}
