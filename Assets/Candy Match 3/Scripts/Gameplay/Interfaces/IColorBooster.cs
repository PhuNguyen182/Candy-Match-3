using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IColorBooster : IBooster
    {
        public BoosterType ColorBoosterType { get; }
        public void SetBoosterType(BoosterType colorBoosterType);
        public UniTask PlayBoosterCombo(int direction, ComboBoosterType comboType, bool isFirstItem);
        public void ChangeItemSprite(int step);
    }
}
