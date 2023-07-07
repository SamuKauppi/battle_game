using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private int currentSceneIndex = 0;

    public void LoadScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
        currentSceneIndex = sceneIndex;
    }

    public int GetCurrentSceneIndex()
    {
        return currentSceneIndex;
    }
}
