using SocketLibrary;
using MemoryPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

#pragma warning disable CA1416

namespace OmokClient
{
    public partial class MainForm : Form
    {
        Dictionary<UInt16, Action<byte[]>> PacketFuncDic = new Dictionary<UInt16, Action<byte[]>>();

        private System.Threading.Timer _pingTimer;


        void SetPacketHandler()
        {
            //PacketFuncDic.Add(PACKET_ID.PACKET_ID_ERROR_NTF, PacketProcess_ErrorNotify);
            PacketFuncDic.Add((ushort)PACKETID.ResLogin, PacketProcess_Login);
            PacketFuncDic.Add((ushort)PACKETID.ResRoomEnter, PacketProcess_RoomEnter);
            PacketFuncDic.Add((ushort)PACKETID.NtfRoomUserList, PacketProcess_RoomUserList);
            PacketFuncDic.Add((ushort)PACKETID.NtfRoomNewUser, PacketProcess_RoomNewUser);
            PacketFuncDic.Add((ushort)PACKETID.ResRoomLeave, PacketProcess_RoomLeave);
            PacketFuncDic.Add((ushort)PACKETID.NtfRoomLeaveUser, PacketProcess_RoomLeaveUser);
            PacketFuncDic.Add((ushort)PACKETID.ResRoomChat, PacketProcess_RoomChatResponse);
            PacketFuncDic.Add((ushort)PACKETID.NtfRoomChat, PacketProcess_RoomChatNotify);
            PacketFuncDic.Add((ushort)PACKETID.ResReadyOmok, PacketProcess_ReadyOmokResponse);
            PacketFuncDic.Add((ushort)PACKETID.NtfReadyOmok, PacketProcess_ReadyOmokNotify);
            PacketFuncDic.Add((ushort)PACKETID.NtfStartOmok, PacketProcess_StartOmokNotify);
            PacketFuncDic.Add((ushort)PACKETID.ResPutOmok, PacketProcess_PutMokResponse);
            PacketFuncDic.Add((ushort)PACKETID.NtfPutOmok, PacketProcess_PutMokNotify);
            PacketFuncDic.Add((ushort)PACKETID.NtfEndOmok, PacketProcess_EndOmokNotify);
            PacketFuncDic.Add((ushort)PACKETID.PongUserConnInfo, PacketProcess_PongUserConnInfo);
            PacketFuncDic.Add((ushort)PACKETID.NtfTurnPass, PacketProcess_TurnPassNotify);
        }


        //헤더와 바디 분리
        void PacketProcess(byte[] packet)
        {
            var header = new MsgPackPacketHeaderInfo();
            header.Read(packet); // 헤더 정보 역직렬화`

            var packetID = header.ID;

            var bodySize = packet.Length - PacketDef.PacketHeaderSize;
            var bodyData = new byte[bodySize];

            Buffer.BlockCopy(packet, PacketDef.PacketHeaderSize, bodyData, 0, bodySize);


            //bodysize 가져오기

            if (PacketFuncDic.ContainsKey(packetID))
            {
                PacketFuncDic[packetID](bodyData);
            }
            else
            {
                DevLog.Write("Unknown Packet Id: " + packetID);
            }
        }


        void PacketProcess_ErrorNotify(byte[] packetData)
        {
            /*var notifyPkt = new ErrorNtfPacket();
            notifyPkt.FromBytes(bodyData);

            DevLog.Write($"에러 통보 받음:  {notifyPkt.Error}");*/
        }


        void PacketProcess_Login(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResLogin>(packetData);
            DevLog.Write($"로그인 결과: {(ErrorCode)responsePkt.Result}");

            if ((ErrorCode)responsePkt.Result == ErrorCode.None)
            {
                CurSceen = ClientSceen.LOGIN;

                InitAndStartPingTimer(0, 1000);
            }
        }

        void PacketProcess_RoomEnter(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomEnter>(packetData);
            DevLog.Write($"방 입장 결과:  {(ErrorCode)responsePkt.Result}");

            if ((ErrorCode)responsePkt.Result == ErrorCode.None)
            {
                CurSceen = ClientSceen.ROOM;
            }
        }

        void PacketProcess_RoomUserList(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfRoomUserList>(packetData);

            for (int i = 0; i < notifyPkt.UserIDList.Count; ++i)
            {
                AddRoomUserList(notifyPkt.UserIDList[i]);
            }

            DevLog.Write($"방의 기존 유저 리스트 받음");
        }

        void PacketProcess_RoomNewUser(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfRoomNewUser>(packetData);

            AddRoomUserList(notifyPkt.UserID);
            AddRoomChatMessageList(notifyPkt.UserID, "님이 입장하셨습니다.");

            DevLog.Write($"방에 새로 들어온 유저 받음");
        }


        void PacketProcess_RoomLeave(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomLeave>(packetData);

            listBoxRoomUserList.Items.Clear();

            listBoxRoomChatMsg.Items.Clear();

            DevLog.Write($"방 나가기 결과:  {(ErrorCode)responsePkt.Result}");
        }

