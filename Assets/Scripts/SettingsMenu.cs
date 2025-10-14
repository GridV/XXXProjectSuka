using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsMenu : MonoBehaviour
{
    public void Back() => SceneManager.LoadScene("MainMenuURP");
}
