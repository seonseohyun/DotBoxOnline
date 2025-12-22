namespace DotsAndBoxes
{
    partial class MultiLobbyForm
    {
        private System.ComponentModel.IContainer components = null;

        // ===== Designer 컨트롤 =====
        private DotsAndBoxes.RoundedPanel pnlCard;

        private DotsAndBoxes.OutlinedTextControl lblTitle;

        private System.Windows.Forms.Label lblInviteCodeTitle;
        private System.Windows.Forms.TextBox txtInviteCode;

        private System.Windows.Forms.Label lblPlayersTitle;
        private System.Windows.Forms.Label lblPlayer1;
        private System.Windows.Forms.Label lblPlayer2;
        private System.Windows.Forms.Label lblPlayer3;

        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.Button btnBack;

        private System.Windows.Forms.Panel pnlPlayer1Border;

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

            this.btnBack = new System.Windows.Forms.Button();

            this.lblInviteCodeTitle = new System.Windows.Forms.Label();
            this.txtInviteCode = new System.Windows.Forms.TextBox();

            this.lblPlayersTitle = new System.Windows.Forms.Label();
            this.lblPlayer1 = new System.Windows.Forms.Label();
            this.pnlPlayer1Border = new System.Windows.Forms.Panel();
            this.lblPlayer2 = new System.Windows.Forms.Label();
            this.lblPlayer3 = new System.Windows.Forms.Label();

            this.btnStart = new System.Windows.Forms.Button();
            this.btnExit = new System.Windows.Forms.Button();

            this.SuspendLayout();

            // Form
            this.ClientSize = new System.Drawing.Size(600, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MultiLobbyForm";
            this.Text = "MultiLobbyForm";

            // pnlCard
            this.pnlCard.Location = new System.Drawing.Point(40, 40);
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(520, 520);
            this.pnlCard.BackColor = System.Drawing.Color.Transparent;

            // btnBack
            this.btnBack.Location = new System.Drawing.Point(20, 20);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(90, 32);
            this.btnBack.TabIndex = 0;
            this.btnBack.Text = "◀ Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.BtnBack_Click);

            // lblTitle
            this.lblTitle.Location = new System.Drawing.Point(40, 60);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(440, 70);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "MULTI PLAY LOBBY";

            // Invite Code Title
            this.lblInviteCodeTitle.AutoSize = true;
            this.lblInviteCodeTitle.Location = new System.Drawing.Point(70, 155);
            this.lblInviteCodeTitle.Name = "lblInviteCodeTitle";
            this.lblInviteCodeTitle.Size = new System.Drawing.Size(70, 15);
            this.lblInviteCodeTitle.TabIndex = 2;
            this.lblInviteCodeTitle.Text = "Invite Code";

            // txtInviteCode
            this.txtInviteCode.Location = new System.Drawing.Point(250, 148);
            this.txtInviteCode.Name = "txtInviteCode";
            this.txtInviteCode.ReadOnly = true;
            this.txtInviteCode.Size = new System.Drawing.Size(200, 23);
            this.txtInviteCode.TabIndex = 3;
            this.txtInviteCode.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;

            // Players Title
            this.lblPlayersTitle.AutoSize = true;
            this.lblPlayersTitle.Location = new System.Drawing.Point(70, 215);
            this.lblPlayersTitle.Name = "lblPlayersTitle";
            this.lblPlayersTitle.Size = new System.Drawing.Size(46, 15);
            this.lblPlayersTitle.TabIndex = 4;
            this.lblPlayersTitle.Text = "Game Players";

            // Player Slots
            this.pnlPlayer1Border = new System.Windows.Forms.Panel();
            this.lblPlayer1.Location = new System.Drawing.Point(70, 245);
            this.lblPlayer1.Name = "lblPlayer1";
            this.lblPlayer1.Size = new System.Drawing.Size(380, 35);
            this.lblPlayer1.TabIndex = 5;
            this.lblPlayer1.Text = "Player1 (Host)";
            this.lblPlayer1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblPlayer2.Location = new System.Drawing.Point(70, 290);
            this.lblPlayer2.Name = "lblPlayer2";
            this.lblPlayer2.Size = new System.Drawing.Size(380, 35);
            this.lblPlayer2.TabIndex = 6;
            this.lblPlayer2.Text = "Waiting for Player2...";
            this.lblPlayer2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            this.lblPlayer3.Location = new System.Drawing.Point(70, 335);
            this.lblPlayer3.Name = "lblPlayer3";
            this.lblPlayer3.Size = new System.Drawing.Size(380, 35);
            this.lblPlayer3.TabIndex = 7;
            this.lblPlayer3.Text = "Waiting for Player3...";
            this.lblPlayer3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // btnStart
            this.btnStart.Location = new System.Drawing.Point(270, 410);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(180, 55);
            this.btnStart.TabIndex = 8;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.BtnStart_Click);

            // btnExit
            this.btnExit.Location = new System.Drawing.Point(70, 410);
            this.btnExit.Name = "btnExit";              
            this.btnExit.Size = new System.Drawing.Size(180, 55);
            this.btnExit.TabIndex = 9;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.BtnExit_Click);

            // Add to Card
            this.pnlCard.Controls.Add(this.btnBack);
            this.pnlCard.Controls.Add(this.lblTitle);

            this.pnlCard.Controls.Add(this.lblInviteCodeTitle);
            this.pnlCard.Controls.Add(this.txtInviteCode);

            this.pnlCard.Controls.Add(this.lblPlayersTitle);
            this.pnlCard.Controls.Add(this.lblPlayer1);
            this.pnlCard.Controls.Add(this.lblPlayer2);
            this.pnlCard.Controls.Add(this.lblPlayer3);

            this.pnlCard.Controls.Add(this.btnStart);
            this.pnlCard.Controls.Add(this.btnExit);

            this.Controls.Add(this.pnlCard);

            this.ResumeLayout(false);
        }
        #endregion
    }
}
