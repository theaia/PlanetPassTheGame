using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
    public float minScale;
    public float maxScale;
    private float scaleAmount;
	
	public PlaceObject placementScript;

	public delegate void SetupBuilding();
	public event SetupBuilding OnSetupBuilding;


	private void Start()
	{
		scaleAmount = Random.Range(minScale, maxScale);
		transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y + scaleAmount, transform.localScale.z);

		if(placementScript == null)
		{
			BuildingPlaced();
		}
	}

	public void BuildingPlaced()
	{
		if(OnSetupBuilding != null)
			OnSetupBuilding.Invoke();
	}
}
