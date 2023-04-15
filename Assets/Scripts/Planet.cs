using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class Planet : MonoBehaviour
{
	public float rotationSpeed;
	bool dragging;
	private Rigidbody rb;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
	}

	private void FixedUpdate()
	{
		float x = Input.GetAxis("Horizontal") * rotationSpeed * Time.fixedDeltaTime;
		float y = Input.GetAxis("Vertical") * rotationSpeed * Time.fixedDeltaTime;

		if(x != 0)
			rb.AddTorque(Vector3.down * x, ForceMode.VelocityChange);
		if(y != 0)
			rb.AddTorque(Vector3.right * y, ForceMode.VelocityChange);
	}
}
