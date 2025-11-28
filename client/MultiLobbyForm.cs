using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DotsAndBoxes
{
    public partial class MultiLobbyForm : Form
    {
        private TextBox txtInviteCode;
        private Label lblPlayer1;
        private Label lblPlayer2;
        private Label lblPlayer3;
        private Button btnStart;
        private Button btnExit;


        public MultiLobbyForm()
        {
            InitializeComponent();
            BuildUI();
        }

        // 초대코드만 받는 생성자 
        public MultiLobbyForm(string inviteCode) : this()
        {
            txtInviteCode.Text = inviteCode;
        }

        private void BuildUI()
        {
            this.Text = "Multi Lobby";
            this.BackColor = Color.White;
            this.Size = new Size(500, 600);

            // ===== 제목 =====
            var lblTitle = new Label();
            lblTitle.Text = "Multi Play Lobby";
            lblTitle.Font = new Font("맑은 고딕", 18, FontStyle.Bold);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(140, 40);
            this.Controls.Add(lblTitle);

            // ===== 초대 코드 영역 =====
            var lblCodeTitle = new Label();
            lblCodeTitle.Text = "Invite Code";
            lblCodeTitle.Font = new Font("맑은 고딕", 12, FontStyle.Regular);
            lblCodeTitle.AutoSize = true;
            lblCodeTitle.Location = new Point(60, 120);
            this.Controls.Add(lblCodeTitle);

            txtInviteCode = new TextBox();
            txtInviteCode.Location = new Point(60, 150);
            txtInviteCode.Width = 360;
            txtInviteCode.ReadOnly = true;             // 초대코드 표시 전용
            txtInviteCode.TextAlign = HorizontalAlignment.Center;
            //txtInviteCode.Text = "ABCD-1234";          // TODO: 실제 서버 코드로 교체
            this.Controls.Add(txtInviteCode);

            // ===== 플레이어 슬롯 영역 =====
            var lblPlayersTitle = new Label();
            lblPlayersTitle.Text = "Players";
            lblPlayersTitle.Font = new Font("맑은 고딕", 12, FontStyle.Regular);
            lblPlayersTitle.AutoSize = true;
            lblPlayersTitle.Location = new Point(60, 210);
            this.Controls.Add(lblPlayersTitle);

            // Player1
            lblPlayer1 = new Label();
            lblPlayer1.BorderStyle = BorderStyle.FixedSingle;
            lblPlayer1.Text = "Player1 (Host)";
            lblPlayer1.TextAlign = ContentAlignment.MiddleLeft;
            lblPlayer1.Font = new Font("맑은 고딕", 11);
            lblPlayer1.Size = new Size(360, 35);
            lblPlayer1.Location = new Point(60, 240);
            this.Controls.Add(lblPlayer1);

            // Player2
            lblPlayer2 = new Label();
            lblPlayer2.BorderStyle = BorderStyle.FixedSingle;
            lblPlayer2.Text = "Waiting for Player2...";
            lblPlayer2.TextAlign = ContentAlignment.MiddleLeft;
            lblPlayer2.Font = new Font("맑은 고딕", 11);
            lblPlayer2.Size = new Size(360, 35);
            lblPlayer2.Location = new Point(60, 280);
            this.Controls.Add(lblPlayer2);

            // Player3
            lblPlayer3 = new Label();
            lblPlayer3.BorderStyle = BorderStyle.FixedSingle;
            lblPlayer3.Text = "Waiting for Player3...";
            lblPlayer3.TextAlign = ContentAlignment.MiddleLeft;
            lblPlayer3.Font = new Font("맑은 고딕", 11);
            lblPlayer3.Size = new Size(360, 35);
            lblPlayer3.Location = new Point(60, 320);
            this.Controls.Add(lblPlayer3);

            // ===== Start 버튼 =====
            btnStart = new Button();
            btnStart.Text = "Start";
            btnStart.Font = new Font("맑은 고딕", 14, FontStyle.Bold);
            btnStart.Size = new Size(220, 60);
            btnStart.Location = new Point(140, 420);
            btnStart.Click += BtnStart_Click;
            this.Controls.Add(btnStart);

            // ===== Exit 버튼 =====
            btnExit = new Button();
            btnExit.Text = "나가기";
            btnExit.Font = new Font("맑은 고딕", 12, FontStyle.Bold);
            btnExit.Size = new Size(220, 50);
            btnExit.Location = new Point(140, 500);    // Start 밑에 배치
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);
        }

        // TODO: 나중에 서버에서 유저 리스트를 받을 때 이 메서드를 이용해서 UI 업데이트
        public void UpdatePlayers(string p1, string p2, string p3)
        {
            if (!string.IsNullOrEmpty(p1)) lblPlayer1.Text = p1;
            if (!string.IsNullOrEmpty(p2)) lblPlayer2.Text = p2;
            if (!string.IsNullOrEmpty(p3)) lblPlayer3.Text = p3;
        }

        // Start 버튼 클릭 → GamePlayForm 으로 화면 전환
        private void BtnStart_Click(object sender, EventArgs e)
        {
            // TODO: 여기에서 "모든 플레이어가 준비됐는지" 검사하는 로직 추가 예정

            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new GamePlayForm());
        }

        // 나가기 클릭
        private void BtnExit_Click(object sender, EventArgs e)
        {
            // TODO: 서버에 "로비 나가기" 요청 보내기 (HTTP)
            // 예: POST /lobby/leave { userId, inviteCode }
            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new HomeForm());
        }

    }
}
