using UnityEngine;
using TMPro;

public class GrassMaker : MonoBehaviour {
	
	Vector4 channelMask = new Vector4(0f,0f,1f,0f);
	int spritesX = 1;
	int spritesY = 1;

	public float grassScale = 1.0f;

	public TextMeshProUGUI percentText;

	// Update is called once per frame
	void Update () {
	
		// Get how many splats are in the splat atlas
		spritesX = GrassManagerSystem.instance.grassX;
		spritesY = GrassManagerSystem.instance.grassY;

		// Cast a ray from the camera to the mouse pointer and draw a splat there.
		// This just picks a rendom scale and bias for a 4x4 splat atlas
		// You could use a larger atlas of splat textures and pick a scale and offset for the specific splat you want to use
		if (Input.GetMouseButton (0)) {
			Ray ray = Camera.main.ScreenPointToRay( Input.mousePosition );
			RaycastHit hit;
			if( Physics.Raycast( ray, out hit, 10000 ) ){
				//Debug.Log("hit the raycast thing");
				Vector3 leftVec = Vector3.Cross ( hit.normal, Vector3.up );
				float randScale = Random.Range(0.5f,1.5f);
				
				GameObject newSplatObject = new GameObject();
				newSplatObject.transform.position = hit.point;
				if( leftVec.magnitude > 0.001f ){
					newSplatObject.transform.rotation = Quaternion.LookRotation( leftVec, hit.normal );
				}
				newSplatObject.transform.RotateAround( hit.point, hit.normal, Random.Range(-180, 180 ) );
				newSplatObject.transform.localScale = new Vector3( randScale, randScale * 0.5f, randScale ) * grassScale;

				Grass newSplat;
				newSplat.splatMatrix = newSplatObject.transform.worldToLocalMatrix;
				newSplat.channelMask = channelMask;

				float splatscaleX = 1.0f / spritesX;
				float splatscaleY = 1.0f / spritesY;
				float splatsBiasX = Mathf.Floor( Random.Range(0,spritesX * 0.99f) ) / spritesX;
				float splatsBiasY = Mathf.Floor( Random.Range(0,spritesY * 0.99f) ) / spritesY;

				newSplat.scaleBias = new Vector4(splatscaleX, splatscaleY, splatsBiasX, splatsBiasY );

				GrassManagerSystem.instance.AddSplat (newSplat);

				Destroy( newSplatObject );

			}
		}
	
	}
}
