using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Mainhome.Miscs
{
    public class MapCloud : MonoBehaviour
    {
        [SerializeField] private int animationIndex;
        [SerializeField] private Animator cloudAnimator;

        private readonly int _cloudHash = Animator.StringToHash("Cloud");

        private void Awake()
        {
            cloudAnimator.SetInteger(_cloudHash, animationIndex);
        }
    }
}
