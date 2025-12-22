using System.Drawing;
using System.Windows.Forms;

namespace DotsAndBoxes
{
    partial class GameResultForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblScoreSummary;
        private System.Windows.Forms.Label lblResultMessage;
        private System.Windows.Forms.Panel pnlButtonArea;
        private System.Windows.Forms.Button btnRestart;
        private System.Windows.Forms.Button btnGoMain;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        private void InitializeComponent()
        {
            this.lblResultMessage = new System.Windows.Forms.Label();
            this.pnlButtonArea = new System.Windows.Forms.Panel();
            this.btnRestart = new System.Windows.Forms.Button();
            this.btnGoMain = new System.Windows.Forms.Button();
            this.SuspendLayout();

            this.lblScoreSummary = new System.Windows.Forms.Label();
            this.lblScoreSummary.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblScoreSummary.Height = 100;
            this.lblScoreSummary.Name = "lblScoreSummary";
            this.lblScoreSummary.TabIndex = 1;
            this.lblScoreSummary.Text = ""; 
            this.lblScoreSummary.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // lblResultMessage
            this.lblResultMessage.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblResultMessage.Height = 140;
            this.lblResultMessage.Name = "lblResultMessage";
            this.lblResultMessage.TabIndex = 0;
            this.lblResultMessage.Text = "WIN";
            this.lblResultMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            // pnlButtonArea
            this.pnlButtonArea.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlButtonArea.Height = 120;
            this.pnlButtonArea.Name = "pnlButtonArea";
            this.pnlButtonArea.TabIndex = 2;

            // btnRestart
            this.btnRestart.Name = "btnRestart";
            this.btnRestart.Size = new System.Drawing.Size(180, 55);
            this.btnRestart.Location = new System.Drawing.Point(300, 410);
            this.btnRestart.TabIndex = 0;
            this.btnRestart.Text = "RESTART";
            this.btnRestart.UseVisualStyleBackColor = true;
            this.btnRestart.Click += new System.EventHandler(this.BtnRestart_Click);

            // btnGoMain
            this.btnGoMain.Name = "btnGoMain";
            this.btnGoMain.Size = new System.Drawing.Size(180, 55);
            this.btnGoMain.Location = new System.Drawing.Point(100, 410);
            this.btnGoMain.TabIndex = 1;
            this.btnGoMain.Text = "MAIN";
            this.btnGoMain.UseVisualStyleBackColor = true;
            this.btnGoMain.Click += new System.EventHandler(this.BtnMain_Click);

            // pnlButtonArea Controls
            this.Controls.Add(this.btnRestart);
            this.Controls.Add(this.btnGoMain);

            // GameResultForm
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(450, 520);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "GameResultForm";
            this.Text = "GameResultForm";

            // Add Controls
            this.Controls.Add(this.pnlButtonArea);
            this.Controls.Add(this.lblScoreSummary);
            this.Controls.Add(this.lblResultMessage);

            this.ResumeLayout(false);
        }
        #endregion
    }
}
