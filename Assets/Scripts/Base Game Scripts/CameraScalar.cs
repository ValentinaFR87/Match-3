using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    private Board board;
    public float CameraOffset;
    public float aspectRatio = 0.987f;
    public float padding = 0.5f;
    public float yOffset = 1;
    // Start is called before the first frame update
    void Start()
    {
        board=FindObjectOfType<Board>();
        if(board != null)
        {
            RepositionCamera(board.width-1,board.height-1 );
        }
    }

    private void RepositionCamera(float x, float y)
    {
        Vector3 tempPosition=new Vector3(x/2,y/2+yOffset, CameraOffset);
        transform.position = tempPosition;
        if (board.width >= board.height)
        {
            Camera.main.orthographicSize = (board.width / 2 + padding) / aspectRatio;
        }
        else
        {
            Camera.main.orthographicSize = board.height / 2 + padding;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
