using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
using System.Collections.Generic;

public struct Grass {
	public Matrix4x4 splatMatrix;
	public Vector4 channelMask;
	public Vector4 scaleBias;
}

public class GrassManagerSystem
{
	static GrassManagerSystem m_Instance;
	static public GrassManagerSystem instance {
		get {
			if (m_Instance == null)
				m_Instance = new GrassManagerSystem();
			return m_Instance;
		}
	}

	public int grassX;
	public int grassY;

	public Vector4 scores;

	// a list of splats to be drawn
	internal List<Grass> m_Grass = new List<Grass>();
	
	public void AddSplat (Grass splat)
	{
		Debug.Log ("Adding Splat");
		m_Grass.Add (splat);
	}

	// a 2d list of renderers to draw splats to
	internal List<List<Renderer>> m_RendererAray = new List<List<Renderer>> ();

	// a list of renderers to draw splats to
	internal List<Renderer> m_Renderers = new List<Renderer>();


	public void AddRenderer (Renderer renderer)
	{
		while (renderer.lightmapIndex >= m_RendererAray.Count) {
			m_RendererAray.Add (new List<Renderer> ());
		}

		Debug.Log ("Adding Renderer");
		m_RendererAray [renderer.lightmapIndex].Add (renderer);
		m_Renderers.Add (renderer);
	}

}

public class GrassManager : MonoBehaviour {

	public static GrassManager Instance;

	[SerializeField]
	public LightmapData lightmapData;

	[SerializeField]
	public LightmapSettings lightmapSettings;

	public int sizeX;
	public int sizeY;

	public Texture2D splatTexture;
	public int grassX = 4;
	public int grassY = 4;

	public List<RenderTexture> grassTexList;
	public List<RenderTexture> grassTexAltList;
	public List<RenderTexture> worldPosTexList;

	public RenderTexture grassTex;
	public RenderTexture grassTexAlt;

    public RenderTexture worldPosTex;
	public RenderTexture worldPosTexTemp;
	public RenderTexture worldTangentTex;
	public RenderTexture worldBinormalTex;
	private Camera rtCamera;

	private Material grassBlitMaterial;

	private bool evenFrame = false;

	public Vector4 scores = Vector4.zero;
	public float blueScore;

	public RenderTexture scoreTex;
	public RenderTexture RT4;
	public Texture2D Tex4;

	// this will keep duplicate splat managers from being enabled at once
	void Awake () {

		if (GrassManager.Instance != null) {
			if (GrassManager.Instance != this) {
				Destroy (this);
			}
		} else {
			GrassManager.Instance = this;
		}

	}

