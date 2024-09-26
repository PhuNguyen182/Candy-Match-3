using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IItemTransform
    {
        public void SwitchTo(ItemType itemType);
        public UniTask Transform(float delay = 0);
        public void TransformImmediately();
    }
}
