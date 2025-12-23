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
            this.DoubleBuffered = true;
            ApplyTheme();
            SetupPlaceholders();
        }
        private bool _nicknameConfirmed = false;
        
        // 콤보박스(cbBoardSize) 선택 인덱스
        private int GetBoardIndex()
        {
            int idx = cbBoardSize.SelectedIndex;

            // 선택 안 되어있으면 기본값 0(5x5)로 처리
            if (idx < 0) idx = 0;

            return idx;
        }


        // 테마 적용
        private void ApplyTheme()
        {
            // 폼 기본
            Theme.ApplyForm(this);

            // 카드
            Theme.ApplyCard(pnlCard);

            // 버튼들
            Theme.ApplyButton(btnBack);
            Theme.ApplyButton(btnCreateRoom);
            Theme.ApplyButton(btnJoinRoom);
            Theme.ApplyButton(btnNicknameOk);

            // 콤보박스/텍스트박스는 Theme에 넣지 말고 여기서 최소 세팅
            cbBoardSize.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            cbPlayerCount.Font = new Font("Segoe UI", 11f, FontStyle.Bold);

            txtNickname.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            txtRoomCode.Font = new Font("Segoe UI", 11f, FontStyle.Bold);

            cbBoardSize.BackColor = Theme.C_CARD_BG;
            cbPlayerCount.BackColor = Theme.C_CARD_BG;

            txtNickname.BackColor = Theme.C_CARD_BG;
            txtRoomCode.BackColor = Theme.C_CARD_BG;

            // 타이틀 (OutlinedTextControl) - 화면 고유 연출이라 여기서 처리
            lblTitle.Text = "MULTIPLAYER OPTIONS";
            lblTitle.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
            lblTitle.ForeColor = Theme.C_TEXT;
            lblTitle.OutlineColor = Color.FromArgb(170, 170, 170);
            lblTitle.OutlineThickness = 1.3f;
            lblTitle.LetterSpacing = 2.5f;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;

            // 라벨용 Bold 폰트 
            var labelFont = new Font("Segoe UI", 13f, FontStyle.Bold);

            lblBoardSizeTitle.Font = labelFont;
            lblPlayerCountTitle.Font = labelFont;
            lblNicknameTitle.Font = labelFont;
            lblRoomCodeTitle.Font = labelFont;
            lblBoardSizeTitle.ForeColor = Theme.C_TEXT;
            lblPlayerCountTitle.ForeColor = Theme.C_TEXT;
            lblNicknameTitle.ForeColor = Theme.C_TEXT;
            lblRoomCodeTitle.ForeColor = Theme.C_TEXT;
        }

        //  placeholder 처리
        private void SetupPlaceholders()
        {
            SetPlaceholder(txtNickname, "Enter Nickname");
            SetPlaceholder(txtRoomCode, "Enter Room Code");
        }

        private void SetPlaceholder(TextBox tb, string placeholder)
        {
            tb.Tag = placeholder;

            // 초기 상태 placeholder
            tb.Text = placeholder;
            tb.ForeColor = Color.Gray;

            tb.GotFocus += TextBox_GotFocus;
            tb.LostFocus += TextBox_LostFocus;
        }

        private void TextBox_GotFocus(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            string placeholder = tb.Tag as string;

            if (tb.ForeColor == Color.Gray && tb.Text == placeholder)
            {
                tb.Text = "";
                tb.ForeColor = Theme.C_TEXT;
            }
        }

        private void TextBox_LostFocus(object sender, EventArgs e)
        {
            var tb = (TextBox)sender;
            string placeholder = tb.Tag as string;

            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                tb.Text = placeholder;
                tb.ForeColor = Color.Gray;
            }
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
                // 플레이어 수 설정을 세션에 저장
                if (cbPlayerCount.SelectedIndex == 0)     
                    AppSession.MaxPlayers = 2;                     
                else                               
                    AppSession.MaxPlayers = 3;


                int selectedBoardIndex = GetBoardIndex();
                // playerId + nickname 같이 보냄
                //var roomRes = await ServerApi.CreateRoomAsync(AppSession.PlayerId, AppSession.MaxPlayers);
                var roomRes = await ServerApi.CreateRoomAsync(
                    AppSession.PlayerId,
                    AppSession.MaxPlayers,
                    selectedBoardIndex
                );
                AppSession.BoardIndex = roomRes.boardIndex;

                // 로비로 이동 (초대코드 전달)
                MainForm main = (MainForm)this.ParentForm;
                main.LoadChildForm(
                    new MultiLobbyForm(
                        roomRes.roomId,         
                        AppSession.PlayerId,     
                        roomRes.inviteCode,     
                        roomRes.players,        // string[]
                        roomRes.playerInfos,    // PlayerInfo[]
                        0,                       //lastRound (새 방이므로 0)
                        AppSession.BoardIndex
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
                
                // 참가한 사람도 이 방의 인원 수를 알고 있게 함
                AppSession.MaxPlayers = joinRes.maxPlayers;  

                // 2) 참가에 성공하면 해당 roomId의 현재 상태를 조회해서 players 가져오기
                var state = await ServerApi.GetRoomStateAsync(joinRes.roomId);

                var playersList = state.players != null
                    ? state.players.ToList()
                    : new List<string>();

                 AppSession.BoardIndex = state.boardIndex;

                // 3) 로비로 이동
                MainForm main = (MainForm)this.ParentForm;
                var infos = state.playerInfos ?? state.playersInfos;
                main.LoadChildForm(
                    new MultiLobbyForm(
                        joinRes.roomId,          // 방 ID
                        AppSession.PlayerId,     // 내 playerId
                        joinRes.inviteCode,      // 초대 코드
                        state.players,        // string[]
                        infos,     // PlayerInfo[]
                        0,          //lastRound (초기 입장은 0)
                        AppSession.BoardIndex
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
