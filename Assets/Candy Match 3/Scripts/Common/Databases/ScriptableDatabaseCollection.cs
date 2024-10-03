using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.LevelDesign.Databases;

namespace CandyMatch3.Scripts.Common.Databases
{
    public class ScriptableDatabaseCollection : MonoBehaviour
    {
        [SerializeField] public ItemDatabase ItemDatabase;
        [SerializeField] public TileDatabase TileDatabase;
        [SerializeField] public TargetDatabase TargetDatabase;
        [SerializeField] public EffectDatabase EffectDatabase;
        [SerializeField] public MiscCollection MiscCollection;
        [SerializeField] public InGameBoosterPackDatabase InGameBoosterPackDatabase;
        [SerializeField] public StatefulSpriteDatabase StatefulSpriteDatabase;
        [SerializeField] public ExplodeEffectCollection ExplodeEffectCollection;
    }
}
