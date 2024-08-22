using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GlobalScripts.Effects;

namespace CandyMatch3.Scripts.Common.Databases
{
    [CreateAssetMenu(fileName = "Effect Database", menuName = "Scriptable Objects/Databases/Effect Database")]
    public class EffectDatabase : ScriptableObject
    {
        [Header("Sound Effect")]
        [SerializeField] public ItemSoundEffect SoundEffect;

        [Header("Match Effects")]
        [SerializeField] public GameObject BlueMatchEffect;
        [SerializeField] public GameObject GreenMatchEffect;
        [SerializeField] public GameObject OrangeMatchEffect;
        [SerializeField] public GameObject PurpleMatchEffect;
        [SerializeField] public GameObject RedMatchEffect;
        [SerializeField] public GameObject YellowMatchEffect;

        [Header("Special Item Effects")]
        [SerializeField] public GameObject BiscuitBreakEffect;
        [SerializeField] public GameObject ChocolateBreakEffect;
        [SerializeField] public GameObject MarshmallowBreakEffect;
        [SerializeField] public GameObject CollectibleEffect;

        [Header("Booster Effects")]
        [SerializeField] public GameObject SpawnBoosterEffect;
        [SerializeField] public GameObject ColorfulEffect;
        [SerializeField] public GameObject StripedHorizontal;
        [SerializeField] public GameObject StripedVertical;
        [SerializeField] public GameObject WrappedEffect;

        [Header("Stateful Effects")]
        [SerializeField] public GameObject IceEffect;
        [SerializeField] public GameObject HoneyEffect;
        [SerializeField] public GameObject SyrupEffect;
    }
}
