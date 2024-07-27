using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IGridStateful
    {
        public StatefulGroupType GroupType { get; }
        
        public void Release();
    }
}
