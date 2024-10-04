using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Gameplay.Miscs
{
    public class SwitchHand : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private readonly int _switchHash = Animator.StringToHash("Switch");
        private readonly int _directionHash = Animator.StringToHash("Direction");

        public void Switch(Vector3Int direction)
        {
            if (direction == Vector3Int.up)
                animator.SetInteger(_directionHash, 1);

            else if (direction == Vector3Int.down)
                animator.SetInteger(_directionHash, 2);

            else if (direction == Vector3Int.left)
                animator.SetInteger(_directionHash, 3);

            else if (direction == Vector3Int.right)
                animator.SetInteger(_directionHash, 4);

            animator.SetTrigger(_switchHash);
        }
    }
}
