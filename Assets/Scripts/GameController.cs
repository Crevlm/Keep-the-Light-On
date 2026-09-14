using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //Checks to see if the Restart Key in the New Input System has been pressed. If it has call Restart Game
    public void OnRestart()
    {
        //Debug.Log("Restart Button Pressed");
        RestartGame();

    }

    //Reloads the scene after confirming the player has hit "R" key binded to Restart in the Input System. 
    public void RestartGame()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    public void OnQuit()
    {
        QuitGame();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

}
