using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using GlobalScripts.Service;
using GlobalScripts.Utils;

namespace CandyMatch3.Scripts.GameData.GameResources
{
    [Serializable]
    public class GameResourceManager : IService
    {
        public long LivesTimer;
        public List<ResourceData> GameResouces;

        public Dictionary<GameResourceType, ResourceData> ResourceCollection { get; private set; }

        public GameResourceManager()
        {
            GameResouces = new();
            ResourceCollection = new();
        }

        public void Initialize()
        {
            ResourceCollection.Clear();

            for (int i = 0; i < GameResouces.Count; i++)
            {
                ResourceCollection.Add(GameResouces[i].ID, GameResouces[i]);
            }
        }

        public DateTime GetHeartTime()
        {
            return TimeUtils.BinaryToDatetime(LivesTimer);
        }

        public void SetHeartTime(DateTime time)
        {
            LivesTimer = TimeUtils.DatetimeToBinary(time);
        }

        public void EarnResource(GameResourceType resourceType, int amount)
        {
            if (ResourceCollection.ContainsKey(resourceType))
            {
                ResourceData resource = ResourceCollection[resourceType];
                resource.Amount = resource.Amount + amount;
                ResourceCollection[resourceType] = resource;
            }

            else
            {
                ResourceData resource = new ResourceData
                {
                    ID = resourceType,
                    Amount = amount
                };

                ResourceCollection.Add(resourceType, resource);
            }
        }

        public void SpendResource(GameResourceType resourceType, int amount)
        {
            if (ResourceCollection.ContainsKey(resourceType))
            {
                ResourceData resource = ResourceCollection[resourceType];
                resource.Amount = resource.Amount - amount;
                ResourceCollection[resourceType] = resource;
            }
        }

        public int GetResource(GameResourceType resourceType)
        {
            if (!ResourceCollection.TryGetValue(resourceType, out var resource))
            {
                ResourceCollection.Add(resourceType, new ResourceData
                {
                    ID = resourceType,
                    Amount = 0
                });
            }

            return resource.Amount;
        }

        public void SetResource(GameResourceType resourceType, int amount)
        {
            if (ResourceCollection.ContainsKey(resourceType))
            {
                ResourceCollection[resourceType] = new ResourceData
                {
                    ID = resourceType,
                    Amount = amount
                };
            }

            else
            {
                ResourceData resource = new ResourceData
                {
                    ID = resourceType,
                    Amount = amount
                };

                ResourceCollection.Add(resourceType, resource);
            }
        }

        public void ReleaseBack()
        {
            GameResouces.Clear();

            foreach (ResourceData resource in ResourceCollection.Values)
            {
                GameResouces.Add(resource);
            }
        }
    }
}
