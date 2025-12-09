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
        private TextBox txtNickname;
        private bool _nicknameConfirmed = false;

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
            btnCreateRoom.Location = new System.Drawing.Point(150, 478);
            btnCreateRoom.Click += BtnCreateRoom_Click;
            this.Controls.Add(btnCreateRoom);


            // 초대코드 입력 및 방 참여 버튼 (Room code + Join button)
            txtRoomCode = new TextBox();
            txtRoomCode.Text = "Enter Room Code";
            txtRoomCode.ForeColor = System.Drawing.Color.Gray;
            txtRoomCode.Location = new System.Drawing.Point(60, 380);
            txtRoomCode.Width = 220;
            
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
            btnJoinRoom.Size = new System.Drawing.Size(150, 28);
            btnJoinRoom.Location = new System.Drawing.Point(350, 378);
            btnJoinRoom.Click += BtnJoinRoom_Click;
            this.Controls.Add(btnJoinRoom);

            // 닉네임 입력 (Nickname TextBox)
            txtNickname = new TextBox();
            txtNickname.Text = "Enter Nickname";
            txtNickname.ForeColor = System.Drawing.Color.Gray;
            txtNickname.Location = new System.Drawing.Point(60, 330);
            txtNickname.Width = 220;

            // Placeholder 기능 
            txtNickname.GotFocus += (s, e) =>
            {
                if (txtNickname.ForeColor == System.Drawing.Color.Gray)
                {
                    txtNickname.Text = "";
                    txtNickname.ForeColor = System.Drawing.Color.Black;
                }
            };
            txtNickname.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(txtNickname.Text))
                {
                    txtNickname.Text = "Enter Nickname";
                    txtNickname.ForeColor = System.Drawing.Color.Gray;
                }
            };
            this.Controls.Add(txtNickname);
            // 닉네임 변경 시 NicknameConfirmed 초기화
            txtNickname.TextChanged += TxtNickname_TextChanged;

            // 닉네임 확인 버튼
            Button btnNicknameOk = new Button();
            btnNicknameOk.Text = "Nickname OK";
            btnNicknameOk.Font = new System.Drawing.Font("Arial", 11);
            btnNicknameOk.Size = new System.Drawing.Size(150, 28);
            btnNicknameOk.Location = new System.Drawing.Point(350, 328);
            btnNicknameOk.Click += BtnNicknameOk_Click;   // 아래에서 만들 핸들러
            this.Controls.Add(btnNicknameOk);
        }

        // ====== 버튼 이벤트 함수 ======

        // 닉네임 확인 클릭
        private async void BtnNicknameOk_Click(object sender, EventArgs e)
        {
            string nickname = txtNickname.Text.Trim();

            // 아직 입력 안 했거나, placeholder 상태일 때
            if (string.IsNullOrWhiteSpace(nickname) || txtNickname.ForeColor == System.Drawing.Color.Gray)
            {
                MessageBox.Show("닉네임을 입력해 주세요.", "닉네임 확인",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtNickname.Focus();
                return;
            }
            try
            {
                // 서버에 /connect 요청 보내서 세션 발급
                var res = await ServerApi.ConnectAsync(nickname);

                // AppSession에 서버에서 받은 playerId / playerName 저장
                AppSession.PlayerId = res.playerId;
                AppSession.PlayerName = res.playerName;
                
                // 닉네임 확인 플래그
                _nicknameConfirmed = true;

                // 여기까지 왔으면 정상 입력 + 서버 등록 성공
                MessageBox.Show(
                    $"닉네임이 \"{res.playerName}\"(으)로 등록되었습니다.",
                    "닉네임 확인",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "서버 연결에 실패했습니다.\n" + ex.Message,
                    "닉네임 확인 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }


        }

        // 닉네임 재입력시 다시 확인
        private void TxtNickname_TextChanged(object sender, EventArgs e)
        {
            // 확인 상태와 세션을 초기화
            _nicknameConfirmed = false;
            AppSession.PlayerId = null;
            AppSession.PlayerName = null;
        }

        // 방 "만들기" 클릭
        private async void BtnCreateRoom_Click(object sender, EventArgs e)
        {
            // 닉네임
            string nickname = txtNickname.Text.Trim();
            // 초대코드 텍스트
            string code = txtRoomCode.Text.Trim();

            // 닉네임 검사 추가 (placeholder 포함)
            if (string.IsNullOrWhiteSpace(nickname) || txtNickname.ForeColor == System.Drawing.Color.Gray)
            {
                MessageBox.Show("닉네임을 입력해주세요.", "알림",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtNickname.Focus();
                return;
            }
            // 초대코드가 입력돼 있으면 새 방 생성 막기
            if (!string.IsNullOrWhiteSpace(code) &&
                !(txtRoomCode.ForeColor == System.Drawing.Color.Gray && code == "Enter Room Code"))
            {
                MessageBox.Show(
                    "초대코드 입력시 새로운 방을 생성할 수 없습니다.",
                    "방 생성 불가",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }
            try
            {
                // playerId + nickname 같이 보냄
                var roomRes = await ServerApi.CreateRoomAsync(AppSession.PlayerId);

                // 로비로 이동 (초대코드 전달)
                MainForm main = (MainForm)this.ParentForm;
                main.LoadChildForm(
                    new MultiLobbyForm(
                        roomRes.roomId,         
                        AppSession.PlayerId,     
                        roomRes.inviteCode,     
                        roomRes.players,        // string[]
                        roomRes.playerInfos     // PlayerInfo[]
                    )
                );
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
            string nickname = txtNickname.Text.Trim();
            string code = txtRoomCode.Text.Trim();

            // [1] 닉네임 입력 여부 체크
            if (string.IsNullOrWhiteSpace(nickname) ||
                (txtNickname.ForeColor == Color.Gray && nickname == "Enter Nickname"))
            {
                MessageBox.Show(
                    "닉네임을 먼저 입력해주세요.",
                    "입장 불가",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // [2] 닉네임 확인 버튼 눌렀는지 체크
            if (!_nicknameConfirmed)
            {
                MessageBox.Show(
                    "닉네임 확인 버튼을 눌러주세요.",
                    "입장 불가",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // [3] 초대코드 입력 여부
            if (string.IsNullOrWhiteSpace(code) ||
                (txtRoomCode.ForeColor == Color.Gray && code == "Enter Room Code"))
            {
                MessageBox.Show(
                    "초대 코드를 입력해주세요.",
                    "입력 필요",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return;
            }

            // [4] 세션(PlayerId) 유효성 체크
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
                // 방 참가
                var joinRes = await ServerApi.JoinRoomAsync(AppSession.PlayerId, code);

                // 2) 참가에 성공했으니 해당 roomId의 현재 상태를 조회해서 players 가져오기
                var state = await ServerApi.GetRoomStateAsync(joinRes.roomId);

                var playersList = state.players != null
                    ? state.players.ToList()
                    : new List<string>();

                // 3) 로비로 이동
                MainForm main = (MainForm)this.ParentForm;
                var infos = state.playerInfos ?? state.playersInfos;
                main.LoadChildForm(
                    new MultiLobbyForm(
                        joinRes.roomId,          // 방 ID
                        AppSession.PlayerId,     // 내 playerId
                        joinRes.inviteCode,      // 초대 코드
                        state.players,        // string[]
                        infos     // PlayerInfo[]
                    )
                );
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
