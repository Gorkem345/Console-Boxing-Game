namespace CbgDesktopClient
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            prgTimer = new ProgressBar();
            lblStatus = new Label();
            btnReady = new Button();
            panelW = new Panel();
            panelS = new Panel();
            panelA = new Panel();
            panelD = new Panel();
            SuspendLayout();
            // 
            // prgTimer
            // 
            prgTimer.Location = new Point(176, 402);
            prgTimer.Name = "prgTimer";
            prgTimer.Size = new Size(505, 23);
            prgTimer.TabIndex = 4;
            prgTimer.Visible = false;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 24F);
            lblStatus.Location = new Point(12, 9);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(708, 48);
            lblStatus.TabIndex = 5;
            lblStatus.Text = "Waiting...";
            // 
            // btnReady
            // 
            btnReady.AutoSize = true;
            btnReady.Font = new Font("Segoe UI", 20F);
            btnReady.Location = new Point(373, 185);
            btnReady.Name = "btnReady";
            btnReady.Size = new Size(107, 51);
            btnReady.TabIndex = 6;
            btnReady.Text = "Ready!";
            btnReady.UseVisualStyleBackColor = true;
            // 
            // panelW
            // 
            panelW.Location = new Point(387, 76);
            panelW.Name = "panelW";
            panelW.Size = new Size(80, 80);
            panelW.TabIndex = 7;
            // 
            // panelS
            // 
            panelS.Location = new Point(387, 270);
            panelS.Name = "panelS";
            panelS.Size = new Size(80, 80);
            panelS.TabIndex = 8;
            // 
            // panelA
            // 
            panelA.Location = new Point(273, 170);
            panelA.Name = "panelA";
            panelA.Size = new Size(80, 80);
            panelA.TabIndex = 8;
            // 
            // panelD
            // 
            panelD.Location = new Point(497, 170);
            panelD.Name = "panelD";
            panelD.Size = new Size(80, 80);
            panelD.TabIndex = 8;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(panelS);
            Controls.Add(panelA);
            Controls.Add(panelD);
            Controls.Add(panelW);
            Controls.Add(btnReady);
            Controls.Add(lblStatus);
            Controls.Add(prgTimer);
            KeyPreview = true;
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private ProgressBar prgTimer;
        private Label lblStatus;
        private Button btnReady;
        private Panel panelW;
        private Panel panelS;
        private Panel panelA;
        private Panel panelD;
    }
}
