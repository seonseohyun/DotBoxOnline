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
    public partial class SingleOptionsForm : Form
    {
        public SingleOptionsForm()
        {
            InitializeComponent();
            BuildUI();
        }

        private ComboBox cbBoardSize;
        private ComboBox cbDifficulty;

        private void BuildUI()
        {
            this.Text = "Single Play Options";
            this.Size = new System.Drawing.Size(500, 700);

            Label lblTitle = new Label();
            lblTitle.Text = "Single Play Options";
            lblTitle.Dock = DockStyle.Top;
            lblTitle.Height = 80;
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblTitle.Font = new System.Drawing.Font("Arial", 20, System.Drawing.FontStyle.Bold);
            this.Controls.Add(lblTitle);

            // 맵 크기 설정
            Label lblBoard = new Label();
            lblBoard.Text = "Board Size";
            lblBoard.Location = new System.Drawing.Point(60, 150);
            lblBoard.Font = new System.Drawing.Font("Arial", 14);
            this.Controls.Add(lblBoard);

            cbBoardSize = new ComboBox();
            cbBoardSize.Location = new System.Drawing.Point(200, 150);
            cbBoardSize.Items.Add("5 x 5");
            cbBoardSize.Items.Add("6 x 6");
            cbBoardSize.Items.Add("7 x 7");
            cbBoardSize.SelectedIndex = 0;
            this.Controls.Add(cbBoardSize);

            // 난이도 설정
            Label lblDiff = new Label();
            lblDiff.Text = "Difficulty";
            lblDiff.Location = new System.Drawing.Point(60, 230);
            lblDiff.Font = new System.Drawing.Font("Arial", 14);
            this.Controls.Add(lblDiff);

            cbDifficulty = new ComboBox();
            cbDifficulty.Location = new System.Drawing.Point(200, 230);
            cbDifficulty.Items.Add("Easy");
            cbDifficulty.Items.Add("Normal");
            cbDifficulty.Items.Add("Hard");
            cbDifficulty.SelectedIndex = 1;
            this.Controls.Add(cbDifficulty);

            // 싱글 게임시작 버튼
            Button btnStart = new Button();
            btnStart.Text = "Start Game";
            btnStart.Font = new System.Drawing.Font("Arial", 14);
            btnStart.Size = new System.Drawing.Size(200, 50);
            btnStart.Location = new System.Drawing.Point(150, 400);
            this.Controls.Add(btnStart);
        }
    }
}
