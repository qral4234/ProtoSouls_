using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public AudioSource menuMusic; 

    private void Start()
    {
        if(menuMusic != null)
        {
            menuMusic.Play();
        }
    }

    public void PlayGame()
    {
        // "MainScene" isimli sahneyi yükle
        SceneManager.LoadScene("MainScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
