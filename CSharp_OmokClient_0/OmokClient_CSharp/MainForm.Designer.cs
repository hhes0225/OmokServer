﻿
using System.Drawing;
using System.Windows.Forms;

namespace OmokClient
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.apiGroupBox = new System.Windows.Forms.GroupBox();
            this.hiveAddrLabel = new System.Windows.Forms.Label();
            this.hiveAddrTextBox = new System.Windows.Forms.TextBox();
            this.btnCreateAccount = new System.Windows.Forms.Button();
            this.gameAddrLabel = new System.Windows.Forms.Label();
            this.gameAddrTextBox = new System.Windows.Forms.TextBox();
            this.btnLoginApi = new System.Windows.Forms.Button();

            this.loginGroupBox = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxIP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBoxPort = new System.Windows.Forms.TextBox();
            this.checkBoxLocalHostIP = new System.Windows.Forms.CheckBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.textBoxUserID = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBoxAT = new System.Windows.Forms.TextBox();
            this.button3 = new System.Windows.Forms.Button();

            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button7 = new System.Windows.Forms.Button();
            this.textBoxRoomSendMsg = new System.Windows.Forms.TextBox();
            this.listBoxRoomChatMsg = new System.Windows.Forms.ListBox();
            this.listBoxRoomUserList = new System.Windows.Forms.ListBox();
            this.button6 = new System.Windows.Forms.Button();
            this.button5 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.textBoxRoomNumber = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.listBoxLog = new System.Windows.Forms.ListBox();

            this.panel1 = new System.Windows.Forms.Panel();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelTurnTime = new System.Windows.Forms.Label();
            this.btnTest = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();

            //
            // API group box
            //
            this.apiGroupBox.Controls.Add(this.hiveAddrLabel);
            this.apiGroupBox.Controls.Add(this.hiveAddrTextBox);
            this.apiGroupBox.Controls.Add(this.btnCreateAccount);
            this.apiGroupBox.Controls.Add(this.gameAddrLabel);
            this.apiGroupBox.Controls.Add(this.gameAddrTextBox);
            this.apiGroupBox.Controls.Add(this.btnLoginApi);
            this.apiGroupBox.Location = new System.Drawing.Point(16, 10);
            this.apiGroupBox.Name = "apiGroupBox";
            this.apiGroupBox.Size = new System.Drawing.Size(555, 90);
            this.apiGroupBox.TabIndex = 12;
            this.apiGroupBox.TabStop = false;
            this.apiGroupBox.Text = "API Server";
            // 
            // HiveAddrLabel
            // 
            this.hiveAddrLabel.AutoSize = true;
            this.hiveAddrLabel.Location = new System.Drawing.Point(16, 25);
            this.hiveAddrLabel.Name = "label1";
            this.hiveAddrLabel.Size = new System.Drawing.Size(65, 17);
            this.hiveAddrLabel.TabIndex = 0;
            this.hiveAddrLabel.Text = "Hive 서버 주소";
            // 
            // hiveAddrTextBox
            // 
            this.hiveAddrTextBox.Location = new System.Drawing.Point(120, 23);
            this.hiveAddrTextBox.Name = "hiveAddrTextBox";
            this.hiveAddrTextBox.Size = new System.Drawing.Size(300, 25);
            this.hiveAddrTextBox.TabIndex = 1;
            this.hiveAddrTextBox.Text = "127.0.0.1:11500";
            // 
            // btnCreateAccount
            // 
            this.btnCreateAccount.Location = new System.Drawing.Point(430, 20);
            this.btnCreateAccount.Name = "btnCreateAccount";
            this.btnCreateAccount.Size = new System.Drawing.Size(90, 27);
            this.btnCreateAccount.TabIndex = 5;
            this.btnCreateAccount.Text = "계정 생성";
            this.btnCreateAccount.UseVisualStyleBackColor = true;
            this.btnCreateAccount.Click += new System.EventHandler(this.btnCreateAccount_Click);
            // 
            // GameAddrLabel
            // 
            this.gameAddrLabel.AutoSize = true;
            this.gameAddrLabel.Location = new System.Drawing.Point(16, 55);
            this.gameAddrLabel.Name = "label1";
            this.gameAddrLabel.Size = new System.Drawing.Size(65, 17);
            this.gameAddrLabel.TabIndex = 0;
            this.gameAddrLabel.Text = "Game 서버 주소";
            // 
            // hiveAddrTextBox
            // 
            this.gameAddrTextBox.Location = new System.Drawing.Point(120, 53);
            this.gameAddrTextBox.Name = "gameAddrTextBox";
            this.gameAddrTextBox.Size = new System.Drawing.Size(300, 25);
            this.gameAddrTextBox.TabIndex = 1;
            this.gameAddrTextBox.Text = "localhost:11502";
            // 
            // btnCreateAccount
            // 
            this.btnLoginApi.Location = new System.Drawing.Point(430, 50);
            this.btnLoginApi.Name = "btnLoginApi";
            this.btnLoginApi.Size = new System.Drawing.Size(90, 27);
            this.btnLoginApi.TabIndex = 5;
            this.btnLoginApi.Text = "로그인";
            this.btnLoginApi.UseVisualStyleBackColor = true;
            this.btnLoginApi.Click += new System.EventHandler(this.btnLogin_Click);


            //
            // login group box
            //
            this.loginGroupBox.Controls.Add(this.button3);
            this.loginGroupBox.Controls.Add(this.textBoxAT);
            this.loginGroupBox.Controls.Add(this.label4);
            this.loginGroupBox.Controls.Add(this.textBoxUserID);
            this.loginGroupBox.Controls.Add(this.label3);
            this.loginGroupBox.Controls.Add(this.btnDisconnect);
            this.loginGroupBox.Controls.Add(this.btnConnect);
            this.loginGroupBox.Controls.Add(this.checkBoxLocalHostIP);
            this.loginGroupBox.Controls.Add(this.textBoxPort);
            this.loginGroupBox.Controls.Add(this.label2);
            this.loginGroupBox.Controls.Add(this.textBoxIP);
            this.loginGroupBox.Controls.Add(this.label1);
            this.loginGroupBox.Location = new System.Drawing.Point(16, 105);
            this.loginGroupBox.Name = "loginGroupBox";
            this.loginGroupBox.Size = new System.Drawing.Size(555, 115);
            this.loginGroupBox.TabIndex = 12;
            this.loginGroupBox.TabStop = false;
            this.loginGroupBox.Text = "Socket Server";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "서버 주소";
            // 
            // textBoxIP
            // 
            this.textBoxIP.Location = new System.Drawing.Point(82, 45);
            this.textBoxIP.Name = "textBoxIP";
            this.textBoxIP.Size = new System.Drawing.Size(85, 25);
            this.textBoxIP.Text = "34.47.76.34";
            this.textBoxIP.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(181, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "포트 번호";
            // 
            // textBoxPort
            // 
            this.textBoxPort.Location = new System.Drawing.Point(249, 45);
            this.textBoxPort.Name = "textBoxPort";
            this.textBoxPort.Size = new System.Drawing.Size(85, 25);
            this.textBoxPort.Text = "12021";
            this.textBoxPort.TabIndex = 3;
            // 
            // checkBoxLocalHostIP
            // 
            this.checkBoxLocalHostIP.AutoSize = true;
            this.checkBoxLocalHostIP.Checked = false;
            this.checkBoxLocalHostIP.CheckState = System.Windows.Forms.CheckState.Unchecked;
            this.checkBoxLocalHostIP.Location = new System.Drawing.Point(345, 10);
            this.checkBoxLocalHostIP.Name = "checkBoxLocalHostIP";
            this.checkBoxLocalHostIP.Size = new System.Drawing.Size(111, 21);
            this.checkBoxLocalHostIP.TabIndex = 4;
            this.checkBoxLocalHostIP.Text = "localhost 사용";
            this.checkBoxLocalHostIP.UseVisualStyleBackColor = true;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(345, 45);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(70, 27);
            this.btnConnect.TabIndex = 5;
            this.btnConnect.Text = "접속하기";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Location = new System.Drawing.Point(425, 45);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(70, 27);
            this.btnDisconnect.TabIndex = 6;
            this.btnDisconnect.Text = "접속 끊기";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);

            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(48, 17);
            this.label3.TabIndex = 7;
            this.label3.Text = "UserID";
            // 
            // textBoxUserID
            // 
            this.textBoxUserID.Location = new System.Drawing.Point(82, 85);
            this.textBoxUserID.Name = "textBoxUserID";
            this.textBoxUserID.Size = new System.Drawing.Size(85, 25);
            this.textBoxUserID.TabIndex = 8;
            this.textBoxUserID.Text = "jacking751@gmail.com";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(173, 85);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(74, 17);
            this.label4.TabIndex = 9;
            this.label4.Text = "AuthToken";
            // 
            // textBoxAT
            // 
            this.textBoxAT.Location = new System.Drawing.Point(249, 85);
            this.textBoxAT.Name = "textBoxAT";
            this.textBoxAT.Size = new System.Drawing.Size(85, 25);
            this.textBoxAT.TabIndex = 10;
            this.textBoxAT.Text = "eyJhbGciOiJub25lIiwidHlwIjoiSldUIn0.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJqYWNraW5nNzUxQGdtYWlsLmNvbSIsIm5iZiI6MTcxNjExODI2NSwiZXhwIjoxNzE2MTM5ODY1fQ.";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(345, 80);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(70, 27);
            this.button3.TabIndex = 11;
            this.button3.Text = "LogIn";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            

            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button7);
            this.groupBox1.Controls.Add(this.textBoxRoomSendMsg);
            this.groupBox1.Controls.Add(this.listBoxRoomChatMsg);
            this.groupBox1.Controls.Add(this.listBoxRoomUserList);
            this.groupBox1.Controls.Add(this.button6);
            this.groupBox1.Controls.Add(this.button5);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.textBoxRoomNumber);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(16, 220);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(555, 210);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Room";


            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(460, 180);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(44, 26);
            this.button7.TabIndex = 18;
            this.button7.Text = "채팅";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // textBoxRoomSendMsg
            // 
            this.textBoxRoomSendMsg.Location = new System.Drawing.Point(19, 180);
            this.textBoxRoomSendMsg.Name = "textBoxRoomSendMsg";
            this.textBoxRoomSendMsg.Size = new System.Drawing.Size(430, 25);
            this.textBoxRoomSendMsg.TabIndex = 17;
            // 
            // listBoxRoomChatMsg
            // 
            this.listBoxRoomChatMsg.FormattingEnabled = true;
            this.listBoxRoomChatMsg.ItemHeight = 17;
            this.listBoxRoomChatMsg.Location = new System.Drawing.Point(180, 65);
            this.listBoxRoomChatMsg.Name = "listBoxRoomChatMsg";
            this.listBoxRoomChatMsg.Size = new System.Drawing.Size(325, 120);
            this.listBoxRoomChatMsg.TabIndex = 16;
            // 
            // listBoxRoomUserList
            // 
            this.listBoxRoomUserList.FormattingEnabled = true;
            this.listBoxRoomUserList.ItemHeight = 17;
            this.listBoxRoomUserList.Location = new System.Drawing.Point(18, 65);
            this.listBoxRoomUserList.Name = "listBoxRoomUserList";
            this.listBoxRoomUserList.Size = new System.Drawing.Size(150, 120);
            this.listBoxRoomUserList.TabIndex = 15;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(259, 27);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(102, 26);
            this.button6.TabIndex = 14;
            this.button6.Text = "게임 Ready";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(188, 27);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(65, 26);
            this.button5.TabIndex = 13;
            this.button5.Text = "나가기";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(117, 26);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(65, 26);
            this.button4.TabIndex = 12;
            this.button4.Text = "입장";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // textBoxRoomNumber
            // 
            this.textBoxRoomNumber.Location = new System.Drawing.Point(65, 27);
            this.textBoxRoomNumber.Name = "textBoxRoomNumber";
            this.textBoxRoomNumber.Size = new System.Drawing.Size(46, 25);
            this.textBoxRoomNumber.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 17);
            this.label5.TabIndex = 8;
            this.label5.Text = "방 번호";
            // 
            // listBoxLog
            // 
            this.listBoxLog.FormattingEnabled = true;
            this.listBoxLog.ItemHeight = 17;
            this.listBoxLog.Location = new System.Drawing.Point(12, 440);
            this.listBoxLog.Name = "listBoxLog";
            this.listBoxLog.Size = new System.Drawing.Size(565, 200);
            this.listBoxLog.TabIndex = 17;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.labelTurnTime);
            this.panel1.BackColor = System.Drawing.Color.Peru;
            this.panel1.Location = new System.Drawing.Point(585, 53);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(603, 659);
            this.panel1.TabIndex = 18;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            this.panel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseDown);
            this.panel1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.panel1_MouseMove);
            //
            // label Turn Timer
            //
            this.labelTurnTime.AutoSize = true;
            this.labelTurnTime.Location = new System.Drawing.Point(330, 420);
            this.labelTurnTime.Name = "labelTurnTime";
            this.labelTurnTime.Size = new System.Drawing.Size(100, 30);
            this.labelTurnTime.TabIndex = 0;
            this.labelTurnTime.Text = "Time left : 00";
            this.labelTurnTime.Font = new Font("HY견고딕", 10);
            //this.labelTurnTime.TextAlign= System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelStatus
            // 
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(16, 635);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(68, 17);
            this.labelStatus.TabIndex = 19;
            this.labelStatus.Text = "서버 상태:";
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(594, 12);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 24);
            this.btnTest.TabIndex = 20;
            this.btnTest.Text = "test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1200, 660);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.listBoxLog);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.loginGroupBox);
            this.Controls.Add(this.apiGroupBox);
            
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "MainForm";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

            // 새 창을 생성합니다.
            this.registerForm = new Form();
            registerForm.Size = new System.Drawing.Size(500, 250);
            registerForm.Text = "Hive 회원가입";

            // 아이디 라벨을 생성하고 창에 추가합니다.
            this.labelCreateID = new Label();
            labelCreateID.Location = new System.Drawing.Point(20, 20);
            labelCreateID.Text = "ID: ";
            registerForm.Controls.Add(labelCreateID);

            // 아이디 텍스트박스를 생성하고 창에 추가합니다.
            this.textboxCreateID = new TextBox();
            textboxCreateID.Location = new System.Drawing.Point(150, 20);
            textboxCreateID.Name = "txtId";
            textboxCreateID.Size = new System.Drawing.Size(200, 27);
            registerForm.Controls.Add(textboxCreateID);

            // 비밀번호 라벨을 생성하고 창에 추가합니다.
            this.labelCreatePw = new Label();
            labelCreatePw.Location = new System.Drawing.Point(20, 60);
            labelCreatePw.Text = "Password: ";
            registerForm.Controls.Add(labelCreatePw);

            // 비밀번호 텍스트박스를 생성하고 창에 추가합니다.
            this.textboxCreatePw = new TextBox();
            textboxCreatePw.Location = new System.Drawing.Point(150, 60);
            textboxCreatePw.Name = "txtPassword";
            textboxCreatePw.Size = new System.Drawing.Size(200, 27);
            textboxCreatePw.PasswordChar = '*';
            registerForm.Controls.Add(textboxCreatePw);

            // 회원가입 완료 버튼을 생성하고 창에 추가합니다.
            this.btnRegister = new Button();
            btnRegister.Location = new System.Drawing.Point(150, 100);
            btnRegister.Name = "btnRegister";
            btnRegister.Size = new System.Drawing.Size(200, 40);
            btnRegister.Text = "회원가입";
            btnRegister.UseVisualStyleBackColor = true;
            registerForm.Controls.Add(btnRegister);

            // 회원가입 완료 버튼의 클릭 이벤트를 처리하는 로직을 추가합니다.
            btnRegister.Click += new System.EventHandler(this.btnNewFormCreateAccount_Click);

            // 새 창을 생성합니다.
            this.loginForm = new Form();
            loginForm.Size = new System.Drawing.Size(500, 250);
            loginForm.Text = "Hive 로그인";

            // 아이디 라벨을 생성하고 창에 추가합니다.
            this.labelLoginID = new Label();
            labelLoginID.Location = new System.Drawing.Point(20, 20);
            labelLoginID.Text = "ID: ";
            loginForm.Controls.Add(labelLoginID);

            // 아이디 텍스트박스를 생성하고 창에 추가합니다.
            this.textboxLoginID = new TextBox();
            textboxLoginID.Location = new System.Drawing.Point(150, 20);
            textboxLoginID.Name = "txtId";
            textboxLoginID.Size = new System.Drawing.Size(200, 27);
            loginForm.Controls.Add(textboxLoginID);

            // 비밀번호 라벨을 생성하고 창에 추가합니다.
            this.labelLoginPw = new Label();
            labelLoginPw.Location = new System.Drawing.Point(20, 60);
            labelLoginPw.Text = "Password: ";
            loginForm.Controls.Add(labelLoginPw);

            // 비밀번호 텍스트박스를 생성하고 창에 추가합니다.
            this.textboxLoginPw = new TextBox();
            textboxLoginPw.Location = new System.Drawing.Point(150, 60);
            textboxLoginPw.Name = "txtPassword";
            textboxLoginPw.Size = new System.Drawing.Size(200, 27);
            textboxLoginPw.PasswordChar = '*';
            loginForm.Controls.Add(textboxLoginPw);

            // 회원가입 완료 버튼을 생성하고 창에 추가합니다.
            this.btnLogin = new Button();
            btnLogin.Location = new System.Drawing.Point(150, 100);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new System.Drawing.Size(200, 40);
            btnLogin.Text = "로그인";
            btnLogin.UseVisualStyleBackColor = true;
            loginForm.Controls.Add(btnLogin);

            // 회원가입 완료 버튼의 클릭 이벤트를 처리하는 로직을 추가합니다.
            btnLogin.Click += new System.EventHandler(this.btnNewFormLogin_Click);

        }

        #endregion

        private System.Windows.Forms.GroupBox apiGroupBox;
        private System.Windows.Forms.Label hiveAddrLabel;
        private System.Windows.Forms.TextBox hiveAddrTextBox;
        private System.Windows.Forms.Button btnCreateAccount;
        private System.Windows.Forms.Label gameAddrLabel;
        private System.Windows.Forms.TextBox gameAddrTextBox;
        private System.Windows.Forms.Button btnLoginApi;

        private System.Windows.Forms.GroupBox loginGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBoxPort;
        private System.Windows.Forms.CheckBox checkBoxLocalHostIP;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnDisconnect;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBoxUserID;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBoxAT;
        private System.Windows.Forms.Button button3;

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.TextBox textBoxRoomSendMsg;
        private System.Windows.Forms.ListBox listBoxRoomChatMsg;
        private System.Windows.Forms.ListBox listBoxRoomUserList;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.TextBox textBoxRoomNumber;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox listBoxLog;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelTurnTime;
        private System.Windows.Forms.Button btnTest;

        private System.Windows.Forms.Form registerForm;
        private System.Windows.Forms.Label labelCreateID;
        private System.Windows.Forms.TextBox textboxCreateID;
        private System.Windows.Forms.Label labelCreatePw;
        private System.Windows.Forms.TextBox textboxCreatePw;
        private System.Windows.Forms.Button btnRegister;

        private System.Windows.Forms.Form loginForm;
        private System.Windows.Forms.Label labelLoginID;
        private System.Windows.Forms.TextBox textboxLoginID;
        private System.Windows.Forms.Label labelLoginPw;
        private System.Windows.Forms.TextBox textboxLoginPw;
        private System.Windows.Forms.Button btnLogin;


    }
}

