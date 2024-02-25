using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState//перечисл€емы типа дл€ отслеживани€ состо€ни€ игры
{
    wait,
    move,
    win,
    lose,
    pause
}

public enum TileKind
{
    Breakable,
    Blank,
    Normal
}

[System.Serializable]
public class TileType
{
    public int x;
    public int y;
    public TileKind tileKind;
}

public class Board : MonoBehaviour
{
    [Header("Scriptable Object Stuff")]
    public World world;
    public int level;


    public GameState currentState= GameState.move;
    [Header("Board Dimensions")]
    public int width;
    public int height;
    public int offSet;//смещение 

    [Header("Prefabs")]
    public GameObject tilePrefab;
    public GameObject breacableTilePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;

    [Header("Layout")]
    public TileType[] boardLayot;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles; 
    public GameObject[,] allDots;
    public Dot currentDot;
    private FindMatches findMatches;
    public int basePieceValue = 20;
    private int streakValue = 1;
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    private CoalManager coalManager;
    public float refillDelay = 0.5f;
    public int[] scoreGoals;

    private void Awake()
    {
        if(world != null)
        {
            if (level < world.levels.Length)
            {
                if (world.levels[level] != null)
                {
                    width = world.levels[level].width;
                    height = world.levels[level].height;
                    dots = world.levels[level].dots;
                    scoreGoals = world.levels[level].scoreCoals;
                    boardLayot = world.levels[level].boardLayout;
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        coalManager=FindObjectOfType<CoalManager>();
        soundManager=FindObjectOfType<SoundManager>();
        scoreManager=FindObjectOfType<ScoreManager>();
        breakableTiles=new BackgroundTile[width,height];
        findMatches=FindObjectOfType<FindMatches>();
        blankSpaces = new bool[width, height];
        allDots = new GameObject[width, height];
        SetUp();
        currentState = GameState.pause;
    }

    public void GenerateBlankSpaces()
    {
        for (int i = 0; i < boardLayot.Length; i++)
        {
            if (boardLayot[i].tileKind== TileKind.Blank)
            {
                blankSpaces[boardLayot[i].x,boardLayot[i].y]= true;
            }
        }
    }

    public void GenerateBreakableTiles()
    {
        for(int i=0;i<boardLayot.Length; i++)
        {
            if (boardLayot[i].tileKind== TileKind.Breakable)
            {
                Vector2 tempPosition = new Vector2(boardLayot[i].x, boardLayot[i].y);
                GameObject tile = Instantiate( breacableTilePrefab, tempPosition, Quaternion.identity);
               breakableTiles[boardLayot[i].x, boardLayot[i].y] = tile.GetComponent<BackgroundTile>();

            }
        }
    }

    private void SetUp()//создаю доску и помещаю элементы
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {


                    Vector2 tempPosition = new Vector2(i, j + offSet);//добавл€ем смещение, чтобы элементы плавно опускались на сцену
                    Vector2 tilePosition = new Vector2(i, j);
                    GameObject backgroundTile = Instantiate(tilePrefab, tilePosition, Quaternion.identity) as GameObject;
                    backgroundTile.transform.parent = this.transform;
                    backgroundTile.name = "(" + i + "," + j + ")";
                    int dotToUse = Random.Range(0, dots.Length);

                    int maxIterations = 0;
                    while (MatchesAt(i, j, dots[dotToUse]) && maxIterations < 100)
                    {
                        dotToUse = Random.Range(0, dots.Length);
                        maxIterations++;
                    }
                    maxIterations = 0;

                    GameObject dot = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    dot.GetComponent<Dot>().row = j;
                    dot.GetComponent<Dot>().col = i;
                    dot.transform.parent = this.transform;
                    dot.name = "(" + i + "," + j + ")";
                    allDots[i, j] = dot;
                }
            }
        }
    }

