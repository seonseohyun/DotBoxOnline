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
        private string _roomId;      // 방 ID
        private string _myPlayerId;  // 내 playerId
        private int _maxPlayers;
        private System.Windows.Forms.Timer _lobbyTimer;
        private PlayerInfo[] _playerInfos;   // 닉네임 매핑용
        private List<string> _currentPlayers = new List<string>(); // 게임 화면 상단에 띄울 "닉네임" 리스트
        private List<string> _currentPlayerIds = new List<string>(); // 서버 playerId 리스트 (호스트 판별용)
        private int _lastJoinedGameRound = 0; //마지막으로 입장한 라운드 번호


        public MultiLobbyForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            ApplyTheme();

        }

        // 방 정보 + 내 playerId + 초대코드 + players + playerInfos 받는 생성자
        public MultiLobbyForm(
            string roomId,
            string myPlayerId,
            string inviteCode,
            string[] players,
            PlayerInfo[] playerInfos,
            int lastRound ) : this()
        {
            _roomId = roomId;
            _myPlayerId = myPlayerId;
            _lastJoinedGameRound = lastRound;
            _maxPlayers = AppSession.MaxPlayers;

            // 재시작 여부 판단 
            bool isRestart =
                string.IsNullOrEmpty(inviteCode) ||
                players == null || players.Length == 0 ||
                playerInfos == null || playerInfos.Length == 0;

            if (isRestart)
            {
                // 재시작이면 서버에서 최신 방 상태 다시 가져오기
                this.Load += async (s, e) =>
                {
                    var state = await ServerApi.GetRoomStateAsync(_roomId);

                    txtInviteCode.Text = state.inviteCode;

                    _currentPlayerIds = state.players != null
                        ? state.players.ToList()
                        : new List<string>();

                    var infos = state.playerInfos ?? state.playersInfos;

                    _currentPlayers = BuildDisplayNames(_currentPlayerIds, infos);

                    UpdatePlayers(_currentPlayers);

                    StartLobbyTimer();
                };
                return; // 기존 코드 실행 안 함
            }

            // 초대코드로 정상 입장 시
            txtInviteCode.Text = inviteCode;
            _playerInfos = playerInfos;   // 닉네임 정보 저장
            _currentPlayerIds = players != null ? players.ToList() : new List<string>(); // playerId 리스트 저장
            _currentPlayers = BuildDisplayNames(_currentPlayerIds, playerInfos); // 닉네임 리스트 생성
            UpdatePlayers(_currentPlayers); // 라벨 업데이트 (닉네임 기준)
            StartLobbyTimer(); // 로비 상태 주기적으로 체크
        }
        // 테마/스타일 적용
        private void ApplyTheme()
        {
            Theme.ApplyForm(this);
            Theme.ApplyCard(pnlCard);

            // 버튼
            Theme.ApplyButton(btnStart);
            Theme.ApplyButton(btnExit);
            Theme.ApplyButton(btnBack);

            // 타이틀
            lblTitle.Text = "MULTI PLAY LOBBY";
            lblTitle.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
            lblTitle.ForeColor = Theme.C_TEXT;
            lblTitle.OutlineColor = Color.FromArgb(170, 170, 170);
            lblTitle.OutlineThickness = 1.3f;
            lblTitle.LetterSpacing = 2.5f;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // 섹션 라벨
            var sectionFont = new Font("Segoe UI", 12.5f, FontStyle.Bold);
            lblInviteCodeTitle.Font = sectionFont;
            lblPlayersTitle.Font = sectionFont;

            lblInviteCodeTitle.ForeColor = Theme.C_TEXT;
            lblPlayersTitle.ForeColor = Theme.C_TEXT;

            // 초대코드 텍스트박스
            txtInviteCode.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            txtInviteCode.BackColor = Theme.C_CARD_BG;
            txtInviteCode.ForeColor = Theme.C_TEXT;

            // 플레이어 슬롯 라벨
            var slotFont = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblPlayer1.Font = slotFont;
            lblPlayer2.Font = slotFont;
            lblPlayer3.Font = slotFont;

            lblPlayer1.BorderStyle = BorderStyle.FixedSingle;
            lblPlayer2.BorderStyle = BorderStyle.FixedSingle;
            lblPlayer3.BorderStyle = BorderStyle.FixedSingle;

            lblPlayer1.BackColor = Theme.C_CARD_BG;
            lblPlayer2.BackColor = Theme.C_CARD_BG;
            lblPlayer3.BackColor = Theme.C_CARD_BG;

            lblPlayer1.ForeColor = Theme.C_TEXT;
            lblPlayer2.ForeColor = Theme.C_TEXT;
            lblPlayer3.ForeColor = Theme.C_TEXT;
        }

        // [추가] 뒤로가기 버튼 (로비에서 홈으로)
        private void BtnBack_Click(object sender, EventArgs e)
        {
            // 타이머 정리
            if (_lobbyTimer != null)
            {
                _lobbyTimer.Stop();
                _lobbyTimer.Dispose();
                _lobbyTimer = null;
            }

            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new HomeForm());
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
            lblPlayer1.ForeColor = Color.Black;                               
            lblPlayer2.ForeColor = Color.Black;                              
            lblPlayer3.ForeColor = Color.Black;

            if (_maxPlayers == 2)                                             
            {
                // 2인 모드에서는 3번 슬롯 비활성/회색 처리                                     
                lblPlayer3.Text = " X ";
                lblPlayer3.ForeColor = Color.Gray;
                
            }

            // 2) 방장 여부 판단 (players[0] = Host)
            bool iAmHost = (_currentPlayerIds.Count > 0 && _currentPlayerIds[0] == _myPlayerId);

            // 3) 방장일 때만 Start 버튼 활성화
            btnStart.Enabled = (iAmHost && players.Count >= 2);

            // 4) 방장 표시 (Player1 라벨 배경색만 변경)
            if (_currentPlayerIds.Count > 0)
            {
                lblPlayer1.BackColor = Color.LemonChiffon;
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

                // 2) 게임 시작 후, 최신 방 상태 조회
                var state = await ServerApi.GetRoomStateAsync(_roomId);
                int gameRound = state.gameRound;

                // 2-1) 플레이어 목록 준비
                List<string> playersForGame;

                if (_currentPlayers != null && _currentPlayers.Count > 0)
                {
                    // 방 상태에서 이미 받아둔 "닉네임" 목록이 있으면 그걸 사용
                    playersForGame = new List<string>(_currentPlayers);
                }
                else
                {

                    var playersIdList = state.players != null
                        ? state.players.ToList()
                        : new List<string>();

                    var infos = state.playerInfos ?? state.playersInfos;

                    playersForGame = BuildDisplayNames(playersIdList, infos);
                }

                // 2) 방장 자신의 화면은 바로 게임 화면으로 전환
                MainForm main = (MainForm)this.ParentForm;
                main.LoadChildForm(new GamePlayForm(5, playersForGame, _currentPlayerIds, _roomId, _myPlayerId, gameRound));
            }
            catch (Exception ex)
            {
                // 에러 (2명 미만, 이미 시작됨 등)메시지 띄우고 다시 활성화
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
                if (!string.IsNullOrEmpty(state.currentTurn) && state.gameRound > _lastJoinedGameRound)
                {
                    // 새 라운드로 갱신
                    _lastJoinedGameRound = state.gameRound;
                    _lobbyTimer.Stop();

                    MainForm main = (MainForm)this.ParentForm;
                    // 게임 화면에 정보넘김
                    main.LoadChildForm(new GamePlayForm(5, _currentPlayers, _currentPlayerIds, _roomId, _myPlayerId, state.gameRound));
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
