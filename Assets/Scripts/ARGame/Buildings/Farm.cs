using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Farm : Building
{
    [SerializeField]
    private float _boostIncreasePercentage = 0.1f;

    public override void OnUpgradeBuilding()
    {
        if (Owner.TotalCoins >= UpgradingCost)
        {            
            CoinsBoost *= (1 + _boostIncreasePercentage);
            UpgradingCost *= UpgradeCostMultiplier;
        }
    }
}
