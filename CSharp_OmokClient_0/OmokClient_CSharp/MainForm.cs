using SocketLibrary;
using MemoryPack;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;


namespace OmokClient
{
    public partial class MainForm : Form
    {
        string ID;
        string AuthToken;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            PacketBuffer.Init((8096 * 10), SocketLibrary.MsgPackPacketHeaderInfo.HeadSize, 2048);

            IsNetworkThreadRunning = true;
            NetworkReadThread = new System.Threading.Thread(this.NetworkReadProcess);
            NetworkReadThread.Start();
            NetworkSendThread = new System.Threading.Thread(this.NetworkSendProcess);
            NetworkSendThread.Start();

            IsBackGroundProcessRunning = true;
            dispatcherUITimer.Tick += new EventHandler(BackGroundProcess);
            dispatcherUITimer.Interval = 100;
            dispatcherUITimer.Start();

            btnDisconnect.Enabled = false;

            SetPacketHandler();


            Omok_Init();
            DevLog.Write("프로그램 시작 !!!", LOG_LEVEL.INFO);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsNetworkThreadRunning = false;
            IsBackGroundProcessRunning = false;

            Network.Close();
        }

        private void BtnCreateAccount_Click(object sender, EventArgs e)
        {
            // 새 창을 표시합니다.
            registerForm.ShowDialog();
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            loginForm.ShowDialog();
        }

        private async void BtnNewFormCreateAccount_Click(Object sender, EventArgs e)
        {
            var address = hiveAddrTextBox.Text;
            var id = textboxCreateID.Text;
            var pw = textboxCreatePw.Text;

            if(id!="" && pw != "") { 
                //http client 객체 생성
                HttpClient client = new HttpClient();

                //POST 요청에 첨부할 데이터 생성
                var postData = new { Email = id, Password = pw, Nickname = " " };
                string json = JsonSerializer.Serialize(postData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                //POST 요청 보내기
                HttpResponseMessage response = await client.PostAsync("http://"+address+"/CreateAccount", content);

                // 응답 처리
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("계정 생성이 완료되었습니다.");
                }
                else
                {
                    MessageBox.Show("계정 생성에 실패하였습니다.");
                }

                textboxCreateID.Clear();
                textboxCreatePw.Clear();
                registerForm.Close();
            }
            else
            {
                MessageBox.Show("회원가입 실패! - 올바른 입력 형식 아님");
            }
        }

        private async void BtnNewFormLogin_Click(Object sender, EventArgs e)
        {
            var hiveAddress = hiveAddrTextBox.Text;
            var gameAPIAddress= gameAddrTextBox.Text;
            var id = textboxLoginID.Text;
            var pw = textboxLoginPw.Text;

            if (id != "" && pw != "")
            {
                //http client 객체 생성
                HttpClient client = new HttpClient();

                //POST 요청에 첨부할 데이터 생성
                var postData = new { Email = id, Password = pw};
                string json = JsonSerializer.Serialize(postData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                //POST 요청 보내기
                HttpResponseMessage response = await client.PostAsync("http://" + hiveAddress + "/Login", content);

                // 응답 처리
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    LoginResponse loginResponse = JsonSerializer.Deserialize<LoginResponse>(jsonString);

                    var gameAPIPostData = new {Email = id, AuthToken = loginResponse.authToken};
                    json = JsonSerializer.Serialize(gameAPIPostData);
                    content = new StringContent(json, Encoding.UTF8, "application/json");

                    HttpResponseMessage authTokenResponse = await client.PostAsync("http://" + gameAPIAddress + "/Login", content);

                    if(authTokenResponse.IsSuccessStatusCode)
                    {
                        MessageBox.Show("로그인 성공!");
                        textboxLoginID.Clear();
                        textboxLoginPw.Clear();

                        textBoxUserID.Text = id;
                        textBoxAT.Text = loginResponse.authToken;

                        ID = id;
                        AuthToken = loginResponse.authToken;

                        loginForm.Close();
                    }
                    else
                    {
                        MessageBox.Show("Game API 서버 로그인 실패!");
                    }

                }
                else
                {
                    MessageBox.Show("Hive 서버 로그인 실패!");
                }

                
            }
            else
            {
                MessageBox.Show("로그인 실패! - 올바른 입력 형식 아님");
            }
        }

