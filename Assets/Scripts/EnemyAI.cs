using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class EnemyAI : MonoBehaviour
{
    public enum EnemyState
    {
        Patroling,
        Chasing,
        Waiting,
        Attacking
    }

    public EnemyState currentState;

    private NavMeshAgent _AiAgent;

    private Transform _playerTransform;

    [SerializeField] Transform[] _patrolPoints;

   
    [SerializeField] float _visionRange = 10;
    [SerializeField] float _visionAngle = 120;
    private Vector3 _playerLastPosition;

   
    [SerializeField] float _attackRange = 2; 
    [SerializeField] float _attackCooldown = 1; 
    private float _lastAttackTime;

   
    private int _currentPatrolIndex = 0;

   
    private float _waitTimer = 0f;
    private float _waitDuration = 5f; 

    void Awake()
    {
        _AiAgent = GetComponent<NavMeshAgent>();
        _playerTransform = GameObject.FindWithTag("Player").transform;
    }

    void Start()
    {
        currentState = EnemyState.Patroling;
        SetPatrolPoint();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Patroling:
                Patrol();
                break;
            case EnemyState.Chasing:
                Chase();
                break;
            case EnemyState.Waiting:
                Waiting();
                break;
            case EnemyState.Attacking:
                Attacking();
                break;
        }
    }

    void Patrol()
    {
        if (OnRange())  currentState = EnemyState.Chasing; 
        
        else if (_AiAgent.remainingDistance < 0.5f && !_AiAgent.pathPending){
            currentState = EnemyState.Waiting;
            _waitTimer = 0f; 
        }
    }

    void SetPatrolPoint()
    {
        _AiAgent.destination = _patrolPoints[_currentPatrolIndex].position;
    }

    void Chase()
    {
        if (!OnRange()) currentState = EnemyState.Patroling; 
        
        else if (Vector3.Distance(transform.position, _playerTransform.position) <= _attackRange) currentState = EnemyState.Attacking; 
        
        else _AiAgent.destination = _playerTransform.position;
        
    }

    void Waiting()
    {
        _waitTimer += Time.deltaTime; 

        if (_waitTimer >= _waitDuration) {
            _currentPatrolIndex++; 
            if (_currentPatrolIndex >= _patrolPoints.Length) _currentPatrolIndex = 0; 
            
            currentState = EnemyState.Patroling; 
            SetPatrolPoint(); 
        }
    }

    void Attacking()
    {
        if (Vector3.Distance(transform.position, _playerTransform.position) <= _attackRange){
            Debug.Log("Atacando al jugador!"); 
            _lastAttackTime = Time.time;
            currentState = EnemyState.Chasing; 
        }
        else currentState = EnemyState.Chasing; 
        
    }

    bool OnRange()
    {
        Vector3 directionToPlayer = _playerTransform.position - transform.position;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer > _visionRange) return false;
        

        if (angleToPlayer > _visionAngle * 0.5f) return false;
        

        RaycastHit hit;
        if (Physics.Raycast(transform.position, directionToPlayer, out hit, distanceToPlayer))
        {
            if (hit.collider.CompareTag("Player")){
                _playerLastPosition = _playerTransform.position;
                return true; 
            }
            else return false;
            
        }

        return true; 
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _visionRange);

        Gizmos.color = Color.green;
        Vector3 fovLine1 = Quaternion.AngleAxis(_visionAngle * 0.5f, transform.up) * transform.forward * _visionRange;
        Vector3 fovLine2 = Quaternion.AngleAxis(-_visionAngle * 0.5f, transform.up) * transform.forward * _visionRange;

        Gizmos.DrawLine(transform.position, transform.position + fovLine1);
        Gizmos.DrawLine(transform.position, transform.position + fovLine2);
    }
}


   
        
        
    


