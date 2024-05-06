using System;
using System.Collections.Generic;
using System.Drawing;

namespace CSCommon;

public class OmokRule
{
    public enum StoneType { None, Black, White };

    const int BoardSize = 19;
    int[,] GameBoard = new int[BoardSize, BoardSize];
    public bool BlackPlayerTurn { get; private set; } = true;

    public bool GameFinish { get; private set; } = true;

    public int CurTuenCount { get; private set; } = 0;


    public int PrevXPos { get; private set; } = -1;
    public int PrevYPos { get; private set; } = -1;

    public int CurrentXPos { get; private set; } = -1;
    public int CurrentYPos { get; private set; } = -1;

    private Stack<Point> st = new Stack<Point>();

    public void StartGame()
    {
        Array.Clear(GameBoard, 0, BoardSize * BoardSize);
        PrevXPos = PrevYPos = -1;
        CurrentXPos = CurrentYPos = -1;
        BlackPlayerTurn = true;
        CurTuenCount = 1;
        GameFinish = false;

        st.Clear();
    }

    public void EndGame()
    {
        GameFinish = true;
    }

    public int GetStoneByPos(int x, int y)
    {
        return GameBoard[x, y];
    }

    public bool IsBlackTurn()
    {
        return ((CurTuenCount % 2) == 1);
    }

    public PutStoneResult PutStone(int x, int y)
    {
        if (BlackPlayerTurn)
        {   // 검은 돌
            GameBoard[x, y] = (int)StoneType.Black;
        }
        else
        {
            // 흰 돌
            GameBoard[x, y] = (int)StoneType.White;
        }


        PrevXPos = CurrentXPos;
        PrevYPos = CurrentYPos;

        CurrentXPos = x;
        CurrentYPos = y;

        BlackPlayerTurn = !BlackPlayerTurn;                   // 차례 변경

        ++CurTuenCount;
        st.Push(new Point(x, y));

        return PutStoneResult.Success;
    }

    public void WinAndFinishGame(bool Result)
    {
        //승리효과음.Play();
        //MessageBox.Show((돌종류)바둑판[x, y] + " 승");
        GameFinish = true;
    }

}
