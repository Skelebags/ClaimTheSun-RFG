using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HQBuildingController : BuildingController
{

    protected override void Awake()
    {
        base.Awake();

        state = State.ready;

        GetComponent<NavMeshObstacle>().enabled = true;
    }
}
