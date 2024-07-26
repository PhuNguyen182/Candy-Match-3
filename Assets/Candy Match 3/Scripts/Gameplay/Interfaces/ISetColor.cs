using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface ISetColor
    {
        public CandyColor CandyColor { get; }

        public void SetColor(CandyColor candyColor);
    }
}
