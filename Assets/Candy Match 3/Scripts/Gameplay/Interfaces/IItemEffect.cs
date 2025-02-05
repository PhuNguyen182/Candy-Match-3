using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IItemEffect
    {
        public void PlayStartEffect();
        public void PlayMatchEffect();
        public void PlayBreakEffect();
        public void PlayBoosterEffect(BoosterType boosterType);
        public void PlayReplaceEffect();
    }
}
