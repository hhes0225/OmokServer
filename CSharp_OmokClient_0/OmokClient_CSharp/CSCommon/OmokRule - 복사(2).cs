//using System;
//using System.Collections.Generic;
//using System.Drawing;

//namespace CSCommon;

//public class OmokRule
//{
//    public enum StoneType { None, Black, White };

//    const int BoardSize = 19;


//    int[,] GameBoard = new int[BoardSize, BoardSize];
//    public bool BlackPlayerTurn { get; private set; } = true;

//    public bool GameFinish { get; private set; } = true;

//    //bool AI모드 = true;
//    //돌종류 컴퓨터돌;

//    public int CurTuenCount { get; private set; } = 0;


//    public int PrevXPos { get; private set; } = -1;
//    public int PrevYPos { get; private set; } = -1;

//    public int CurrentXPos { get; private set; } = -1;
//    public int CurrentYPos { get; private set; } = -1;

//    private Stack<Point> st = new Stack<Point>();

//    public void StartGame()
//    {
//        Array.Clear(GameBoard, 0, BoardSize * BoardSize);
//        PrevXPos = PrevYPos = -1;
//        CurrentXPos = CurrentYPos = -1;
//        BlackPlayerTurn = true;
//        CurTuenCount = 1;
//        GameFinish = false;

//        st.Clear();
//    }

//    public void EndGame()
//    {
//        GameFinish = true;
//    }

//    public int GetStoneByPos(int x, int y)
//    {
//        return GameBoard[x, y];
//    }

//    public bool IsBlackTurn()
//    {
//        return ((CurTuenCount % 2) == 1);
//    }

//    public PutStoneResult PutStone(int x, int y)
//    {
//        //TODO 서버로 부터 받은 결과가 실패인 경우 현재 둔 돌의 정보를 지워야 한다

//        if (BlackPlayerTurn)
//        {   // 검은 돌
//            GameBoard[x, y] = (int)StoneType.Black;
//        }
//        else
//        {
//            // 흰 돌
//            GameBoard[x, y] = (int)StoneType.White;
//        }


//        PrevXPos = CurrentXPos;
//        PrevYPos = CurrentYPos;

//        CurrentXPos = x;
//        CurrentYPos = y;

//        BlackPlayerTurn = !BlackPlayerTurn;                   // 차례 변경

//        ++CurTuenCount;
//        st.Push(new Point(x, y));

//        return PutStoneResult.Success;
//    }


//    public void Undo()
//    {
//        st.Pop();
//        GameBoard[CurrentXPos, CurrentYPos] = (int)StoneType.None;

//        if (st.Count != 0)
//        {
//            CurrentXPos = st.Peek().X;
//            CurrentYPos = st.Peek().Y;
//        }
//        else
//        {
//            CurrentXPos = CurrentYPos = -1;
//        }
//    }

//    public void CheckWinningCondition(int x, int y)
//    {
//        if (CheckRow(x, y) == 5)        // 같은 돌 개수가 5개면 (6목이상이면 게임 계속) 
//        {
//            //승리효과음.Play();
//            //MessageBox.Show((돌종류)바둑판[x, y] + " 승");
//            GameFinish = true;
//        }

//        else if (CheckCol(x, y) == 5)
//        {
//            //승리효과음.Play();
//            //MessageBox.Show((돌종류)바둑판[x, y] + " 승");
//            GameFinish = true;
//        }

//        else if (CheckDiagonal(x, y) == 5)
//        {
//            //승리효과음.Play();
//            //MessageBox.Show((돌종류)바둑판[x, y] + " 승");
//            GameFinish = true;
//        }

//        else if (CheckReverseDiagonal(x, y) == 5)
//        {
//            //승리효과음.Play();
//            //MessageBox.Show((돌종류)바둑판[x, y] + " 승");
//            GameFinish = true;
//        }
//    }

//    int CheckRow(int x, int y)      // ㅡ 확인
//    {
//        int continuousStoneNum = 1;

//        for (int i = 1; i <= 5; i++)
//        {
//            if (x + i <= 18 && GameBoard[x + i, y] == GameBoard[x, y])
//                continuousStoneNum++;

//            else
//                break;
//        }

//        for (int i = 1; i <= 5; i++)
//        {
//            if (x - i >= 0 && GameBoard[x - i, y] == GameBoard[x, y])
//                continuousStoneNum++;

//            else
//                break;
//        }

//        return continuousStoneNum;
//    }

//    int CheckCol(int x, int y)      // | 확인
//    {
//        int continuousStoneNum = 1;

//        for (int i = 1; i <= 5; i++)
//        {
//            if (y + i <= 18 && GameBoard[x, y + i] == GameBoard[x, y])
//                continuousStoneNum++;

//            else
//                break;
//        }

//        for (int i = 1; i <= 5; i++)
//        {
//            if (y - i >= 0 && GameBoard[x, y - i] == GameBoard[x, y])
//                continuousStoneNum++;

//            else
//                break;
//        }

//        return continuousStoneNum;
//    }

//    int CheckDiagonal(int x, int y)      // / 확인
//    {
//        int continuousStoneNum = 1;

//        for (int i = 1; i <= 5; i++)
//        {
//            if (x + i <= 18 && y - i >= 0 && GameBoard[x + i, y - i] == GameBoard[x, y])
//                continuousStoneNum++;

//            else
//                break;
//        }

//        for (int i = 1; i <= 5; i++)
//        {
//            if (x - i >= 0 && y + i <= 18 && GameBoard[x - i, y + i] == GameBoard[x, y])
//                continuousStoneNum++;

//            else
//                break;
//        }

//        return continuousStoneNum;
//    }

//    int CheckReverseDiagonal(int x, int y)     // ＼ 확인
//    {
//        int continuousStoneNum = 1;

//        for (int i = 1; i <= 5; i++)
//        {
//            if (x + i <= 18 && y + i <= 18 && GameBoard[x + i, y + i] == GameBoard[x, y])
//                continuousStoneNum++;

//            else
//                break;
//        }

//        for (int i = 1; i <= 5; i++)
//        {
//            if (x - i >= 0 && y - i >= 0 && GameBoard[x - i, y - i] == GameBoard[x, y])
//                continuousStoneNum++;

//            else
//                break;
//        }

//        return continuousStoneNum;
//    }

//}
