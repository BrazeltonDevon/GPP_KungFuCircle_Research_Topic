using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    private CharacterController charCtrl;
    private StarterAssetsInputs _input;

    // Start is called before the first frame update
    void Start()
    {
        charCtrl = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();
    }

    // Update is called once per frame
    void Update()
    {
        // left mouse button is released
        

        if(_input.attack)
        {
            Attack();
            _input.attack = false;
        }
    }

    void Attack()
    {
        Vector3 p1 = transform.forward;


        // Cast a sphere wrapping character controller 10 meters forward
        // to see if it is about to hit anything.
        Vector3 pos = transform.position;
        pos.y += charCtrl.height / 2;

        RaycastHit hitInfo;

        Debug.DrawRay(pos, transform.TransformDirection(Vector3.forward) * 2f, Color.yellow);
        if (Physics.Raycast(pos, transform.TransformDirection(Vector3.forward), out hitInfo, 2f))
        {
            hitInfo.collider.SendMessage("TakeDamage");
        }

      //Collider[] hitColliders = Physics.Raycast();
      //foreach (var hitCollider in hitColliders)
      //{
      //    if(hitCollider.gameObject.layer == 6)
      //    {
      //        hitCollider.gameObject.GetComponent<EnemySubject>().SendMessage("TakeDamage");
      //    }
      //
      //
      //}

        
    }

}
