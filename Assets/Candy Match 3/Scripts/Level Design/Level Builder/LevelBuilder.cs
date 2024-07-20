using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;

namespace CandyMatch3.Scripts.LevelDesign.LevelBuilder
{
    public class LevelBuilder : MonoBehaviour
    {
        [SerializeField] private GridInformation gridInformation;

        [Header("Tilemaps")]
        [SerializeField] private Tilemap boardTilemap;
    }
}
