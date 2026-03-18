using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Handles scene transition from the title screen.
/// </summary>
public class TitleMenuUI : MonoBehaviour
{
    [FormerlySerializedAs("gameSceneName")]
    [SerializeField] private string GameSceneName = "Game";

    /// <summary>
    /// Called by the Start button to open the main game scene.
    /// </summary>
    public void OnClickStart()
    {
        SceneManager.LoadScene(GameSceneName);
    }
}
