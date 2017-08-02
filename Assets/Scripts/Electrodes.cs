using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class Electrodes
    {
        public class ElectrodeData
        {
            public string name;
            public float x;
            public float y;
            public float z;
            public string percussiveFileName;
            public string melodicFileName;

            public int electrodeIndex;

            public AudioSource melodic;
            public AudioSource percussive;

            public AudioClip percussive_clip;
            public AudioClip melody_clip;
            
            public Light light;

            public ElectrodeData(string name, int electrodeIndex, double x, double y, double z, string melodicFileName, string percussiveFileName)
            {
                this.name = name;
                this.electrodeIndex = electrodeIndex;
                this.x = (float)x * 1.0f;
                this.y = (float)y * 1.0f;
                this.z = (float)z * 1.0f;
                this.percussiveFileName = percussiveFileName;
                this.melodicFileName = melodicFileName;

                LoadSoundResources();
            }

            private void LoadSoundResources()
            {                
                percussive_clip = Resources.Load<AudioClip>(percussiveFileName);
                melody_clip = Resources.Load<AudioClip>(melodicFileName);
            }
        }

        public List<ElectrodeData> electrode;
        public delegate GameObject ObjectSpawner(GameObject original, Vector3 pos, Quaternion rot);

        private GameObject parent;
        private GameObject template;
        private ObjectSpawner spawn;

        public Electrodes(GameObject parent, GameObject template, ObjectSpawner spawn)
        {
            this.parent = parent;
            this.template = template;
            this.spawn = spawn;

            SpawnElectrodes();
        }

        private void SpawnElectrodes()
        {
            electrode = new List<ElectrodeData>()
        {
            //new ElectrodeData("Afz", 0, .934, .000, .358, "", ""), //{{0.934, 0, .358}, -- This one is labeled as GND... is it used?
            new ElectrodeData("Fp1", 0, .950, .309, -0.0349, "Brain Sound Bounces/DR-1", "Brain Sound Bounces/OR1-1"),//{0.95, .309, -0.0349},
            new ElectrodeData("Fp2", 1, .950, -.309, -0.0349, "Brain Sound Bounces/WS-1", "Brain Sound Bounces/OR2-1"),//{0.95, -.309, -0.0349},
            new ElectrodeData("AF3", 2, .885, .376, .276, "Brain Sound Bounces/DR-2", "Brain Sound Bounces/OR1-2"),//{0.885, .376, .276},
            new ElectrodeData("AF4", 3, .885, -.376, .276, "Brain Sound Bounces/WS-2", "Brain Sound Bounces/OR2-2"),//{0.885, -.376, .276},
            new ElectrodeData("F7", 4, .587, .809, -0.0349, "Brain Sound Bounces/DR-3", "Brain Sound Bounces/OR1-3"),//{0.587, .809, -0.0349},
            new ElectrodeData("F3", 5, .673, .545, .5, "Brain Sound Bounces/DR-4", "Brain Sound Bounces/OR1-4"),//    {0.673, .545, .5},
            new ElectrodeData("Fz", 6, .719, 0.000, .695, "Brain Sound Bounces/CH-1", "Brain Sound Bounces/TC3-1"),//{0.719, 0, .695},
            new ElectrodeData("F4", 7, .673, -.545, .5, "Brain Sound Bounces/WS-3", "Brain Sound Bounces/OR2-3"),//{0.673, -.545, .5},
            new ElectrodeData("F8", 8, .587, -.809, -0.0349, "Brain Sound Bounces/WS-4", "Brain Sound Bounces/OR2-4"),//{0.587, -.809, -0.0349},
            new ElectrodeData("FC5", 9, .339, .883, .326, "Brain Sound Bounces/RE-1", "Brain Sound Bounces/AC-1"),//{0.339, .883, 0.326},
            new ElectrodeData("FC1", 10, .375, .375, .848, "Brain Sound Bounces/CH-2","Brain Sound Bounces/TC3-2"),//{0.375, .375, .848},
            new ElectrodeData("FC2", 11, .375, -.375, .848, "Brain Sound Bounces/CH-3", "Brain Sound Bounces/TC3-3"),//{0.375, -.375, .848},
            new ElectrodeData("FC6", 12, .339, -.883, .326, "Brain Sound Bounces/MK-1", "Brain Sound Bounces/WR-1"),//{ 0.339, -.883, .326},
            new ElectrodeData("T7", 13, 0.0, .999, -0.0349, "Brain Sound Bounces/RE-2", "Brain Sound Bounces/AC-2"),//{6.12E-17, .999, -0.0349},
            new ElectrodeData("C3", 14, 0.0, .719, .695, "Brain Sound Bounces/RE-3", "Brain Sound Bounces/AC-3"),//{4.40E-17, .719, .695},
            new ElectrodeData("Cz", 15, 0.0, 0.0, 1.0, "Brain Sound Bounces/CH-4", "Brain Sound Bounces/TC3-4"),//{3.75E-33, -6.12E-17, 1},
            new ElectrodeData("C4", 16, 0.0, .719, .695, "Brain Sound Bounces/MK-2", "Brain Sound Bounces/WR-2"),//{4.40E-17, .719, .695},
            new ElectrodeData("T8", 17, 0.0, -.999, -.0349, "Brain Sound Bounces/MK-3", "Brain Sound Bounces/WR-3"), //{6.12E-17, -.999, -.0349},
            new ElectrodeData("CP5", 18, -0.339, .883, .326, "Brain Sound Bounces/RE-4", "Brain Sound Bounces/AC-4"),//{-0.339, .883, .326},
            new ElectrodeData("CP1", 19, -0.375, .375, .848, "Brain Sound Bounces/CB-1", "Brain Sound Bounces/TC4-1"),//{-0.375, .375, .848},
            new ElectrodeData("CP2", 20, -0.375, -.375, .848, "Brain Sound Bounces/VV-1", "Brain Sound Bounces/TC1-1"),//{-0.375, -.375, .848},
            new ElectrodeData("CP6", 21, -0.339, .883, .326, "Brain Sound Bounces/MK-4", "Brain Sound Bounces/WR-3"),//{-0.339, .883, .326},
            new ElectrodeData("P7", 22, -0.587, .809, -.0349, "Brain Sound Bounces/CB-2", "Brain Sound Bounces/TC4-2"),//{-0.587, .809, -.0349},
            new ElectrodeData("P3", 23, -0.673, .545, .5, "Brain Sound Bounces/CB-3", "Brain Sound Bounces/TC4-3"),//{-0.673, .545, .5},
            new ElectrodeData("Pz", 24, -0.719, 0, .695, "Brain Sound Bounces/FL-1", "Brain Sound Bounces/TC2-1"),//{-0.719, -8.81E-17, .695},
            new ElectrodeData("P4", 25, -0.673, -.545, .5, "Brain Sound Bounces/VV-2", "Brain Sound Bounces/TC1-2"),//{-0.673, -.545, .5},
            new ElectrodeData("P8", 26, -0.587, -.809, -0.0349, "Brain Sound Bounces/VV-3", "Brain Sound Bounces/TC1-3"),//{-0.587, -.809, -0.0349},
            new ElectrodeData("PO7", 27, -0.809, .587, -0.0349, "Brain Sound Bounces/CB-4", "Brain Sound Bounces/TC4-4"),//{-0.809, .587, -0.0349},
            new ElectrodeData("PO3", 28, -0.885, .376, .276, "Brain Sound Bounces/FL-2", "Brain Sound Bounces/TC2-2"),//{-0.885, 0.376, 0.276},
            new ElectrodeData("PO4", 29, -0.885, -0.376, .276, "Brain Sound Bounces/FL-3", "Brain Sound Bounces/TC2-3"),//{-0.885, -0.376, .276},
            new ElectrodeData("PO8", 30, -0.809, -0.587, -0.0349, "Brain Sound Bounces/VV-4", "Brain Sound Bounces/TC1-4"),//{-0.809, -0.587, -0.0349},
            new ElectrodeData("Oz", 31, -0.999, 0.000,  -0.0349, "Brain Sound Bounces/FL-4", "Brain Sound Bounces/TC2-4"),//{-0.999, -1.22E-16, -0.0349}
         };

            foreach (ElectrodeData e in electrode)
            {
                // Create a new electrode based on the supplied electrode template. And add it as a component to the supplied parent GameObject.
                GameObject newElectrode = spawn(template, new Vector3(e.x, e.y, e.z) * 5.0f, Quaternion.identity);
                newElectrode.transform.SetParent(parent.transform, false);
                newElectrode.name = e.name;
                newElectrode.SetActive(true);

                // Get the AudioSource objects from our template. We expect to have a Melodic source and a Percussive source.
                AudioSource[] srcs = newElectrode.GetComponentsInChildren<AudioSource>(true);
                if (srcs[0].name.Equals("Melodic"))
                {
                    e.melodic = srcs[0];
                    e.percussive = srcs[1];
                }
                else
                {
                    e.melodic = srcs[1];
                    e.percussive = srcs[0];
                }

                e.melodic.clip = e.melody_clip;
                e.percussive.clip = e.percussive_clip;
                e.light = newElectrode.GetComponentInChildren<Light>();
            }
        }
    }
}
