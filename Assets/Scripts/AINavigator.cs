using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class AINavigator : MonoBehaviour
{

    public NavMeshAgent navAgent;

    public Transform target;

    public float interval;

    void Start()
    {
        Task navTask = new Task(FollowTarget());
    }

    IEnumerator FollowTarget()
    {

        navAgent.SetDestination(target.position);

        float i = 0;

        while (i < Mathf.Infinity)
        {
            if (i < interval)
            {
                i += Time.deltaTime;
            }
            else
            {
                navAgent.SetDestination(target.position);
                i = 0;
            }
            yield return null;
        }


    }



}
