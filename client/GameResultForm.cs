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

            this.DoubleBuffered = true;
            ApplyTheme();
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
            pnlButtonArea.BackColor = Theme.C_BG;

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
        public void SetScoreSummary(string summaryText)
        {
            lblScoreSummary.Text = summaryText ?? "";
        }
    }
}
