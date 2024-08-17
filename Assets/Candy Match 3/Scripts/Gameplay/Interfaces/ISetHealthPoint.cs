namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface ISetHealthPoint
    {
        public int MaxHealthPoint { get; }

        public void SetHealthPoint(int healthPoint);
    }
}
