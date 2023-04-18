using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceObject : MonoBehaviour
{
	public Building buildingController;
    private Camera gameCam;

	private Transform earth;

	public bool canMoveAgain;

	public LayerMask whatIsPlanet;

	public bool isMoving = true;

	private Collider col;

	private Animator camAnim;
	private Animator anim;

	public GameObject buildEffect;

	private void Start()
	{
		camAnim = Camera.main.GetComponent<Animator>();
		anim = GetComponent<Animator>();	
		col = GetComponent<Collider>();
		gameCam = Camera.main;	
		earth = GameObject.FindGameObjectWithTag("Earth").transform;
	}

	private void Update()
	{
		if(!isMoving)
			return;
		
		Ray ray = gameCam.ScreenPointToRay(Input.mousePosition);
		RaycastHit hitInfo;

		if(Physics.Raycast(ray, out hitInfo, Mathf.Infinity, whatIsPlanet))
		{ 
			transform.position = hitInfo.point;
			transform.rotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
		}

		if(Input.GetMouseButtonUp(0) || GameControl.Instance.IsTransitioningWorlds)
		{
			PlaceDown();
		}
	}

	private void PlaceDown() {
		if (anim != null)
			anim.SetTrigger("popIn");
		//camAnim.SetTrigger("shake");

		Instantiate(buildEffect, transform.position, Quaternion.identity, earth.transform);
		//transform.parent = earth.transform;
		isMoving = false;
		col.enabled = true;

		if (buildingController != null)
			buildingController.BuildingPlaced();
	}

}
