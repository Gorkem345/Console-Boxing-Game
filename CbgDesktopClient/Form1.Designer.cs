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
            lblW = new Label();
            lblS = new Label();
            lblA = new Label();
            lblD = new Label();
            prgTimer = new ProgressBar();
            SuspendLayout();
            // 
            // lblW
            // 
            lblW.AccessibleName = "";
            lblW.AutoSize = true;
            lblW.Font = new Font("Segoe UI", 72F);
            lblW.ForeColor = SystemColors.AppWorkspace;
            lblW.Location = new Point(340, 4);
            lblW.Name = "lblW";
            lblW.Size = new Size(137, 128);
            lblW.TabIndex = 0;
            lblW.Text = "▲";
            // 
            // lblS
            // 
            lblS.AutoSize = true;
            lblS.Font = new Font("Segoe UI", 72F);
            lblS.ForeColor = SystemColors.AppWorkspace;
            lblS.Location = new Point(340, 260);
            lblS.Name = "lblS";
            lblS.Size = new Size(137, 128);
            lblS.TabIndex = 1;
            lblS.Text = "▼";
            // 
            // lblA
            // 
            lblA.AutoSize = true;
            lblA.Font = new Font("Segoe UI", 108F);
            lblA.ForeColor = SystemColors.AppWorkspace;
            lblA.Location = new Point(159, 99);
            lblA.Name = "lblA";
            lblA.Size = new Size(204, 191);
            lblA.TabIndex = 2;
            lblA.Text = "◄";
            // 
            // lblD
            // 
            lblD.AutoSize = true;
            lblD.Font = new Font("Segoe UI", 108F);
            lblD.ForeColor = SystemColors.AppWorkspace;
            lblD.Location = new Point(447, 99);
            lblD.Name = "lblD";
            lblD.Size = new Size(204, 191);
            lblD.TabIndex = 3;
            lblD.Text = "►";
            // 
            // prgTimer
            // 
            prgTimer.Location = new Point(153, 402);
            prgTimer.Name = "prgTimer";
            prgTimer.Size = new Size(505, 23);
            prgTimer.TabIndex = 4;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(prgTimer);
            Controls.Add(lblD);
            Controls.Add(lblA);
            Controls.Add(lblS);
            Controls.Add(lblW);
            KeyPreview = true;
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblW;
        private Label lblS;
        private Label lblA;
        private Label lblD;
        private ProgressBar prgTimer;
    }
}
