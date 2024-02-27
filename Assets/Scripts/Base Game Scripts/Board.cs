using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState//перечисляемы типа для отслеживания состояния игры
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
    Lock,
    Concrete,
    Slime,
    Normal
}

[System.Serializable]
public class MatchType
{
    public int type;
    public string color;
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
    public GameObject lockTilePrefab;
    public GameObject concreteTilePrefab;
    public GameObject slimePiecePrefab;
    public GameObject[] dots;
    public GameObject destroyEffect;

    [Header("Layout")]
    public TileType[] boardLayot;
    private bool[,] blankSpaces;
    private BackgroundTile[,] breakableTiles;
    public BackgroundTile[,] lockTiles;
    private BackgroundTile[,] concreteTiles;
    private BackgroundTile[,] slimeTiles;
    public GameObject[,] allDots;


    public MatchType matchType;
    public Dot currentDot;
    private FindMatches findMatches;
    public int basePieceValue = 20;
    private int streakValue = 1;
    private ScoreManager scoreManager;
    private SoundManager soundManager;
    private CoalManager coalManager;
    public float refillDelay = 0.5f;
    public int[] scoreGoals;
    private bool makeSlime = true;

    private void Awake()
    {
        if (PlayerPrefs.HasKey("Текущий уровень"))
        {
            level = PlayerPrefs.GetInt("Текущий уроваень");
        }
        if(world != null)
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

    // Start is called before the first frame update
    void Start()
    {
        coalManager=FindObjectOfType<CoalManager>();
        soundManager=FindObjectOfType<SoundManager>();
        scoreManager=FindObjectOfType<ScoreManager>();
        breakableTiles=new BackgroundTile[width,height];
        lockTiles=new BackgroundTile[width,height];
        concreteTiles=new BackgroundTile[width,height];
        slimeTiles=new BackgroundTile[width,height];
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
    public void GenerateConcreteTiles()
    {
        for (int i = 0; i < boardLayot.Length; i++)
        {
            if (boardLayot[i].tileKind == TileKind.Concrete)
            {
                Vector2 tempPosition = new Vector2(boardLayot[i].x, boardLayot[i].y);
                GameObject tile = Instantiate(concreteTilePrefab, tempPosition, Quaternion.identity);
                concreteTiles[boardLayot[i].x, boardLayot[i].y] = tile.GetComponent<BackgroundTile>();

            }
        }
    }
    private void GenerateLockTiles()
    {
        for (int i = 0; i < boardLayot.Length; i++)
        {
            if (boardLayot[i].tileKind == TileKind.Lock)
            {
                Vector2 tempPosition = new Vector2(boardLayot[i].x, boardLayot[i].y);
                GameObject tile = Instantiate(lockTilePrefab, tempPosition, Quaternion.identity);
                lockTiles[boardLayot[i].x, boardLayot[i].y] = tile.GetComponent<BackgroundTile>();

            }
        }
    }

    private void GenerateSlimeTiles()
    {
        for (int i = 0; i < boardLayot.Length; i++)
        {
            if (boardLayot[i].tileKind == TileKind.Slime)
            {
                Vector2 tempPosition = new Vector2(boardLayot[i].x, boardLayot[i].y);
                GameObject tile = Instantiate(slimePiecePrefab, tempPosition, Quaternion.identity);
                slimeTiles[boardLayot[i].x, boardLayot[i].y] = tile.GetComponent<BackgroundTile>();

            }
            
        }
    }

    private void SetUp()//создаю доску и помещаю элементы
    {
        GenerateBlankSpaces();
        GenerateBreakableTiles();
        GenerateLockTiles();
        GenerateConcreteTiles();
        GenerateSlimeTiles();
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j] && !concreteTiles[i,j] && !slimeTiles[i,j])
                {


                    Vector2 tempPosition = new Vector2(i, j + offSet);//добавляем смещение, чтобы элементы плавно опускались на сцену
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

    private MatchType ColOrRow()
    {
        List<GameObject> matchCopy=findMatches.currentMatches as List<GameObject>;
        matchType.type = 0;
        matchType.color = "";
        for(int i=0; i < matchCopy.Count; i++)
        {
            Dot thisDot = matchCopy[i].GetComponent<Dot>();
            string color = matchCopy[i].tag;
            int col=thisDot.col;
            int row=thisDot.row;
            int colMatch = 0;
            int rowMatch = 0;

            for(int j=0; j < matchCopy.Count; j++)
            {
                Dot nextDot= matchCopy[j].GetComponent<Dot>();
                if (nextDot == thisDot)
                {
                    continue;
                }
                if (nextDot.col == thisDot.col && nextDot.tag==color)
                {
                    colMatch++;
                }

                if (nextDot.row == thisDot.row && nextDot.tag == color)
                {
                    rowMatch++;
                }
            }
            //return3 бомба столбца или строки
            //return2 бомба уничтожающая вокруг себя
            //return1 бомба уничтожающая одинаковые элементы

            if (colMatch == 4 || rowMatch == 4)
            {
                matchType.type = 1;
                matchType.color=color;
                return matchType;
            }
            else if(colMatch==2|| rowMatch == 2)
            {
                matchType.type = 2;
                matchType.color = color;
                return matchType;
            }
            else if(colMatch==3|| rowMatch == 3)
            {
                matchType.type = 3;
                matchType.color = color;
                return matchType;
            }
        }

        matchType.type = 0;
        matchType.color = "";
        return matchType;

    }
    private void CheckToMakeBombs()
    {
        if(findMatches.currentMatches.Count >=4)
        {
            MatchType typeOfMatch = ColOrRow();
            if (typeOfMatch.type == 1)
            {
                if (currentDot != null && currentDot.isMatched && currentDot.tag==typeOfMatch.color )
                {
                  
                        currentDot.isMatched = false;
                        currentDot.MakeColorBomb();
                }
                else
                {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched && otherDot.tag == typeOfMatch.color)
                            {
                                otherDot.isMatched = false;
                                otherDot.MakeColorBomb();
                            }
                        }
                }
                
            }
            else if(typeOfMatch.type == 2)
            {
                if (currentDot != null && currentDot.isMatched && currentDot.tag == typeOfMatch.color)
                {
                    currentDot.isMatched = false;
                    currentDot.MakeAdjecentBomb();
                }
                    else
                    {
                        if (currentDot.otherDot != null)
                        {
                            Dot otherDot = currentDot.otherDot.GetComponent<Dot>();
                            if (otherDot.isMatched && otherDot.tag==typeOfMatch.color)
                            {
                                otherDot.isMatched = false;
                                otherDot.MakeAdjecentBomb();
                               
                            }
                        }
                    }
                
            }
            else if (typeOfMatch.type == 3)
            {
                findMatches.CheckBombs(typeOfMatch);
            }
        }
        
    }