        private async void BtnMatching_Click(object sender, EventArgs e)
        {
            var gameAPIAddress = gameAddrTextBox.Text;

            if (gameAPIAddress != "")
            {
                //http client 객체 생성
                HttpClient client = new HttpClient();

                //POST 요청에 첨부할 데이터 생성
                var postData = new { UserID = ID };
                string json = JsonSerializer.Serialize(postData);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                //POST 요청 보내기
                HttpResponseMessage response = await client.PostAsync("http://" + gameAPIAddress + "/MatchingRequest", content);

                // 응답 처리
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    CheckMatchingResponse checkMatchingResponse = JsonSerializer.Deserialize<CheckMatchingResponse>(jsonString);

                    btnMatching.Enabled = false;

                    textBoxIP.Text = checkMatchingResponse.SocketServerAddress;
                    textBoxPort.Text = checkMatchingResponse.SocketServerPort;
                    textBoxRoomNumber.Text = checkMatchingResponse.RoomNumber;

                    ConnectSocketServer();
                    Login();
                    RoomEnter();
                }
                else
                {
                    MessageBox.Show("매칭 실패!");
                }
            }
            else
            {
                MessageBox.Show("게임 서버 주소를 입력해 주세요.");
            }

        }

        // 접속하기
        private void button1_Click(object sender, EventArgs e)
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

        // 접속 끊기
        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            PostSendPacket((ushort)PACKETID.ReqRoomLeave, null);
            SetDisconnectd();
            Network.Close();
        }

        // 로그인 요청
        private void button3_Click(object sender, EventArgs e)
        {
            var loginReq = new SocketLibrary.PKTReqLogin();
            //bodyData setting
            loginReq.UserID = textBoxUserID.Text;
            loginReq.AuthToken = textBoxAT.Text;
            var body = MemoryPackSerializer.Serialize(loginReq);

            PostSendPacket((ushort)PACKETID.ReqDbLogin, body);
            DevLog.Write($"로그인 요청:  {textBoxUserID.Text}, {textBoxAT.Text}");
        }

        // 방 입장
        private void button4_Click(object sender, EventArgs e)
        {
            var roomEnterReq = new SocketLibrary.PKTReqRoomEnter()
            {
                RoomNum = int.Parse(textBoxRoomNumber.Text)
            };

            var body = MemoryPackSerializer.Serialize(roomEnterReq);

            PostSendPacket((ushort)PACKETID.ReqRoomEnter, body);
            DevLog.Write($"방 입장 요청:  {textBoxRoomNumber.Text} 번");
        }

        // 방 나가기
        private void button5_Click(object sender, EventArgs e)
        {
            PostSendPacket((ushort)PACKETID.ReqRoomLeave, null);
            DevLog.Write("방 나가기 요청");
        }

        // 게임 준비 완료
        private void button6_Click(object sender, EventArgs e)
        {
            var userReadyReq = new SocketLibrary.PKTReqReadyOmok()
            {
                RoomNumber = int.Parse(textBoxRoomNumber.Text),
                UserID = textBoxUserID.Text
            };

            var body = MemoryPackSerializer.Serialize(userReadyReq);

            PostSendPacket((ushort)PACKETID.ReqReadyOmok, body);

            DevLog.Write($"게임 준비 완료 요청");
        }

        // 채팅
        private void button7_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxRoomSendMsg.Text))
            {
                MessageBox.Show("채팅 메시지를 입력하세요");
                return;
            }

            var requestPkt = new SocketLibrary.PKTReqRoomChat();
            requestPkt.ChatMessage = textBoxRoomSendMsg.Text;
            var packet = MemoryPackSerializer.Serialize(requestPkt);

            PostSendPacket((ushort)PACKETID.ReqRoomChat, packet);
            DevLog.Write($"방 채팅 요청");

            textBoxRoomSendMsg.Clear();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            StartGame(true, "My", "Other");
        }
    }
}

public class LoginResponse
{
    public ErrorCode result { get; set; } = ErrorCode.None;
    public string authToken { get; set; } = "";
}