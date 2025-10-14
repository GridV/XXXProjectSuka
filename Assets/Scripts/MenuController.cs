using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Start pressed");
        SceneManager.LoadScene("Game_First"); // замени на имя своей игровой сцены
    }

    public void OpenSettings()
    {
        Debug.Log("Settings pressed");
        SceneManager.LoadScene("SettingsS"); // замени на имя сцены с настройками
    }

    public void QuitGame()
    {
        Debug.Log("Quit pressed");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // чтобы работало в редакторе
#endif
    }
}
