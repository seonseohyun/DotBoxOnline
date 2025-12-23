// ===== SingleOptionForm.cs =====
using System;
using System.Drawing;
using System.Windows.Forms;
using static DotsAndBoxes.GamePlayForm;

namespace DotsAndBoxes
{
    public partial class SingleOptionForm : Form
    {
        public SingleOptionForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            ApplyTheme();
        }
        private void ApplyTheme()
        {
            // 폼 기본
            Theme.ApplyForm(this);

            // 카드
            Theme.ApplyCard(pnlCard);

            // 버튼
            Theme.ApplyButton(btnBack);
            Theme.ApplyButton(btnStart);

            // ===== 폰트 규칙 =====
            var labelFont = new Font("Segoe UI", 12.5f, FontStyle.Bold);
            var inputFont = new Font("Segoe UI", 12f, FontStyle.Bold);

            // 라벨
            lblBoardSizeTitle.Font = labelFont;
            lblDifficultyTitle.Font = labelFont;
            lblBoardSizeTitle.ForeColor = Theme.C_TEXT;
            lblDifficultyTitle.ForeColor = Theme.C_TEXT;

            // 콤보박스
            cbBoardSize.Font = inputFont;
            cbDifficulty.Font = inputFont;
            cbBoardSize.BackColor = Theme.C_CARD_BG;
            cbDifficulty.BackColor = Theme.C_CARD_BG;

            // 타이틀 (OutlinedTextControl)
            lblTitle.Text = "SINGLE PLAY OPTIONS";
            lblTitle.Font = new Font("Segoe UI", 20f, FontStyle.Bold);
            lblTitle.ForeColor = Theme.C_TEXT;
            lblTitle.OutlineColor = Color.FromArgb(170, 170, 170);
            lblTitle.OutlineThickness = 1.3f;
            lblTitle.LetterSpacing = 2.5f;
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        }

        private MainForm GetMainForm()
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f is MainForm)
                    return (MainForm)f;
            }
            return null;
        }

        private void BtnStartGame_Click(object sender, EventArgs e)
        {
            int boardSize = cbBoardSize.SelectedIndex + 5;
            GamePlayForm.AIDifficulty difficulty = (GamePlayForm.AIDifficulty)cbDifficulty.SelectedIndex;

            MainForm main = GetMainForm();  
            if (main == null)
            {
                MessageBox.Show("MainForm을 찾지 못했습니다.");
                return;
            }
            main.LoadChildForm(new GamePlayForm(boardSize, difficulty));
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            //this.Close(); // 이전 폼으로 돌아가기
            MainForm main = (MainForm)this.ParentForm;
            main.LoadChildForm(new HomeForm()); 

        }
    }
}
