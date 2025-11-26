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
    public partial class MultiOptionForm : Form
    {
        public MultiOptionForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private ComboBox cbBoardSize;
        private ComboBox cbPlayerCount;
        private TextBox txtRoomCode;

        private void BuildUI()
        {
            this.Text = "Multiplay Options";
            this.Size = new System.Drawing.Size(500, 650);

            // 제목
            Label lblTitle = new Label();
            lblTitle.Text = "Multiplayer Options";
            lblTitle.Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 80;
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.Controls.Add(lblTitle);

            // 맵 사이즈 선택 (Board Size)
            Label lblBoardSize = new Label();
            lblBoardSize.Text = "Board Size";
            lblBoardSize.Location = new System.Drawing.Point(60, 150);
            lblBoardSize.Font = new System.Drawing.Font("Arial", 14);
            lblBoardSize.AutoSize = true;
            this.Controls.Add(lblBoardSize);

            cbBoardSize = new ComboBox();
            cbBoardSize.Location = new System.Drawing.Point(200, 150);
            cbBoardSize.Items.Add("5 x 5");
            cbBoardSize.Items.Add("6 x 6");
            cbBoardSize.Items.Add("7 x 7");
            cbBoardSize.SelectedIndex = 0;
            this.Controls.Add(cbBoardSize);

            // 플레이어 수 설정 (Player Count)
            Label lblPlayerCount = new Label();
            lblPlayerCount.Text = "Player Count";
            lblPlayerCount.Location = new System.Drawing.Point(60, 210);
            lblPlayerCount.Font = new System.Drawing.Font("Arial", 14);
            lblPlayerCount.AutoSize = true;
            this.Controls.Add(lblPlayerCount);

            cbPlayerCount = new ComboBox();
            cbPlayerCount.Location = new System.Drawing.Point(200, 210);
            cbPlayerCount.Items.Add("2 Players");
            cbPlayerCount.Items.Add("3 Players");
            cbPlayerCount.SelectedIndex = 0;
            this.Controls.Add(cbPlayerCount);

            // 방 만들기 버튼 (Creat Room)
            Button btnCreateRoom = new Button();
            btnCreateRoom.Text = "Create Room";
            btnCreateRoom.Font = new System.Drawing.Font("Arial", 14);
            btnCreateRoom.Size = new System.Drawing.Size(200, 50);
            btnCreateRoom.Location = new System.Drawing.Point(150, 290);
            this.Controls.Add(btnCreateRoom);

            // 초대코드 입력 및 방 참여 버튼 (Room code + Join button)
            txtRoomCode = new TextBox();
            txtRoomCode.Text = "Enter Room Code";
            txtRoomCode.ForeColor = System.Drawing.Color.Gray;
            txtRoomCode.Location = new System.Drawing.Point(60, 380);
            txtRoomCode.Width = 280;

            // Placeholder 기능
            txtRoomCode.GotFocus += (s, e) =>
            {
                if (txtRoomCode.ForeColor == System.Drawing.Color.Gray)
                {
                    txtRoomCode.Text = "";
                    txtRoomCode.ForeColor = System.Drawing.Color.Black;
                }
            };
            txtRoomCode.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtRoomCode.Text))
                {
                    txtRoomCode.Text = "Enter Room Code";
                    txtRoomCode.ForeColor = System.Drawing.Color.Gray;
                }
            };

            this.Controls.Add(txtRoomCode);

            Button btnJoinRoom = new Button();
            btnJoinRoom.Text = "Join Room";
            btnJoinRoom.Font = new System.Drawing.Font("Arial", 11);
            btnJoinRoom.Size = new System.Drawing.Size(100, 28);
            btnJoinRoom.Location = new System.Drawing.Point(350, 378);
            this.Controls.Add(btnJoinRoom);
        }

    }
}
