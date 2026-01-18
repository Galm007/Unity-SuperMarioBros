using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void OnStartButtonPressed()
    {
        SceneManager.LoadScene("Level1");
    }
}
