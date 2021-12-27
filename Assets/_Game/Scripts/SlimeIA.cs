using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SlimeIA : MonoBehaviour
{
    [Header("vida")]
    [SerializeField] private int heath;
    [SerializeField] private ENEMY_STATE state;

    private readonly int layerDeath = 10;
    private Animator anim;

    // Condições
    private bool isDeath;
    private bool isWalk;
    private bool isAlert;
    private bool isPlayerVisible;
    private bool isAttack;

    private bool isDeadPlayer = false;

    //I.A
    private NavMeshAgent agent;
    private Vector3 destination;
    private int idWayPoint; 
    private int rand;

    //GAME_MANAGER
    private GameManager _gameManager;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = FindObjectOfType(typeof(GameManager)) as GameManager;

        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        ChangeState(state);
    }

    // Update is called once per frame
    void Update()
    {
        StateManager();

        if (agent.desiredVelocity.magnitude >= 0.1f)
            isWalk = true;
        else
            isWalk = false;

        anim.SetBool("isWalk", isWalk);
        anim.SetBool("isAlert", isAlert);
         
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player") && ( state == ENEMY_STATE.IDLE || state == ENEMY_STATE.PATROL))
        {
            if (other.GetComponent<PlayerController>().isDead)
            {
                isDeadPlayer = true;
                return;
            }
            

            isPlayerVisible = true;
            ChangeState(ENEMY_STATE.ALERT);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerVisible = false;
        }
    }

    #region MEUS METODOS
    void GetHit(int amount)
    {
        if (isDeath) return;

        heath -= amount;
        if (heath >= 1)
        {
            ChangeState(ENEMY_STATE.FURY);
            anim.SetTrigger("GetHit");
        }
        else
        {
            anim.SetTrigger("Die");
            gameObject.layer = layerDeath;
            StopAllCoroutines();
            ChangeState(ENEMY_STATE.DIE);
        }
    }

    IEnumerator Death()
    {
        yield return new WaitForSeconds(2f);
        gameObject.SetActive(false);
        isDeath = true;

        if (Rand(0, 100) <= _gameManager.percentDrop) Instantiate(_gameManager.gemPrefab, transform.position + new  Vector3(0,1,0), _gameManager.gemPrefab.transform.rotation);
    }

    void StateManager()
    {

        if (isDeadPlayer && (state == ENEMY_STATE.FOLLOW || state == ENEMY_STATE.FURY || state == ENEMY_STATE.ALERT ))
        {
            ChangeState(ENEMY_STATE.IDLE);
        }

        switch (state)
        {
            case ENEMY_STATE.IDLE:
                break;
            case ENEMY_STATE.ALERT:
                LookAt();
                break;
            case ENEMY_STATE.FOLLOW:
                LookAt();
                agent.stoppingDistance = _gameManager.slimeDistanceToAttack;
                destination = _gameManager.player.position;
                agent.destination = destination;

                if(agent.remainingDistance <= agent.stoppingDistance)
                {
                    Attack();
                }
                break;
            case ENEMY_STATE.FURY:
                destination = _gameManager.player.position;
                agent.destination = destination;

                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    Attack();
                }
                break;
            case ENEMY_STATE.PATROL:
                break;

        }
    }

    private void Attack()
    {
        if (!isAttack && isPlayerVisible)
        {
            isAttack = true;
            anim.SetTrigger("Attack");
        }
    }

    void ChangeState(ENEMY_STATE newState)
    {
        StopAllCoroutines();
        isAlert = false;
        switch (newState)
        {
            case ENEMY_STATE.IDLE:
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;
                StartCoroutine(nameof(Idle));
                break;
            case ENEMY_STATE.ALERT:
                agent.stoppingDistance = 0;
                destination = transform.position;
                agent.destination = destination;
                isAlert = true;
                StartCoroutine(nameof(Alert));
                break;
            case ENEMY_STATE.PATROL:
                agent.stoppingDistance = 0;
                idWayPoint = Rand(0, _gameManager.slimeWayPoints.Length);
                destination = _gameManager.slimeWayPoints[idWayPoint].position;
                agent.destination = destination;
                StartCoroutine(nameof(Patrol));
                break;
            case ENEMY_STATE.FURY:
                destination = transform.position;
                agent.stoppingDistance = _gameManager.slimeDistanceToAttack;
                agent.destination = destination; 
                break;
            case ENEMY_STATE.FOLLOW:
                agent.stoppingDistance = _gameManager.slimeDistanceToAttack;
                StartCoroutine(nameof(Follow));
                break;
            case ENEMY_STATE.DIE:
                destination = transform.position;
                agent.destination = destination;
                StartCoroutine(nameof(Death));
                break;

        }

        state = newState;
    }   

    IEnumerator Idle()
    {
        yield return new WaitForSeconds(_gameManager.slimeIdleWaitTime);
        StayStill(50);

    }

    IEnumerator Patrol()
    {
        yield return new WaitUntil(() => agent.remainingDistance <= 0);
        StayStill(30);
    }

    IEnumerator Follow()
    {
        yield return new WaitUntil(() => !isPlayerVisible);
        print("PERDI VOCE");
        yield return new WaitForSeconds(_gameManager.slimeAlertTime);
        StayStill(50);
    }
    IEnumerator Alert()
    {
        yield return new WaitForSeconds(_gameManager.slimeAlertTime);
        if (isPlayerVisible)
        {
            ChangeState(ENEMY_STATE.FOLLOW);
        }
        else
        {
            StayStill(10);
        }
    }
    IEnumerator DelayAttack()
    {
        yield return new WaitForSeconds(_gameManager.slimeAttackDelay);
        isAttack = false;
    }
    void StayStill(int percent)
    {
        if (Rand(0, 100) <= percent)
        {
            ChangeState(ENEMY_STATE.IDLE);
        }
        else
        {
            ChangeState(ENEMY_STATE.PATROL);
        }
    }

    void AttackIsDone()
    {
        StartCoroutine(nameof(DelayAttack));
    }

    void LookAt()
    {
        
        Vector3 lookDirection = (_gameManager.player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, _gameManager.slimeLookAtSpeed * Time.deltaTime);
    }
    int Rand(int min, int max)
    {
        return Random.Range(min, max);
    }
    #endregion
}
