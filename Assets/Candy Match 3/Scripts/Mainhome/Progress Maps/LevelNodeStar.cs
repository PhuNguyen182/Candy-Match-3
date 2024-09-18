using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyMatch3.Scripts.Mainhome.ProgressMaps
{
    public class LevelNodeStar : MonoBehaviour
    {
        [SerializeField] private Animator[] starAnimators;

        public void UpdateStars(int star, bool useAnimation)
        {
            for (int i = 0; i < starAnimators.Length; i++)
            {
                bool isActive = i + 1 <= star;
                starAnimators[i].enabled = useAnimation;
                starAnimators[i].gameObject.SetActive(isActive);
            }
        }
    }
}
