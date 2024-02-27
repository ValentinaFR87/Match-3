using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PauseManager : MonoBehaviour
{

    public GameObject pausePanel;
    private Board board;
    public string newLevel;
    public bool pause = false;
    public Image soundButton;
    public Sprite musicOn;
    public Sprite musicOff;
    private SoundManager sound;

    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 1)
            {
                 soundButton.sprite = musicOff;
            }
            else
            {
               soundButton.sprite = musicOn;
            }
        }
        else
        {
            soundButton.sprite = musicOn;
        }
        pausePanel.SetActive(false);
        board=GameObject.FindWithTag("Board").GetComponent<Board>();
        sound=FindObjectOfType<SoundManager>();
        
    }

    // Update is called once per frame
    void Update()
    {
     if(pause && !pausePanel.activeInHierarchy)
        {
            pausePanel.SetActive(true);
            board.currentState = GameState.pause;

        }
        if(!pause && pausePanel.activeInHierarchy){
            pausePanel.SetActive(false);
            board.currentState = GameState.move;
        }
    }

    public void SoundButton()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                soundButton.sprite = musicOn;
                PlayerPrefs.SetInt("Sound", 1);
                sound.adjustVolume();
            }
            else
            {
                soundButton.sprite = musicOff;
                PlayerPrefs.SetInt("Sound", 0);
                sound.adjustVolume();
            }
        }
        else
        {
            soundButton.sprite = musicOff;
            PlayerPrefs.SetInt("Sound", 0);
            sound.adjustVolume();
        }
    }

    public void PauseGame()
    {
        pause=!pause;
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Splash");
    }
}
