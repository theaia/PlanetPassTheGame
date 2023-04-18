using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootStrap : MonoBehaviour
{
	IEnumerator Start() {
		yield return new WaitForSeconds(.5f);
		SceneManager.LoadScene("MainLevel");
	}
	
}
