using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindMatches : MonoBehaviour//для определения конкретного колиества совпадений, понадобиться в дальнейшей разработке
{

    private Board board;
    public List<GameObject> currentMatches = new List<GameObject>();//создаю список который будет хранить совпадения
    // Start is called before the first frame update
    void Start()//инициализирует переменную board, которая хранит ссылку на компонент Board, найденный на сцене.
    {//Этот компонент отвечает за игровую доску и содержит информацию о расположении элементов на ней.
        board =FindObjectOfType<Board>();
    }

    public void FindAllMatche()//запускаем метод из скрипта Board,который осуществляет поиск всех одинаковых элементов на игровой доске.
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private List<GameObject> IsAdjacentBomb(Dot dot1, Dot dot2, Dot dot3)//проверяет является ли элементы бомбой которая уничтожает по 6-9 элементов
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot1.col,dot1.row));
        }
        if (dot2.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot2.col, dot2.row));
        }
        if (dot3.isAdjacentBomb)
        {
            currentMatches.Union(GetAdjacentPieces(dot3.col, dot3.row));
        }
        return currentDots;
    }

    private List<GameObject> IsRowBomb(Dot dot1, Dot dot2,Dot dot3)//проверяет является ли элементы бомбой строки
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot1.row));
            board.BombRow(dot1.row);
        }
        if (dot2.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot2.row));
            board.BombRow(dot2.row);
        }
        if (dot3.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(dot3.row));
            board.BombRow(dot3.row);
        }
        return currentDots;
    }

    private List<GameObject> IsColBomb(Dot dot1, Dot dot2, Dot dot3)//проверяет является ли элементы бомбой столбца
    {
        List<GameObject> currentDots = new List<GameObject>();
        if (dot1.isColBomb)
        {
            currentMatches.Union(GetColPieces(dot1.col));
            board.BombCol(dot1.col);
        }
        if (dot2.isColBomb)
        {
            currentMatches.Union(GetColPieces(dot2.col));
            board.BombCol(dot2.col);
        }
        if (dot3.isColBomb)
        {
            currentMatches.Union(GetColPieces(dot3.col));
            board.BombCol(dot3.col);
        }
        return currentDots;
    }

    private void AddToListAndMatch(GameObject dot)//Если найдены три одинаковых элемента в ряд, они добавляются в список
    {
        if (!currentMatches.Contains(dot))
        {
            currentMatches.Add(dot);
        }
        dot.GetComponent<Dot>().isMatched = true;
    }

    private void GetNearbyPieces(GameObject dot1, GameObject dot2, GameObject dot3)//добавляем элементы в список currentMatches
    {
        AddToListAndMatch(dot1);
        AddToListAndMatch(dot2);
        AddToListAndMatch(dot3);
       
    }

    private IEnumerator FindAllMatchesCo()
    {
        //yield return new WaitForSeconds(.2f);//задержка на 0.2 секунды
        
        for (int i = 0; i < board.width; i++)
        {
            for(int j = 0; j < board.height; j++)
            {
                GameObject currentDot = board.allDots[i,j];
               
                if(currentDot != null)
                {
                    Dot currentDotDot = currentDot.GetComponent<Dot>();
                    if (i>0 && i < board.width - 1)//проверяю наличие элементов по горизонтали
                    {
                        GameObject leftDot = board.allDots[i-1,j];//элемент слева
                        GameObject rightDot = board.allDots[i + 1, j];//элемент справа
                        if (leftDot != null && rightDot != null)
                        {
                            Dot leftDotDot = leftDot.GetComponent<Dot>();
                            Dot rightDotDot = rightDot.GetComponent<Dot>();
                            if (leftDot != null && rightDot != null)
                            {
                                if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                                {
                                    currentMatches.Union(IsRowBomb(leftDotDot, currentDotDot, rightDotDot));
                                    currentMatches.Union(IsColBomb(leftDotDot, currentDotDot, rightDotDot));
                                    currentMatches.Union(IsAdjacentBomb(leftDotDot, currentDotDot, rightDotDot));
                                    GetNearbyPieces(leftDot, currentDot, rightDot);

                                }
                            }
                        }
                    }
                    if (j > 0 && j < board.height - 1)//наличие элементов по вертикали
                    {
                        GameObject upDot = board.allDots[i , j + 1];//элемент сверху
                        GameObject downDot = board.allDots[i, j - 1];//элемент снизу
                        if (upDot != null && downDot != null)
                        {
                            Dot upDotDot = upDot.GetComponent<Dot>();
                            Dot downDotDot = downDot.GetComponent<Dot>();
                            if (upDot != null && downDot != null)
                            {
                                if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                                {
                                    currentMatches.Union(IsColBomb(upDotDot, currentDotDot, downDotDot));
                                    currentMatches.Union(IsRowBomb(upDotDot, currentDotDot, downDotDot));
                                    currentMatches.Union(IsAdjacentBomb(upDotDot, currentDotDot, downDotDot));
                                    GetNearbyPieces(upDot, currentDot, downDot);
                                }
                            }
                        }
                    }
                }
            }
        }
       yield return null;
        
    }

    List<GameObject> GetAdjacentPieces(int col,int row)//возвращает список GameObject, содержащий смежные элементы игрового поля по указанным координатам col и row
    {
        List<GameObject> dots = new List<GameObject>();
        for(int i=col-1;i<=col+1;i++)
        {
            for(int j=row-1; j<=row+1;j++) { 
            //находиться ли элемент внутри доски
            if(i>=0 && i<board.width&&j>=0 && j<board.height)
                {
                    if (board.allDots[i, j] != null)
                    {
                        dots.Add(board.allDots[i, j]);
                        board.allDots[i, j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }
        return dots;
    }

    List<GameObject> GetColPieces(int col)//метод возвращает список содержащий элементы в указанном столбце col.
    {
        List<GameObject> dots = new List<GameObject>();
        for(int i=0;i<board.height;i++)
        {
            if (board.allDots[col, i] != null)
            {
                Dot dot = board.allDots[col,i].GetComponent<Dot>();
                if (dot.isRowBomb)
                {
                    dots.Union(GetRowPieces(i)).ToList();
                }
                dots.Add(board.allDots[col, i]);
                dot.isMatched = true;
            }
        }
        return dots;
    }

    public void MatchPiecesOfColor(string color)//помечает все элементы на игровом поле с тегом, соответствующим указанному цвету
    {
        for(int i=0;i<board.width;i++)
        {
            for(int j=0;j<board.height;j++)
            {
               
                if (board.allDots[i,j] != null)
                {
                    if (board.allDots[i,j].tag==color) {
                        board.allDots[i,j].GetComponent<Dot>().isMatched = true;
                    }
                }
            }
        }  
    }
    List<GameObject> GetRowPieces(int row)//метод возвращает список содержащий элементы в указанном столбце row
    {
        List<GameObject> dots = new List<GameObject>();
        for (int i = 0; i < board.width; i++)
        {
            if (board.allDots[ i,row] != null)
            {
                Dot dot = board.allDots[ i,row].GetComponent<Dot>();
                if (dot.isColBomb)
                {
                    dots.Union(GetColPieces(i)).ToList();
                }
                dots.Add(board.allDots[ i,row]);
                dot.isMatched = true;
            }
        }
        return dots;
    }
    public void CheckBombs(MatchType matchType)//проверям бомбым row and col и создаем их 
    {
        
        if(board.currentDot!= null)
        {
            if (board.currentDot.isMatched && board.currentDot.tag==matchType.color)
            {
                
                board.currentDot.isMatched= false;

                if((board.currentDot.swipeAngle>-45 && board.currentDot.swipeAngle<=45)
                    || board.currentDot.swipeAngle < -135 && board.currentDot.swipeAngle >= 135)
                {
                    board.currentDot.MakeColBomb();
                }
                else
                {
                    
                    board.currentDot.MakeRowBomb();
                }
            }
            
            else if(board.currentDot.otherDot!=null) {
                Dot otherDot = board.currentDot.otherDot.GetComponent<Dot>();
                
                if (otherDot.isMatched && otherDot.tag==matchType.color) { 
                    
                    otherDot.isMatched = false;
                   
                    if ((board.currentDot.swipeAngle > -45 && board.currentDot.swipeAngle <= 45)
                   || board.currentDot.swipeAngle < -135 && board.currentDot.swipeAngle >= 135)
                    {
                        
                        otherDot.MakeRowBomb();
                    }
                    else
                    {
                        
                       otherDot.MakeColBomb();
                    }
                }
            }
        }
    }
  
}
