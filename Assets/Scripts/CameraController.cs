using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomMax;
	public float zoomMin;
	private Camera cam;
    public float zoomAmount;

    void Start(){ 
    
        cam = GetComponent<Camera>();
    }

    void Update(){
		if (Input.GetAxis("Mouse ScrollWheel") > 0f && cam.fieldOfView > zoomMax) // forward
		{
			cam.fieldOfView -= zoomAmount;
		} else if(Input.GetAxis("Mouse ScrollWheel") < 0f && cam.fieldOfView < zoomMin)
		{
			cam.fieldOfView += zoomAmount;
		}
	}
}
