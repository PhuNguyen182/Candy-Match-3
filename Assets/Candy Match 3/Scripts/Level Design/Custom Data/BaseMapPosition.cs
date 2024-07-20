using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.LevelDesign.CustomData
{
    public class BaseMapPosition<TMapData>
    {
        public TMapData MapData;
        public Vector3Int Position;
    }
}