    // Use this for initialization
    void Start () {
		blueScore = 0;

		GrassManagerSystem.instance.grassX = grassX;
		GrassManagerSystem.instance.grassY = grassY;

		grassBlitMaterial = new Material (Shader.Find ("Splatoonity/SplatBlit"));
		
		grassTex = new RenderTexture (sizeX, sizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		grassTex.Create ();
		grassTexAlt = new RenderTexture (sizeX, sizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		grassTexAlt.Create ();
		worldPosTex = new RenderTexture (sizeX, sizeY, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		worldPosTex.Create ();
		worldPosTexTemp = new RenderTexture (sizeX, sizeY, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
		worldPosTexTemp.Create ();
		worldTangentTex = new RenderTexture (sizeX, sizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		worldTangentTex.Create ();
		worldBinormalTex = new RenderTexture (sizeX, sizeY, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		worldBinormalTex.Create();

		grassTexList = new List<RenderTexture> ();
		grassTexAltList = new List<RenderTexture> ();
		worldPosTexList = new List<RenderTexture> ();

		Shader.SetGlobalTexture ("_SplatTex", grassTex);
		Shader.SetGlobalTexture ("_WorldPosTex", worldPosTex);
		Shader.SetGlobalTexture ("_WorldTangentTex", worldTangentTex);
		Shader.SetGlobalTexture ("_WorldBinormalTex", worldBinormalTex);
		Shader.SetGlobalVector ("_SplatTexSize", new Vector4 (sizeX, sizeY, 0, 0));


		// Textures for tallying scores 
		// needs to be higher precision because it will be mipped down to 4x4 ldr texture for final score keeping
		scoreTex = new RenderTexture (sizeX/8, sizeY/8, 0, RenderTextureFormat.ARGBHalf, RenderTextureReadWrite.Linear);
		scoreTex.autoGenerateMips = true;
		scoreTex.useMipMap = true;
		scoreTex.Create ();
		RT4 = new RenderTexture (4, 4, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
		RT4.Create ();
		Tex4 = new Texture2D (4, 4, TextureFormat.ARGB32, false);

		
		GameObject rtCameraObject = new GameObject ();
		rtCameraObject.name = "rtCameraObject";
		rtCameraObject.transform.position = Vector3.zero;
		rtCameraObject.transform.rotation = Quaternion.identity;
		rtCameraObject.transform.localScale = Vector3.one;
		rtCamera = rtCameraObject.AddComponent<Camera> ();
		rtCamera.renderingPath = RenderingPath.Forward;
		rtCamera.clearFlags = CameraClearFlags.SolidColor;
		rtCamera.backgroundColor = new Color (0, 0, 0, 0);
		rtCamera.orthographic = true;
		rtCamera.nearClipPlane = 0.0f;
		rtCamera.farClipPlane = 1.0f;
		rtCamera.orthographicSize = 1.0f;
		rtCamera.aspect = 1.0f;
		rtCamera.useOcclusionCulling = false;
		rtCamera.enabled = false;

		RenderTextures ();
		BleedTextures ();
		StartCoroutine( UpdateScores() );

    }

	/*
	// Render textures using shader replacement.
	// This will render all objects in the scene though.
	// You could cull based on layers though.
	void RenderTextures() {

		Material worldPosMaterial = new Material (Shader.Find ("Splatoonity/WorldPosUnwrap"));
		Material worldNormalMaterial = new Material (Shader.Find ("Splatoonity/WorldNormalUnwrap"));

		rtCamera.targetTexture = worldPosTex;
		rtCamera.RenderWithShader (Shader.Find ("Splatoonity/WorldPosUnwrap"), null);

		rtCamera.targetTexture = worldTangentTex;
		rtCamera.RenderWithShader (Shader.Find ("Splatoonity/WorldTangentUnwrap"), null);

		rtCamera.targetTexture = worldBinormalTex;
		rtCamera.RenderWithShader (Shader.Find ("Splatoonity/WorldBinormalUnwrap"), null);
	}
	*/

	// Render textures with a command buffer.
	// This is more flexible as you can explicitly add more objects to render without worying about layers.
	// You could also have multiple instances for chunks of a scene.
	void RenderTextures() {

		// Set the culling mask to Nothing so we can draw renderers explicitly
		rtCamera.cullingMask = LayerMask.NameToLayer("Nothing");

		Material worldPosMaterial = new Material (Shader.Find ("Splatoonity/WorldPosUnwrap"));
		Material worldTangentMaterial = new Material (Shader.Find ("Splatoonity/WorldTangentUnwrap"));
		Material worldBiNormalMaterial = new Material (Shader.Find ("Splatoonity/WorldBinormalUnwrap"));

		// You could collect all objects you want rendererd and loop through DrawRenderer
		// but for this example I'm just drawing the one renderer.
		//Renderer envRenderer = this.gameObject.GetComponent<Renderer> ();

		int rendererCount = GrassManagerSystem.instance.m_Renderers.Count;

		// You could also use a multi render target and only have to draw each renderer once.
		CommandBuffer cb = new CommandBuffer();
		cb.SetRenderTarget(worldPosTex);
		cb.ClearRenderTarget(true, true, new Color(0,0,0,0) );
		//cb.DrawRenderer(envRenderer, worldPosMaterial);
		for (int i = 0; i < rendererCount; i++) {
			cb.DrawRenderer (GrassManagerSystem.instance.m_Renderers[i], worldPosMaterial);
		}

		cb.SetRenderTarget(worldTangentTex);
		cb.ClearRenderTarget(true, true, new Color(0,0,0,0) );
		//cb.DrawRenderer(envRenderer, worldTangentMaterial);
		for (int i = 0; i < rendererCount; i++) {
			cb.DrawRenderer (GrassManagerSystem.instance.m_Renderers[i], worldTangentMaterial);
		}

		cb.SetRenderTarget(worldBinormalTex);
		cb.ClearRenderTarget(true, true, new Color(0,0,0,0) );
		//cb.DrawRenderer(envRenderer, worldBiNormalMaterial);
		for (int i = 0; i < rendererCount; i++) {
			cb.DrawRenderer (GrassManagerSystem.instance.m_Renderers[i], worldBiNormalMaterial);
		}

		// Only have to render the camera once!
		rtCamera.AddCommandBuffer (CameraEvent.AfterEverything, cb);
		rtCamera.Render ();
	}


	void BleedTextures() {
		Graphics.Blit (Texture2D.blackTexture, grassTex, grassBlitMaterial, 1);		
		Graphics.Blit (Texture2D.blackTexture, grassTexAlt, grassBlitMaterial, 1);

		grassBlitMaterial.SetVector("_SplatTexSize", new Vector2( sizeX, sizeY ) );

		// Bleed the world position out 2 pixels
		Graphics.Blit (worldPosTex, worldPosTexTemp, grassBlitMaterial, 2);
		Graphics.Blit (worldPosTexTemp, worldPosTex, grassBlitMaterial, 2);

		// Don't need this guy any more
		worldPosTexTemp.Release();
		worldPosTexTemp = null;
	}


	// Blit the splats
	// This is similar to how a deferred decal would work
	// except instead of getting the world position from the depth
	// use the world position that is stored in the texture.
	// Each splat is tested against the entire world position texture.
	void PaintSplats() {

		if (GrassManagerSystem.instance.m_Grass.Count > 0) {
			
			Matrix4x4[] SplatMatrixArray = new Matrix4x4[10];
			Vector4[] SplatScaleBiasArray = new Vector4[10];
			Vector4[] SplatChannelMaskArray = new Vector4[10];

			// Render up to 10 splats per frame.
			int i = 0;
			while( GrassManagerSystem.instance.m_Grass.Count > 0 && i < 10 ){
				SplatMatrixArray [i] = GrassManagerSystem.instance.m_Grass [0].splatMatrix;
				SplatScaleBiasArray [i] = GrassManagerSystem.instance.m_Grass [0].scaleBias;
				SplatChannelMaskArray [i] = GrassManagerSystem.instance.m_Grass [0].channelMask;
				GrassManagerSystem.instance.m_Grass.RemoveAt(0);
				i++;
			}
			grassBlitMaterial.SetMatrixArray ( "_SplatMatrix", SplatMatrixArray );
			grassBlitMaterial.SetVectorArray ( "_SplatScaleBias", SplatScaleBiasArray );
			grassBlitMaterial.SetVectorArray ( "_SplatChannelMask", SplatChannelMaskArray );

			grassBlitMaterial.SetInt ( "_TotalSplats", i );

			grassBlitMaterial.SetTexture ("_WorldPosTex", worldPosTex);

			// Ping pong between the buffers to properly blend splats.
			// If this were a compute shader you could just update one buffer.
			if (evenFrame) {
				grassBlitMaterial.SetTexture ("_LastSplatTex", grassTexAlt);
				Graphics.Blit (splatTexture, grassTex, grassBlitMaterial, 0);
				Shader.SetGlobalTexture ("_SplatTex", grassTex);
				evenFrame = false;
			} else {
				grassBlitMaterial.SetTexture ("_LastSplatTex", grassTex);
				Graphics.Blit (splatTexture, grassTexAlt, grassBlitMaterial, 0);
				Shader.SetGlobalTexture ("_SplatTex", grassTexAlt);
				evenFrame = true;
			}

		}

	}

	// Update the scores by mipping the splat texture down to a 4x4 texture and sampling the pixels.
	// Space the whole operation out over a few frames to keep everything running smoothly.
	// Only update the scores once every second.
	IEnumerator UpdateScores() {

		while( true ){
			blueScore = CalculateBluePercentage(RenderTextureToTexture2D(scoreTex), blueThreshold);
			if (blueScore > 0) { 
				blueScore = Mathf.Clamp(blueScore + Mathf.Lerp(0, 15, blueScore/100f),0f, 100f); //Gives a bit of an error threshold that increases the closer we get to the end.
			}
			yield return new WaitForEndOfFrame();

			Graphics.Blit (grassTex, scoreTex, grassBlitMaterial, 3);
			Graphics.Blit (scoreTex, RT4, grassBlitMaterial, 4);

			RenderTexture.active = RT4;
			Tex4.ReadPixels (new Rect (0, 0, 4, 4), 0, 0);
			Tex4.Apply ();

			yield return new WaitForSeconds(0.01f);

			Color scoresColor = new Color(0,0,0,0);
			scoresColor += Tex4.GetPixel(0,0);
			scoresColor += Tex4.GetPixel(0,1);
			scoresColor += Tex4.GetPixel(0,2);
			scoresColor += Tex4.GetPixel(0,3);

			yield return new WaitForSeconds(0.01f);

			scoresColor += Tex4.GetPixel(1,0);
			scoresColor += Tex4.GetPixel(1,1);
			scoresColor += Tex4.GetPixel(1,2);
			scoresColor += Tex4.GetPixel(1,3);

			yield return new WaitForSeconds(0.01f);

			scoresColor += Tex4.GetPixel(2,0);
			scoresColor += Tex4.GetPixel(2,1);
			scoresColor += Tex4.GetPixel(2,2);
			scoresColor += Tex4.GetPixel(2,3);

			yield return new WaitForSeconds(0.01f);

			scoresColor += Tex4.GetPixel(3,0);
			scoresColor += Tex4.GetPixel(3,1);
			scoresColor += Tex4.GetPixel(3,2);
			scoresColor += Tex4.GetPixel(3,3);

			scores.x = scoresColor.r;
			scores.y = scoresColor.g;
			scores.z = scoresColor.b;
			scores.w = scoresColor.a;

			GrassManagerSystem.instance.scores = scores;

			yield return new WaitForSeconds (1.0f);

		}

	}
	
	// Update is called once per frame
	void Update () {

		PaintSplats ();

	}

	public int blueThreshold = 128;


	Texture2D RenderTextureToTexture2D(RenderTexture renderTexture) {
		Texture2D texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
		RenderTexture.active = renderTexture;
		texture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture.Apply();
		RenderTexture.active = null;
		return texture;
	}

	float CalculateBluePercentage(Texture2D texture, int blueThreshold) {
		int width = texture.width;
		int height = texture.height;
		int totalPixels = width * height;

		Color[] pixels = texture.GetPixels();

		int bluePixelCount = 0;

		for (int i = 0; i < pixels.Length; i++) {
			Color pixel = pixels[i];

			if (pixel.b * 255 >= blueThreshold) {
				bluePixelCount++;
			}
		}

		float bluePercentage = (float)bluePixelCount / totalPixels * 100;
		return bluePercentage;
	}
}
