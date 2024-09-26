using UnityEngine;
using Cysharp.Threading.Tasks;

namespace CandyMatch3.Scripts.Gameplay.Interfaces
{
    public interface IMatchAnimation
    {
        public UniTask MatchTo(Vector3 position, float duration);
    }
}
