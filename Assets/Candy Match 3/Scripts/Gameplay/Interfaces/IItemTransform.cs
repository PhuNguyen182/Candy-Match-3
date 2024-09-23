using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IItemTransform
    {
        public UniTask TransformTo(ItemType itemType);
    }
}
