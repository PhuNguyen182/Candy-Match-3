using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IColorBooster : IBooster
    {
        public BoosterType ColorBoosterType { get; }
        public void SetBoosterType(BoosterType colorBoosterType);
    }
}
