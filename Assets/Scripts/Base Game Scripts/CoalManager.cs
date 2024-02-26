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
    private Board board;
    private EndGameManager endGame;
    // Start is called before the first frame update
    void Start()
    {
        board=FindObjectOfType<Board>();
        endGame = FindObjectOfType<EndGameManager>();
        GetGoals();
        SetupCoals();
    }

    void GetGoals()
    {
        if (board != null)
        {
            if (board.world != null)
            {
                if (board.level < board.world.levels.Length)
                {
                    if (board.world.levels[board.level] != null)
                    {
                        levelCoals = board.world.levels[board.level].levelGoals;
                        for (int i = 0; i < levelCoals.Length; i++)
                        {
                            levelCoals[i].numberCollected = 0;
                        }
                    }
                }
            }
        }
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

   public void UpdateGoals()
    {
        int coalsCompleted = 0;
        for(int i = 0; i < levelCoals.Length; i++)
        {
            currentCoals[i].thisText.text = "" + levelCoals[i].numberCollected + "/" + levelCoals[i].numberNeede;
            if (levelCoals[i].numberCollected >= levelCoals[i].numberNeede)
            {
                coalsCompleted++;
                currentCoals[i].thisText.text = "" + levelCoals[i].numberNeede + "/" + levelCoals[i].numberNeede; ;
            }
            if (coalsCompleted >= levelCoals.Length)
            {
                if (endGame != null)
                {
                    endGame.WinGame();
                }
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
