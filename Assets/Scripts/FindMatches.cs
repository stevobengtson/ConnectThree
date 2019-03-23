using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FindMatches : MonoBehaviour
{
    private Board board;

    [Header("Debug")]
    public List<GameObject> currentMatches = new List<GameObject>();

    void Start()
    {
        board = FindObjectOfType<Board>();
    }

    public void FindAllMatches()
    {
        StartCoroutine(FindAllMatchesCo());
    }

    private IEnumerator FindAllMatchesCo()
    {
        yield return new WaitForSeconds(0.2f);
        for (int column = 0; column < board.width; column++)
        {
            for (int row = 0; row < board.height; row++)
            {
                GameObject currentDotObject = board.allDots[column, row];
                if (currentDotObject != null)
                {
                    Dot currentDot = currentDotObject.GetComponent<Dot>();
                    CheckHorizontalMatches(column, row, currentDot);
                    CheckVerticalMatches(column, row, currentDot);
                }
            }
        }
    }

    private void CheckHorizontalMatches(int column, int row, Dot currentDot)
    {
        if (column > 0 && column < board.width - 1)
        {
            GameObject leftDot = board.allDots[column - 1, row];
            GameObject rightDot = board.allDots[column + 1, row];
            if (leftDot != null && rightDot != null)
            {
                Dot leftDotComp = leftDot.GetComponent<Dot>();
                Dot rightDotComp = rightDot.GetComponent<Dot>();
                if (currentDot.dotType == leftDotComp.dotType && currentDot.dotType == rightDotComp.dotType)
                {
                    CheckHorizontalBombs(currentDot, leftDotComp, rightDotComp, row, column);
                    MatchAndTrackDots(new Dot[] { leftDotComp, rightDotComp, currentDot });
                }
            }

        }
    }

    private void CheckVerticalMatches(int column, int row, Dot currentDot)
    {
        if (row > 0 && row < board.height - 1)
        {
            GameObject upDot = board.allDots[column, row + 1];
            GameObject downDot = board.allDots[column, row - 1];
            if (upDot != null && downDot != null)
            {
                Dot upDotComp = upDot.GetComponent<Dot>();
                Dot downDotComp = downDot.GetComponent<Dot>();
                if (currentDot.dotType == upDotComp.dotType && currentDot.dotType == downDotComp.dotType)
                {
                    CheckHorizontalBombs(currentDot, upDotComp, downDotComp, row, column);
                    MatchAndTrackDots(new Dot[] { upDotComp, downDotComp, currentDot });
                }
            }

        }
    }

    private void MatchAndTrackDots(Dot[] dots)
    {
        for (int dotIndex = 0; dotIndex < dots.Length; dotIndex++)
        {
            MatchAndTrack(dots[dotIndex]);
        }
    }

    private void MatchAndTrack(Dot dot)
    {
        if (!currentMatches.Contains(dot.gameObject))
        {
            currentMatches.Add(dot.gameObject);
        }
        dot.isMatched = true;
    }

    private void CheckHorizontalBombs(Dot currentDot, Dot leftDot, Dot rightDot, int row, int column)
    {
        if (currentDot.isRowBomb || leftDot.isRowBomb || rightDot.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(row));
        }

        if (currentDot.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(column));
        }

        if (leftDot.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(column - 1));
        }

        if (rightDot.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(column + 1));
        }
    }

    private void CheckVerticalBombs(Dot currentDot, Dot upDot, Dot downDot, int row, int column)
    {
        if (currentDot.isColumnBomb || upDot.isColumnBomb || downDot.isColumnBomb)
        {
            currentMatches.Union(GetColumnPieces(column));
        }

        if (currentDot.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(row));
        }

        if (upDot.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(row + 1));
        }

        if (downDot.isRowBomb)
        {
            currentMatches.Union(GetRowPieces(row - 1));
        }
    }

    private List<GameObject> GetColumnPieces(int column)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int row = 0; row < board.height; row++)
        {
            AddPiece(column, row, dots);
        }

        return dots;
    }

    private List<GameObject> GetRowPieces(int row)
    {
        List<GameObject> dots = new List<GameObject>();
        for (int column = 0; column < board.width; column++)
        {
            AddPiece(column, row, dots);
        }

        return dots;
    }

    private void AddPiece(int column, int row, List<GameObject> dots)
    {
        if (board.allDots[column, row] != null)
        {
            dots.Add(board.allDots[column, row]);
            board.allDots[column, row].GetComponent<Dot>().isMatched = true;
        }
    }
}
