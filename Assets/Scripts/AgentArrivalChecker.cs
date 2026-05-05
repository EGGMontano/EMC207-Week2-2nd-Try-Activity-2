using UnityEngine;
using UnityEngine.AI;

public class AgentArrivalChecker : MonoBehaviour
{
    private NavMeshAgent agent;
    public Transform destinationPosition, destinationPosition2, destinationPosition3;
    private int AAAAAAAAAAAH = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        if (AAAAAAAAAAAH == 0)
        {
            agent.SetDestination(destinationPosition.position);
            HasReachedDestination(agent);
            Debug.Log(AAAAAAAAAAAH);
        }

        if (AAAAAAAAAAAH == 1)
        {
            agent.SetDestination(destinationPosition2.position);
            HasReachedDestination(agent);
            Debug.Log(AAAAAAAAAAAH);
        }

        if (AAAAAAAAAAAH == 2)
        {
            agent.SetDestination(destinationPosition3.position);
            HasReachedDestination(agent);
            Debug.Log(AAAAAAAAAAAH);
        }

        if (AAAAAAAAAAAH == 3)
        {
            AAAAAAAAAAAH -= 3;
            Debug.Log(AAAAAAAAAAAH);
        }
    }

    bool HasReachedDestination(NavMeshAgent _agent)
    {
        if (_agent.remainingDistance > agent.stoppingDistance)
            return false;

        if (_agent.pathPending)
            return false;

        if (_agent.hasPath && agent.velocity.sqrMagnitude > 0)
        {
            AAAAAAAAAAAH++;
            return false;
        }

        return true;
    }
}