    public void BombRow(int row)
    {
        for(int i = 0;i<width;i++)
        {

            for (int j = 0; j < height; j++)
            {
                if (concreteTiles[i, j])
                {
                    concreteTiles[i, row].TakeDamage(1);
                    if (concreteTiles[i, row].hitPoints <= 0)
                    {
                        concreteTiles[i, row] = null;
                    }
                }
            }
        }
    }

    public void BombCol(int col)
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (concreteTiles[i,j])
                {
                    concreteTiles[col, i].TakeDamage(1);
                    if (concreteTiles[col, i].hitPoints <= 0)
                    {
                        concreteTiles[col, i] = null;
                    }
                }
            }
        }
    }

    private void OnDestroyMatchesAt(int col,int row)//метод удаляем элементы и присваевает на их место null 
    {
        if (allDots[col,row].GetComponent<Dot>().isMatched)
        {
           
           if (breakableTiles[col, row] != null)
            {
               
                breakableTiles[col, row].TakeDamage(1);//с какого раза белая плитка исчезнет
                if (breakableTiles[col, row].hitPoints <= 0)
                {
                    breakableTiles[col,row]= null;
                }
            }
            if (lockTiles[col, row] != null)
            {

                lockTiles[col, row].TakeDamage(1);//с какого раза белая плитка исчезнет
                if (lockTiles[col, row].hitPoints <= 0)
                {
                    lockTiles[col, row] = null;
                }
            }
            DamageConcrete(col,row);
            DamageSlime(col,row);
            if (coalManager != null)
            {
                coalManager.CompareCoal(allDots[col, row].tag.ToString());
                coalManager.UpdateGoals();
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

    private void DamageConcrete(int col,int row)
    {
        if (col > 0)
        {
            if (concreteTiles[col - 1, row])
            {
                concreteTiles[col-1,row].TakeDamage(1);
                if (concreteTiles[col-1, row].hitPoints <= 0)
                {
                    concreteTiles[col - 1, row] = null;
                }
            }
        }
        if (col < width-1)
        {
            if (concreteTiles[col + 1, row])
            {
                concreteTiles[col+1, row].TakeDamage(1);
                if (concreteTiles[col + 1, row].hitPoints <= 0)
                {
                    concreteTiles[col + 1, row] = null;
                }
            }
        }
        if (row > 0)
        {
            if (concreteTiles[col , row- 1])
            {
                concreteTiles[col, row-1].TakeDamage(1);
                if (concreteTiles[col , row- 1].hitPoints <= 0)
                {
                    concreteTiles[col, row - 1] = null;
                }
            }
        }
        if (row <height-1)
        {
            if (concreteTiles[col , row + 1])
            {
                concreteTiles[col, row+1].TakeDamage(1);
                if (concreteTiles[col , row + 1].hitPoints <= 0)
                {
                    concreteTiles[col , row + 1] = null;
                }
            }
        }
    }

    private void DamageSlime(int col, int row)
    {
        if (col > 0)
        {
            if (slimeTiles[col - 1, row])
            {
                slimeTiles[col - 1, row].TakeDamage(1);
                if (slimeTiles[col - 1, row].hitPoints <= 0)
                {
                    slimeTiles[col - 1, row] = null;
                }
                makeSlime = false;
            }
        }
        if (col < width - 1)
        {
            if (slimeTiles[col + 1, row])
            {
                slimeTiles[col + 1, row].TakeDamage(1);
                if (slimeTiles[col + 1, row].hitPoints <= 0)
                {
                    slimeTiles[col + 1, row] = null;
                }
                makeSlime = false;
            }
        }
        if (row > 0)
        {
            if (slimeTiles[col, row - 1])
            {
                slimeTiles[col, row - 1].TakeDamage(1);
                if (slimeTiles[col, row - 1].hitPoints <= 0)
                {
                    slimeTiles[col, row - 1] = null;
                }
                makeSlime = false;
            }
        }
        if (row < height - 1)
        {
            if (slimeTiles[col, row + 1])
            {
                slimeTiles[col, row + 1].TakeDamage(1);
                if (slimeTiles[col, row + 1].hitPoints <= 0)
                {
                    slimeTiles[col, row + 1] = null;
                }
                makeSlime = false;
            }
        }
    }

    private IEnumerator DecreaseRowCo2()
    {
        for(int i=0;i<width;i++) { 
             for(int j=0;j<height;j++)
            {
                if (!blankSpaces[i,j] && allDots[i,j] == null && !concreteTiles[i,j] && !slimeTiles[i,j])
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

    private IEnumerator DecreaseRowCo()// проходит по каждой ячейке в сетке и сдвигает элементы вниз, если под ними есть пустое место.
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
    public void DestroyMatches()//проверяется есть ли в ячейке игровой элмент и если есть, вызывается метод удаления.
    {
        
        if (findMatches.currentMatches.Count >= 4)
        {
            CheckToMakeBombs();
        }
        findMatches.currentMatches.Clear();
        for (int i=0; i < width; i++)
        {
            for (int j=0; j < height; j++)
            {
                if (allDots[i, j] != null)
                {
                    OnDestroyMatchesAt(i, j);
                }
            }
        }
      
        StartCoroutine(DecreaseRowCo2());
       
    }

   private void RefillBoard()//создает новые элементы случайные элементы
    {
        for(int i=0; i<width; i++)
        {
            for(int j=0; j<height; j++)
            {
                if (allDots[i, j]== null && !blankSpaces[i, j] && !concreteTiles[i,j]&& !slimeTiles[i,j])
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

    private bool MatchesOnBoard()//проверят есть ли совпадения
    {
        findMatches.FindAllMatche();
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

    private IEnumerator FillBoardCo()//вызывает метод заполнения, и в случае совпадений происходит удаление.
    {
        
        yield return new WaitForSeconds(refillDelay);
        RefillBoard();
        yield return new WaitForSeconds(refillDelay);
        while (MatchesOnBoard())
        { 
            streakValue ++;
            DestroyMatches();
            yield break;

        }

        currentDot = null;
        CheckToMakeSlime();       
        if (IsDeadlocked())
        {
            ShuffleBoard();
            
        } 
        yield return new WaitForSeconds(refillDelay);
        
        if(currentState!=GameState.pause)
            currentState = GameState.move;
        makeSlime = true;
        streakValue = 1;
    }


    private void CheckToMakeSlime()//проходимся по доске и если есть объект makeSlime=true вызываем метод MakeMewSlime
    {
        for(int i=0; i < width; i++)
        {
            for( int j=0; j < height; j++)
            {
                if (slimeTiles[i,j]!= null && makeSlime) {
                    MakeNewSlime();
                    return;
                }
            }
        }
    }

    private Vector2 CheckForAdjacent(int col,int row)//Проверяет ближайшие элементы к указанной ячейке по горизонтали и вертикали.
    {//Возвращает направление, в котором есть смежный элемент.
        if ( col < width - 1&&allDots[col+1,row] )
        {
            return Vector2.right;
        }
        if ( col >0&&allDots[col - 1, row] )
        {
            return Vector2.left;
        }
        if ( row < height - 1&&allDots[col , row+ 1] )
        {
            return Vector2.up;
        }
        if (  row < 0&&allDots[col , row- 1])
        {
            return Vector2.down;
        }
        return Vector2.zero;
    }

    private void MakeNewSlime()//Ищет случайную пустую ячейку для создания новой "слизи"
    {
        bool slime = false;
        int loops = 0;
        while(!slime && loops<200) {
            int newX=Random.Range(0,width);
            int newY=Random.Range(0,height);
            if (slimeTiles[newX, newY])
            {
                Vector2 adjacent=CheckForAdjacent(newX,newY);
                if(adjacent!=Vector2.zero)
                {
                    Destroy(allDots[newX+(int)adjacent.x,newY+(int)adjacent.y]);
                    Vector2 tempPosition = new Vector2(newX + (int)adjacent.x, newY + (int)adjacent.y);
                    GameObject tile=Instantiate(slimePiecePrefab,tempPosition,Quaternion.identity);
                    slimeTiles[newX + (int)adjacent.x, newY + (int)adjacent.y] = tile.GetComponent<BackgroundTile>();
                    slime = true;

                }
            }
            loops++;
        }
    }

    private void SwitchPieces(int col, int row, Vector2 direction)
    {// Меняет местами два элемента в массиве allDots в указанном направлении
        if (allDots[col + (int)direction.x, row + (int)direction.y] != null)
        {
            GameObject holder = allDots[col + (int)direction.x, row + (int)direction.y] as GameObject;
            allDots[col + (int)direction.x, row + (int)direction.y] = allDots[col, row];
            allDots[col, row] = holder;
        }
    }

    private bool CheckForMatches()
    {
        //Если обнаруживает комбинацию из трех одинаковых элементов по горизонтали или вертикали, возвращает true.
        for (int i = 0; i < width; i++)
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
    {//проверяет есть ли возможность делать ходы дальше
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

    private IEnumerator ShuffleBoard()//менем местами игровые элементы так, чтобы появились совпадения
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
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (!blankSpaces[i, j]&& !concreteTiles[i,j]&& !slimeTiles[i,j])
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
