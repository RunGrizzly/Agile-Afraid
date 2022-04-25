using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class GridNavigator : MonoBehaviour
{

    public NavMeshAgent agent;

    void Start()
    {

        BrainControl.Get().eventManager.e_levelLoaded.AddListener((l) => transform.position = l.startInput.blocks[0].transform.position);
        BrainControl.Get().eventManager.e_endInput.AddListener(() =>
        {

            agent.SetDestination(BrainControl.Get().grid.path.corners[BrainControl.Get().grid.path.corners.Length - 1]);

            // if (NavMesh.SamplePosition(BrainControl.Get().sessionManager.currentSession.currentLevel.targetInput.blocks[0].transform.position, out hit, 1.0f, NavMesh.AllAreas))
            // {
            //     agent.SetDestination(hit.position);
            // }
        });
    }
}
