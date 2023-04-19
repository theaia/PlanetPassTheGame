using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Build : MonoBehaviour
{
    [System.Serializable]
    public class BuildingProperties
    {
        public bool unlocked;
        public GameObject prefab;
        public float defaultRechargeSpeed;
        public float rechargeSpeedMod = 1f;
        public bool onRecharge;
        public GameObject rechargeIcon, buttonObj;
    }

    public BuildingProperties[] buildings;
    public GameObject noneText;

    public void BuildObject(int index)
    { 
        if(buildings[index].onRecharge)
            return;
        
        Instantiate(buildings[index].prefab, Vector3.zero, Quaternion.identity);
        buildings[index].onRecharge = true;
        buildings[index].rechargeIcon.SetActive(true);

        StartCoroutine(RechargeBuilding(buildings[index]));
    }

    private IEnumerator RechargeBuilding(BuildingProperties props)
    {
        yield return new WaitForSeconds(props.defaultRechargeSpeed * props.rechargeSpeedMod);
        props.onRecharge = false;
        props.rechargeIcon.SetActive(false);
    }

    public void UnlockBuilding(int index)
    {
        noneText.SetActive(false);
        buildings[index].unlocked = true;
        buildings[index].buttonObj.SetActive(true);
    }
}

