using Cysharp.Threading.Tasks;
using CandyMatch3.Scripts.Common.Enums;

namespace CandyMatch3.Scripts.Common.Messages
{
    public struct DecreaseTargetMessage
    {
        public UniTask Task;
        public TargetEnum TargetType;
        public bool HasMoveTask;
    }
}
