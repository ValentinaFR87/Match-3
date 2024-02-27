using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource[] destroyNoise;
    public AudioSource backMusic;

    private void Start()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
               backMusic.Play();
                backMusic.volume = 0;
            }
            backMusic.Play();
            backMusic.volume = 1;
        }
        else
        {
            backMusic.Play();
            backMusic.volume = 1;
        }
    }

    public void adjustVolume()
    {

        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                backMusic.volume = 0;
            }
            else
            {
                backMusic.volume = 1;
            }
        }
    }
    public void PlayRandomDestriyNoise()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if(PlayerPrefs.GetInt("Sound")==1)
            {
                int clipToPlay = Random.Range(0, destroyNoise.Length);
                destroyNoise[clipToPlay].Play();
            }
        }
        else
        {
            int clipToPlay = Random.Range(0, destroyNoise.Length);
            destroyNoise[clipToPlay].Play();
        }
        
    }
    
}
