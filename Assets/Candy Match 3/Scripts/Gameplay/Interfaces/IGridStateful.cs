using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IGridStateful
    {
        public bool IsLocked { get; }
        public bool CanContainItem { get; }
        public StatefulGroupType GroupType { get; }
        
        public void Release();
    }
}
