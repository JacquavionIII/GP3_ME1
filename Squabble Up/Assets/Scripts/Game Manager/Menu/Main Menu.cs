using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        //SceneManager.LoadScene("Game Scene");
        SceneManager.LoadScene("Enemy and Lock-on");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
