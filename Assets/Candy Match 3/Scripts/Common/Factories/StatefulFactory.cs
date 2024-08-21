using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CandyMatch3.Scripts.Common.Enums;
using CandyMatch3.Scripts.Common.CustomData;
using CandyMatch3.Scripts.Gameplay.Statefuls;
using CandyMatch3.Scripts.Gameplay.Interfaces;
using CandyMatch3.Scripts.Gameplay.GridCells;
using CandyMatch3.Scripts.Common.Databases;
using GlobalScripts.Factories;

public class StatefulFactory : BaseFactory<StatefulBlockPosition, BaseStateful>
{
    private readonly GridCellManager _gridCellManager;
    private readonly StatefulSpriteDatabase _statefulSpriteDatabase;

    public StatefulFactory(GridCellManager gridCellManager, StatefulSpriteDatabase statefulSpriteDatabase)
    {
        _gridCellManager = gridCellManager;
        _statefulSpriteDatabase = statefulSpriteDatabase;
    }

    public override BaseStateful Produce(StatefulBlockPosition param)
    {
        IGridCell gridCell = _gridCellManager.Get(param.Position);

        BaseStateful stateful = param.ItemData.GroupType switch
        {
            StatefulGroupType.Available => new AvailableState(),
            StatefulGroupType.Ice => new IceState(_statefulSpriteDatabase.IceState),
            StatefulGroupType.Honey => new HoneyState(_statefulSpriteDatabase.HoneyState),
            StatefulGroupType.Syrup => new SyrupState(_statefulSpriteDatabase.SyrupStates),
            _ => new NotAvailableState()
        };

        stateful.GridCellView = gridCell.GridCellView;
        stateful.SetHealthPoint(param.ItemData.HealthPoint);
        return stateful;
    }
}
