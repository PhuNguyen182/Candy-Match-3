using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameItems.Colored
{
    public class ColoredBooster : BaseItem, ISetColor, IColorBooster, IItemAnimation, IItemEffect, IColorfulEffect, IItemSuggest
    {
        [SerializeField] private BoosterType colorBoosterType;
        [SerializeField] private ItemAnimation itemAnimation;

        [Header("Colored Sprites")]
        [SerializeField] private Sprite[] normalSprites;
        [SerializeField] private Sprite[] wrappedSprites;
        [SerializeField] private Sprite[] horizontalSprites;
        [SerializeField] private Sprite[] verticalSprites;

        private bool _isMatchable;
        private float _explodeTimer = 0;
        private bool _isBoardStop;

        private Sprite _normalSprite;
        private Sprite _horizontalIcon;
        private Sprite _verticalIcon;

        private IPublisher<DecreaseTargetMessage> _decreaseTargetPublisher;
        private IPublisher<ActivateBoosterMessage> _activateBoosterPublisher;
        private ISubscriber<BoardStopMessage> boardStopSubscriber;

        public override bool IsMatchable => _isMatchable;

        public override bool IsMoveable => true;

        public override bool Replacable => false;

        public bool IsActivated { get; set; }

        public bool IsNewCreated { get; set; }

        public BoosterType ColorBoosterType => colorBoosterType;

        public bool IsActive { get; set; }

        private void Update()
        {
            if (!IsActive)
                return;

            if (!_isBoardStop)
            {
                IsLocking = true;
                _explodeTimer = 0;
                return;
            }

            else
            {
                if (_explodeTimer < Match3Constants.ExplosionDelay)
                {
                    IsLocking = true;
                    _explodeTimer += Time.deltaTime;

                    if (_explodeTimer > Match3Constants.ExplosionDelay)
                        TriggerBooster();
                }
            }
        }

        public override void ResetItem()
        {
            base.ResetItem();
            SetMatchable(true);
            OnItemReset().Forget();
        }

        public override void SetMatchable(bool isMatchable)
        {
            IsActive = !isMatchable;
            _isMatchable = isMatchable;
        }

        public override void InitMessages()
        {
            _decreaseTargetPublisher = GlobalMessagePipe.GetPublisher<DecreaseTargetMessage>();
            _activateBoosterPublisher = GlobalMessagePipe.GetPublisher<ActivateBoosterMessage>();
            DisposableBagBuilder builder = DisposableBag.CreateBuilder();

            boardStopSubscriber = GlobalMessagePipe.GetSubscriber<BoardStopMessage>();
            boardStopSubscriber.Subscribe(message => _isBoardStop = message.IsStopped)
                               .AddTo(builder);

            messageDisposable = builder.Build();
        }

        public override async UniTask ItemBlast()
        {
            await UniTask.CompletedTask;
        }

        public void SetColor(CandyColor candyColor)
        {
            this.candyColor = candyColor;
        }

        public void SetBoosterType(BoosterType colorBoosterType)
        {
            this.colorBoosterType = colorBoosterType;

            int colorIndex = candyColor switch
            {
                CandyColor.Blue => 0,
                CandyColor.Green => 1,
                CandyColor.Orange => 2,
                CandyColor.Purple => 3,
                CandyColor.Red => 4,
                CandyColor.Yellow => 5,
                _ => -1
            };

            Sprite colorSprite = colorBoosterType switch
            {
                BoosterType.Horizontal => horizontalSprites[colorIndex],
                BoosterType.Vertical => verticalSprites[colorIndex],
                BoosterType.Wrapped => wrappedSprites[colorIndex],
                _ => null
            };

            _normalSprite = normalSprites[colorIndex];
            _horizontalIcon = horizontalSprites[colorIndex];
            _verticalIcon = verticalSprites[colorIndex];

            itemGraphics.SetItemSprite(colorSprite);
            SetTargetType();
        }

        public void ChangeItemSprite(int step)
        {
            if(step == 1)
                itemGraphics.SetItemSprite(_horizontalIcon);
            else if(step == 2)
                itemGraphics.SetItemSprite(_verticalIcon);
        }

        public UniTask PlayBoosterCombo(int direction, ComboBoosterType comboType, bool isFirstItem)
        {
            UniTask boosterTask = UniTask.CompletedTask;

            if (comboType == ComboBoosterType.StripedWrapped)
                boosterTask = itemAnimation.PlayStripedWrapped();
            
            else if(comboType == ComboBoosterType.DoubleWrapped)
                boosterTask = itemAnimation.PlayDoubleWrapped(direction, isFirstItem);
            
            return boosterTask;
        }

        public async UniTask Activate()
        {
            Explode();
            await UniTask.CompletedTask;
        }

        public void Explode()
        {
            float x = WorldPosition.x;
            float y = WorldPosition.y;

            Vector3 position = colorBoosterType switch
            {
                BoosterType.Horizontal => new(0, y),
                BoosterType.Vertical => new(x, 0),
                BoosterType.Wrapped => new(x, y),
                _ => Vector3.zero
            };

            SoundEffectType soundEffect = colorBoosterType == BoosterType.Wrapped ? SoundEffectType.CandyWrap
                                                                                       : SoundEffectType.LineVerticalHorizontal;
            EffectManager.Instance.SpawnBoosterEffect(itemType, colorBoosterType, position);
            EffectManager.Instance.PlaySoundEffect(soundEffect);
        }

        public bool Break()
        {
            return !IsNewCreated;
        }

        public override void ReleaseItem()
        {
            _decreaseTargetPublisher.Publish(new DecreaseTargetMessage
            {
                TargetType = targetType,
                Task = UniTask.CompletedTask,
                HasMoveTask = false
            });

            itemAnimation.ToggleSuggest(false);
            SimplePool.Despawn(this.gameObject);
        }

        public void BounceOnTap()
        {
            itemAnimation.BounceTap();
        }

        public UniTask BounceInDirection(Vector3 direction)
        {
            return itemAnimation.BounceMove(direction);
        }

        public UniTask MoveTo(Vector3 position, float duration)
        {
            return itemAnimation.MoveTo(position, duration);
        }

        public void JumpDown(float amptitude)
        {
            itemAnimation.JumpDown(amptitude);
        }

        public UniTask SwapTo(Vector3 position, float duration, bool isMoveFirst)
        {
            return itemAnimation.SwapTo(position, duration, isMoveFirst);
        }

        private void SetTargetType()
        {
            targetType = itemType switch
            {
                ItemType.BlueHorizontal => TargetEnum.BlueHorizontal,
                ItemType.BlueVertical => TargetEnum.BlueVertical,
                ItemType.BlueWrapped => TargetEnum.BlueWrapped,

                ItemType.GreenHorizontal => TargetEnum.GreenHorizontal,
                ItemType.GreenVertical => TargetEnum.GreenVertical,
                ItemType.GreenWrapped => TargetEnum.GreenWrapped,

                ItemType.OrangeHorizontal => TargetEnum.OrangeHorizontal,
                ItemType.OrangeVertical => TargetEnum.OrangeVertical,
                ItemType.OrangeWrapped => TargetEnum.OrangeWrapped,

                ItemType.PurpleHorizontal => TargetEnum.PurpleHorizontal,
                ItemType.PurpleVertical => TargetEnum.PurpleVertical,
                ItemType.PurpleWrapped => TargetEnum.PurpleWrapped,

                ItemType.RedHorizontal => TargetEnum.RedHorizontal,
                ItemType.RedVertical => TargetEnum.RedVertical,
                ItemType.RedWrapped => TargetEnum.RedWrapped,

                ItemType.YellowHorizontal => TargetEnum.YellowHorizontal,
                ItemType.YellowVertical => TargetEnum.YellowVertical,
                ItemType.YellowWrapped => TargetEnum.YellowWrapped,

                _ => TargetEnum.None
            };
        }

        public void PlayColorfulEffect()
        {
            itemAnimation.TriggerVibrate();
        }

        public void PlayBoosterEffect(BoosterType boosterType)
        {
            
        }

        public void PlayStartEffect()
        {
            EffectManager.Instance.SpawnNewCreatedEffect(WorldPosition);
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.BoosterAward);
        }

        public void PlayMatchEffect()
        {
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.CandyMatch);
            EffectManager.Instance.SpawnColorEffect(candyColor, WorldPosition);
        }

        public void PlayBreakEffect()
        {
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.CandyMatch);
            EffectManager.Instance.SpawnColorEffect(candyColor, WorldPosition);
        }

        public void PlayReplaceEffect()
        {
            EffectManager.Instance.SpawnNewCreatedEffect(WorldPosition);
            EffectManager.Instance.PlaySoundEffect(SoundEffectType.BoosterAward);
        }

        private async UniTask OnItemReset()
        {
            _explodeTimer = 0;
            IsLocking = false;
            IsNewCreated = true;
            IsActivated = false;

            TimeSpan delay = TimeSpan.FromSeconds(Match3Constants.ItemMatchDelay);
            await UniTask.Delay(delay, false, PlayerLoopTiming.FixedUpdate, destroyToken);
            IsNewCreated = false;
        }

        private void TriggerBooster()
        {
            _activateBoosterPublisher.Publish(new ActivateBoosterMessage
            {
                Position = GridPosition,
                Sender = this
            });
        }

        public void Highlight(bool isActive)
        {
            itemAnimation.ToggleSuggest(isActive);
        }

        public void TriggerNextStage(int stage = 0)
        {
            if(stage == 3)
                itemGraphics.SetItemSprite(_normalSprite);
            
            itemAnimation.TriggerVibrate(stage);
        }

        public override void OnDisappear()
        {
            IsActive = false;
            _isBoardStop = false;
        }
    }
}
