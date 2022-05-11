using Mirror;
using System.Collections;
using UnityEngine;

public class WarriorBehaviour : NetworkBehaviour
{    
    [SerializeField]
    private float _attackDistance;
    [SerializeField]
    private float _attackBoost;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private Rigidbody _rigidbody;
    [SerializeField]
    private Renderer _renderer;
    [SerializeField]
    private TextMesh _countText;

    private IEnumerator _marchCoroutine;
    private IEnumerator _siegeCoroutine;
    
    private Building _targetBuilding;
    private Transform _target;

    [SyncVar(hook=(nameof(OnCountChanged)))]
    private float _syncCount;    

    public Renderer Renderer => _renderer;
    public TextMesh CountText => _countText;
    public Player Owner { get; set; }
    
    public float Count
    {
        get
        {
            return _syncCount;
        }
        set
        {                                    
            _syncCount = value;
        }
    }

    public void OnCountChanged(float oldValue, float newValue)
    {
        _countText.text =
                newValue.
                ToString().
                Substring(0, Mathf.Min(4, newValue.ToString().Length));
    }

    public void StartMarch(Building targetBuilding)
    {
        _targetBuilding = targetBuilding;
        _target = targetBuilding.transform;

        _marchCoroutine = MarshToTarget();
        StartCoroutine(_marchCoroutine);
    }

    public IEnumerator MarshToTarget()
    {
        var direction = _target.position - transform.position;
        direction = direction.normalized;

        while (
            Vector3.Distance(transform.position, _target.position)
            > _attackDistance)
        {
            _rigidbody.velocity = direction * _speed;            

            yield return null;
        }

        _rigidbody.velocity = Vector3.zero;

        if (Owner != _targetBuilding.Owner)
        {
            StartSiege();
        }
        else
        {
            _targetBuilding.WarriorsCount += Count;
            Count = 0;                     
                        
            DestroyGameObject();            
        }
    }   
    
    public void MoveToTarget()
    {        
        var direction = _target.position - transform.position;
        _rigidbody.velocity = direction.normalized * _speed;       
    }

    public void StartSiege()
    {
        _siegeCoroutine = SiegeBuilding();
        StartCoroutine(_siegeCoroutine);
    }

    public IEnumerator SiegeBuilding()
    {
        _targetBuilding.StopProduction();
        
        while (Count > 0 && _targetBuilding.WarriorsCount > 0)
        {
            yield return null;

            var attackValue = Time.deltaTime * _attackBoost;
            _targetBuilding.Defend(attackValue);
            Count -= attackValue;
        }
        
        if (Count > 0)
        {
            SetupCapturedTargetBuild();
        }
        _targetBuilding.StartProduction();
                
        DestroyGameObject();        
    }

    private void SetupCapturedTargetBuild()
    {
        _targetBuilding.Owner = Owner;
        _targetBuilding.Defend(-Count);
        _targetBuilding.Color = Owner.Color;
        _targetBuilding.Renderer.material.color = Owner.Color;
    }

    private void DestroyGameObject()
    {
        NetworkServer.UnSpawn(gameObject);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (_siegeCoroutine != null)
        {
            StopCoroutine(_siegeCoroutine);
        }
        
        if (_marchCoroutine != null)
        {
            StopCoroutine(_marchCoroutine);
        }            
    }
}