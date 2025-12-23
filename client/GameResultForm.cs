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
    public partial class GameResultForm : Form
    {
        private Label lblEmojiLine;
        // 재시작에 필요한 정보 저장용
        private int _boardSize;
        private bool _isAIMode;
        private List<string> _players;
        private GamePlayForm.AIDifficulty _aiDifficulty;
        private string _roomId;
        private string _myPlayerId;
        private int _gameRound;
        private int _boardIndex;

        // 외부에서 읽을 용도
        public GameResultAction Action { get; private set; } = GameResultAction.None;

        // 결과창에서 어떤 버튼 눌렀는지 구분용
        public enum GameResultAction
        {
            None,
            Restart,
            GoMain
        }
        //  게임 설정을 받는 생성자
        public GameResultForm(
            int boardSize,
            bool isAIMode,
            List<string> players,
            GamePlayForm.AIDifficulty aiDifficulty)
        {
            _boardSize = boardSize;
            _isAIMode = isAIMode;
            _aiDifficulty = aiDifficulty;
            _players = players ?? new List<string>();

            InitializeComponent();

            this.DoubleBuffered = true;
            ApplyTheme();
            InitEmojiLabel();

            this.Load += (s, e) => CenterButtons();
            this.Resize += (s, e) => CenterButtons();
            pnlButtonArea.Resize += (s, e) => CenterButtons();
        }

        // 멀티용 생성자
        public GameResultForm(
            int boardSize,
            bool isAIMode,
            List<string> players,
            GamePlayForm.AIDifficulty aiDifficulty,
            string roomId,
            string myPlayerId,
            int gameRound)
            : this(boardSize, isAIMode, players, aiDifficulty)
        {
            _roomId = roomId;
            _myPlayerId = myPlayerId;
            _gameRound = gameRound;
        }

        private void ApplyTheme()
        {
            Theme.ApplyForm(this);
            pnlButtonArea.BackColor = Theme.C_CARD_BG;

            // 큰 결과 텍스트
            lblResultMessage.ForeColor = Theme.C_TEXT;
            lblResultMessage.Font = new Font("Segoe UI", 34f, FontStyle.Bold);

            // 점수요약
            lblScoreSummary.ForeColor = Theme.C_TEXT_DIM;
            lblScoreSummary.Font = new Font("Segoe UI", 20f, FontStyle.Bold);

            // 버튼
            Theme.ApplyButton(btnRestart);
            Theme.ApplyButton(btnGoMain);

            btnRestart.Text = "RESTART";
            btnGoMain.Text = "MAIN";
            btnRestart.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            btnGoMain.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
        }
        /// 이모지 라벨
        private void InitEmojiLabel()
        {
            // 이미 만들었으면 중복 생성 방지
            if (lblEmojiLine != null) return;

            lblEmojiLine = new Label();

            // 자동 크기 쓰면 이모지가 줄바꿈/흔들릴 수 있어서 고정 높이 추천
            lblEmojiLine.AutoSize = false;
            lblEmojiLine.Height = 52; // 여백 채우기용. 원하면 40~70 사이로 조절
            lblEmojiLine.TextAlign = ContentAlignment.MiddleCenter;

            // 이모지는 폰트 영향 큼 -> 윈도우 기본 이모지 폰트로 고정
            lblEmojiLine.Font = new Font("Segoe UI Emoji", 22f, FontStyle.Regular);

            // 테마 색상 적용
            lblEmojiLine.ForeColor = Theme.C_TEXT;
            lblEmojiLine.BackColor = Color.Transparent;

            // 기본은 비워둠 (SetResultText에서 설정)
            lblEmojiLine.Text = "";

            // ------------------------------
            // 배치: lblScoreSummary 바로 아래에 위치시키기
            // ------------------------------
            // ScoreSummary 라벨 기준으로 아래에 붙인다.
            lblEmojiLine.Left = lblScoreSummary.Left;
            lblEmojiLine.Top = lblScoreSummary.Bottom + 10; // 스코어 아래 간격
            lblEmojiLine.Width = lblScoreSummary.Width;

            // 컨트롤 추가
            this.Controls.Add(lblEmojiLine);

            // 혹시 버튼 패널이 위로 덮는 경우를 방지: 라벨을 앞으로
            lblEmojiLine.BringToFront();
        }
        // 버튼 가운데 정렬
        private void CenterButtons()
        {
            // 버튼 사이 간격
            int gap = 30;

            // 두 버튼 총 너비
            int totalWidth = btnGoMain.Width + gap + btnRestart.Width;

            // pnlButtonArea 안에서 시작 X
            int startX = (pnlButtonArea.ClientSize.Width - totalWidth) / 2;

            // pnlButtonArea 안에서 Y(세로 중앙 느낌)
            int y = (pnlButtonArea.ClientSize.Height - btnGoMain.Height) / 2 -32;

            btnGoMain.Location = new Point(startX, y);
            btnRestart.Location = new Point(startX + btnGoMain.Width + gap, y);

        }
        private void BtnRestart_Click(object sender, EventArgs e)
        {
            Action = GameResultAction.Restart;
            MainForm main = (MainForm)this.ParentForm;

            if (_isAIMode)
            {
                // 싱글(AI) 모드는 AI 난이도 포함해서 다시 시작
                main.LoadChildForm(new GamePlayForm(_boardSize, _aiDifficulty));
            }
            else
            {
                main.LoadChildForm(new MultiLobbyForm(_roomId, _myPlayerId, null, null, null, _gameRound, _boardIndex));
            }

        }

        private void BtnMain_Click(object sender, EventArgs e)
        {
            Action = GameResultAction.GoMain;

            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new HomeForm()); // 또는 Main 화면으로
        }

        // 결과 텍스트 바꾸는 용도
        public void SetResultText(string resultText)
        {
            lblResultMessage.Text = resultText;

            lblEmojiLine.AutoSize = false;
            lblEmojiLine.Width = this.ClientSize.Width;
            lblEmojiLine.TextAlign = ContentAlignment.MiddleCenter;
            lblEmojiLine.Left = 0;
            lblEmojiLine.Top = lblScoreSummary.Bottom + 10;
            lblEmojiLine.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            InitEmojiLabel();

            string upper = (resultText ?? "").ToUpperInvariant();

            if (upper.Contains("WIN"))
            {
                lblEmojiLine.Text = "🎉 🎉 🎉 🎉 🎉 🎉 🎉";
            }
            else if (upper.Contains("DRAW"))
            {
                lblEmojiLine.Text = "💢 💢 💢 💢 💢 💢 💢";
            }
            else if (upper.Contains("LOSE") || upper.Contains("LOST") || upper.Contains("YOU LOSE"))
            {
                lblEmojiLine.Text = "💔 💔 💔 💔 💔 💔 💔";
            }
            else
            {
                lblEmojiLine.Text = "";
            }

        }
        public void SetScoreSummary(string summaryText)
        {
            lblScoreSummary.Text = summaryText ?? "";
        }
    }
}
