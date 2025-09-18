using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        //SceneManager.LoadScene("Game Scene");
        SceneManager.LoadScene("Capture The Flag");
    }

    public void ExplainGame()
    {
        //SceneManager.LoadScene("Explain Scene");
        SceneManager.LoadScene("Explain");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
