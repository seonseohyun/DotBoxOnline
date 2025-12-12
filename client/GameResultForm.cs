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
        private Label lblResultMessage;
        private Label lblScoreSummary; // 점수 요약용 라벨
        private Button btnRestart;
        private Button btnGoMain;
        private Panel pnlButtonArea;

        // 재시작에 필요한 정보 저장용
        private int _boardSize;
        private bool _isAIMode;
        private List<string> _players;
        private GamePlayForm.AIDifficulty _aiDifficulty;
        private string _roomId;
        private string _myPlayerId;
        private int _gameRound;

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
            BuildUI();
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

        private void BuildUI()
        {
            // 폼 기본 설정
            this.Text = "Game - Result";
            this.ClientSize = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            // === 결과 메시지 영역 ===
            lblResultMessage = new Label();
            lblResultMessage.Text = "win"; // 나중에 win/lose/draw로 변경
            lblResultMessage.Dock = DockStyle.Top;
            lblResultMessage.Height = 120;
            lblResultMessage.Font = new Font("맑은 고딕", 24, FontStyle.Bold);
            lblResultMessage.TextAlign = ContentAlignment.MiddleCenter;

            // === 점수 요약 라벨 ===
            lblScoreSummary = new Label();                                  // 수정
            lblScoreSummary.Text = "";                                      // 수정
            lblScoreSummary.Dock = DockStyle.Top;                           // 수정
            lblScoreSummary.Height = 40;                                    // 수정
            lblScoreSummary.Font = new Font("맑은 고딕", 12, FontStyle.Regular); // 수정
            lblScoreSummary.TextAlign = ContentAlignment.MiddleCenter;      // 수정

            // === 버튼 영역 ===
            pnlButtonArea = new Panel();
            pnlButtonArea.Dock = DockStyle.Bottom;
            pnlButtonArea.Height = 100;

            btnRestart = new Button();
            btnRestart.Text = "restart";
            btnRestart.Size = new Size(120, 40);
            btnRestart.Location = new Point(50, 30);
            btnRestart.Click += BtnRestart_Click;

            btnGoMain = new Button();
            btnGoMain.Text = "main";
            btnGoMain.Size = new Size(120, 40);
            btnGoMain.Location = new Point(230, 30);
            btnGoMain.Click += BtnMain_Click;

            pnlButtonArea.Controls.Add(btnRestart);
            pnlButtonArea.Controls.Add(btnGoMain);

            // === 폼에 컨트롤 추가 ===
            this.Controls.Add(pnlButtonArea);
            this.Controls.Add(lblScoreSummary);
            this.Controls.Add(lblResultMessage);
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
                main.LoadChildForm(new MultiLobbyForm(_roomId, _myPlayerId, null, null, null, _gameRound));
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
        }

        // 점수 요약 텍스트 바꾸는 용도
        public void SetScoreSummary(string summaryText)
        {
            lblScoreSummary.Text = summaryText;
        }
    }
}
