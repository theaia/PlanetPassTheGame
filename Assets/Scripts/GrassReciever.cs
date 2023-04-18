using UnityEngine;

public class GrassReciever : MonoBehaviour {

	// need to add all the renderers before Start of Splat Manager
	void Start () {
		Renderer thisRenderer = this.gameObject.GetComponent<Renderer> ();
		if (thisRenderer != null) {
			GrassManagerSystem.instance.AddRenderer (thisRenderer);
		}
	}

}
