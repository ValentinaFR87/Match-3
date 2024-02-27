using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class BackToMain : MonoBehaviour
{

    public string sceneToLoad;
    private GameData gameData;
    private Board board;

    public void WinOk()
    {
        if(gameData != null)
        {
            gameData.saveData.isActive[board.level+1] = true;
            gameData.Save();
        }
        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoseOk()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

    // Start is called before the first frame update
    void Start()
    {
        board=FindObjectOfType<Board>();
        gameData=FindObjectOfType<GameData>();
    }

    
}
