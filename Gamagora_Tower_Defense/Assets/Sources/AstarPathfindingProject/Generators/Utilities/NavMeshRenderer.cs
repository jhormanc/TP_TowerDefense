using UnityEngine;
using System.Collections;
using UnityEditor.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class NavMeshRenderer : MonoBehaviour {
	
	/** Last level loaded. Used to check for scene switches */
	string lastLevel = "";
	
	/** Used to get rid of the compiler warning that #lastLevel is not used */
	public string SomeFunction () {
		return lastLevel;
	}
	
	// Update is called once per frame
	void Update () {
		#if UNITY_EDITOR
		if (lastLevel == "") {
			lastLevel = EditorSceneManager.GetActiveScene().name;
		}
		
		if (lastLevel != EditorSceneManager.GetActiveScene().name) {
			DestroyImmediate (gameObject);
		}
		#endif
	}
}