namespace DotsAndBoxes
{
    partial class MultiOptionForm
    {
        private System.ComponentModel.IContainer components = null;

        // 컨트롤 필드(Designer 전용) 
        private DotsAndBoxes.RoundedPanel pnlCard;

        private DotsAndBoxes.OutlinedTextControl lblTitle;

        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnCreateRoom;
        private System.Windows.Forms.Button btnJoinRoom;
        private System.Windows.Forms.Button btnNicknameOk;

        private System.Windows.Forms.Label lblBoardSizeTitle;
        private System.Windows.Forms.Label lblPlayerCountTitle;
        private System.Windows.Forms.Label lblNicknameTitle;
        private System.Windows.Forms.Label lblRoomCodeTitle;

        private System.Windows.Forms.ComboBox cbBoardSize;
        private System.Windows.Forms.ComboBox cbPlayerCount;

        private System.Windows.Forms.TextBox txtNickname;
        private System.Windows.Forms.TextBox txtRoomCode;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.pnlCard = new DotsAndBoxes.RoundedPanel();
            this.lblTitle = new DotsAndBoxes.OutlinedTextControl();

            this.btnBack = new System.Windows.Forms.Button();
            this.btnCreateRoom = new System.Windows.Forms.Button();
            this.btnJoinRoom = new System.Windows.Forms.Button();
            this.btnNicknameOk = new System.Windows.Forms.Button();

            this.lblBoardSizeTitle = new System.Windows.Forms.Label();
            this.lblPlayerCountTitle = new System.Windows.Forms.Label();
            this.lblNicknameTitle = new System.Windows.Forms.Label();
            this.lblRoomCodeTitle = new System.Windows.Forms.Label();

            this.cbBoardSize = new System.Windows.Forms.ComboBox();
            this.cbPlayerCount = new System.Windows.Forms.ComboBox();

            this.txtNickname = new System.Windows.Forms.TextBox();
            this.txtRoomCode = new System.Windows.Forms.TextBox();

            this.SuspendLayout();


            // MultiOptionForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MultiOptionForm";
            this.Text = "MultiOptionForm";

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

            // lblTitle (OutlinedTextControl)
            this.lblTitle.Location = new System.Drawing.Point(40, 60);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(440, 70);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "MULTIPLAYER OPTIONS";

            // Labels
            this.lblBoardSizeTitle.AutoSize = true;
            this.lblBoardSizeTitle.Location = new System.Drawing.Point(70, 170);
            this.lblBoardSizeTitle.Name = "lblBoardSizeTitle";
            this.lblBoardSizeTitle.Size = new System.Drawing.Size(68, 15);
            this.lblBoardSizeTitle.TabIndex = 2;
            this.lblBoardSizeTitle.Text = "Board Size";

            this.lblPlayerCountTitle.AutoSize = true;
            this.lblPlayerCountTitle.Location = new System.Drawing.Point(70, 220);
            this.lblPlayerCountTitle.Name = "lblPlayerCountTitle";
            this.lblPlayerCountTitle.Size = new System.Drawing.Size(82, 15);
            this.lblPlayerCountTitle.TabIndex = 3;
            this.lblPlayerCountTitle.Text = "Player Count";

            this.lblNicknameTitle.AutoSize = true;
            this.lblNicknameTitle.Location = new System.Drawing.Point(70, 280);
            this.lblNicknameTitle.Name = "lblNicknameTitle";
            this.lblNicknameTitle.Size = new System.Drawing.Size(63, 15);
            this.lblNicknameTitle.TabIndex = 4;
            this.lblNicknameTitle.Text = "Nickname";

            this.lblRoomCodeTitle.AutoSize = true;
            this.lblRoomCodeTitle.Location = new System.Drawing.Point(70, 330);
            this.lblRoomCodeTitle.Name = "lblRoomCodeTitle";
            this.lblRoomCodeTitle.Size = new System.Drawing.Size(70, 15);
            this.lblRoomCodeTitle.TabIndex = 5;
            this.lblRoomCodeTitle.Text = "Room Code";

