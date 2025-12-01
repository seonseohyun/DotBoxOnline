// ===== SingleOptionForm.cs =====
using System;
using System.Drawing;
using System.Windows.Forms;

namespace DotsAndBoxes
{
    public partial class SingleOptionForm : Form
    {
        public SingleOptionForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private ComboBox cbBoardSize;
        private ComboBox cbDifficulty;
        private Button btnBack;

        private void BuildUI()
        {
            this.Text = "Single Play Options";
            this.Size = new Size(500, 700);

            // ===== 뒤로가기 버튼 =====
            btnBack = new Button();
            btnBack.Text = "◀ Back";
            btnBack.Size = new Size(80, 30);
            btnBack.Location = new Point(10, 10);
            btnBack.Click += BtnBack_Click;
            this.Controls.Add(btnBack);
            btnBack.BringToFront();

            // ===== 제목 =====
            Label lblTitle = new Label();
            lblTitle.Text = "Single Play Options";
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(100, 60);
            lblTitle.Font = new Font("Arial", 20, FontStyle.Bold);
            this.Controls.Add(lblTitle);

            // ===== 맵 크기 설정 =====
            Label lblBoard = new Label();
            lblBoard.Text = "Board Size";
            lblBoard.Location = new Point(60, 150);
            lblBoard.Font = new Font("Arial", 14);
            this.Controls.Add(lblBoard);

            cbBoardSize = new ComboBox();
            cbBoardSize.Location = new Point(200, 150);
            cbBoardSize.Items.Add("5 x 5");
            cbBoardSize.Items.Add("6 x 6");
            cbBoardSize.Items.Add("7 x 7");
            cbBoardSize.SelectedIndex = 0;
            this.Controls.Add(cbBoardSize);

            // ===== 난이도 설정 =====
            Label lblDiff = new Label();
            lblDiff.Text = "Difficulty";
            lblDiff.Location = new Point(60, 230);
            lblDiff.Font = new Font("Arial", 14);
            this.Controls.Add(lblDiff);

            cbDifficulty = new ComboBox();
            cbDifficulty.Location = new Point(200, 230);
            cbDifficulty.Items.Add("Easy");
            cbDifficulty.Items.Add("Normal");
            cbDifficulty.Items.Add("Hard");
            cbDifficulty.SelectedIndex = 1;
            this.Controls.Add(cbDifficulty);

            // ===== 싱글 게임시작 버튼 =====
            Button btnStart = new Button();
            btnStart.Text = "Start Game";
            btnStart.Font = new Font("Arial", 14);
            btnStart.Size = new Size(200, 50);
            btnStart.Location = new Point(150, 400);
            btnStart.Click += BtnStartGame_Click;
            this.Controls.Add(btnStart);
        }

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            int boardSize = cbBoardSize.SelectedIndex + 5;
            GamePlayForm.AIDifficulty difficulty = (GamePlayForm.AIDifficulty)cbDifficulty.SelectedIndex;

            GamePlayForm gameForm = new GamePlayForm(boardSize, difficulty);
            gameForm.Show();
            this.Hide();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            this.Close(); // 이전 폼으로 돌아가기
        }
    }
}
