namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IItemEffect
    {
        public void PlayStartEffect();
        public void PlayMatchEffect();
        public void PlayBreakEffect(int healthPoint);
        public void PlayReplaceEffect();
    }
}
