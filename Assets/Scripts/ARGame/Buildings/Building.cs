using Mirror;
using System.Collections;
using UnityEngine;

public abstract class Building : NetworkBehaviour
{
    [SerializeField]
    private float _coinsPerSecond = 1;
    [SerializeField]
    private float _warriorsPerSecond = 1;
    [SerializeField]
    private float _startUpgradeCost = 50;
    [SerializeField]
    private float _minWarriorsCountToSend = 10;
    [Range(0, 1)]
    [SerializeField]    
    private float _addiotionPersentForUpgrading = 0.1f;        
    [SerializeField]
    private TextMesh _warriorsTextField;
    [SerializeField]
    private WarriorBehaviour _warriourPrefab;
    [SerializeField]
    private Renderer _renderer;

    private float _coinsCounter = 0;    
    private IEnumerator _timerCoroutine;

    [SyncVar(hook = nameof(OnWarriorsCountChanged))]
    private float _syncWarriorCount;
    [SyncVar(hook = nameof(OnOwnerChanged))]    
    private Player _syncOwner;
    [SyncVar(hook = nameof(OnColorChanged))]
    private Color _syncColor;

    public Renderer Renderer => _renderer;
    public float CoinsBoost { get; set; }
    public float WarriorsBoost { get; set; }
    public float UpgradingCost { get; set; }
    public float UpgradeCostMultiplier { get; private set; }    

    public float WarriorsCount
    {
        get => _syncWarriorCount;
        set => _syncWarriorCount = value;
    }    
    
    public Player Owner
    {
        get => _syncOwner;
        set => _syncOwner = value;        
    }

    public Color Color
    {
        get => _syncColor;
        set => _syncColor = value;
    }

    public void Awake()
    {
        CoinsBoost = _coinsPerSecond;
        WarriorsBoost = _warriorsPerSecond;
        UpgradingCost = _startUpgradeCost;
        UpgradeCostMultiplier = 1 + _addiotionPersentForUpgrading;
        
        _syncWarriorCount = 0;                
    }

    public void OnWarriorsCountChanged(float oldCount, float newCount)
    {
        if (isLocalPlayer)
        {
            CmdChangeWarriorsText(newCount);
        }
        else
        {
            ChangeWarriorsText(newCount);
        }
    }

    public void OnOwnerChanged(Player oldOwner, Player newOwner)
    {        
    }

    public void OnColorChanged(Color oldColor, Color newColor)
    {
        _renderer.material.color = newColor;
    }

    public void OnChoseBuilding()
    {
        var localPlayer =
            NetworkClient.localPlayer.gameObject.GetComponent<Player>();

        if (localPlayer == _syncOwner && !_syncOwner.ChosenBuilding)
        {
            localPlayer.ChosenBuilding = this;
        }
        else
        {
            if (localPlayer.ChosenBuilding)
            {
                localPlayer.TargetBuilding = this;
            }
        }
    }

    [Command]
    public void CmdChangeWarriorsText(float newCount)
    {
        ChangeWarriorsText(newCount);
    }
            
    public void ChangeWarriorsText(float newCount)
    {   
        _warriorsTextField.text = newCount.ToString();        
    }    

    [Command]
    public void CmdSendWarriors(Building target)
    {
        SendWarriors(target);
    }
    
    [ClientRpc]
    public void SendWarriors(Building target)
    {
        if (_syncWarriorCount >_minWarriorsCountToSend)
        {            
            var warrior = Instantiate(
                _warriourPrefab,
                transform.position,
                Quaternion.identity);
            warrior.Owner = Owner;
            var warriorCount = _syncWarriorCount / 2;
            warrior.Count = warriorCount;
            warrior.CountText.text = warriorCount.ToString();
            _syncWarriorCount /= 2;            
            warrior.transform.LookAt(target.transform.position);            
            warrior.StartMarch(target);                   
        }        
    }

    public abstract void OnUpgradeBuilding();

    [Command]
    public void CmdStartProduction()
    {
        StartProduction();
    }
    
    public void StartProduction()
    {                        
        if (_timerCoroutine == null)
        {
            _timerCoroutine = IncreaseCounter();
            StartCoroutine(_timerCoroutine);
        }
        else
        {
            StopProduction();
            _timerCoroutine = IncreaseCounter();
            StartCoroutine(_timerCoroutine);
        }        
    }
    
    public void Defend(float value)
    {
        _syncWarriorCount -= value;
    }

    public void StopProduction()
    {        
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }                
    }
        
    private IEnumerator IncreaseCounter()
    {                        
        while (true)
        {
            yield return null;     
            
            var coinsAdditionValue = Time.deltaTime * CoinsBoost;            
                
            if (_syncOwner.isLocalPlayer)
            {
                CmdChangeWarriorCount();
            }                
            else
            {
                ChangeWarriorCount();
            }            
            _syncOwner.ChangeTotalCoinsOnValue(coinsAdditionValue);
        }               
    }

    [Command]
    private void CmdChangeWarriorCount()
    {
        ChangeWarriorCount();
    }

    [Server]
    private void ChangeWarriorCount()
    {        
        _syncWarriorCount += Time.deltaTime * WarriorsBoost;
    }

    private void OnDestroy()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }        
    }
}
