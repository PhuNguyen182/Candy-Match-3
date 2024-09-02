using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Gameplay.Statefuls;
using CandyMatch3.Scripts.Common.Databases;

namespace CandyMatch3.Scripts.Common.Factories
{
    public class FactoryManager
    {
        public ItemFactory ItemFactory { get; }
        public StatefulFactory StatefulFactory { get; }

        public FactoryManager(GridCellManager gridCellManager
            , StatefulSpriteDatabase statefulSpriteDatabase
            , ItemDatabase itemDatabase, Transform itemContainer)
        {
            ItemFactory = new(itemDatabase, itemContainer);
            StatefulFactory = new(gridCellManager, statefulSpriteDatabase);
        }

        public BaseStateful ProduceStateful(StatefulBlockPosition blockPosition)
        {
            return StatefulFactory.Produce(blockPosition);
        }
    }
}
