using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IItemSuggest
    {
        public UniTask Highlight();

        public void Cancel();
    }
}
