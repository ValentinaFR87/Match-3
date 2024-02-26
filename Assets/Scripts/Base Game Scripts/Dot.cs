using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Dot : MonoBehaviour
{
    public int col;
    public int row;
    public int prewiousCol;
    public int prewiousRow;
    public int targetX;
    public int targetY;
    public bool isMatched = false;
    public GameObject otherDot;


    private EndGameManager endGameManager;
    private HintManager hintManager;
    private FindMatches findMatches;
    private Board board;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 tempPosition;

    [Header("Swipe Stuff")]
    public float swipeAngle = 0;
    public float swipeResist = 1f;

    [Header("Powerup stuff")]
    public bool isColorBomb;
    public bool isColBomb;
    public bool isRowBomb;
    public bool isAdjacentBomb;
    public GameObject AdjacentMarker;
    public GameObject rowArrow;
    public GameObject colArrow;
    public GameObject colorBomb;

    //��� ����������
    private void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(1))
        {
            isAdjacentBomb = true;
            GameObject marker= Instantiate(AdjacentMarker, transform.position,Quaternion.identity);
            marker.transform.parent = this.transform;
           
        }
    }

    // Start is called before the first frame update
    void Start()//�� ����� ������ ��������� ������� �����
    {   

        isColBomb=false;
        isRowBomb=false;
        isColorBomb=false;
        isAdjacentBomb=false;

        endGameManager=FindObjectOfType<EndGameManager>();
        hintManager=FindAnyObjectByType<HintManager>();
        board=FindObjectOfType<Board>();
        findMatches= FindObjectOfType<FindMatches>();
        //targetX=(int)transform.position.x;
        //targetY = (int)transform.position.y;
        //row = targetY;
        //col= targetX;
        //prewiousCol = col;
        //prewiousRow = row;
    }

    // Update is called once per frame
    void Update()//���������� ������ ���� � �������� �� ����������� ��������� �� �������� ����.
    {
       // FindMatches();
       /* if(isMatched)//���� ���� ���������� �������� ������ �� �����������
        {
            SpriteRenderer mySprite = GetComponent<SpriteRenderer>();
            mySprite.color=new Color(1f,1f,1f,.2f);
        }
       */
        targetX = col;
        targetY = row;
        if(Mathf.Abs(targetX-transform.position.x) > .1)//������� ����������� ��������� � ������� �������
        {
            
            tempPosition=new Vector2(targetX,transform.position.y);
            transform.position=Vector2.Lerp(transform.position, tempPosition, .2f);
            if (board.allDots[col,row]!=this.gameObject)
            {
                board.allDots[col,row] = this.gameObject;
            }
            findMatches.FindAllMatche();//����� ����������� ��������� ����� ������� �����������
        }
        else
        {
            
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position=tempPosition;
        }
        if (Mathf.Abs(targetY - transform.position.y) > .1)//������� ����������� ��������� � ������� �������
        {
           
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .2f);
            if (board.allDots[col, row] != this.gameObject)
            {
                board.allDots[col, row] = this.gameObject;
            }
            findMatches.FindAllMatche();//����� ����������� ��������� ����� ������� �����������
        }
        else
        {
            
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = tempPosition;
        }
    }

    public IEnumerator CheckMoveCo()//���� ����� ������ ��������� �� �� ����� 
    {//���� �� �������� ������� �� ������� ��
       if(isColorBomb)
        {
            findMatches.MatchPiecesOfColor(otherDot.tag);
            isMatched = true;
        }else if(otherDot.GetComponent<Dot>().isColorBomb)
        {
            findMatches.MatchPiecesOfColor(this.gameObject.tag);
            otherDot.GetComponent<Dot>().isMatched = true;
        }
        yield return new WaitForSeconds(.5f);//����������� � ��������� � ���������, ���� �� ����������� ���������
        if (otherDot !=null )
        {
            if(!isMatched && !otherDot.GetComponent<Dot>().isMatched )
            {//���� �������� �� ��������� � �� �������� ����������, �� ��� ������������ �� ���� ���������� �������
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().col= col;
                row = prewiousRow;
                col=prewiousCol;
                yield return new WaitForSeconds(.5f);
                board.currentDot = null;
                board.currentState = GameState.move;
            }
            else//���� �������� ���������, �� ��� ��������� � �������� ����.
            {
                if (endGameManager != null)
                {
                    if (endGameManager.requirements.gameType == GameType.Moves)
                    {
                        endGameManager.DecreaseCounterValue();
                    }
                }
                board.DestroyMatches();
               
            }
            //oftherDot = null;
        }
    }

    private void OnMouseDown()//��� ������� ���� ������������ ��������� ������� �������
    { 
        if (hintManager != null)//����������� ��������� ����� ������� ��������
        {
            hintManager.DestroyHint();
        }
        if (board.currentState == GameState.move)//���� move ����� ����������� ��������
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //Debug.Log(firstTouchPosition);
        }
    }

    private void OnMouseUp()//��� ���������� ���� ����������� ���� � ������� ������ CalculateAngle();, �� ������� ���� ������� ����
    {
        if (board.currentState == GameState.move)//���� move ����� ����������� ��������
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    void CalculateAngle()//�������� ���� � ��������
    {
        if (Mathf.Abs(finalTouchPosition.y - firstTouchPosition.y) > swipeResist || Mathf.Abs(finalTouchPosition.x - firstTouchPosition.x) > swipeResist)
        {
            board.currentState = GameState.wait;//�� ����� wait �������� �������� �� ����� ������.
            swipeAngle = Mathf.Atan2(finalTouchPosition.y - firstTouchPosition.y, finalTouchPosition.x - firstTouchPosition.x) * 180 / Mathf.PI;
            MovePieces();
            board.currentDot = this;
        }
        else
        {
            board.currentState = GameState.move;//���� move ����� ����������� ��������
            
        }
    }

    void MovePiecesActual(Vector2 direction)
    {
        otherDot = board.allDots[col + (int)direction.x, row+(int)direction.y];
        prewiousCol = col;
        prewiousRow = row;
        if (otherDot != null)
        {
            otherDot.GetComponent<Dot>().col += -1 * (int)direction.x;
            otherDot.GetComponent<Dot>().row += -1 * (int)direction.y;
            col += (int)direction.x;
            row += (int)direction.y;
            StartCoroutine(CheckMoveCo());
        }
        else
        {
            board.currentState=GameState.move;
        }
    }

    void MovePieces()//� ����������� �� ���� �������� � ����� ������� ������������� �������
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && col < board.width - 1)//����� ������
        {
            /* otherDot = board.allDots[col+1,row];
             prewiousCol = col;
             prewiousRow = row;
             otherDot.GetComponent<Dot>().col -= 1;
             col += 1;
             StartCoroutine(CheckMoveCo());
            */
            MovePiecesActual(Vector2.right);
        }
        else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)//����� �����
        {
            /*  otherDot = board.allDots[col, row+1];
              prewiousCol = col;
              prewiousRow = row;
              otherDot.GetComponent<Dot>().row -= 1;
              row += 1;
              StartCoroutine(CheckMoveCo());
            */
            MovePiecesActual(Vector2.up);
        }
        else if ((swipeAngle > 135 || swipeAngle <= -135) && col > 0)//����� �����
        {

            /* 
             otherDot = board.allDots[col - 1, row];
            prewiousCol = col;
            prewiousRow = row;
            otherDot.GetComponent<Dot>().col += 1;
            col -= 1;
            StartCoroutine(CheckMoveCo());
            */
            MovePiecesActual(Vector2.left);
        }
        else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)//����� ����
        {
            /*
            otherDot = board.allDots[col, row-1];
            prewiousCol = col;
            prewiousRow = row;
            otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
            StartCoroutine(CheckMoveCo());
            */
            MovePiecesActual(Vector2.down);
        }
        else
        {

            board.currentState = GameState.move;

        }
        
        
    }

    void FindMatches()//���� ���������� �������� 
    {
        if(col>0 && col < board.width - 1)//���������� �������� �� �����������
        {
            GameObject leftDot1 = board.allDots[col-1, row];
            GameObject rightDot1 = board.allDots[col + 1, row];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (leftDot1.tag == this.gameObject.tag && rightDot1.tag == this.gameObject.tag)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
        if (row > 0 && row < board.height - 1)//��������� �������� �� ���������
        {
            GameObject upDot1 = board.allDots[col , row+1];
            GameObject downDot1 = board.allDots[col, row-1];
            if (upDot1 != null && downDot1 != null)
            {
                if (upDot1.tag == this.gameObject.tag && downDot1.tag == this.gameObject.tag)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }

    public void MakeRowBomb()
    {
        isRowBomb = true;
        GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColBomb()
    {
        isColBomb = true;
        GameObject arrow = Instantiate(colArrow, transform.position, Quaternion.identity);
        arrow.transform.parent = this.transform;
    }

    public void MakeColorBomb()
    {
        isColorBomb = true;
        GameObject color = Instantiate(colorBomb, transform.position, Quaternion.identity);
        color.transform.parent = this.transform;
        this.gameObject.tag = "Color";
    }

    public void MakeAdjecentBomb()
    {
        isAdjacentBomb = true;
        GameObject marker = Instantiate(AdjacentMarker, transform.position, Quaternion.identity);
        marker.transform.parent = this.transform;
    }
}
