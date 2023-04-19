using UnityEngine;
using System.Collections;

public class SplatMakerExample : MonoBehaviour {
	
	Vector4 channelMask = new Vector4(0,0,1,0);

	int splatsX = 1;
	int splatsY = 1;

	
	// Update is called once per frame
	void Update () {
		if(GameControl.Instance.mySheep != null || GameControl.Instance.IsGameOver()) {
			return;
		}
		// Get how many splats are in the splat atlas
		splatsX = SplatManagerSystem.instance.splatsX;
		splatsY = SplatManagerSystem.instance.splatsY;
			
		// Cast a ray from the camera to the mouse pointer and draw a splat there.
		// This just picks a rendom scale and bias for a 4x4 splat atlas
		// You could use a larger atlas of splat textures and pick a scale and offset for the specific splat you want to use
		if (Input.GetMouseButton (0)) {
			
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;
			if( Physics.Raycast( ray, out hit, 10000 ) ){
				
				Vector3 leftVec = Vector3.Cross ( hit.normal, Vector3.up );
				float randScale = Random.Range(0.5f,1.5f);
				
				GameObject newSplatObject = new GameObject();
				newSplatObject.transform.position = hit.point;
				if( leftVec.magnitude > 0.001f ){
					newSplatObject.transform.rotation = Quaternion.LookRotation( leftVec, hit.normal );
				}
				newSplatObject.transform.RotateAround( hit.point, hit.normal, Random.Range(-180, 180 ) );
				newSplatObject.transform.localScale = new Vector3( randScale, randScale * 0.5f, randScale ) * GameControl.Instance.SplatScale;

				Splat newSplat;
				newSplat.splatMatrix = newSplatObject.transform.worldToLocalMatrix;
				newSplat.channelMask = channelMask;

				float splatscaleX = 1.0f / splatsX;
				float splatscaleY = 1.0f / splatsY;
				float splatsBiasX = Mathf.Floor( Random.Range(0,splatsX * 0.99f) ) / splatsX;
				float splatsBiasY = Mathf.Floor( Random.Range(0,splatsY * 0.99f) ) / splatsY;

				newSplat.scaleBias = new Vector4(splatscaleX, splatscaleY, splatsBiasX, splatsBiasY );

				SplatManagerSystem.instance.AddSplat (newSplat);

				GameObject.Destroy( newSplatObject );

			}
		}
	
	}
}
