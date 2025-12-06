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

        // 초대코드 + playerId 같이 받는 생성자
        public MultiLobbyForm(string inviteCode, List<string> players) : this()
        {
            txtInviteCode.Text = inviteCode;
            // 서버에서 받은 players 리스트
            UpdatePlayers(players);
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
            txtInviteCode.ReadOnly = true;
            txtInviteCode.TextAlign = HorizontalAlignment.Center;
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
            btnExit.Location = new Point(140, 500);
            btnExit.Click += BtnExit_Click;
            this.Controls.Add(btnExit);
        }

        // 플레이어 리스트
        public void UpdatePlayers(List<string> players)
        {
            lblPlayer1.Text = players.Count > 0 ? players[0] : "Waiting for Player1...";
            lblPlayer2.Text = players.Count > 1 ? players[1] : "Waiting for Player2...";
            lblPlayer3.Text = players.Count > 2 ? players[2] : "Waiting for Player3...";
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new GamePlayForm(5)); // 멀티 모드 보드 크기 5
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new HomeForm());
        }
    }
}
