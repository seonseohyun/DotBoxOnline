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
        private string _roomId;      // 방 ID
        private string _myPlayerId;  // 내 playerId
        private System.Windows.Forms.Timer _lobbyTimer;
        private PlayerInfo[] _playerInfos;   // 닉네임 매핑용
        private List<string> _currentPlayers = new List<string>(); // 게임 화면 상단에 띄울 "닉네임" 리스트
        private List<string> _currentPlayerIds = new List<string>(); // 서버 playerId 리스트 (호스트 판별용)


        public MultiLobbyForm()
        {
            InitializeComponent();
            BuildUI();
        }

        // 방 정보 + 내 playerId + 초대코드 + players + playerInfos 받는 생성자
        public MultiLobbyForm(
            string roomId,
            string myPlayerId,
            string inviteCode,
            string[] players,
            PlayerInfo[] playerInfos) : this()
        {
            _roomId = roomId;
            _myPlayerId = myPlayerId;
            txtInviteCode.Text = inviteCode;
            _playerInfos = playerInfos;   // 닉네임 정보 저장

            // 1) playerId 리스트 저장
            _currentPlayerIds = players != null ? players.ToList() : new List<string>();

            // 2) 닉네임 리스트 생성 (playerInfos 이용)
            _currentPlayers = BuildDisplayNames(_currentPlayerIds, playerInfos);

            // 3) 라벨 업데이트 (닉네임 기준)
            UpdatePlayers(_currentPlayers);

            // 4) 로비 상태 주기적으로 체크
            StartLobbyTimer();
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

        // 타이머 세팅
        private void StartLobbyTimer()
        {
            _lobbyTimer = new System.Windows.Forms.Timer();
            _lobbyTimer.Interval = 1000; // 1초마다
            _lobbyTimer.Tick += async (s, e) => await RefreshRoomStateAsync();
            _lobbyTimer.Start();
        }

        // 닉네임 리스트 만드는 
        private List<string> BuildDisplayNames(List<string> playerIds, PlayerInfo[] infos)
        {
            var result = new List<string>();

            foreach (var id in playerIds)
            {
                var info = infos?.FirstOrDefault(p => p.playerId == id);
                result.Add(info != null ? info.playerName : id);
            }

            return result;
        }

        // 플레이어 리스트
        public void UpdatePlayers(List<string> players)
        {
            // 1) 플레이어 라벨 업데이트
            lblPlayer1.Text = players.Count > 0 ? players[0] : "Waiting for Player1...";
            lblPlayer2.Text = players.Count > 1 ? players[1] : "Waiting for Player2...";
            lblPlayer3.Text = players.Count > 2 ? players[2] : "Waiting for Player3...";

            // 2) 방장 여부 판단 (players[0] = Host)
            bool iAmHost = (_currentPlayerIds.Count > 0 && _currentPlayerIds[0] == _myPlayerId);

            // 3) 방장일 때만 Start 버튼 활성화
            btnStart.Enabled = (iAmHost && players.Count >= 2);

            // 4) 방장 표시 (Player1 라벨 배경색만 변경)
            if (_currentPlayerIds.Count > 0)
            {
                lblPlayer1.BackColor = Color.LightYellow;
                lblPlayer1.Font = new Font(lblPlayer1.Font, FontStyle.Bold);
            }
            else
            {
                lblPlayer1.BackColor = Color.White;
                lblPlayer1.Font = new Font(lblPlayer1.Font, FontStyle.Regular);
            }
        }

        // 시작버튼 클릭
        private async void BtnStart_Click(object sender, EventArgs e)
        {
            // 방장만 눌릴 수 있음 (UpdatePlayers에서 btnStart.Enabled = iAmHost로 관리 중)
            btnStart.Enabled = false;

            try
            {
                // 1) 서버에 게임 시작 요청
                var startRes = await ServerApi.GameStartAsync(_roomId, _myPlayerId);

                // 1-1) 플레이어 목록 준비
                List<string> playersForGame;

                if (_currentPlayers != null && _currentPlayers.Count > 0)
                {
                    // 방 상태에서 이미 받아둔 "닉네임" 목록이 있으면 그걸 사용
                    playersForGame = new List<string>(_currentPlayers);
                }
                else
                {
                    // 혹시 몰라서 다시 한 번 상태 조회
                    var state = await ServerApi.GetRoomStateAsync(_roomId);

                    var playersIdList = state.players != null
                        ? state.players.ToList()
                        : new List<string>();

                    var infos = state.playerInfos ?? state.playersInfos;

                    playersForGame = BuildDisplayNames(playersIdList, infos);
                }

                // 2) 방장 자신의 화면은 바로 게임 화면으로 전환
                MainForm main = (MainForm)this.ParentForm;
                main.LoadChildForm(new GamePlayForm(5, playersForGame));
            }
            catch (Exception ex)
            {
                // 에러 (2명 미만, 이미 시작됨 등) -> 메시지 띄우고 다시 활성화
                MessageBox.Show(
                    ex.Message,
                    "게임 시작 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                btnStart.Enabled = true;
            }
        }

        // 방장 아닌 유저 게임시작 (/room/state 조회 + 게임 시작 감지)
        private async Task RefreshRoomStateAsync()
        {
            try
            {
                var state = await ServerApi.GetRoomStateAsync(_roomId);

                // 1) playerId 리스트
                var playersIdList = state.players != null
                    ? state.players.ToList()
                    : new List<string>();

                // 저장
                _currentPlayerIds = playersIdList;

                // 2) playerInfos 또는 playersInfos 중 살아있는 쪽 사용
                var infos = state.playerInfos ?? state.playersInfos;


                // 닉네임 리스트로 변환
                _currentPlayers = BuildDisplayNames(_currentPlayerIds, infos);
                
                // 3) 라벨 갱신 (닉네임)
                UpdatePlayers(_currentPlayers);

                // 4) 내가 방장이면 여기서 끝
                bool iAmHost = (_currentPlayerIds.Count > 0 && _currentPlayerIds[0] == _myPlayerId);
                if (iAmHost)
                    return;

                // 5) 방장이 아니면 게임 시작 감지
                if (!string.IsNullOrEmpty(state.currentTurn))
                {
                    _lobbyTimer.Stop();

                    MainForm main = (MainForm)this.ParentForm;
                    // 게임 화면에는 닉네임 리스트 넘김
                    main.LoadChildForm(new GamePlayForm(5, _currentPlayers));
                }
            }
            catch
            {
                // 일시적 오류 무시
            }
        }

        // 나가기버튼 클릭
        private async void BtnExit_Click(object sender, EventArgs e)
        {
            // 중복 클릭 방지
            btnExit.Enabled = false;

            try
            {
                // 서버에 방 나가기 요청
                var leaveRes = await ServerApi.LeaveRoomAsync(_roomId, _myPlayerId);
                
                // 로컬 상태 초기화 (이전 기록 리셋) 
                _roomId = null;               
                _myPlayerId = null;

                if (_currentPlayers != null)   // 닉네임 리스트 비우기
                    _currentPlayers.Clear();

                if (_currentPlayerIds != null) // playerId 리스트 비우기
                    _currentPlayerIds.Clear();


                //  타이머 정리
                if (_lobbyTimer != null)
                {
                    _lobbyTimer.Stop();
                    _lobbyTimer.Dispose();
                    _lobbyTimer = null;
                }

                // 홈 화면으로 이동
                MainForm main = (MainForm)this.ParentForm;
                main.LoadChildForm(new HomeForm());
            }
            catch (Exception ex)
            {
                // 실패 → 에러만 보여주고 로비에 그대로 남음
                MessageBox.Show(
                    ex.Message,
                    "방 나가기 실패",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                // 다시 눌러볼 수 있게 버튼 복구
                btnExit.Enabled = true;
            }
        }
    }
}
