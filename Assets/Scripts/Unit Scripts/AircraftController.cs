using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AircraftController : UnitController
{

    public override void MoveOrder(Vector3 targetPosition)
    {
        state = State.idle;
        //agent.SetDestination(targetPosition);
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(new Vector3(targetPosition.x, transform.position.y, targetPosition.z), path);
        agent.path = path;
        path = null;
        attackTarget = null;
    }

}
