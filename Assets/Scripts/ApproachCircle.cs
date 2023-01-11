using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ApproachCircle
    : KungFuCircle
{
    [SerializeField]
    private Vector3[] localPositions;

    private Dictionary<int, Pair<EnemySubject, int>> engagedSlots;
    private List<int> takenSpotNrs;

    public float GIZMO_RADIUS_POINT = 0.5f;
    public Color GIZMO_COLOR_POINT = Color.blue;
    public bool gizmo_draw = false;

    public float offsetBetweenPoints = 50f;

    public List<EnemySubject> debug = new List<EnemySubject>();

    private void Awake()
    {
        maximumSlots = GetMaxGridCapacity();

        localPositions = new Vector3[maximumSlots];
        var arc = degrees / maximumSlots;
        for (int i = 0; i < maximumSlots; i++)
        {
            // arc * i
  
            localPositions[i] = ComputePosition(arc * i + offsetBetweenPoints, radius);
        }

        SetCurrentGridCapacity(GetMaxGridCapacity());
       // Debug.Log(GetCurrentGridCapacity());

        engagedSlots = new Dictionary<int, Pair<EnemySubject, int>>();
        takenSpotNrs = new List<int>();
    }

    private void Draw()
    {
        for (int i = 0; i < maximumSlots; i++)
        {
            DebugDrawing.DrawCircle
                (GetGlobalPosition(i)
                , radius, GIZMO_COLOR_POINT);
        }

        foreach(var position in GetGlobalFreePositions())
        {
            DebugDrawing.DrawCircle(position, 0.5f, Color.green);
        }

        foreach (var position in GetGlobalNotFreePositions())
        {
            DebugDrawing.DrawCircle(position, 0.5f, Color.blue);
        }
    }

    // DEPRECIATED*** was intending to have a melee and approach circle
    // but ended up using distance calculations instead to determine
    // when an enemy should attack
   // public override ECircles GetCircleType()
   // {
   //     return ECircles.Approach;
   // }

    public override bool IsContains(EnemySubject enemy)
    {
        return engagedSlots.ContainsKey(enemy.gameObject.GetInstanceID());
    }

    public Vector3 GetGlobalPosition(int slot)
    {
        return localPositions[slot] + transform.position;
    }

    public Vector3 GetGlobalPosition(EnemySubject enemy)
    {
        int slot = engagedSlots[enemy.gameObject.GetInstanceID()].Second;
        return GetGlobalPosition(slot);
    }

    public Vector3 GetGlobalPositionByInstance(int instance)
    {
        int slot = engagedSlots[instance].Second;
        return GetGlobalPosition(slot);
    }

    public override bool Register(EnemySubject enemy)
    {
        int instance = enemy.gameObject.GetInstanceID();

        if (engagedSlots.ContainsKey(instance))
        {
           // Debug.Log("Returned false on ContainsKey(instance)");
            return false;
        }


        if (engagedSlots.Count >= maximumSlots)
        {
          //  Debug.Log("Returned false on engagedSlot.Count >= maxSlots");
            return false;
        }

        int availWeight = GetCurrentGridCapacity();
        int enemyWeight = enemy.GetGridWeight();

        // if enemy weight is more than available weight don't add them
        if (availWeight < enemyWeight)
        {
            //Debug.Log(availWeight);
           // Debug.Log("Returned false on availWeight < enemyWeight");
            return false;
        }
         
        engagedSlots.Add(instance
            , new Pair<EnemySubject, int>(enemy, 0));

        //Debug.Log("Added enemy to approach circle!");

        SetCurrentGridCapacity(availWeight - enemyWeight);
        debug.Add(enemy);
        //FindNearestSlotToEnemies();
        takenSpotNrs.Add(FindNearestSlotToEnemy(enemy));

        return true;
    }

    public override bool Unregister(EnemySubject enemy)
    {
        bool isContains = IsContains(enemy);
        int instance = enemy.gameObject.GetInstanceID();
        if (isContains)
        {
            SetCurrentGridCapacity(GetCurrentGridCapacity() + enemy.GetGridWeight());

            // remove taken spot number from taken numbers list
            takenSpotNrs.Remove(engagedSlots[instance].Second);
            // remove entirely from engaged slots list
            engagedSlots.Remove(instance);
            debug.Remove(enemy);
        }

        return isContains;
    }


    private int FindNearestSlotToEnemy(EnemySubject enemy)
    {
        var pos = enemy.transform.position;
        int slot = 0;
        float minDist = float.MaxValue;

        for (int i = 0; i < localPositions.Length; i++)
        {
            if (!takenSpotNrs.Contains(i))
            {
                float distance = Vector3.Distance(pos, localPositions[i]);
                if (distance < minDist)
                {
                    minDist = distance;
                    slot = i;
                }

            }

        }

        engagedSlots[enemy.gameObject.GetInstanceID()].Second = slot;

        return slot;
    }

    // gets all free slot positions
    private List<Vector3> GetGlobalFreePositions()
    {
        var center = transform.position;
        List<Vector3> ret = new List<Vector3>();
        var engaged = engagedSlots.Values.ToList().ConvertAll(x => x.Second);

        for (int i = 0; i < localPositions.Length; i++)
        {
            if (!engaged.Contains(i))
            {
                ret.Add(localPositions[i] + center);
            }
        }

        return ret;
    }

    // gets all occupied slot positions
    private List<Vector3> GetGlobalNotFreePositions()
    {
        var center = transform.position;
        List<Vector3> ret = new List<Vector3>();
        var engaged = engagedSlots.Values.ToList().ConvertAll(x => x.Second);

        for (int i = 0; i < localPositions.Length; i++)
        {
            if (engaged.Contains(i))
            {
                ret.Add(localPositions[i] + center);
            }
        }

        return ret;
    }




    public void Update()
    {
        if (gizmo_draw) Draw();
    }
}

