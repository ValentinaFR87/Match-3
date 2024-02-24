using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlankCoal
{
    public int numberNeede;
    public int numberCollected;
    public Sprite goalSprite;
    public string matchValue;
}

public class CoalManager : MonoBehaviour
{
    public BlankCoal[] levelCoals;
    public List<CoalPanel> currentCoals = new List<CoalPanel>();
    public GameObject coalPrebaf;
    public GameObject coalIntroParent;
    public GameObject coalGameParent;
    // Start is called before the first frame update
    void Start()
    {
        SetupCoals();
    }

    void SetupCoals()
    {
        for(int i = 0; i < levelCoals.Length; i++)
        {
            GameObject coal = Instantiate(coalPrebaf, coalIntroParent.transform.position, Quaternion.identity);
            coal.transform.SetParent(coalIntroParent.transform,false);
            CoalPanel panel=coal.GetComponent<CoalPanel>();
            panel.thisSprite = levelCoals[i].goalSprite;
            panel.thisString = "0/" + levelCoals[i].numberNeede;
            GameObject gameCoal= Instantiate(coalPrebaf, coalGameParent.transform.position, Quaternion.identity);
            gameCoal.transform.SetParent(coalGameParent.transform,false);
            panel = gameCoal.GetComponent<CoalPanel>();
            currentCoals.Add(panel);
            panel.thisSprite = levelCoals[i].goalSprite;
            panel.thisString = "0/" + levelCoals[i].numberNeede;
        }
    }

   public void UpdateCoals()
    {
        int coalsCompleted = 0;
        for(int i = 0; i < levelCoals.Length; i++)
        {
            currentCoals[i].thisText.text = "" + levelCoals[i].numberCollected + "/" + levelCoals[i].numberNeede;
            if (levelCoals[i].numberCollected >= levelCoals[i].numberNeede)
            {
                coalsCompleted++;
                currentCoals[i].thisText.text = "" + levelCoals[i].numberNeede;
            }
            if (coalsCompleted >= levelCoals.Length)
            {
                Debug.Log("You win!");
            }
        }
    }

    public void CompareCoal(string coalToCompare)
    {
        for(int i=0; i<levelCoals.Length;i++)
        {
            if (coalToCompare == levelCoals[i].matchValue)
            {
                levelCoals[i].numberCollected++;
            }
        }
    }
}
