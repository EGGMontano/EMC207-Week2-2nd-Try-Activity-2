using UnityEngine;
using Unity.AI;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    //Waypoints
    public Transform[] destination;
    private NavMeshAgent carAgent;
    public int currentWaypointIndex = 0;

    //Traffic rules
    public Stoplight trafficLight;
    public Transform stopPoint;
    public float stopDistance = 2f;

    //Speed
    public float normalSpeed;
    public float slowSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        carAgent = GetComponent<NavMeshAgent>();
        carAgent.speed = normalSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        FollowTrafficLight();
        if (carAgent.pathPending)
        {
            return;
        }
        if (carAgent.remainingDistance <= carAgent.stoppingDistance)
        {
            GoToNextWaypoint();
        }
    }

    private void FollowTrafficLight()
    {
        float distanceToSopPoint = Vector3.Distance(transform.position, stopPoint.position);
        if (trafficLight.isGreen && distanceToSopPoint <= stopDistance * 2)
        {
            carAgent.isStopped = true;
            carAgent.speed = 0;
        }
        else if (trafficLight.isGreen && distanceToSopPoint <= stopDistance * 4)
        {
            carAgent.isStopped = false;
            carAgent.speed = slowSpeed;
        }
        else
        {
            carAgent.isStopped = false;
            carAgent.speed = normalSpeed;
        }

    }

    private void MoveToCurrentWaypoint()
    {
        carAgent.SetDestination(destination[currentWaypointIndex].position);
    }

    private void GoToNextWaypoint()
    {
        currentWaypointIndex++; //Go to next waypoint
        if (currentWaypointIndex >= destination.Length)
        {
            currentWaypointIndex = 0;
        }
        MoveToCurrentWaypoint();
    }
}
