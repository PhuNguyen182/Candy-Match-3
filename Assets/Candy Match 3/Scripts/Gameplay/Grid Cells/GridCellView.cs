using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using Cysharp.Threading.Tasks;
using DG.Tweening;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CandyMatch3.Scripts.Gameplay.GridCells
{
    public class GridCellView : MonoBehaviour, IGridCellView
    {
        [SerializeField] private SpriteRenderer groundCell;
        [SerializeField] private Vector3Int gridPosition;

        public Vector3Int GridPosition => gridPosition;

        private CancellationToken _destroyToken;

        private void Awake()
        {
            _destroyToken = this.GetCancellationTokenOnDestroy();
        }

        public void SetGridPosition(Vector3Int position)
        {
            gridPosition = position;
            gameObject.name = $"Grid Cell: ({gridPosition.x}, {gridPosition.y})";
        }

        public void PlayGlowGroundCell()
        {

        }

        public void SetWorldPosition(Vector3 position)
        {
            transform.position = position;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            string pos = $"({GridPosition.x}, {GridPosition.y})";
            DrawString(pos, transform.position + Vector3.up * 0.5f, Color.black);
        }

        static void DrawString(string text, Vector3 worldPos, Color? colour = null)
        {
            Handles.BeginGUI();

            if (colour.HasValue)
                GUI.color = colour.Value;

            SceneView view = SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));

            GUI.Label(new Rect(screenPos.x - (size.x / 2)
                               , -screenPos.y + view.position.height + 4
                               , size.x, size.y), text);
            Handles.EndGUI();
        }
#endif
    }
}
