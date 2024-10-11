using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IItemSuggest
    {
        public bool IsSuggesting { get; set; }
        public void Highlight(bool isActive);
    }
}
