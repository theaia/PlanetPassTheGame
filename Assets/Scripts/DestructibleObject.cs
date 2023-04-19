using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleObject : MonoBehaviour
{
    public GameObject destroyEffect;
	public bool canDestroy;

	public bool isEnemy;

	public bool destroyParent = false;

	private Animator camAnim;

	private Transform earth;

	private void Start()
	{
		camAnim = Camera.main.GetComponent<Animator>();
		earth = GameObject.FindGameObjectWithTag("Earth").transform;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(!canDestroy)
			return;
		
		if(isEnemy)
		{
			if(other.CompareTag("Bomb"))
			{
				camAnim.SetTrigger("shake");

				Debug.Log("BombHit");
				Instantiate(other.GetComponent<DestructibleObject>().destroyEffect, other.transform.position, other.transform.rotation, earth.transform);
				Instantiate(destroyEffect, transform.position, transform.rotation, earth.transform);
				Destroy(other.gameObject);
				if (destroyParent) Destroy(transform.parent.gameObject);
				else Destroy(gameObject);

            }
			else if (other.CompareTag("Building"))
            {
                camAnim.SetTrigger("shake");

                Debug.Log("Building Hit");
                Instantiate(other.GetComponent<DestructibleObject>().destroyEffect, other.transform.position, other.transform.rotation, earth.transform);
                Destroy(other.gameObject);
            }
		} 
	}

	public void StartBehavior()
    {
        canDestroy = true;
    }

	private void OnDisable() {
		if (GameControl.Instance && isEnemy) {
			GameControl.Instance.AddScore(5);
		}
	}
}
