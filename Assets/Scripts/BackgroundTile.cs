using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BackgroundTile : MonoBehaviour
{
    public int hitPoints;
    private SpriteRenderer sprite;
    private CoalManager coalManager;

    private void Start()
    {
        coalManager=FindObjectOfType<CoalManager>();
        sprite = GetComponent<SpriteRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        if(hitPoints <= 0)
        {
            if(coalManager!=null)
            {
                coalManager.CompareCoal(this.gameObject.tag);
                coalManager.UpdateCoals();
            }
            Destroy(this.gameObject);
        }
    }
    public void TakeDamage(int damage)
    {
        hitPoints-=damage;
        MakeLinghter();

    }
   
   
    void MakeLinghter()
    {
        Color color = sprite.color;
        float newAlpha = color.a * .8f;
        sprite.color=new Color(color.r,color.g,color.b, newAlpha);
    }
   
  
}
