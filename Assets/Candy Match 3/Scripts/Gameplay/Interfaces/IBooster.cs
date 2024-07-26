using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IBooster
    {
        public UniTask Activate();
        public void Explode();
    }
}
