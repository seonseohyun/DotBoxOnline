namespace DotsAndBoxes
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.mainPanel = new System.Windows.Forms.Panel();
            this.SuspendLayout();

            // mainPanel
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.BackColor = System.Drawing.Color.White;
            this.mainPanel.Name = "mainPanel";

            // MainForm
            this.ClientSize = new System.Drawing.Size(600, 600);
            this.Controls.Add(this.mainPanel);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Dots and Boxes";

            this.ResumeLayout(false);
        }

        #endregion
    }
}