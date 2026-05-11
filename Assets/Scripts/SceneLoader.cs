using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Simple scene switching helper exposing two public methods:
/// - LoadLevelByIndex(int index)
/// - LoadLevelByName(string name)
///
/// Place this on a GameObject and call the methods from UI buttons
/// or other scripts. Uses Build Settings order for index-based loads.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Header("Load Options")]
    [Tooltip("Use async scene loading (recommended)")]
    public bool useAsync = true;

    [Tooltip("Load scene in Single or Additive mode")]
    public LoadSceneMode loadMode = LoadSceneMode.Single;

    [Tooltip("Invoked after the scene finishes loading")]
    public UnityEvent onSceneLoaded;

    /// <summary>
    /// Load a scene by its build index. Validates index against Build Settings.
    /// </summary>
    public void LoadLevelByIndex(int index)
    {
        int sceneCount = SceneManager.sceneCountInBuildSettings;
        if (index < 0 || index >= sceneCount)
        {
            Debug.LogError($"SceneLoader: Invalid scene index {index}. Build Settings contains {sceneCount} scenes.");
            return;
        }

        if (useAsync)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(index, loadMode);
            if (op == null)
            {
                Debug.LogError("SceneLoader: Failed to start async load by index.");
                return;
            }
            op.completed += _ => onSceneLoaded?.Invoke();
        }
        else
        {
            SceneManager.LoadScene(index, loadMode);
            onSceneLoaded?.Invoke();
        }
    }

    /// <summary>
    /// Load a scene by name. Validates presence in Build Settings before loading.
    /// </summary>
    public void LoadLevelByName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("SceneLoader: scene name is null or empty.");
            return;
        }

        if (!SceneExistsInBuildSettings(name))
        {
            Debug.LogError($"SceneLoader: Scene '{name}' not found in Build Settings.");
            return;
        }

        if (useAsync)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(name, loadMode);
            if (op == null)
            {
                Debug.LogError("SceneLoader: Failed to start async load by name.");
                return;
            }
            op.completed += _ => onSceneLoaded?.Invoke();
        }
        else
        {
            SceneManager.LoadScene(name, loadMode);
            onSceneLoaded?.Invoke();
        }
    }

    private bool SceneExistsInBuildSettings(string sceneName)
    {
        int count = SceneManager.sceneCountInBuildSettings;
        for (int i = 0; i < count; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (string.IsNullOrEmpty(path))
                continue;

            string fileName = Path.GetFileNameWithoutExtension(path);
            if (string.Equals(fileName, sceneName, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
