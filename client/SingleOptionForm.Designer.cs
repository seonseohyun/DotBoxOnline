namespace DotsAndBoxes
{
    partial class SingleOptionForm
    {
        private System.ComponentModel.IContainer components = null;

        // ===== Designer 필드 =====
        private DotsAndBoxes.RoundedPanel pnlCard;

        private DotsAndBoxes.OutlinedTextControl lblTitle;

        private System.Windows.Forms.Label lblBoardSizeTitle;
        private System.Windows.Forms.Label lblDifficultyTitle;

        private System.Windows.Forms.ComboBox cbBoardSize;
        private System.Windows.Forms.ComboBox cbDifficulty;

        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnStart;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.pnlCard = new DotsAndBoxes.RoundedPanel();
            this.lblTitle = new DotsAndBoxes.OutlinedTextControl();

            this.lblBoardSizeTitle = new System.Windows.Forms.Label();
            this.lblDifficultyTitle = new System.Windows.Forms.Label();

            this.cbBoardSize = new System.Windows.Forms.ComboBox();
            this.cbDifficulty = new System.Windows.Forms.ComboBox();

            this.btnBack = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // Form
            this.ClientSize = new System.Drawing.Size(600, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SingleOptionForm";

            // pnlCard
            this.pnlCard.Location = new System.Drawing.Point(40, 40);
            this.pnlCard.Size = new System.Drawing.Size(520, 520);
            this.pnlCard.BackColor = System.Drawing.Color.Transparent;

            // btnBack
            this.btnBack.Location = new System.Drawing.Point(20, 20);
            this.btnBack.Size = new System.Drawing.Size(90, 32);
            this.btnBack.Text = "◀ Back";
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);

            // lblTitle
            this.lblTitle.Location = new System.Drawing.Point(40, 100);
            this.lblTitle.Size = new System.Drawing.Size(440, 70);
            this.lblTitle.Text = "SINGLE PLAY OPTIONS";

            // Board Size
            this.lblBoardSizeTitle.Location = new System.Drawing.Point(80, 230);
            this.lblBoardSizeTitle.Text = "Board Size";

            this.cbBoardSize.Location = new System.Drawing.Point(260, 230);
            this.cbBoardSize.Size = new System.Drawing.Size(160, 23);
            this.cbBoardSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBoardSize.Items.AddRange(new object[] { "5 x 5", "6 x 6", "7 x 7" });
            this.cbBoardSize.SelectedIndex = 0;

            // Difficulty
            this.lblDifficultyTitle.Location = new System.Drawing.Point(80, 300);
            this.lblDifficultyTitle.Text = "Difficulty";

            this.cbDifficulty.Location = new System.Drawing.Point(260, 300);
            this.cbDifficulty.Size = new System.Drawing.Size(160, 23);
            this.cbDifficulty.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDifficulty.Items.AddRange(new object[] { "Easy", "Normal", "Hard" });
            this.cbDifficulty.SelectedIndex = 1;

            // btnStart
            this.btnStart.Location = new System.Drawing.Point(160, 420);
            this.btnStart.Size = new System.Drawing.Size(200, 50);
            this.btnStart.Text = "Start Game";
            this.btnStart.Click += new System.EventHandler(this.BtnStartGame_Click);

            // Add Controls
            this.pnlCard.Controls.Add(this.btnBack);
            this.pnlCard.Controls.Add(this.lblTitle);

            this.pnlCard.Controls.Add(this.lblBoardSizeTitle);
            this.pnlCard.Controls.Add(this.cbBoardSize);

            this.pnlCard.Controls.Add(this.lblDifficultyTitle);
            this.pnlCard.Controls.Add(this.cbDifficulty);

            this.pnlCard.Controls.Add(this.btnStart);

            this.Controls.Add(this.pnlCard);

            this.ResumeLayout(false);
        }
        #endregion
    }
}
