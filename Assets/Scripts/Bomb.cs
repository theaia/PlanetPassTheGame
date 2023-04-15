using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
	public float explosionRadius;
	public LayerMask whatIsDestructible;

	public GameObject destroyEffect;

	private Transform earth;

	private Animator camAnim;

	private void Start()
	{
		camAnim = Camera.main.GetComponent<Animator>();
		earth = GameObject.FindGameObjectWithTag("Earth").transform;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.CompareTag("Enemy")){ 
			Collider[] colsInfo = Physics.OverlapSphere(transform.position, explosionRadius, whatIsDestructible);
			for(int i = 0; i < colsInfo.Length; i++)
			{
				Instantiate(colsInfo[i].GetComponent<DestructibleObject>().destroyEffect, colsInfo[i].transform.position, Quaternion.identity, earth.transform);
				Destroy(colsInfo[i].gameObject);
			}
			camAnim.SetTrigger("shake");
			Instantiate(destroyEffect, transform.position, Quaternion.identity, earth.transform);
			Destroy(gameObject);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, explosionRadius);
	}
}
