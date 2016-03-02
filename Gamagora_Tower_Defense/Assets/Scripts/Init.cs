using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{

    public void LoadScene(string name)
    {
        SceneManager.LoadSceneAsync(name);
    }

    public void LaunchGame()
    {
        StartCoroutine("LoadGame");
    }

    public IEnumerator LoadGame()
    {
        LoadScene("scene");
        yield return new WaitForSeconds(1f);
    }
}
