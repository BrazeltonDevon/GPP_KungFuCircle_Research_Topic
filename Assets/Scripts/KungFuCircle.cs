using System.Collections;
using System.Collections.Generic;
using UnityEngine;


       //public enum EKungFuStateAttack
       //{
       //    IDLE,
       //    SELECTION,
       //    RUNNING
       //}

        [System.Serializable]
public abstract class KungFuCircle : MonoBehaviour
{
    // Start is called before the first frame update

    //[SerializeField]
    //int maxCapacity = 4;

    [Range(60f, 360f)]
    public float degrees = 360f;

    [Range(0.5f, 10f)]
    public float radius;
    [Tooltip("key - available weight slots;\nval - capacity weight slots")]

    private int currentGridCapacity = 7;
    [SerializeField]
    private int maxGridCapacity = 7;
    [SerializeField]
    private PairFloat timer;
    [SerializeField]
    public float distanceFromPlayer = 1;

    public int maximumSlots = 7;
    protected Vector3 ComputePosition(float degrees, float radius)
    {
        //var vertical = Mathf.Sin(Mathf.Deg2Rad * degrees);
        //var horizontal = Mathf.Cos(Mathf.Deg2Rad * degrees);

        //var dir = new Vector3(horizontal, 0, vertical);

        //var finalPos = (transform.localPosition + dir) * radius;
        //return finalPos;

        return new Vector3
            (Mathf.Cos(Mathf.Deg2Rad * degrees) * (radius + distanceFromPlayer)
            , 0f
            , Mathf.Sin(Mathf.Deg2Rad * degrees) * (radius + distanceFromPlayer));
    }

    public int GetCurrentGridCapacity()
    {
        return currentGridCapacity;
    }

    public int GetMaxGridCapacity()
    {
        return maxGridCapacity;
    }

    public bool UseCurrentGridCapacity(int weight)
    {
        if (currentGridCapacity < weight)
        {
            return false;
        }

        currentGridCapacity -= weight;
        return true;
    }

    public void SetCurrentGridCapacity(int weight)
    {
        currentGridCapacity = weight;
    }

    public void ResetGridCapacity()
    {
        currentGridCapacity = maxGridCapacity;
    }

            public abstract bool Register(EnemySubject enemy);
            public abstract bool Unregister(EnemySubject enemy);
            public abstract bool IsContains(EnemySubject enemy);
            public abstract ECircles GetCircleType();

        }

