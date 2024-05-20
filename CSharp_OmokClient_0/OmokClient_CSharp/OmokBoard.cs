using SocketLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OmokClient
{
    public partial class MainForm : Form
    {
        SocketLibrary.OmokRule OmokLogic = new SocketLibrary.OmokRule();

        #region 오목 게임 상수
        private int StartPos = 30;
        private int GridSize = 30;
        private int StoneSize = 20;
        private int IntersectionSize = 10;
        private int BoardSize = 19;

        private Pen BlackPen = new Pen(Color.Black);
        private SolidBrush RedColor = new SolidBrush(Color.Red);
        private SolidBrush BlackColor = new SolidBrush(Color.Black);
        private SolidBrush WhiteColor = new SolidBrush(Color.White);

        private SoundPlayer StartSoundEffect;
        private SoundPlayer EndSoundEffect;
        private SoundPlayer WinSoundEffect;
        private SoundPlayer PutStoneSoundEffect;
        private SoundPlayer UndoReqSoundEffect;
        private SoundPlayer ErrorSoundEffect;
        #endregion

        private int PrevXPos = -1, PrevYPos = -1;

        bool IsMyTurn = false;

        private bool AIMode = false;
        private SocketLibrary.OmokRule.StoneType ComputerStone = SocketLibrary.OmokRule.StoneType.None;

        string MyPlayerName = "";
        string BlackUserID = "";
        string WhiteUserID = "";
        
        AIPlayer OmokAI = new();

        private Timer TurnTimer;
        const int Timespan = 10;
        int RemainingTime = Timespan;

        void Omok_Init()
        {
            DoubleBuffered = true;

            var curDir = System.Windows.Forms.Application.StartupPath;
            StartSoundEffect = new SoundPlayer($"{curDir}\\sound\\대국시작.wav");
            WinSoundEffect = new SoundPlayer($"{curDir}\\sound\\대국승리.wav");
            PutStoneSoundEffect = new SoundPlayer($"{curDir}\\sound\\바둑돌소리.wav");
            UndoReqSoundEffect = new SoundPlayer($"{curDir}\\sound\\무르기.wav");
            ErrorSoundEffect = new SoundPlayer($"{curDir}\\sound\\오류.wav");
            EndSoundEffect = new SoundPlayer($"{curDir}\\sound\\대국종료.wav");

            //ai = new AI(바둑판);
            //컴퓨터돌 = 돌종류.백돌;
        }

        //오목 게임 시작
        void StartGame(bool isMyTurn, string blackUserID, string whiteUserID)
        {
            BlackUserID=blackUserID;
            WhiteUserID=whiteUserID;

            IsMyTurn = isMyTurn;

            PrevXPos = PrevYPos = -1;
            StartSoundEffect.Play();

            labelTurnTime.Text = $"Time Left: {RemainingTime}";
            InitRemainingTime();
            TurnTimer = new Timer();
            TurnTimer.Interval = 1000;
            TurnTimer.Tick += new EventHandler(TurnTimer_Tick);
            TurnTimer.Start();

            OmokLogic.StartGame();

            panel1.Invalidate();
        }
        #region 구버전 StartGame
        //void StartGame(bool isMyTurn, string myPlayerName, string otherPlayerName)
        //{
        //    MyPlayerName = myPlayerName;

        //    if (isMyTurn)
        //    {
        //        BlackUserID = myPlayerName;
        //        WhiteUserID = otherPlayerName;
        //    }
        //    else
        //    {
        //        BlackUserID = otherPlayerName;
        //        WhiteUserID = myPlayerName;
        //    }

        //    IsMyTurn = isMyTurn;

        //    PrevXPos = PrevYPos = -1;
        //    StartSoundEffect.Play();

        //    OmokLogic.StartGame();

        //    //if (AIMode == true && 컴퓨터돌 == CSCommon.OmokRule.돌종류.흑돌)
        //    //{
        //    //    컴퓨터두기();
        //    //}

        //    panel1.Invalidate();
        //}
        #endregion

        void EndGame()
        {
            OmokLogic.EndGame();

            if(TurnTimer != null)
            {
                TurnTimer.Stop();
            }

            EndSoundEffect.Play();

            MyPlayerName = "";
            WhiteUserID = "";
            BlackUserID = "";
        }
                      

        void DisableAIMode()
        {
            if (AIMode == true)
            {
                AIMode = false;
            }
        }

        private void TurnTimer_Tick(object sender, EventArgs e)
        {
            RemainingTime--;
            labelTurnTime.Text = $"Time Left: {RemainingTime}";

            if (RemainingTime == 0)
            {
                TurnTimer.Stop();
            }
        }

        public void InitRemainingTime()
        {
            RemainingTime = Timespan+1;
        }

        #region omok UI
        void panel1_Paint(object sender, PaintEventArgs e)
        {
            for (int i = 0; i < BoardSize; i++)                     // 바둑판 선 그리기
            {
                e.Graphics.DrawLine(BlackPen, StartPos, StartPos + i * GridSize, StartPos + 18 * GridSize, StartPos + i * GridSize);
                e.Graphics.DrawLine(BlackPen, StartPos + i * GridSize, StartPos, StartPos + i * GridSize, StartPos + 18 * GridSize);
            }

            for (int i = 0; i < 3; i++)                              // 화점 그리기
            {
                for (int j = 0; j < 3; j++)
                {
                    Rectangle r = new Rectangle(StartPos + GridSize * 3 + GridSize * i * 6 - IntersectionSize / 2,
                        StartPos + GridSize * 3 + GridSize * j * 6 - IntersectionSize / 2, IntersectionSize, IntersectionSize);

                    e.Graphics.FillEllipse(BlackColor, r);
                }
            }

            if (OmokLogic.GameFinish == false)
            {
                for (int i = 0; i < BoardSize; i++)
                {
                    for (int j = 0; j < BoardSize; j++)
                    {
                        DrawStone(i, j);
                    }
                }

                if (OmokLogic.CurrentXPos >= 0 && OmokLogic.CurrentYPos >= 0)
                {
                    ShowCurrentStone();
                }

                CurrentTurnPlayerInfo();
            }
        }

        void DrawStone(int x, int y)
        {
            Graphics g = panel1.CreateGraphics();

            Rectangle r = new Rectangle(StartPos + GridSize * x - StoneSize / 2,
                StartPos + GridSize * y - StoneSize / 2, StoneSize, StoneSize);

            if (OmokLogic.GetStoneByPos(x, y) == (int)SocketLibrary.OmokRule.StoneType.Black)                              // 검은 돌
            {
                g.FillEllipse(BlackColor, r);
            }
            else if (OmokLogic.GetStoneByPos(x, y) == (int)SocketLibrary.OmokRule.StoneType.White)                         // 흰 돌
            {
                g.FillEllipse(WhiteColor, r);
            }
        }

        void ShowCurrentStone()
        {
            // 가장 최근에 놓은 돌에 화점 크기만한 빨간 점으로 표시하기
            Graphics g = panel1.CreateGraphics();

            Rectangle AreaForPrevStoneRedraw = new Rectangle(StartPos + GridSize * OmokLogic.PrevXPos - StoneSize / 2,
                StartPos + GridSize * OmokLogic.PrevYPos - StoneSize / 2, StoneSize, StoneSize);

            Rectangle r = new Rectangle(StartPos + GridSize * OmokLogic.CurrentXPos - IntersectionSize / 2,
                    StartPos + GridSize * OmokLogic.CurrentYPos - IntersectionSize / 2, IntersectionSize, IntersectionSize);

            // 초기화값이 -1이므로 -1보다 큰 값이 존재하면 찍은 값이 존재함
            if (OmokLogic.PrevXPos >= 0 && OmokLogic.PrevYPos >= 0)
            {
                // 전돌 다시 찍어서 빨간 점 없애기
                if (OmokLogic.GetStoneByPos(OmokLogic.PrevXPos, OmokLogic.PrevYPos) == (int)SocketLibrary.OmokRule.StoneType.Black)
                {
                    g.FillEllipse(BlackColor, AreaForPrevStoneRedraw);
                }
                else if (OmokLogic.GetStoneByPos(OmokLogic.PrevXPos, OmokLogic.PrevYPos) == (int)SocketLibrary.OmokRule.StoneType.White)
                {
                    g.FillEllipse(WhiteColor, AreaForPrevStoneRedraw);
                }
            }

            // 화점 크기만큼 빨간 점 찍기
            g.FillEllipse(RedColor, r);
        }

        void CurrentTurnPlayerInfo()        // 화면 하단에 다음에 둘 돌의 색을 표시
        {
            Graphics g = panel1.CreateGraphics();
            string str;
            Font infoFont = new Font("HY견고딕", 10);

            if (OmokLogic.IsBlackTurn())
            {
                str = "현재 턴 돌";
                g.FillEllipse(BlackColor, StartPos + 115, 599, StoneSize, StoneSize);
                g.DrawString(str, infoFont, BlackColor, StartPos, 600);

                g.DrawString($"PlayerName: {BlackUserID }", infoFont, BlackColor, (StartPos + 120 + StoneSize), 600);
            }

            else                 // 다음 돌 표시(흰 돌)
            {
                str = "현재 턴 돌";
                g.FillEllipse(WhiteColor, StartPos + 115, 599, StoneSize, StoneSize);
                g.DrawString(str, infoFont, BlackColor, StartPos, 600);

                g.DrawString($"PlayerName: {WhiteUserID }", infoFont, BlackColor, (StartPos + 120 + StoneSize), 600);
            }
        }


        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (OmokLogic.GameFinish || IsMyTurn == false)
            {
                return;
            }


            int x, y;

            // 왼쪽클릭만 허용
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            x = (e.X - StartPos + 10) / GridSize;
            y = (e.Y - StartPos + 10) / GridSize;

            // 바둑판 크기를 벗어나는지 확인
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
            {
                return;
            }
            // 바둑판 해당 좌표에 아무것도 없고, 게임이 끝나지 않았으면
            else if (OmokLogic.GetStoneByPos(x, y) == (int)SocketLibrary.OmokRule.StoneType.None && !OmokLogic.GameFinish)
            {
                PlayerPutStoneRequest(false, x, y);
            }
        }

        void PlayerPutStoneRequest(bool isNotify, int x, int y)
        {
            SendPacketOmokPut(x, y);
        }

        void PlayerPutStoneResponse(bool isNotify, int x, int y)
        {
            var ret = OmokLogic.PutStone(x, y);
            DrawStone(x, y);
            ShowCurrentStone();
            

            if (isNotify == false)
            {
                IsMyTurn = false;

            }
            else
            {
                IsMyTurn = true;
            }

            Rectangle r = new Rectangle(StartPos, 590, StartPos + StoneSize + 350, StoneSize + 10);
            panel1.Invalidate(r);
            TurnTimer.Start();
            InitRemainingTime();
        }

        void PlayerTurnPassResponse(bool isNotify)
        {
            OmokLogic.PassTurn();
            ShowCurrentStone();

           

            if (isNotify == false)
            {
                IsMyTurn = false;

            }
            else
            {
                IsMyTurn = true;
            }
            

            Rectangle r = new Rectangle(StartPos, 590, StartPos + StoneSize + 350, StoneSize + 10);
            panel1.Invalidate(r);
            TurnTimer.Start();
            InitRemainingTime();
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)     // 현재 차례의 돌 잔상 구현 (마우스 움직일때)
        {
            if (OmokLogic.GameFinish || IsMyTurn == false)
            {
                return;
            }

            int x, y;

            Color transparentBlack = Color.FromArgb(70, Color.Black);
            Color transparentWhite = Color.FromArgb(70, Color.White);
            SolidBrush transparentBlackBrush = new SolidBrush(transparentBlack);
            SolidBrush transparentWhiteBrush = new SolidBrush(transparentWhite);

            x = (e.X - StartPos + 10) / GridSize;
            y = (e.Y - StartPos + 10) / GridSize;

            // 바둑판 크기를 벗어나는지 확인
            if (x < 0 || x >= BoardSize || y < 0 || y >= BoardSize)
            {
                return;
            }
            else if (OmokLogic.GetStoneByPos(x, y) == (int)SocketLibrary.OmokRule.StoneType.None &&
                        !OmokLogic.GameFinish &&
                        (PrevXPos != x || PrevYPos != y)
                        )
            {
                // 바둑판 해당 좌표에 아무것도 없고, 좌표가 변경되면
                Graphics g = panel1.CreateGraphics();

                Rectangle AreaForPrevStoneErase = new Rectangle(StartPos + GridSize * PrevXPos - StoneSize / 2,
                                        StartPos + GridSize * PrevYPos - StoneSize / 2, StoneSize, StoneSize);

                Rectangle r = new Rectangle(StartPos + GridSize * x - StoneSize / 2,
                                        StartPos + GridSize * y - StoneSize / 2, StoneSize, StoneSize);

                // 먼저 그린 잔상을 지우고 새로운 잔상을 그린다.
                panel1.Invalidate(AreaForPrevStoneErase);

                if (OmokLogic.IsBlackTurn())
                    g.FillEllipse(transparentBlackBrush, r);
                else
                    g.FillEllipse(transparentWhiteBrush, r);

                PrevXPos = x;
                PrevYPos = y;
            }
        }
        #endregion



        //void 컴퓨터두기()
        //{
        //    int x = 0, y = 0;
        //    CSCommon.PutStoneResult ret;

        //    do
        //    {
        //        OmokAI.AI_PutAIPlayer(ref x, ref y, false, 2);
        //        ret = OmokLogic.PutStone(x, y);
        //    } while (ret != CSCommon.PutStoneResult.Success);

        //    DrawStone(x, y);
        //    ShowCurrentStone();
        //    OmokLogic.CheckWinningCondition(x, y);
        //}
    }
}
