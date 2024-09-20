using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IGridStateful
    {
        public bool IsLocked { get; }
        public bool CanContainItem { get; }
        public bool IsAvailable { get; }

        public StatefulGroupType GroupType { get; }
        public StatefulLayer StatefulLayer { get; }
        public IGridCellView GridCellView { get; set; }
        public TargetEnum TargetType { get; }

        public void Release();
        public void InitMessages();
        public void ResetState();
    }
}
