using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.LoadingScene.Miscs
{
    public class FlyingItem : MonoBehaviour
    {
        [SerializeField] private Transform item;
        [SerializeField] private Animator animator;

        private int _flyHash;

        private void Start()
        {
            _flyHash = Animator.StringToHash("Flying");
            int rand = Random.Range(0, 6);
            animator.SetInteger(_flyHash, rand);
        }
    }
}
