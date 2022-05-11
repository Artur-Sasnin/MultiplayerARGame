using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class Player : NetworkBehaviour
{
    [SerializeField]
    private List<Building> _buildingsPrefab;
    [SerializeField]
    private List<Transform> _buildingsPositions;
    [SerializeField]
    private PlayerUI _playerPrefabUI;
    [SerializeField]
    private GameObject _buildingMenu;    

    private bool _readyForBattle;    
    private float _totalCoins;
    private Building _chosenBuilding;
    private Building _targetBuilding;
    private Color _chosenColor;
    private Color _preparedColor;
    private Text _totalCoinsText;
    
    private SyncList<Building> _buildings = new SyncList<Building>();    

    [SyncVar(hook = nameof(OnColorChanged))]
    private Color _syncColor;
        
    public float TotalCoins => _totalCoins;
    public Color Color => _syncColor;
    public SyncList<Building> Buildings => _buildings;
    
    public Text TotalCoinsText
    {
        get => _totalCoinsText;
        set => _totalCoinsText = value;
    }

    public Building ChosenBuilding
    {
        get => _chosenBuilding;
        set
        {            
            _chosenBuilding = value;
            _chosenBuilding.Renderer.material.color = _chosenColor;
            OnOpenBuildingMenu();            
        }
    }
       
    public Building TargetBuilding
    {
        set
        {            
            if (_readyForBattle && _chosenBuilding != value)
            {                
                if (isServer)
                {
                    _chosenBuilding.SendWarriors(value);
                }
                else
                {
                    _chosenBuilding.CmdSendWarriors(value);
                }
            }
        }
    }

    public override void OnStartServer()
    {        
        _syncColor = new Color(
            UnityEngine.Random.Range(0.0f, 1.0f),
            UnityEngine.Random.Range(0.0f, 1.0f),
            UnityEngine.Random.Range(0.0f, 1.0f));

        CreateBuildings();                
    }

    public void CreateBuildings()
    {
        int length = _buildingsPrefab.Count;

        for (int i = 0; i < length; i++)
        {
            var building = Instantiate(
                _buildingsPrefab[i],
                _buildingsPositions[i].position,
                Quaternion.identity);

            building.Owner = this;
            _buildings.Add(building);

            if (isServer)
            {
                building.Color = _syncColor;
                NetworkServer.Spawn(building.gameObject);
                building.netIdentity.AssignClientAuthority(connectionToClient);
            }
        }
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        _totalCoinsText = Instantiate(_playerPrefabUI).TotalCoinsText;        
        
        foreach (var building in _buildings)
        {            
            building.StartProduction();
        }        
    }            

    public void ChangeTotalCoinsOnValue(float value)
    {
        if (isLocalPlayer)
        {
            _totalCoins += value;
            _totalCoinsText.text = _totalCoins.ToString();
        }                
    }

    public void OnColorChanged(Color oldColor, Color newColor)
    {
        _chosenColor = newColor + Color.yellow;
        _preparedColor = newColor + Color.red;
    }

    public void OnOpenBuildingMenu()
    {
        _buildingMenu.SetActive(true);
    }

    public void OnPurchaseUpdating()
    {
        var cost = _chosenBuilding.UpgradingCost;

        if (cost < TotalCoins)
        {
            ChangeTotalCoinsOnValue(-cost);
            _chosenBuilding.OnUpgradeBuilding();
        }                
    }

    public void OnPrepareForBattle()
    {        
        _readyForBattle = !_readyForBattle;
        
        if (_readyForBattle)
        {
            _chosenBuilding.Renderer.material.color = _preparedColor;
        }        
        else
        {
            _chosenBuilding.Renderer.material.color = _syncColor;
        }
    }

    public void OnCloseBuildingMenu()
    {
        if (_readyForBattle)
        {
            OnPrepareForBattle();
        }

        _chosenBuilding.Renderer.material.color = _syncColor;
        _buildingMenu.SetActive(false);
        _chosenBuilding = null;        
    }
}
