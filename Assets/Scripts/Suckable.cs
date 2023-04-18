using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Suckable : MonoBehaviour
{
    [SerializeField]
    private float suckSpeed = 1;
    public bool inShield = false;

    private bool beingSucked = false;
    private Transform goalTrans;

    [SerializeField]
    private UnityEvent OnSuckStarted = new UnityEvent();
    [SerializeField]
    private UnityEvent OnSucked = new UnityEvent();

    private void OnTriggerEnter(Collider other)
    {
        // if its a shield, become invincible
        if(other.transform.gameObject.layer == 11)
        {
            inShield = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // if its a shield, stop being invincible
        if(other.transform.gameObject.layer == 11)
        {
            inShield = false;
        }
    }

    private void Update()
    {
        if(inShield || GameControl.Instance.IsTransitioningWorlds)
            return;
        
        if(beingSucked)
        {
            transform.position = Vector3.MoveTowards(transform.position, goalTrans.position, suckSpeed);

            if(transform.position == goalTrans.position)
            {
                OnSucked.Invoke();
            }
        }
    }

    public void GetSucked(Transform goal)
    {
        if(inShield || GameControl.Instance.IsTransitioningWorlds)
            return;
        
        goalTrans = goal;
        beingSucked = true;

        OnSuckStarted.Invoke();
    }
}
