using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dot : MonoBehaviour
{
    [Header("Dot attributes")]
    public string dotType;

    [Header("Board Variables")]
    public int column;
    public int row;
    public int previousColumn;
    public int previousRow;
    public int targetColumn;
    public int targetRow;
    public bool isMatched = false;

    [Header("Swipe Variables")]
    public float swipeAngle = 0f;
    public float swipeResist = 1f;

    [Header("Powerups")]
    public bool isColumnBomb;
    public bool isRowBomb;
    public GameObject rowArrow;
    public GameObject columnArrow;

    private GameObject otherDot;
    private Vector2 firstTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2 targetPosition;
    private Board board;
    private SpriteRenderer mySprite;
    private FindMatches findMatches;

    private readonly float RADTODEG = (180 / Mathf.PI);

    void Start()
    {
        isColumnBomb = false;
        isRowBomb = false;

        mySprite = GetComponent<SpriteRenderer>();
        board = FindObjectOfType<Board>();
        findMatches = FindObjectOfType<FindMatches>();
    }

    // DEBUG
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                isRowBomb = true;
                GameObject arrow = Instantiate(rowArrow, transform.position, Quaternion.identity);
                arrow.transform.parent = transform;
            }
            else
            {
                isColumnBomb = true;
                GameObject arrow = Instantiate(columnArrow, transform.position, Quaternion.identity);
                arrow.transform.parent = transform;
            }
        }
    }

    void Update()
    {
        if (isMatched)
        {
            mySprite.color = new Color(1f, 1f, 1f, 0.2f);
        }


        targetColumn = column;
        targetRow = row;

        if (Mathf.Abs(targetColumn - transform.position.x) > 0.1f)
        {
            targetPosition = new Vector2(targetColumn, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, targetPosition, 0.6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        } else
        {
            targetPosition = new Vector2(targetColumn, transform.position.y);
            transform.position = targetPosition;
        }

        if (Mathf.Abs(targetRow - transform.position.y) > 0.1f)
        {
            targetPosition = new Vector2(transform.position.x, targetRow);
            transform.position = Vector2.Lerp(transform.position, targetPosition, 0.6f);
            if (board.allDots[column, row] != this.gameObject)
            {
                board.allDots[column, row] = this.gameObject;
            }
            findMatches.FindAllMatches();
        }
        else
        {
            targetPosition = new Vector2(transform.position.x, targetRow);
            transform.position = targetPosition;
        }
    }

    public IEnumerator CheckMoveCo()
    {
        yield return new WaitForSeconds(0.5f);
        if (otherDot != null)
        {
            if(!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().row = row;
                otherDot.GetComponent<Dot>().column = column;
                row = previousRow;
                column = previousColumn;
                yield return new WaitForSeconds(0.5f);
                board.currentState = GameState.move;
            }
            else
            {
                board.DestroyMatches();
            }
            otherDot = null;
        }
    }

    private void OnMouseDown()
    {
        if (board.currentState == GameState.move)
        {
            firstTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void OnMouseUp()
    {
        if (board.currentState == GameState.move)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            CalculateAngle();
        }
    }

    private void CalculateAngle()
    {
        float x = finalTouchPosition.x - firstTouchPosition.x;
        float y = finalTouchPosition.y - firstTouchPosition.y;

        if (Mathf.Abs(y) > swipeResist || Mathf.Abs(x) > swipeResist)
        {
            board.currentState = GameState.wait;
            //Vector2 direction = (finalTouchPosition - firstTouchPosition).normalized;
            //Debug.DrawLine(firstTouchPosition, firstTouchPosition + direction * 2, Color.red, 25.0f);
            swipeAngle = Mathf.Atan2(y, x) * RADTODEG;
            MovePieces();
        }
        else
        {
            board.currentState = GameState.move;
        }
    }

    private void MovePieces()
    {
        if (swipeAngle > -45 && swipeAngle <= 45 && column < board.width - 1)
        {
            // Right Swipe
            otherDot = board.allDots[column + 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column -= 1;
            column += 1;
        } else if (swipeAngle > 45 && swipeAngle <= 135 && row < board.height - 1)
        {
            // Up Swipe
            otherDot = board.allDots[column, row + 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row -= 1;
            row += 1;
        } else if ((swipeAngle > 135 || swipeAngle <= -135) && column > 0)
        {
            // Left Swipe
            otherDot = board.allDots[column - 1, row];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().column += 1;
            column -= 1;
        } else if (swipeAngle < -45 && swipeAngle >= -135 && row > 0)
        {
            // Down Swipe
            otherDot = board.allDots[column, row - 1];
            previousRow = row;
            previousColumn = column;
            otherDot.GetComponent<Dot>().row += 1;
            row -= 1;
        }
        StartCoroutine(CheckMoveCo());
    }

    private void FindMatches()
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot1 = board.allDots[column - 1, row];
            GameObject rightDot1 = board.allDots[column + 1, row];
            if (leftDot1 != null && rightDot1 != null)
            {
                if (dotType == leftDot1.GetComponent<Dot>().dotType && dotType == rightDot1.GetComponent<Dot>().dotType)
                {
                    leftDot1.GetComponent<Dot>().isMatched = true;
                    rightDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }

        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot1 = board.allDots[column, row + 1];
            GameObject downDot1 = board.allDots[column, row - 1];
            if (upDot1 != null && downDot1 != null)
            {
                if (dotType == upDot1.GetComponent<Dot>().dotType && dotType == downDot1.GetComponent<Dot>().dotType)
                {
                    upDot1.GetComponent<Dot>().isMatched = true;
                    downDot1.GetComponent<Dot>().isMatched = true;
                    isMatched = true;
                }
            }
        }
    }
}