    private bool MatchesAt(int col, int row, GameObject piece)//чтобы при запуске игры не было совпадений сразу
    {
        if (col > 1 && row > 1)
        {
            if (allDots[col - 1, row] != null && allDots[col - 2, row] != null)
            {


                if (allDots[col - 1, row].tag == piece.tag && allDots[col - 2, row].tag == piece.tag)
                {
                    return true;
                }
            }
            if (allDots[col , row - 1] != null && allDots[col , row - 2] != null)
            {
                if (allDots[col, row - 1].tag == piece.tag && allDots[col, row - 2].tag == piece.tag)
                {
                    return true;
                }
            }
        }
        else if (col <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (allDots[col, row - 1] != null && allDots[col, row - 2] != null)
                {
                    if (allDots[col, row - 1].tag == piece.tag && allDots[col, row - 2].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
            if (col > 1)
            {
                if (allDots[col - 1, row ] != null && allDots[col- 2, row ] != null)
                {
                    if (allDots[col - 1, row].tag == piece.tag && allDots[col - 2, row].tag == piece.tag)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool ColOrRow()
    {
        int numberHorizontal = 0;
        int numberVertical = 0;
        Dot firstPiece = findMatches.currentMatches[0].GetComponent<Dot>();
        if (firstPiece != null)
        {
            foreach(GameObject currentPiece in findMatches.currentMatches)
            {
                Dot dot=currentPiece.GetComponent<Dot>();
                if (dot.row == firstPiece.row)
                {
                    numberHorizontal++;
                }
                if(dot.col == firstPiece.col)
                {
                    numberVertical++;
                }
            }
        }
        return (numberHorizontal == 5 || numberVertical == 5);
    }
    private void CheckToMakeBombs()
    {
        if(findMatches.currentMatches.Count==4 || findMatches.currentMatches.Count == 7)
        {
            findMatches.CheckBombs();
        }
        if (findMatches.currentMatches.Count == 5 || findMatches.currentMatches.Count == 8)
        {
            if (ColOrRow())
            {
                if(currentDot != null)
                {
                    if (currentDot.isMatched){
                        if(!currentDot.isColorBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeColorBomb();
                        }
                    }
                    else
                    {
                        if(currentDot.otherDot!= null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isColorBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeColorBomb();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (currentDot != null)
                {
                    if (currentDot.isMatched)
                    {
                        if (!currentDot.isAdjacentBomb)
                        {
                            currentDot.isMatched = false;
                            currentDot.MakeAdjecentBomb();
                        }
                    }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched)
                            {
                                if (!otherDot.isAdjacentBomb)
                                {
                                    otherDot.isMatched = false;
                                    otherDot.MakeAdjecentBomb();
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnDestroyMatchesAt(int col,int row)//метод удал€ем элементы и присваевает на их место null 
    {
        if (allDots[col,row].GetComponent<Dot>().isMatched)
        {
            //how many elements are in the matches pieces list from findmatches?
            if(findMatches.currentMatches.Count>=4)
            {
                CheckToMakeBombs();
            }
           if (breakableTiles[col, row] != null)
            {
               
                breakableTiles[col, row].TakeDamage(1);//с какого раза бела€ плитка исчезнет
                if (breakableTiles[col, row].hitPoints <= 0)
                {
                    breakableTiles[col,row]= null;
                }
            }
            if (coalManager != null)
            {
                coalManager.CompareCoal(allDots[col, row].tag.ToString());
                coalManager.UpdateCoals();
            }
           if(soundManager!= null)
            {
                soundManager.PlayRandomDestriyNoise();
            }
            GameObject particle=Instantiate(destroyEffect, allDots[col, row].transform.position, Quaternion.identity);
            Destroy(particle, 0.7f);
            Destroy(allDots[col,row]);
            scoreManager.IncreaseScore(basePieceValue * streakValue);
            allDots[col,row] = null;
        }
    }

    private IEnumerator DecreaseRowCo2()
    {
        for(int i=0;i<width;i++) { 
             for(int j=0;j<height;j++)
            {
                if (!blankSpaces[i,j] && allDots[i,j] == null)
                {
                    for(int k = j + 1; k < height; k++)
                    {
                        if (allDots[i, k] != null)
                        {
                            allDots[i, k].GetComponent<Dot>().row = j;
                            allDots[i, k] = null;
                            break;
                        }
                    }
                }
             }
        }
        yield return new WaitForSeconds(refillDelay * 0.5f);
        StartCoroutine(FillBoardCo());
    }

    private IEnumerator DecreaseRowCo()// проходит по каждой €чейке в сетке и сдвигает элементы вниз, если под ними есть пустое место.
    {
        int nullCount = 0;
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i,j] = null;
                }

            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(refillDelay*0.5f);
        StartCoroutine(FillBoardCo());
    }
    public void DestroyMatches()//провер€етс€ есть ли в €чейке игровой элмент и если есть, вызываетс€ метод удалени€.
    {
        for(int i=0; i < width; i++)
        {
            for (int j=0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    OnDestroyMatchesAt(i, j);
                }
            }
        }
        findMatches.currentMatches.Clear();
        StartCoroutine(DecreaseRowCo2());
       
    }

   private void RefillBoard()//создает новые элементы случайные элементы
    {
        for(int i=0; i<width; i++)
        {
            for(int j=0; j<height; j++)
            {
                if (allDots[i, j]== null && !blankSpaces[i,j])
                {
                    Vector2 tempPosition = new Vector2(i, j+offSet);
                    int dotToUse=Random.Range(0,dots.Length);
                    int maxIterations = 0;

                    while (MatchesAt(i, j, dots[dotToUse])&& maxIterations < 100)
                    {
                        maxIterations++;    
                        dotToUse=Random.Range(0,dots.Length);
                    }
                    maxIterations = 0;

                    GameObject piece = Instantiate(dots[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().col = i;
                }
            }
        }
    }

    private bool MatchesOnBoard()//провер€т есть ли совпадени€
    {
        for (int i=0; i < width; i++)
        {
            for(int j=0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private IEnumerator FillBoardCo()//вызывает метод заполнени€, и в случае совпадений происходит удаление.
    {
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);

        while (MatchesOnBoard())
        {
            streakValue ++;
            DestroyMatches();
            yield return new WaitForSeconds(2*refillDelay);
            
        }
        findMatches.currentMatches.Clear();
        currentDot = null;
       
        if (IsDeadlocked())
        {
            ShuffleBoard();
            Debug.Log("Deadlocked!!!!");
        } 
        yield return new WaitForSeconds(refillDelay);
        currentState = GameState.move;
        streakValue = 1;
    }

    private void SwitchPieces(int col, int row, Vector2 direction)
    {
        GameObject holder = allDots[col + (int)direction.x, row + (int)direction.y] as GameObject;
        allDots[col + (int)direction.x, row + (int)direction.y] = allDots[col, row];
        allDots[col,row]= holder;
    }

    private bool CheckForMatches()
    {
        
        for(int i = 0; i < width; i++)
        {
            for(int  j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (i < width - 2)
                    {
                        if (allDots[i + 1, j] != null && allDots[i + 2, j] != null)
                        {
                            if (allDots[i + 1, j].tag == allDots[i, j].tag
                                && allDots[i + 2, j].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                    if (j < height - 2)
                    {
                        if (allDots[i, j + 1] != null && allDots[i, j + 2] != null)
                        {
                            if (allDots[i, j + 1].tag == allDots[i, j].tag
                                && allDots[i, j + 2].tag == allDots[i, j].tag)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool SwitchAndCheck(int col,int row, Vector2 direction)
    {
        SwitchPieces(col, row, direction);
        if (CheckForMatches())
        {
            SwitchPieces(col,row,direction);
            return true;
        }
        SwitchPieces(col,row,direction);
        return false;
    }
    
    private bool IsDeadlocked()
    {
        for(int i=0;i<width;i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    if (i < width - 1)
                    {
                       if( SwitchAndCheck(i, j, Vector2.right))
                        {
                            return false;
                        }
                    }
                    if(j < height - 1)
                    {
                        if(SwitchAndCheck(i, j, Vector2.up))
                        {
                            return false;
                        }
                    }
                }
            }
        }
        return true;
    }

    private IEnumerator ShuffleBoard()
    {
        yield return new WaitForSeconds(0.5f);
        List<GameObject> newBoard = new List<GameObject>();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    newBoard.Add(allDots[i, j]);
                }
            }
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j])
                {
                    int pieceToUse=Random.Range(0,newBoard.Count);
                   
                     int maxIterations = 0;
                    while (MatchesAt(i, j, newBoard[pieceToUse]) && maxIterations < 100)
                    {
                        pieceToUse = Random.Range(0, newBoard.Count);
                        maxIterations++;
                    }
                    Dot piece = newBoard[pieceToUse].GetComponent<Dot>();
                    maxIterations = 0;
                    piece.col = i;
                    piece.row= j;
                    allDots[i, j] = newBoard[pieceToUse];
                    newBoard.Remove(newBoard[pieceToUse]);
                }
            }
        }
        if (IsDeadlocked())
        {
            ShuffleBoard();
        }
    }
}
