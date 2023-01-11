using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;


        // DEPRECIATED*** no longer using melee circle
        public enum ECircles
        {
            Melee,
            Approach,
        }


        public class KungFuData
        {
            //public ActionsManager actionManager;

            public EnemySubject subject;
            public ECircles type;

            public KungFuData (EnemySubject subject, ECircles type)
            {
                this.subject = subject;
                this.type = type;
                //this.actionManager = subject.GetActionsManager ();
            }


        }

        [RequireComponent (typeof (ApproachCircle))]

        public class FightingCircle :
           MonoBehaviour
        {
            private Dictionary<int, KungFuData> enemies;


            [SerializeField]
            [Tooltip ("First - available attacks\nSecond - max attacks")]
            private PairFloat attacksWeight;

            [SerializeField]
            private ApproachCircle _approachCircle;

             private void Awake ()
            {

              _approachCircle = GetComponentInParent<ApproachCircle>();
             
              enemies = new Dictionary<int, KungFuData> ();



            }

            public bool Register (EnemySubject enemy, ECircles type)
            {
                if (IsContains(enemy))
                {
                    return false; 
                //Move(enemy, type);
                }

                 bool isRegistered;
                isRegistered = _approachCircle.Register(enemy);


                if (isRegistered)
                {
                    enemies.Add (enemy.gameObject.GetInstanceID ()
                        , new KungFuData (enemy, type));

                }

                return isRegistered;
            }

            // intended for use in moving enemy from one circle to another **DEPRECIATED

           //public bool Move(EnemySubject enemy, ECircles to)
           //{
           //    int instance = enemy.gameObject.GetInstanceID ();
           //
           //    var data = enemies[instance];
           //
           //    if (to.Equals (data.type))
           //        return true;
           //
           //    // unregister from current circle
           //
           //    _approachCircle.Unregister(enemy);
           //
           //    bool hasMoved;
           //
           //    hasMoved = _approachCircle.Register(enemy);
           //
           //    if (hasMoved)
           //    {
           //        data.type = to;
           //    }
           //
           //    return hasMoved;
           //}

            public bool Unregister (EnemySubject enemy)
            {
                int instance = enemy.gameObject.GetInstanceID ();

                if (!enemies.ContainsKey (instance))
                    return false;

                 bool isUnregistered;

                isUnregistered = _approachCircle.Unregister(enemy);

                if (isUnregistered)
                {
                    enemies.Remove(instance);
                }

                return isUnregistered;
            }

            public bool IsContains (EnemySubject enemy)
            {
                return enemies.ContainsKey (enemy.gameObject.GetInstanceID ());
            }


             public Vector3 GetSlotPositionFromApproachCircle (EnemySubject enemy)
            {
                var approach = GetApproachCircle();

                if(enemy)
                 {
                      int instance = enemy.gameObject.GetInstanceID();
                      return approach.GetGlobalPositionByInstance(instance);
                 }


                     return Vector3.zero;
                }

            public ApproachCircle GetApproachCircle ()
            {
                 return _approachCircle;
            }


        }

