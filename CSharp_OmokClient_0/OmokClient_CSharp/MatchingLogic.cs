using MemoryPack;
using SocketLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OmokClient;

public partial class MainForm 
{
    private Timer _checkMatchingTimer;

    void InitAndStartCheckMatchingTimer(int dueTime, int interval)
    {
        TimerCallback callback = new TimerCallback(MatchingLogic_CheckMatchingReq);
        _checkMatchingTimer = new Timer(callback, null, dueTime, interval);
    }

    private async void MatchingLogic_CheckMatchingReq(object state)
    {
        var pingInfo = new{ UserID = ID };
        var gameAPIAddress = gameAddrTextBox.Text;

        //http client 객체 생성
        HttpClient client = new HttpClient();

        //POST 요청에 첨부할 데이터 생성
        var postData = new { UserID = ID };
        string json = JsonSerializer.Serialize(postData);
        StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

        //POST 요청 보내기
        HttpResponseMessage response = await client.PostAsync("http://" + gameAPIAddress + "/CheckMatching", content);

        if (response.IsSuccessStatusCode)
        {
            string jsonString = await response.Content.ReadAsStringAsync();
            CheckMatchingResponse checkMatchingResponse = JsonSerializer.Deserialize<CheckMatchingResponse>(jsonString);

            //매칭 성공
            if(checkMatchingResponse.SocketServerAddress != "")
            {
                textBoxIP.Text = checkMatchingResponse.SocketServerAddress;
                textBoxPort.Text = checkMatchingResponse.SocketServerPort;
                textBoxRoomNumber.Text = checkMatchingResponse.RoomNumber;

                ConnectSocketServer();
                RoomEnter();
            }

        }
    }

    private void ConnectSocketServer()
    {
        string address = textBoxIP.Text;

        if (checkBoxLocalHostIP.Checked)
        {
            address = "127.0.0.1";
        }

        int port = Convert.ToInt32(textBoxPort.Text);

        if (Network.Connect(address, port))
        {
            labelStatus.Text = string.Format("{0}. 서버에 접속 중", DateTime.Now);
            btnConnect.Enabled = false;
            btnDisconnect.Enabled = true;

            DevLog.Write($"서버에 접속 중", LOG_LEVEL.INFO);
        }
        else
        {
            labelStatus.Text = string.Format("{0}. 서버에 접속 실패", DateTime.Now);
        }

        PacketBuffer.Clear();
    }

    private void Login()
    {
        var loginReq = new SocketLibrary.PKTReqLogin();
        //bodyData setting
        loginReq.UserID = textBoxUserID.Text;
        loginReq.AuthToken = textBoxAT.Text;
        var body = MemoryPackSerializer.Serialize(loginReq);

        PostSendPacket((ushort)PACKETID.ReqDbLogin, body);
        DevLog.Write($"로그인 요청:  {textBoxUserID.Text}, {textBoxAT.Text}");
    }

    private void RoomEnter()
    {
        var roomEnterReq = new SocketLibrary.PKTReqRoomEnter()
        {
            RoomNum = int.Parse(textBoxRoomNumber.Text)
        };

        var body = MemoryPackSerializer.Serialize(roomEnterReq);

        PostSendPacket((ushort)PACKETID.ReqRoomEnter, body);
        DevLog.Write($"방 입장 요청:  {textBoxRoomNumber.Text} 번");
    }
}


public class CheckMatchingResponse
{
    public ErrorCode Result { get; set; } = ErrorCode.None;
    public string SocketServerAddress { get; set; } = "";
    public string SocketServerPort { get; set; } = "";
    public string RoomNumber { get; set; } = "";
}


