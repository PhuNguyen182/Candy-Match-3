using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface ISetColorBooster
    {
        public ColorBoosterType ColorBoosterType { get; }

        public void SetBoosterColor(ColorBoosterType colorBoosterType);
    }
}
