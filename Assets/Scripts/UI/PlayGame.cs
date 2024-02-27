using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayGame : MonoBehaviour
{
    public string levelToLoad;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public void GoPlay()
    {
        SceneManager.LoadScene(levelToLoad);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
