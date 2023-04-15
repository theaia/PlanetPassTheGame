using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRotation : MonoBehaviour
{
	public float moveSpeed = 1;

	public Vector3 rotationDirection = Vector3.right;

	public GameObject destroyEffect;

	public bool notFlying;
	private Transform earth;

	public float timeToChangeDir = 2f;
	public float maxHp = 10, hp;
	public int xpValue = 20;

	private void Start()
	{
		hp = maxHp;
		
		if(notFlying == true)
		{
			earth = GameObject.FindGameObjectWithTag("Earth").transform;
			transform.parent = earth.transform;
		}

		// Invoke("ChangeRotateDirection", timeToChangeDir);
    
	}

	private void Update()
	{
		transform.Rotate(rotationDirection * Time.deltaTime * moveSpeed);
	}

	public void TakeDamage(float damage)
	{
		hp -= damage;
		if(hp <= 0)
		{
			Death();
		}
	}

	// private void ChangeRotateDirection()
	// {
	// 	if(!this.enabled)
	// 		return;
		
	// 	transform.rotation = Random.rotation;
	// 	Invoke("ChangeRotateDirection", timeToChangeDir);
	// }


	private void Death()
	{ 
		Instantiate(destroyEffect, transform.GetChild(1).position, Quaternion.identity);
		GameObject.Find("Game Control").GetComponent<GameControl>().IncreaseXP(xpValue);
		Destroy(gameObject);
	}
}
