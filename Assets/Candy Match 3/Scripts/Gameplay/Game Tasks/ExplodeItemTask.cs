using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using GlobalScripts.Extensions;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.Databases;
using CandyMatch3.Scripts.Common.Constants;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Common.DataStructs;
using CandyMatch3.Scripts.Gameplay.Effects;
using CandyMatch3.Scripts.Common.Messages;
using Cysharp.Threading.Tasks;
using MessagePipe;

namespace CandyMatch3.Scripts.Gameplay.GameTasks
{
    public class ExplodeItemTask : IDisposable
    {
        private readonly GridCellManager _gridCellManager;
        private readonly ExplodeEffectCollection _explodeEffectCollection;
        private readonly ISubscriber<UseInGameBoosterMessage> _useInGameBoosterSubscriber;

        private bool _anyInGameBoosterUse;
        private IDisposable _disposable;

        public ExplodeItemTask(GridCellManager gridCellManager, ExplodeEffectCollection explodeEffectCollection)
        {
            _gridCellManager = gridCellManager;
            _explodeEffectCollection = explodeEffectCollection;

            var builder = DisposableBag.CreateBuilder();
            _useInGameBoosterSubscriber = GlobalMessagePipe.GetSubscriber<UseInGameBoosterMessage>();
            _useInGameBoosterSubscriber.Subscribe(_ => OnInGameBoosterUsed()).AddTo(builder);
            _disposable = builder.Build();
        }

        public void Explode(Vector3 position, ExplodeType explodeType)
        {
            if (_anyInGameBoosterUse)
            {
                _anyInGameBoosterUse = false;
                return;
            }

            ExplodeEffectData explodeData = explodeType switch
            {
                ExplodeType.SingleWrapped => _explodeEffectCollection.SingleWrappedExplode,
                ExplodeType.DoubleWrapped => _explodeEffectCollection.DoubleWrappedExplode,
                _ => default
            };

            ExplodeEffect explodeEffect = EffectManager.Instance.PlayExplodeEffect(position);
            explodeEffect.PlayExplodeEffect(explodeData.DistortionStrength, explodeData.PropagationSpeed
                                            , explodeData.Wave, explodeData.Magnitude);
            explodeEffect.SetDuration(explodeData.Timer);
            explodeEffect.SetScale(explodeData.Scale);
        }

        public async UniTask Blast(Vector3Int pivot, Vector3Int size)
        {
            BoundsInt bounds = pivot.GetBounds2D(size);
            await Blast(pivot, bounds);
        }

        public async UniTask Blast(Vector3Int pivot, int range)
        {
            BoundsInt bounds = pivot.GetBounds2D(range);
            await Blast(pivot, bounds);
        }

        public async UniTask Blast(Vector3Int pivot, BoundsInt range)
        {
            using(ListPool<Vector3Int>.Get(out List<Vector3Int> boundsEdge))
            {
                boundsEdge.AddRange(range.Iterator2D());
                IGridCell centerCell = _gridCellManager.Get(pivot);
                Vector3 centerPosition = centerCell.WorldPosition;

                IGridCell gridCell = null;
                using (ListPool<UniTask>.Get(out List<UniTask> explodeTasks))
                {
                    for (int i = 0; i < boundsEdge.Count; i++)
                    {
                        if (boundsEdge[i] == pivot)
                            continue;

                        gridCell = _gridCellManager.Get(boundsEdge[i]);

                        if (gridCell == null || !gridCell.HasItem)
                            continue;

                        if (gridCell.IsLocked || !gridCell.IsMoveable)
                            continue;

                        if (gridCell.BlockItem is IItemAnimation animation)
                        {
                            Vector3 direction = (centerPosition - gridCell.WorldPosition).normalized;
                            float distance = GetDistance(pivot, gridCell.GridPosition);
                            float bounce = Match3Constants.ExplodeAmplitude * Mathf.Log(distance, Match3Constants.ExplosionPower);
                            explodeTasks.Add(animation.BounceInDirection(direction * bounce));
                        }
                    }

                    await UniTask.WhenAll(explodeTasks);
                }
            }
        }

        private float GetDistance(Vector3Int center, Vector3Int toPosition)
        {
            Vector3Int offset = toPosition - center;
            float distance = offset.magnitude;
            return distance;
        }

        private void OnInGameBoosterUsed()
        {
            _anyInGameBoosterUse = true;
        }

        public void Dispose()
        {
            _disposable.Dispose();
        }
    }
}
