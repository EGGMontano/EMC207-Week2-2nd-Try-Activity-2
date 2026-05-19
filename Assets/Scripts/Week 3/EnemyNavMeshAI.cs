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

    private void Awake()                                                    //always put your get components in Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()                                                            // changing variables
    {
            if (HasPatrolPoints)
            {
                ChangeState(EnemyStates.Patrol);
            }
            else
            {
                ChangeState(EnemyStates.Idle);
            }
    }

    // Update is called once per frame
    void Update()
    {
        CheckForPlayer();
        UpdateAnimation();
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
        //measure distance between the enemy and player
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
        //Prevents/Stops the NavMeshAgent from moving
        agent.isStopped = true;
        //Clears current paths
        agent.ResetPath();
        //Reset wait timer
        waitTimer = 0f;
    }

    private void UpdateIdle()
    {
        //if the NavMeshAgent does anything while idle
    }

    //PatrolStates
    private void EnterPatrol()
    {
        //Allows the NavMeshAgent to move
        agent.isStopped = false;
        //Sets the speed to walking speed
        agent.speed = walkSpeed;
        //Stops the AI at a certain distance during patrol
        agent.stoppingDistance = patrolStoppingDistance;
        waitTimer = 0f;
        //Moves toward the current Patrol Point
        SetCurrentPatrolDestination();
    }

    private void UpdatePatrol()     //while patrolling
    {
        //If the Patrol Points were removed, change to idle
        if (!HasPatrolPoints)
        {
            ChangeState(EnemyStates.Idle);
            return;
        }
        //If the NavMeshAgent has not reached its destination, just keep patrolling
        if (!ReachedDestination())
        {
            return;
        }

        //If the NavMeshAgent has reached its Patrol Point
        agent.isStopped = true;
        waitTimer += Time.deltaTime;

        //If the NavMeshAgent has waited enough on the Patrol Point
        if (waitTimer >= waitTimeAtWaypoint)
        {
            //Reset wait timer
            waitTimer = 0;
            ChooseNextPatrolPoint();

            //Makes AI able to move again
            agent.isStopped = false;
            SetCurrentPatrolDestination();
        }
    }

    //ChaseState
    private void EnterChase()
    {
        //Allows the NavMeshAgent to move
        agent.isStopped = false;
        //Sets the speed to running/chase speed
        agent.speed = runSpeed;
        //Set how close the enemy should get to the player
        agent.stoppingDistance = chaseStoppingDistance;
        //Reset wait timer
        waitTimer = 0;
    }

    private void UpdateChase()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        //If the player no longer exists
        if (player == null)
        {
            if(HasPatrolPoints)
            {
                ChangeState(EnemyStates.Patrol);
            }
            else
            {
                ChangeState(EnemyStates.Idle);
            }
            return;
        }
        

        //if the enemy is close to player
        if (distanceToPlayer <= chaseStoppingDistance)
        {
            agent.isStopped = true;
            agent.ResetPath();
            //Attack State
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
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


        if (randomPatrol && patrolPoints.Length > 1)
        {
            int nextIndex = patrolIndex;

        }

        
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
        //logic for random patrolling
        if (!HasPatrolPoints)
        {
            return;
        }

        if (randomPatrol && patrolPoints.Length > 1)
        {
            int nextIndex = patrolIndex;
            while (nextIndex == patrolIndex)
            {
                nextIndex = Random.Range(0, patrolPoints.Length);
            }
            patrolIndex = nextIndex;
        }
        else
        {
            //move to next Patrol Point
            patrolIndex++;
            //resets the Patrol Pont to zero
            if (patrolIndex >= patrolPoints.Length)
            {
                patrolIndex = 0;
            }
        }
    }

    //ANIMATIONS
    private void UpdateAnimation()
    {
        float animationSpeed = 0;

        //if agent is moving
        bool isMoving = agent.velocity.magnitude > 0.05 && agent.isStopped;
        if (currentState == EnemyStates.Patrol && isMoving)
        {
            animationSpeed = 0.5f;
        }
        else if (currentState == EnemyStates.Chase && isMoving)
        {
            animationSpeed = 1f;
        }

        animator.SetFloat(speedParameter, animationSpeed, animationDampTime, Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere (transform.position, loseRange);
    }
}
//heh, just for the GitHub