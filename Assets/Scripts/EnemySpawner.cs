using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public float startTimeBtwSpawn;
    public AnimationCurve spawnTimeDecreaseMultiplier;
    public float timeUntilMaxSpawnSpeed = 200f, maxSpawnTimeDecrease = 3f;
    public float extraStartDelay = 3f;


    [Header("Prefabs")]
    [SerializeField]
    private GameObject[] enemies;
    [SerializeField]
    private GameObject endGoal;
    public Transform enemyHolder;

    private float timeBtwSpawn;
    private Transform target;
    private Transform earth;

    private void Start()
    {
        target = GameObject.FindGameObjectWithTag("Target").transform; //Instantiate(endGoal, Vector3.zero, Quaternion.Euler(Random.insideUnitSphere * 180f)).transform.GetChild(0);
        earth = GameObject.FindGameObjectWithTag("Earth").transform;
        target.parent = earth.transform; 

        timeBtwSpawn = startTimeBtwSpawn + extraStartDelay;
    }


    private void Update()
	{
		if(timeBtwSpawn <= 0)
        { 
			GameObject ene = Instantiate(enemies[Random.Range(0, enemies.Length)], Vector3.zero, Random.rotation);
            ene.transform.parent = enemyHolder;
			timeBtwSpawn = startTimeBtwSpawn - (spawnTimeDecreaseMultiplier.Evaluate(Time.time / timeUntilMaxSpawnSpeed) * maxSpawnTimeDecrease);
		} else
        { 
			timeBtwSpawn -= Time.deltaTime;
		}
	}


}