        void PacketProcess_RoomLeaveUser(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfRoomLeaveUser>(packetData);

            RemoveRoomUserList(notifyPkt.UserID);
            if (notifyPkt.UserID != textBoxUserID.Text)
            {
                AddRoomChatMessageList(notifyPkt.UserID, "님이 퇴장하셨습니다.");
            }
            else
            {
                listBoxRoomUserList.Items.Clear();
                listBoxRoomChatMsg.Items.Clear();
            }
            
            DevLog.Write($"방에서 나간 유저 받음");
        }


        void PacketProcess_RoomChatResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResRoomChat>(packetData);

            DevLog.Write($"방 채팅 결과:  {(ErrorCode)responsePkt.Result}");
        }


        void PacketProcess_RoomChatNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfRoomChat>(packetData);

            AddRoomChatMessageList(notifyPkt.UserID, notifyPkt.ChatMessage);
        }

        void AddRoomChatMessageList(string userID, string message)
        {
            if (listBoxRoomChatMsg.Items.Count > 512)
            {
                listBoxRoomChatMsg.Items.Clear();
            }

            listBoxRoomChatMsg.Items.Add($"[{userID}]: {message}");
            listBoxRoomChatMsg.SelectedIndex = listBoxRoomChatMsg.Items.Count - 1;
        }

        void PacketProcess_ReadyOmokResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResReadyOmok>(packetData);


            DevLog.Write($"게임 준비 완료 요청 결과:  {(ErrorCode)responsePkt.Result}");

            if ((ErrorCode)responsePkt.Result == ErrorCode.None)
            {
                CurSceen = ClientSceen.GAME_READY;
            }
        }

        void PacketProcess_ReadyOmokNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfReadyOmok>(packetData);

            AddRoomChatMessageList(notifyPkt.UserID, "님 준비 완료!");

            if (notifyPkt.IsReady==SocketLibrary.UserState.Ready)
            {
                DevLog.Write($"[{notifyPkt.UserID}]님은 게임 준비 완료");
            }
            else
            {
                DevLog.Write($"[{notifyPkt.UserID}]님이 게임 준비 완료 취소");
            }

        }

        void PacketProcess_StartOmokNotify(byte[] packetData)
        {
            IsMyTurn = false;

            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfStartOmok>(packetData);
            
            if (notifyPkt.BlackUserID == textBoxUserID.Text)
            {
                IsMyTurn = true;
            }

            CurSceen = ClientSceen.GAME_PLAYING;
            StartGame(IsMyTurn, notifyPkt.BlackUserID, notifyPkt.WhiteUserID);

            DevLog.Write($"게임 시작. 흑돌 플레이어: {notifyPkt.BlackUserID}");
            AddRoomChatMessageList(notifyPkt.BlackUserID, "님은 흑돌입니다.");
            AddRoomChatMessageList(notifyPkt.WhiteUserID, "님은 백돌입니다.");
        }


        void PacketProcess_PutMokResponse(byte[] packetData)
        {
            var responsePkt = MemoryPackSerializer.Deserialize<PKTResPutMok>(packetData);

            if (responsePkt.Result != (Int16)ErrorCode.None)
            {
                DevLog.Write($"오목 놓기 성공");
            }
            else
            {
                DevLog.Write($"오목 놓기 실패: {(ErrorCode)responsePkt.Result}");
                //OmokLogic.Undo();
            }
        }


        void PacketProcess_PutMokNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfPutMok>(packetData);

            PlayerPutStoneResponse(IsMyTurn, notifyPkt.PosX, notifyPkt.PosY);

            IsMyTurn = !IsMyTurn;
            

            DevLog.Write($"오목 정보: X: {notifyPkt.PosX},  Y: {notifyPkt.PosY},   알:{notifyPkt.Mok}");
        }

        void PacketProcess_TurnPassNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfPutMok>(packetData);

            PlayerTurnPassResponse(IsMyTurn);

            IsMyTurn = !IsMyTurn;

            DevLog.Write($"오목 정보: Turn passed!");
        }


        void PacketProcess_EndOmokNotify(byte[] packetData)
        {
            var notifyPkt = MemoryPackSerializer.Deserialize<PKTNtfEndOmok>(packetData);

            EndGame();

            CurSceen = ClientSceen.ROOM;

            if (notifyPkt.WinUserID == "DRAW")
            {
                DevLog.Write($"오목 GameOver: {notifyPkt.WinUserID}");
            }
            else
            {
                DevLog.Write($"오목 GameOver: Win: {notifyPkt.WinUserID}");
            }
            
        }

        void PacketProcess_PingUserConnInfo(object state)
        {
            var pingPkt = new PTKPingUserConnInfo();
            var body = MemoryPackSerializer.Serialize(pingPkt);

            PostSendPacket((ushort) PACKETID.PingUserConnInfo, body);

        }

        //Login Response 이후 호출됨
        void InitAndStartPingTimer(int dueTime, int interval)
        {
            TimerCallback callback = new TimerCallback(PacketProcess_PingUserConnInfo);
            _pingTimer = new System.Threading.Timer(callback, null, dueTime, interval);
        }

        void PacketProcess_PongUserConnInfo(byte[] packetData)
        {
            var pongPkt = MemoryPackSerializer.Deserialize<PKTPongUserConnINfo>(packetData);

            if (pongPkt.Result != (Int16)ErrorCode.None)
            {
                SetDisconnectd();
            }
        }
    }
}