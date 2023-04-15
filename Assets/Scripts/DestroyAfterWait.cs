using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterWait : MonoBehaviour
{
    public float wait;
    public Building buildingManager;

    private void OnEnable()
    {
        if(buildingManager != null)
            buildingManager.OnSetupBuilding += StartWait;
    }

    private void OnDisable()
    {
        if(buildingManager != null)
            buildingManager.OnSetupBuilding -= StartWait;
    }
    
    public void StartWait()
    {
        Invoke("KillMe", wait);
    }

    private void KillMe()
    {
        // rip
        Destroy(gameObject);
    }
}
