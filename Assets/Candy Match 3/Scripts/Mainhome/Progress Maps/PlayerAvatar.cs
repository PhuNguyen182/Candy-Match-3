using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CandyMatch3.Scripts.Mainhome.ProgressMaps
{
    public class PlayerAvatar : MonoBehaviour
    {
        [SerializeField] private Image playerAvatar;

        private void Start()
        {
            // Set avatar on start here
        }

        public void SetAvatar(Sprite avatar)
        {
            playerAvatar.sprite = avatar;
        }
    }
}
