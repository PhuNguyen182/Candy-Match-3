using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Common.Controllers
{
    public class FramerateController : MonoBehaviour
    {
        [SerializeField] private bool useDesiredValue = true;
        [SerializeField] private int desiredFramerate = 60;

        private void Awake()
        {
#if !UNITY_EDITOR
            Application.targetFrameRate = useDesiredValue ? desiredFramerate : (int)Screen.currentResolution.refreshRateRatio.value;
#endif
        }
    }
}
