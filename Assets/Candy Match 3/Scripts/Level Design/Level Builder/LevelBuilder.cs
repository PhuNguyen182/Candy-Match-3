using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Sirenix.OdinInspector;
using CandyMatch3.Scripts.LevelDesign.Databases;
using CandyMatch3.Scripts.Gameplay.Models;
using Newtonsoft.Json;

namespace CandyMatch3.Scripts.LevelDesign.LevelBuilder
{
    public class LevelBuilder : MonoBehaviour
    {
        [SerializeField] private TileDatabase tileDatabase;
        [SerializeField] private GridInformation gridInformation;

        [Header("Tilemaps")]
        [SerializeField] private Tilemap boardTilemap;

        [HorizontalGroup(GroupID = "Map Clear 1")]
        [Button]
        private void ClearEntities()
        {
        }

        [HorizontalGroup(GroupID = "Map Clear 1")]
        [Button]
        private void ClearCeil()
        {
        }

        [HorizontalGroup(GroupID = "Map Clear 2")]
        [Button]
        private void ClearBoardBottom()
        {
        }

        [HorizontalGroup(GroupID = "Map Clear 2")]
        [Button]
        private void ClearBoard()
        {
            boardTilemap.ClearAllTiles();
        }

        [Button]
        private void ClearAll()
        {
        }

        [Button]
        private void ScanTilemaps()
        {
        }

        [HorizontalGroup(GroupID = "Level Builder")]
        [Button(Style = ButtonStyle.Box)]
        public void Export(int level, bool useResource = true)
        {
            ScanTilemaps();

        }

        [HorizontalGroup(GroupID = "Level Builder")]
        [Button(Style = ButtonStyle.Box)]
        private void Import(int level, bool useResource = true)
        {
        }
    }
}
