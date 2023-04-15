using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldBehavior : MonoBehaviour
{
    public Building buildingManager;
    public float radius = 10f;
    public LayerMask suckableLayer;
    Collider[] cols;

    private void OnEnable()
    {
        if(buildingManager != null)
            buildingManager.OnSetupBuilding += FindThingsToShield;
    }

    private void OnDisable()
    {
        if(buildingManager != null)
            buildingManager.OnSetupBuilding -= FindThingsToShield;
        
        foreach(Collider col in cols)
        {
            col.gameObject.GetComponent<Suckable>().inShield = false;
        }
    }

    private void FindThingsToShield()
    {
        cols = Physics.OverlapSphere(transform.position, radius, suckableLayer);

        foreach(Collider col in cols)
        {
            col.gameObject.GetComponent<Suckable>().inShield = true;
        }
    }


}
