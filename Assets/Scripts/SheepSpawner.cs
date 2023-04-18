using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SheepSpawner : MonoBehaviour
{
    [Header("Properties")]
    [SerializeField]
    [Tooltip("The number of sheep to spawn at the start of the game")]
    private int startingSheep = 1;

    [Header("Prefabs")]
    [SerializeField]
    private GameObject sheepPrefab;
    public LayerMask whatIsPlanet;

    private Transform earth;
    float earthRadius = 20f;
    private List<SheepController> sheep = new List<SheepController>();

    private void Start()
    {
        GameControl.Instance.sheepSpawner = this;
        earth = GameObject.FindGameObjectWithTag("Earth").transform;

        SpawnSheep(startingSheep);
    }

    public void SpawnSheep(int amount)
    {
        for(int i = 0; i < amount; i++)
        {
            // find a random pos on surface of planet
            Vector3 pos = (Random.insideUnitSphere.normalized * earthRadius);
            
            // spawn a sheep there
            SheepController newAgent = Instantiate(sheepPrefab, pos, transform.rotation).GetComponentInChildren<SheepController>();
            newAgent.transform.SetParent(transform);

            // fix its facing direction
            RaycastHit hitInfo;
            Vector3 pointDir = earth.position - newAgent.transform.position;

            if(Physics.Raycast(newAgent.transform.position, pointDir.normalized, out hitInfo, Mathf.Infinity, whatIsPlanet))
            { 
                newAgent.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
            }

            sheep.Add(newAgent);
            GameControl.Instance.curSheep = transform.childCount;
            GameControl.Instance.UpdateSheepCounter();
        }
    }

    public void ClearSheep() {
        foreach(Transform _child in transform) {
            Destroy(_child.gameObject);
		}
    }
}
