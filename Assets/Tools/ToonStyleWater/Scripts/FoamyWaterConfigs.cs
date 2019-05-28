using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WaterFoam
{
    
    public class FoamyWaterConfigs : ScriptableObject
    {
        public List<WaterConfig> configurations = new List<WaterConfig>();
    }

    [Serializable]
    public class WaterConfig
    {
        public string name;
        public string data;

        public WaterConfig()
        {
            name = "New Water Config";
        }
    }
}