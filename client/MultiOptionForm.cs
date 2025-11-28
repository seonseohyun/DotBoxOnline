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
        private Button btnBack;

        private void BuildUI()
        {
            this.Text = "Multiplay Options";
            this.Size = new System.Drawing.Size(500, 650);

            // 뒤로가기 버튼
            btnBack = new Button();
            btnBack.Text = "◀ Back";            // 원하는 텍스트로 변경 가능
            btnBack.Size = new Size(80, 30);
            btnBack.Location = new Point(10, 10);  // 좌측 상단
            btnBack.Click += BtnBack_Click;
            this.Controls.Add(btnBack);
            btnBack.BringToFront();

            // 제목
            Label lblTitle = new Label();
            lblTitle.Text = "Multiplayer Options";
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(100, 60);
            lblTitle.Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
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
            btnCreateRoom.Click += BtnCreateRoom_Click;
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

            // 초대방 입장 버튼 (Join Room)
            Button btnJoinRoom = new Button();
            btnJoinRoom.Text = "Join Room";
            btnJoinRoom.Font = new System.Drawing.Font("Arial", 11);
            btnJoinRoom.Size = new System.Drawing.Size(100, 28);
            btnJoinRoom.Location = new System.Drawing.Point(350, 378);
            btnJoinRoom.Click += BtnJoinRoom_Click;
            this.Controls.Add(btnJoinRoom);
        }

        // ====== 버튼 이벤트 함수 ======
        
        // 방 "만들기" 클릭
        private async void BtnCreateRoom_Click(object sender, EventArgs e)
        {
            // HomeForm에서 /connect를 이미 호출했다는 가정.
            // 혹시 몰라서 방어 코드도 추가.
            if (string.IsNullOrEmpty(AppSession.PlayerId))
            {
                MessageBox.Show(
                    "서버 연결 정보가 없습니다.\n처음 화면으로 돌아갑니다.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                MainForm m = (MainForm)this.ParentForm;
                m.LoadChildForm(new HomeForm());
                return;
            }

            try
            {
                var roomRes = await ServerApi.CreateRoomAsync(AppSession.PlayerId);

                // 로비로 이동 (초대코드 전달)
                MainForm main = (MainForm)this.ParentForm;
                main.LoadChildForm(new MultiLobbyForm(roomRes.inviteCode));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "방 생성 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // 방 "참가" 클릭
        private async void BtnJoinRoom_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(AppSession.PlayerId))
            {
                MessageBox.Show(
                    "서버 연결 정보가 없습니다.\n처음 화면으로 돌아갑니다.",
                    "오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                MainForm m = (MainForm)this.ParentForm;
                m.LoadChildForm(new HomeForm());
                return;
            }

            string code = txtRoomCode.Text.Trim();

            // placeholder 상태 방지
            if (string.IsNullOrWhiteSpace(code) ||
                (txtRoomCode.ForeColor == System.Drawing.Color.Gray && code == "Enter Room Code"))
            {
                MessageBox.Show(
                    "초대 코드를 입력해주세요.",
                    "입력 필요",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            try
            {
                var joinRes = await ServerApi.JoinRoomAsync(AppSession.PlayerId, code);

                // 성공 시 로비로 이동
                MainForm main = (MainForm)this.ParentForm;
                main.LoadChildForm(new MultiLobbyForm(joinRes.inviteCode));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "방 참가 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        // 뒤로가기 클릭
        private void BtnBack_Click(object sender, EventArgs e)
        {
            // ParentForm 은 항상 MainForm 이므로 캐스팅
            MainForm main = (MainForm)this.ParentForm;

            // 초기 화면으로 전환
            main.LoadChildForm(new HomeForm());
        }
    }
}
