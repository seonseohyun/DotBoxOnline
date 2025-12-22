using System.Drawing;
using System.Windows.Forms;

namespace DotsAndBoxes
{
    public partial class GamePlayForm
    {
        private System.ComponentModel.IContainer components = null;

        // 상단 점수/플레이어 표시 (변수명 고정)
        private Label lblPlayer1Score;
        private Label lblPlayer2Score;
        private Label lblPlayer3Score;

        // 테두리 + 표 컨테이너
        private Panel scorePanel;
        private TableLayoutPanel scoreTable;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();

            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.scorePanel = new Panel();
            this.scoreTable = new TableLayoutPanel();

            // 변수명 유지
            this.lblPlayer1Score = new Label();
            this.lblPlayer2Score = new Label();
            this.lblPlayer3Score = new Label();

            this.scorePanel.SuspendLayout();
            this.scoreTable.SuspendLayout();
            this.SuspendLayout();

            // Form 기본 속성
            this.Text = "Dots & Boxes";
            this.ClientSize = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true; // 깜빡임 완화(원하면 GamePlayForm.cs에 둬도 됨)

            // scorePanel : 표 외곽선(바깥 테두리)
            this.scorePanel.Name = "scorePanel";
            this.scorePanel.Dock = DockStyle.Top;
            this.scorePanel.Height = 60;
            this.scorePanel.BorderStyle = BorderStyle.FixedSingle;
            this.scorePanel.BackColor = Color.FromArgb(110, 110, 110);
            this.scorePanel.Padding = new Padding(2);


            // scoreTable : 표 내부(셀 경계선)
            this.scoreTable.Name = "scoreTable";
            this.scoreTable.Dock = DockStyle.Fill;
            this.scoreTable.RowCount = 1;
            this.scoreTable.ColumnCount = 3;
            this.scoreTable.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            //this.scoreTable.BackColor = Color.Black;


            this.scoreTable.ColumnStyles.Clear();
            this.scoreTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            this.scoreTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
            this.scoreTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34f));

            this.scoreTable.RowStyles.Clear();
            this.scoreTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // lblPlayer1Score (변수명 유지)
            this.lblPlayer1Score.Name = "lblPlayer1Score";
            this.lblPlayer1Score.Dock = DockStyle.Fill;
            this.lblPlayer1Score.TextAlign = ContentAlignment.MiddleCenter;
            this.lblPlayer1Score.Font = new Font("Arial", 12, FontStyle.Bold);
            this.lblPlayer1Score.BackColor = Color.White;
            this.lblPlayer1Score.Margin = new Padding(0);
            this.lblPlayer1Score.Text = "Player1: 0"; // 초기값(실제값은 GamePlayForm.cs에서 덮어씀)

            // lblPlayer2Score (변수명 유지)
            this.lblPlayer2Score.Name = "lblPlayer2Score";
            this.lblPlayer2Score.Dock = DockStyle.Fill;
            this.lblPlayer2Score.TextAlign = ContentAlignment.MiddleCenter;
            this.lblPlayer2Score.Font = new Font("Arial", 12, FontStyle.Bold);
            this.lblPlayer2Score.BackColor = Color.White;
            this.lblPlayer2Score.Margin = new Padding(0);
            this.lblPlayer2Score.Text = "Player2: 0";

            // lblPlayer3Score (변수명 유지)
            this.lblPlayer3Score.Name = "lblPlayer3Score";
            this.lblPlayer3Score.Dock = DockStyle.Fill;
            this.lblPlayer3Score.TextAlign = ContentAlignment.MiddleCenter;
            this.lblPlayer3Score.Font = new Font("Arial", 12, FontStyle.Bold);
            this.lblPlayer3Score.BackColor = Color.White;
            this.lblPlayer3Score.Margin = new Padding(0);
            this.lblPlayer3Score.Text = "Player3: 0";

            // 표에 라벨 3개 추가
            this.scoreTable.Controls.Add(this.lblPlayer1Score, 0, 0);
            this.scoreTable.Controls.Add(this.lblPlayer2Score, 1, 0);
            this.scoreTable.Controls.Add(this.lblPlayer3Score, 2, 0);

            // panel -> table
            this.scorePanel.Controls.Add(this.scoreTable);

            // form -> panel
            this.Controls.Add(this.scorePanel);

            this.scoreTable.ResumeLayout(false);
            this.scorePanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}
