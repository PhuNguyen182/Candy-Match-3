using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.EditorOnly
{
    public class TimescaleController : MonoBehaviour
    {
        [SerializeField] private float[] timescales;

        private int _check = 0;
        private int _count = 0;

#if UNITY_EDITOR
        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    _check = _check + 1;
            //    _count = _check % timescales.Length;
            //    Time.timeScale = timescales[_count];
            //}
        }
#endif

    }
}