            // cbBoardSize
            this.cbBoardSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbBoardSize.FormattingEnabled = true;
            this.cbBoardSize.Items.AddRange(new object[] { "5 x 5", "6 x 6", "7 x 7" });
            this.cbBoardSize.Location = new System.Drawing.Point(220, 165);
            this.cbBoardSize.Name = "cbBoardSize";
            this.cbBoardSize.Size = new System.Drawing.Size(160, 23);
            this.cbBoardSize.TabIndex = 6;

            // cbPlayerCount
            this.cbPlayerCount.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPlayerCount.FormattingEnabled = true;
            this.cbPlayerCount.Items.AddRange(new object[] { "2 Players", "3 Players" });
            this.cbPlayerCount.Location = new System.Drawing.Point(220, 215);
            this.cbPlayerCount.Name = "cbPlayerCount";
            this.cbPlayerCount.Size = new System.Drawing.Size(160, 23);
            this.cbPlayerCount.TabIndex = 7;

            // txtNickname
            this.txtNickname.Location = new System.Drawing.Point(220, 275);
            this.txtNickname.Name = "txtNickname";
            this.txtNickname.Size = new System.Drawing.Size(160, 23);
            this.txtNickname.TabIndex = 8;
            this.txtNickname.TextChanged += new System.EventHandler(this.TxtNickname_TextChanged);

            // btnNicknameOk
            this.btnNicknameOk.Location = new System.Drawing.Point(395, 275);
            this.btnNicknameOk.Name = "btnNicknameOk";
            this.btnNicknameOk.Size = new System.Drawing.Size(60, 30);
            this.btnNicknameOk.TabIndex = 9;
            this.btnNicknameOk.Text = "OK";
            this.btnNicknameOk.UseVisualStyleBackColor = true;
            this.btnNicknameOk.Click += new System.EventHandler(this.BtnNicknameOk_Click);

            // txtRoomCode
            this.txtRoomCode.Location = new System.Drawing.Point(220, 330);
            this.txtRoomCode.Name = "txtRoomCode";
            this.txtRoomCode.Size = new System.Drawing.Size(160, 23);
            this.txtRoomCode.TabIndex = 10;

            // btnJoinRoom
            this.btnJoinRoom.Location = new System.Drawing.Point(395, 330);
            this.btnJoinRoom.Name = "btnJoinRoom";
            this.btnJoinRoom.Size = new System.Drawing.Size(60, 30);
            this.btnJoinRoom.TabIndex = 11;
            this.btnJoinRoom.Text = "Join";
            this.btnJoinRoom.UseVisualStyleBackColor = true;
            this.btnJoinRoom.Click += new System.EventHandler(this.BtnJoinRoom_Click);

            // btnCreateRoom
            this.btnCreateRoom.Location = new System.Drawing.Point(160, 420);
            this.btnCreateRoom.Name = "btnCreateRoom";
            this.btnCreateRoom.Size = new System.Drawing.Size(200, 50);
            this.btnCreateRoom.TabIndex = 12;
            this.btnCreateRoom.Text = "Create Room";
            this.btnCreateRoom.UseVisualStyleBackColor = true;
            this.btnCreateRoom.Click += new System.EventHandler(this.BtnCreateRoom_Click);

            // pnlCard Controls Add
            this.pnlCard.Controls.Add(this.btnBack);
            this.pnlCard.Controls.Add(this.lblTitle);

            this.pnlCard.Controls.Add(this.lblBoardSizeTitle);
            this.pnlCard.Controls.Add(this.cbBoardSize);

            this.pnlCard.Controls.Add(this.lblPlayerCountTitle);
            this.pnlCard.Controls.Add(this.cbPlayerCount);

            this.pnlCard.Controls.Add(this.lblNicknameTitle);
            this.pnlCard.Controls.Add(this.txtNickname);
            this.pnlCard.Controls.Add(this.btnNicknameOk);

            this.pnlCard.Controls.Add(this.lblRoomCodeTitle);
            this.pnlCard.Controls.Add(this.txtRoomCode);
            this.pnlCard.Controls.Add(this.btnJoinRoom);

            this.pnlCard.Controls.Add(this.btnCreateRoom);

            // 폼에 카드 추가
            this.Controls.Add(this.pnlCard);

            this.ResumeLayout(false);
        }
        #endregion
    }
}
