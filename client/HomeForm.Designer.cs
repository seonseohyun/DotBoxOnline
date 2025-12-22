namespace DotsAndBoxes
{
    partial class HomeForm
    {
        private System.ComponentModel.IContainer components = null;

        private DotsAndBoxes.OutlinedTextControl lblTitle;

        private System.Windows.Forms.Button btnSinglePlay;
        private System.Windows.Forms.Button btnMultiPlay;

        private DotsAndBoxes.RoundedPanel pnlCard;

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
            // OutlinedTextControl 생성
            this.lblTitle = new DotsAndBoxes.OutlinedTextControl();

            this.btnSinglePlay = new System.Windows.Forms.Button();
            this.btnMultiPlay = new System.Windows.Forms.Button();
            this.pnlCard = new DotsAndBoxes.RoundedPanel();

            this.SuspendLayout();

            this.lblTitle.Location = new System.Drawing.Point(70, 50);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(380, 80);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "DOTS & BOXES";


            // btnSinglePlay
            this.btnSinglePlay.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnSinglePlay.Location = new System.Drawing.Point(160, 220);
            this.btnSinglePlay.Name = "btnSinglePlay";
            this.btnSinglePlay.Size = new System.Drawing.Size(200, 60);
            this.btnSinglePlay.TabIndex = 1;
            this.btnSinglePlay.Text = "Single Play";
            this.btnSinglePlay.UseVisualStyleBackColor = true;
            this.btnSinglePlay.Click += new System.EventHandler(this.BtnSinglePlay_Click);


            // btnMultiPlay
            this.btnSinglePlay.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnMultiPlay.Location = new System.Drawing.Point(160, 300);
            this.btnMultiPlay.Name = "btnMultiPlay";
            this.btnMultiPlay.Size = new System.Drawing.Size(200, 60);
            this.btnMultiPlay.TabIndex = 2;
            this.btnMultiPlay.Text = "Multi Play";
            this.btnMultiPlay.UseVisualStyleBackColor = true;
            this.btnMultiPlay.Click += new System.EventHandler(this.BtnMultiPlay_Click);


            // pnlCard (RoundedPanel)
            this.pnlCard.Name = "pnlCard";
            this.pnlCard.Size = new System.Drawing.Size(520, 420);
            this.pnlCard.BackColor = System.Drawing.Color.Transparent;
            this.pnlCard.Location = new System.Drawing.Point(40, 40);

            // pnlCard에 붙이기
            this.pnlCard.Controls.Add(this.lblTitle);
            this.pnlCard.Controls.Add(this.btnSinglePlay);
            this.pnlCard.Controls.Add(this.btnMultiPlay);


            // HomeForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 600);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "HomeForm";
            this.Text = "HomeForm";

            this.Controls.Add(this.pnlCard);

            this.ResumeLayout(false);
        }

        #endregion
    }
}
