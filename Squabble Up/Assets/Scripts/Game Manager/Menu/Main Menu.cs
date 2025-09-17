using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        //SceneManager.LoadScene("Game Scene");
        SceneManager.LoadScene("Capture The Flag");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
