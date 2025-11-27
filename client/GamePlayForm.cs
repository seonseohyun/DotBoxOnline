using System;
using System.Drawing;
using System.Windows.Forms;
using static DotsAndBoxes.GameResultForm;

namespace DotsAndBoxes
{
    public partial class GamePlayForm : Form
    {
        // 상단 점수/승률 표시
        private TableLayoutPanel tlpScore;
        private Label lblPlayerHeader;
        private Label lblComputerHeader;
        private Label lblPlayerRate;
        private Label lblComputerRate;

        // 게임 보드(도트) 영역
        private Panel pnlGameBoard;

        public enum GameOutcome
        {
            PlayerWin,
            ComputerWin,
            Draw
        }

        public GamePlayForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private void BuildUI()
        {
            // 폼 기본 설정
            this.Text = "Game - Play";
            this.ClientSize = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // === 상단 점수/승률 영역 ===
            tlpScore = new TableLayoutPanel();
            tlpScore.RowCount = 2;
            tlpScore.ColumnCount = 2;
            tlpScore.Dock = DockStyle.Top;
            tlpScore.Height = 80;
            tlpScore.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;

            tlpScore.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlpScore.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tlpScore.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            tlpScore.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

            lblPlayerHeader = new Label();
            lblPlayerHeader.Text = "player";
            lblPlayerHeader.TextAlign = ContentAlignment.MiddleCenter;
            lblPlayerHeader.Dock = DockStyle.Fill;

            lblComputerHeader = new Label();
            lblComputerHeader.Text = "computer";
            lblComputerHeader.TextAlign = ContentAlignment.MiddleCenter;
            lblComputerHeader.Dock = DockStyle.Fill;

            lblPlayerRate = new Label();
            lblPlayerRate.Text = "0";   // 나중에 승률/점수 바인딩
            lblPlayerRate.TextAlign = ContentAlignment.MiddleCenter;
            lblPlayerRate.Dock = DockStyle.Fill;

            lblComputerRate = new Label();
            lblComputerRate.Text = "0"; // 나중에 승률/점수 바인딩
            lblComputerRate.TextAlign = ContentAlignment.MiddleCenter;
            lblComputerRate.Dock = DockStyle.Fill;

            tlpScore.Controls.Add(lblPlayerHeader, 0, 0);
            tlpScore.Controls.Add(lblComputerHeader, 1, 0);
            tlpScore.Controls.Add(lblPlayerRate, 0, 1);
            tlpScore.Controls.Add(lblComputerRate, 1, 1);

            // === 게임 보드 영역 ===
            pnlGameBoard = new Panel();
            pnlGameBoard.Dock = DockStyle.Fill;
            pnlGameBoard.BackColor = Color.White;   // 도트/선 그릴 공간
            // 나중에 OnPaint override 또는 Mouse 이벤트로 게임 구현

            // === 폼에 컨트롤 추가 ===
            this.Controls.Add(pnlGameBoard);
            this.Controls.Add(tlpScore);
        }


        // 게임이 끝났을 때 호출할 메서드
        private void EndGame(GameOutcome outcome)
        {
            // 1) MainForm 찾기
            MainForm main = (MainForm)this.ParentForm;

            // 2) GameResultForm 생성
            var resultForm = new GameResultForm();

            // 3) 결과 텍스트 설정
            switch (outcome)
            {
                case GameOutcome.PlayerWin:
                    resultForm.SetResultText("You Win!");
                    break;
                case GameOutcome.ComputerWin:
                    resultForm.SetResultText("You Lose...");
                    break;
                case GameOutcome.Draw:
                    resultForm.SetResultText("Draw");
                    break;
            }
            // 4) 화면 전환
            main.LoadChildForm(resultForm);


        }

        // 실제 게임 보드/점수 초기화는 여기서 처리
        private void ResetGameBoard()
        {
            // TODO: 게임 데이터 클리어, 점수/승률 초기화, 화면 다시 그림
            // 예시:
            // lblPlayerRate.Text = "0";
            // lblComputerRate.Text = "0";
            // pnlGameBoard.Controls.Clear();
            // 내부 게임 상태도 초기화
        }

        // ★ 나중에 게임 로직에서 이런 식으로 호출하면 됨 ★
        // 예) 플레이어가 이겼을 때:
        private void OnPlayerWin()
        {
            EndGame(GameOutcome.PlayerWin);
        }

        // 예) 컴퓨터 승리:
        private void OnComputerWin()
        {
            EndGame(GameOutcome.ComputerWin);
        }
    }
}
