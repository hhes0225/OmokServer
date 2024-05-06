using System;
using System.Collections.Generic;
using System.Drawing;

namespace CSBaseLib;

public class OmokRule
{
    string BlackPlayerID="";
    string WhitePlayerID="";

    public enum StoneType { None, Black, White };

    const int BoardSize = 19;

    int[,] GameBoard = new int[BoardSize, BoardSize];
    public bool BlackPlayerTurn { get; private set; } = true;

    public bool GameFinish { get; private set; } = true;

    public int CurTurnCount { get; private set; } = 0;


    public int PrevXPos { get; private set; } = -1;
    public int PrevYPos { get; private set; } = -1;

    public int CurrentXPos { get; private set; } = -1;
    public int CurrentYPos { get; private set; } = -1;

    private Stack<Point> st = new Stack<Point>();

    public void SetPlayerColor(string blackPlayerID, string whitePlayerID)
    {
        BlackPlayerID = blackPlayerID;
        WhitePlayerID = whitePlayerID;
    }

    public void StartGame()
    {
        Array.Clear(GameBoard, 0, BoardSize * BoardSize);
        PrevXPos = PrevYPos = -1;
        CurrentXPos = CurrentYPos = -1;
        BlackPlayerTurn = true;
        CurTurnCount = 1;
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

    public string WhoseTurn()
    {
        if((CurTurnCount % 2) == 1)
        {
            return BlackPlayerID;
        }
        return WhitePlayerID;
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

        ++CurTurnCount;
        st.Push(new Point(x, y));

        return PutStoneResult.Success;
    }

    public bool CheckAvailablePosition(int x, int y)
    {
        if (x < 0 || x >= 19 || y < 0 || y >= 19)
        {
            return false; // 범위를 벗어난 경우
        }

        return GameBoard[x, y] == (int)StoneType.None;
    }


    public void Undo()
    {
        st.Pop();
        GameBoard[CurrentXPos, CurrentYPos] = (int)StoneType.None;

        if (st.Count != 0)
        {
            CurrentXPos = st.Peek().X;
            CurrentYPos = st.Peek().Y;
        }
        else
        {
            CurrentXPos = CurrentYPos = -1;
        }
    }

    


    public bool CheckWinningCondition(int x, int y)
    {
        if (CheckRow(x, y) == 5)        // 같은 돌 개수가 5개면 (6목이상이면 게임 계속) 
        {
            GameFinish = true;
        }

        else if (CheckCol(x, y) == 5)
        {
            GameFinish = true;
        }

        else if (CheckDiagonal(x, y) == 5)
        {
            GameFinish = true;
        }

        else if (CheckReverseDiagonal(x, y) == 5)
        {
            GameFinish = true;
        }
        else
        {
            return false;
        }

        return true;
    }

    int CheckRow(int x, int y)      // ㅡ 확인
    {
        int continuousStoneNum = 1;

        for (int i = 1; i <= 5; i++)
        {
            if (x + i <= 18 && GameBoard[x + i, y] == GameBoard[x, y])
                continuousStoneNum++;

            else
                break;
        }

        for (int i = 1; i <= 5; i++)
        {
            if (x - i >= 0 && GameBoard[x - i, y] == GameBoard[x, y])
                continuousStoneNum++;

            else
                break;
        }

        return continuousStoneNum;
    }

    int CheckCol(int x, int y)      // | 확인
    {
        int continuousStoneNum = 1;

        for (int i = 1; i <= 5; i++)
        {
            if (y + i <= 18 && GameBoard[x, y + i] == GameBoard[x, y])
                continuousStoneNum++;

            else
                break;
        }

        for (int i = 1; i <= 5; i++)
        {
            if (y - i >= 0 && GameBoard[x, y - i] == GameBoard[x, y])
                continuousStoneNum++;

            else
                break;
        }

        return continuousStoneNum;
    }

    int CheckDiagonal(int x, int y)      // / 확인
    {
        int continuousStoneNum = 1;

        for (int i = 1; i <= 5; i++)
        {
            if (x + i <= 18 && y - i >= 0 && GameBoard[x + i, y - i] == GameBoard[x, y])
                continuousStoneNum++;

            else
                break;
        }

        for (int i = 1; i <= 5; i++)
        {
            if (x - i >= 0 && y + i <= 18 && GameBoard[x - i, y + i] == GameBoard[x, y])
                continuousStoneNum++;

            else
                break;
        }

        return continuousStoneNum;
    }

    int CheckReverseDiagonal(int x, int y)     // ＼ 확인
    {
        int continuousStoneNum = 1;

        for (int i = 1; i <= 5; i++)
        {
            if (x + i <= 18 && y + i <= 18 && GameBoard[x + i, y + i] == GameBoard[x, y])
                continuousStoneNum++;

            else
                break;
        }

        for (int i = 1; i <= 5; i++)
        {
            if (x - i >= 0 && y - i >= 0 && GameBoard[x - i, y - i] == GameBoard[x, y])
                continuousStoneNum++;

            else
                break;
        }

        return continuousStoneNum;
    }

    #region undo & samsam
    void Undo(object sender, EventArgs e)
    {
        if (!GameFinish && st.Count != 0)
        {
            /*무르기요청.Play();

            if (MessageBox.Show("한 수 무르시겠습니까?", "무르기", MessageBoxButtons.YesNo) == DialogResult.Yes) // MessageBox 띄워서 무르기 여부 확인하고 예를 누르면
            {
                if (AI모드)
                {
                    한수무르기();
                    한수무르기();
                }

                else
                {
                    한수무르기();
                    흑돌차례 = !흑돌차례;
                }


                panel1.Invalidate();
            }*/
        }
    }

    //bool 삼삼확인(int x, int y)     // 33확인
    //{
    //    int 삼삼확인 = 0;

    //    삼삼확인 += 가로삼삼확인(x, y);
    //    삼삼확인 += 세로삼삼확인(x, y);
    //    삼삼확인 += 사선삼삼확인(x, y);
    //    삼삼확인 += 역사선삼삼확인(x, y);

    //    if (삼삼확인 >= 2)
    //        return true;

    //    else
    //        return false;
    //}

    //int 가로삼삼확인(int x, int y)    // 가로 (ㅡ) 확인
    //{
    //    int 돌3개확인 = 1;
    //    int i, j;

    //    for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 → 확인
    //    {
    //        if (x + i > 18)
    //            break;

    //        else if (GameBoard[x + i, y] == GameBoard[x, y])
    //            돌3개확인++;

    //        else if (GameBoard[x + i, y] != (int)StoneType.None)
    //            break;
    //    }

    //    for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ← 확인
    //    {
    //        if (x - j < 0)
    //            break;

    //        else if (GameBoard[x - j, y] == GameBoard[x, y])
    //            돌3개확인++;

    //        else if (GameBoard[x - j, y] != (int)StoneType.None)
    //            break;
    //    }

    //    if (돌3개확인 == 3 && x + i < 18 && x - j > 0)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
    //    {
    //        if ((GameBoard[x + i, y] != (int)StoneType.None && GameBoard[x + i - 1, y] != (int)StoneType.None) || (GameBoard[x - j, y] != (int)StoneType.None && GameBoard[x - j + 1, y] != (int)StoneType.None))
    //        {
    //            return 0;
    //        }

    //        else
    //            return 1;
    //    }

    //    return 0;
    //}

    //private int 세로삼삼확인(int x, int y)    // 세로 (|) 확인
    //{
    //    int 돌3개확인 = 1;
    //    int i, j;

    //    돌3개확인 = 1;

    //    for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 ↓ 확인
    //    {
    //        if (y + i > 18)
    //            break;

    //        else if (GameBoard[x, y + i] == GameBoard[x, y])
    //            돌3개확인++;

    //        else if (GameBoard[x, y + i] != (int)StoneType.None)
    //            break;
    //    }

    //    for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ↑ 확인
    //    {
    //        if (y - j < 0)
    //            break;

    //        else if (GameBoard[x, y - j] == GameBoard[x, y])
    //            돌3개확인++;

    //        else if (GameBoard[x, y - j] != (int)StoneType.None)
    //            break;
    //    }

    //    if (돌3개확인 == 3 && y + i < 18 && y - j > 0)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
    //    {
    //        if ((GameBoard[x, y + i] != (int)StoneType.None && GameBoard[x, y + i - 1] != (int)StoneType.None) || (GameBoard[x, y - j] != (int)StoneType.None && GameBoard[x, y - j + 1] != (int)StoneType.None))
    //        {
    //            return 0;
    //        }

    //        else
    //            return 1;
    //    }

    //    return 0;
    //}

    //int 사선삼삼확인(int x, int y)    // 사선 (/) 확인
    //{
    //    int 돌3개확인 = 1;
    //    int i, j;

    //    돌3개확인 = 1;

    //    for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 ↗ 확인
    //    {
    //        if (x + i > 18 || y - i < 0)
    //            break;

    //        else if (GameBoard[x + i, y - i] == GameBoard[x, y])
    //            돌3개확인++;

    //        else if (GameBoard[x + i, y - i] != (int)StoneType.None)
    //            break;
    //    }

    //    for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ↙ 확인
    //    {
    //        if (x - j < 0 || y + j > 18)
    //            break;

    //        else if (GameBoard[x - j, y + j] == GameBoard[x, y])
    //            돌3개확인++;

    //        else if (GameBoard[x - j, y + j] != (int)StoneType.None)
    //            break;
    //    }

    //    if (돌3개확인 == 3 && x + i < 18 && y - i > 0 && x - j > 0 && y + j < 18)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
    //    {
    //        if ((GameBoard[x + i, y - i] != (int)StoneType.None && GameBoard[x + i - 1, y - i + 1] != (int)StoneType.None) || (GameBoard[x - j, y + j] != (int)StoneType.None && GameBoard[x - j + 1, y + j - 1] != (int)StoneType.None))
    //        {
    //            return 0;
    //        }

    //        else
    //            return 1;
    //    }

    //    return 0;
    //}

    //int 역사선삼삼확인(int x, int y)    // 역사선 (＼) 확인
    //{
    //    int 돌3개확인 = 1;
    //    int i, j;

    //    돌3개확인 = 1;

    //    for (i = 1; i <= 3; i++) // 돌을 둔 위치로부터 ↘ 확인
    //    {
    //        if (x + i > 18 || y + i > 18)
    //            break;

    //        else if (GameBoard[x + i, y + i] == GameBoard[x, y])
    //            돌3개확인++;

    //        else if (GameBoard[x + i, y + i] != (int)StoneType.None)
    //            break;
    //    }

    //    for (j = 1; j <= 3; j++) // 돌을 둔 위치로부터 ↖ 확인
    //    {
    //        if (x - j < 0 || y - j < 0)
    //            break;

    //        else if (GameBoard[x - j, y - j] == GameBoard[x, y])
    //            돌3개확인++;

    //        else if (GameBoard[x - j, y - j] != (int)StoneType.None)
    //            break;
    //    }

    //    if (돌3개확인 == 3 && x + i < 18 && y + i < 18 && x - j > 0 && y - j > 0)    //돌 개수가 3개면서 양쪽 벽에 붙어잇으면 안된다
    //    {
    //        if ((GameBoard[x + i, y + i] != (int)StoneType.None && GameBoard[x + i - 1, y + i - 1] != (int)StoneType.None) || (GameBoard[x - j, y - j] != (int)StoneType.None && GameBoard[x - j + 1, y - j + 1] != (int)StoneType.None))
    //        {
    //            return 0;
    //        }

    //        else
    //            return 1;
    //    }

    //    return 0;
    //}
    #endregion
}

public enum PutStoneResult
{
    Success = 0,
    SamSam = 1,
}
