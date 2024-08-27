using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IBooster
    {
        public bool IsNewCreated { get; set; }
        public bool IsActivated { get; set; }

        public UniTask Activate();
        public void Explode();
    }
}
