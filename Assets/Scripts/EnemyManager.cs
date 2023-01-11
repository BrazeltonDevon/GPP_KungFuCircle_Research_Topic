using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    // just to show the enemies and make sure they are in list
    [SerializeField]
    // ***remove after
    private EnemySubject[] enemies;
    public List<EnemyStruct> allEnemies;
    private List<int> enemyIndexes;

    private Coroutine AI_Loop_Coroutine;
    private Coroutine Register_Coroutine;

    public int aliveEnemyCount;

    [SerializeField]
    private GameObject _player;

    private float _distToPlayer = 0;

    private FightingCircle _fightingCircle;
    private ApproachCircle _approachCircle;



    void Start()
    {
        enemies = GetComponentsInChildren<EnemySubject>();

        foreach(var enemy in enemies)
        {
            EnemyStruct enStruct;
            enStruct.enemyScript = enemy;
            enStruct.enemyAvailability = true;
            allEnemies.Add(enStruct);
        }

        //allEnemies = new EnemyStruct[enemies.Length];

       //for (int i = 0; i < allEnemies.Length; i++)
       //{
       //    allEnemies[i].enemyScript = enemies[i];
       //    allEnemies[i].enemyAvailability = true;
       //}

        // find player
        if(!_player)
        _player = GameObject.FindGameObjectWithTag("Player");

        // get the fighting circle
        _fightingCircle = _player.GetComponent<FightingCircle>();
        _approachCircle = _fightingCircle.GetApproachCircle();

        AI_Loop_Coroutine = StartCoroutine(AI_Loop(null));
        Register_Coroutine = StartCoroutine(Register_Loop());
    }


    IEnumerator Register_Loop()
    {
        // get all enemies and check if can be registered in approach circle
        foreach (var entity in allEnemies)
        {
            // get enemy script
            var enScript = entity.enemyScript;

            // is NOT registered in the fighting circle?
            if (!enScript.IsRegisteredInFC)
            {

                // in range of approach circle
                if (_distToPlayer <= _approachCircle.radius + _approachCircle.distanceFromPlayer)
                {
                    bool isRegistered = _fightingCircle.Register(enScript, ECircles.Approach);

                    // if register is successful, set IsRegistered to true
                    if (isRegistered)
                    {
                        enScript.IsRegisteredInFC = true;
                    }
                    else
                    {
                        // was not registered, most likely because not enough capacity
                        // stay out of approach range and stay there
                        enScript.IsTargetPlayer = false;
                        enScript.IsTargetSlot = false;
                    }
                        
                }
                else // not in range == approach player
                {
                    enScript.IsTargetPlayer = true;
                    enScript.IsTargetSlot = false;
                }



            }
            else
            {
                // if is in fighting circle
                // and distance from assigned slot is beyond a certain point
                // out of range

                // Distance from slot position

                var slotPos = _fightingCircle.GetSlotPositionFromApproachCircle(enScript);
                // get distance to the player
                Vector3 distToSlot = slotPos - enScript.transform.position;
                float distance = distToSlot.magnitude;

                float maxDistfromSlot = 1f;


                if (distance < maxDistfromSlot)
                {
                    enScript.isInRange = true;
                    if(!enScript.IsPreparingAttack() && !enScript.IsRetreating())
                    enScript.isWaiting = true;
                }
                else if(!enScript.IsPreparingAttack())
                {
                    enScript.isInRange = false;
                    enScript.OutOfRangeStopWaiting();
                }
            }


        }


        yield return new WaitForSeconds(.1f);
        StartCoroutine(Register_Loop());
    }

    IEnumerator AI_Loop(EnemySubject enemy)
    {
        Debug.Log("AI LOOP RUNNING");

        if (AliveEnemyCount() == 0)
        {
            Debug.Log("No more enemies alive!");

            StopCoroutine(AI_Loop(null));
            yield break;
        }

        yield return new WaitForSeconds(Random.Range(.5f, 1.5f));

        Debug.Log("Done waiting random sec amount Line 130");

        EnemySubject attackingEnemy = RandomEnemyExcludingOne(enemy);

        if (attackingEnemy == null)
            attackingEnemy = RandomEnemy();

        if (attackingEnemy == null)
        {
            Debug.Log("Attacking enemy == null BREAK");
            yield break;
            // AI_Loop_Coroutine = StartCoroutine(AI_Loop(null);
        }

        Debug.Log("Waiting until retreat == false");
        yield return new WaitUntil(() => attackingEnemy.IsRetreating() == false);

        attackingEnemy.SetAttack();

        yield return new WaitUntil(() => attackingEnemy.IsPreparingAttack() == false);

        Debug.Log("Is going to retreat since attacking is done");
        attackingEnemy.SetRetreat();

        yield return new WaitForSeconds(Random.Range(1f, 1.5f));

        if (AliveEnemyCount() > 0)
            AI_Loop_Coroutine = StartCoroutine(AI_Loop(attackingEnemy));
    }

    public void RemoveEnemy(EnemySubject enemy)
    {
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if(allEnemies[i].enemyScript == enemy)
            {
                allEnemies.Remove(allEnemies[i]);
            }

        }

    }


    public EnemySubject RandomEnemy()
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyAvailability)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemySubject randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyScript;

        return randomEnemy;
    }

    public EnemySubject RandomEnemyExcludingOne(EnemySubject exclude)
    {
        enemyIndexes = new List<int>();

        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyAvailability && allEnemies[i].enemyScript != exclude && allEnemies[i].enemyScript.IsRegisteredInFC)
                enemyIndexes.Add(i);
        }

        if (enemyIndexes.Count == 0)
            return null;

        EnemySubject randomEnemy;
        int randomIndex = Random.Range(0, enemyIndexes.Count);
        randomEnemy = allEnemies[enemyIndexes[randomIndex]].enemyScript;

        return randomEnemy;
    }

    public int AvailableEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyAvailability && allEnemies[i].enemyScript.IsRegisteredInFC)
                count++;
        }
        return count;
    }


    public int AliveEnemyCount()
    {
        int count = 0;
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if(allEnemies[i].enemyScript)
            {
                if (allEnemies[i].enemyScript.isActiveAndEnabled)
                    count++;
            }
         
        }
        aliveEnemyCount = count;
        return count;
    }

    public void SetEnemyAvailiability(EnemySubject enemy, bool state)
    {
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (allEnemies[i].enemyScript == enemy)
            {
                EnemyStruct myEn = allEnemies[i];
                myEn.enemyAvailability = state;

                allEnemies[i] = myEn;
            }
               
        }

    }


}

[System.Serializable]
public struct EnemyStruct
{
    public EnemySubject enemyScript;
    public bool enemyAvailability;
}

