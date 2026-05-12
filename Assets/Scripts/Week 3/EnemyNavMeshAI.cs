using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshAI : MonoBehaviour
{
    private enum EnemyStates
    {
        Idle,
        Patrol,
        Chase
    }

    [Header("References")]
    //The player the enemy will chase
    [SerializeField] Transform player;
    //Enemy Patrol Points
    [SerializeField] Transform[] patrolPoints;

    [Header("Detection")]
    [SerializeField] private float chaseRange = 10.0f;  //Start chase
    [SerializeField] private float loseRange = 16.0f;   //Lose chase

    [Header("Movement")]
    [SerializeField] float walkSpeed = 2.0f;                //AI walk speed
    [SerializeField] float runSpeed = 4.0f;                 //AI run speed
    [SerializeField] float patrolStoppingDistance = 0.2f;
    [SerializeField] float chaseStoppingDistance = 1.0f;
    [SerializeField] float waypointReachDistance = 0.5f;
    [SerializeField] float waitTimeAtWaypoint = 1.5f;       //How long the enemy waits at each patrol point
    [SerializeField] bool randomPatrol = false;
    [SerializeField] float facePlayerSpeed = 8.0f;          //Enemy rotate speed when facing the player

    [Header("Animation")]
    [SerializeField] string speedParameter = "Speed";
    [SerializeField] float animationDampTime = 0.1f;        //makes animation transitions smoother+

    private NavMeshAgent agent;
    private Animator animator;
    private EnemyStates currentState;
    private bool stateInitialized;
    private int patrolIndex;
    private float waitTimer;

    //Property that checks if the patrol points array exists
    private bool HasPatrolPoints
    {
        get
        {
            return patrolPoints != null && patrolPoints.Length > 0;
        }
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            
    }

    // Update is called once per frame
    void Update()
    {
        CheckForPlayer();
        //if patrol point exists, start Patrol State
        switch (currentState)
        {
            case EnemyStates.Idle:
                UpdateIdle();
                break;
            case EnemyStates.Patrol:
                UpdatePatrol();
                break;
            case EnemyStates.Chase:
                UpdateChase();
                break;
        }
    }

    private void CheckForPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (currentState != EnemyStates.Chase && distanceToPlayer <= chaseRange)
        {
            ChangeState(EnemyStates.Chase);
        }

        //if the enemy is chasing but the player gets too far
        else if (currentState == EnemyStates.Chase && distanceToPlayer >= loseRange)
        {
            ChangeState(EnemyStates.Patrol);
        }
    }


    //States Changer
    private void ChangeState(EnemyStates newState)
    {
        //if the enemy is already initialized and the enemy is already in this state,
        //do not restart the same state again
        if (stateInitialized && currentState == newState)
        {
            return; 
        }
        //marks that at least one state has been initialized
        stateInitialized = true;
        //stores the new state
        currentState = newState;

        switch (currentState)
        {
            case EnemyStates.Idle:
                EnterIdle();
                break;
            case EnemyStates.Patrol:
                EnterPatrol();
                break;
            case EnemyStates.Chase:
                EnterChase();
                break;
            default:
                break;
        }
    }


    //IdleState
    //method runs once the enemy enters IdleState
    private void EnterIdle()
    {
        //Prevents/Stops NavMeshAgent from moving
        agent.isStopped = true;
        //Clear current tasks
        agent.ResetPath();
        //Reset wait timer
        waitTimer = 0;
    }

    private void UpdateIdle()
    {

    }

    //PatrolState
    private void EnterPatrol()
    {
        agent.isStopped = false;
        agent.speed = walkSpeed;
        agent.stoppingDistance = patrolStoppingDistance;
        waitTimer = 0;

    }

    private void UpdatePatrol()
    {

    }

    //ChaseState
    private void EnterChase()
    {
        agent.isStopped = false;
        agent.speed = runSpeed;
        agent.stoppingDistance = chaseStoppingDistance;
        waitTimer = 0;
    }

    private void UpdateChase()
    {

    }

    //Checks if it has reached its current destination
    private bool ReachedDestination()
    {
        if (agent.pathPending)
        {
            return false;
        }
        //Sometimes remaining distance can be infinity while the path is unknown
        //This makes it so it doesn't count as reached
        if (agent.remainingDistance == Mathf.Infinity)
        {
            return false; 
        }
        //since we used a lot of stopping distance, this makes it so that it uses whichever number is bigger:
        //agents stopping distance or our custom waypoint reached
        float reachedDistance = Mathf.Max(agent.stoppingDistance, waypointReachDistance);
        //if the remaining distance is small enough, the destination is reached
        return agent.remainingDistance <= reachedDistance;
    }

    //Sends them to the current patrol point
    private void SetCurrentPatrolDestination()
    {
        if (!HasPatrolPoints)
        {
            return;
        }
        Transform point = patrolPoints[patrolIndex];
        if (point == null)
        {
            ChooseNextPatrolPoint();
            point = patrolPoints[patrolIndex];
        }
        if (point != null)
        {
            agent.SetDestination(point.position);
        }
    }
    
    //Chooses the next patrol point
    private void ChooseNextPatrolPoint()
    {

    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere (transform.position, loseRange);
    }
}
