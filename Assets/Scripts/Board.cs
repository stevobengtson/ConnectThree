using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    wait,
    move
}

public class Board : MonoBehaviour
{
    [Header("Board Settings")]
    public int width;
    public int height;
    public int offset;
    public GameObject tilePrefab;
    public GameState currentState = GameState.move;

    [Header("Dot Settings")]
    public GameObject[] dots;
    public GameObject[,] allDots;
    public GameObject destroyEffect;

    private GameObject[,] allTiles;
    private FindMatches findMatches;

    void Start()
    {
        allTiles = new GameObject[width, height];
        allDots = new GameObject[width, height];
        findMatches = FindObjectOfType<FindMatches>();
        Setup();
    }

    private void Setup()
    {
        InitBoard();
    }

    public void InitBoard()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allDots[column, row] != null)
                {
                    Destroy(allDots[column, row]);
                    allDots[column, row] = null;
                }
                GameObject backGroundTile = CreateBackgroundTile(column, row);
                CreateDot(column, row, false);
            }
        }
    }

    private GameObject CreateBackgroundTile(int column, int row)
    {
        Vector2 tempPosition = new Vector2(column, row);
        GameObject backgroundTile = Instantiate(tilePrefab, tempPosition, Quaternion.identity) as GameObject;
        backgroundTile.transform.parent = transform;
        backgroundTile.name = "(" + column + ", " + row + ")";
        allTiles[column, row] = backgroundTile;
        return backgroundTile;
    }

    private GameObject CreateDot(int column, int row, bool canMatch = true)
    {
        GameObject backGroundTile = allTiles[column, row];
        int dotToUse = PickDot(column, row, backGroundTile, canMatch);
        Vector2 startPosition = new Vector2(column, row + offset);
        GameObject dot = Instantiate(dots[dotToUse], startPosition, Quaternion.identity);
        dot.transform.parent = backGroundTile.transform;
        dot.GetComponent<Dot>().column = column;
        dot.GetComponent<Dot>().row = row;
        dot.name = "Dot (" + column + ", " + row + ")";
        allDots[column, row] = dot;
        return dot;
    }

    private int PickDot(int column, int row, GameObject backGroundTile, bool canMatch = true)
    {
        int dotToUse = Random.Range(0, dots.Length);
        if (!canMatch)
        {
            int maxIterations = 0;
            while (MatchesAt(column, row, dots[dotToUse]) && maxIterations < 100)
            {
                dotToUse = Random.Range(0, dots.Length);
                maxIterations++;
            }
        }
        return dotToUse;
    }

    private bool MatchesAt(int column, int row, GameObject piece)
    {
        Dot pieceDotComponent = piece.GetComponent<Dot>();
        if (column > 1 && row > 1)
        {
            if (MatchAt(column - 1, row, pieceDotComponent) && MatchAt(column - 2, row, pieceDotComponent))
            {
                return true;
            }

            if (MatchAt(column, row - 1, pieceDotComponent) && MatchAt(column, row - 2, pieceDotComponent))
            {
                return true;
            }
        }
        else if (column <= 1 || row <= 1)
        {
            if (row > 1)
            {
                if (MatchAt(column, row - 1, pieceDotComponent) && MatchAt(column, row - 2, pieceDotComponent))
                {
                    return true;
                }
            }
            if (column > 1)
            {
                if (MatchAt(column - 1, row, pieceDotComponent) && MatchAt(column - 2, row, pieceDotComponent))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool MatchAt(int column, int row, Dot pieceDotComponent)
    {
        return allDots[column, row].GetComponent<Dot>().dotType == pieceDotComponent.dotType;
    }

    private void DestroyMatchesAt(int column, int row)
    {
        if (allDots[column, row].GetComponent<Dot>().isMatched)
        {
            findMatches.currentMatches.Remove(allDots[column, row]);
            GameObject particle = Instantiate(destroyEffect, allDots[column, row].transform.position, Quaternion.identity);
            Destroy(particle, 0.5f);
            Destroy(allDots[column, row]);
            allDots[column, row] = null;
        }
    }

    public void DestroyMatches()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allDots[column, row] != null)
                {
                    DestroyMatchesAt(column, row);
                }
            }
        }

        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        int nullCount = 0;
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allDots[column, row] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[column, row].GetComponent<Dot>().row -= nullCount;
                    allDots[column, row] = null;
                }
            }
            nullCount = 0;
        }

        yield return new WaitForSeconds(0.4f);
        StartCoroutine(FillBoardCo());
    }

    private void RefillBoard()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allDots[column, row] == null)
                {
                    GameObject dot = CreateDot(column, row);
                    dot.GetComponent<Dot>().row = row;
                    dot.GetComponent<Dot>().column = column;
                }
            }
        }
    }

    private bool MatchesOnBoard()
    {
        for (int column = 0; column < width; column++)
        {
            for (int row = 0; row < height; row++)
            {
                if (allDots[column, row] != null)
                {
                    if (allDots[column, row].GetComponent<Dot>().isMatched)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private IEnumerator FillBoardCo()
    {
        RefillBoard();
        yield return new WaitForSeconds(0.5f);
 
        while (MatchesOnBoard())
        {
            yield return new WaitForSeconds(0.5f);
            DestroyMatches();
        }
        yield return new WaitForSeconds(0.5f);

        currentState = GameState.move;
    }
}