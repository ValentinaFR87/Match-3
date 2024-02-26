using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CoalPanel : MonoBehaviour
{
    public Image thisImage;
    public Sprite thisSprite;
    public Text thisText;
    public string thisString;
    
    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    void Setup()
    {
        thisImage.sprite= thisSprite;
        thisText.text= thisString;
    }
}
