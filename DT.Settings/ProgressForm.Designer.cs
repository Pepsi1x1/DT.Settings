namespace DT.Common
{
    partial class ProgressForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.loadingCircle1 = new DT.Common.LoadingCircle();
            this.progressTextBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // loadingCircle1
            // 
            this.loadingCircle1.Active = false;
            this.loadingCircle1.Color = System.Drawing.Color.DarkGray;
            this.loadingCircle1.InnerCircleRadius = 5;
            this.loadingCircle1.Location = new System.Drawing.Point(145, 46);
            this.loadingCircle1.Name = "loadingCircle1";
            this.loadingCircle1.OuterCircleRadius = 11;
            this.loadingCircle1.RotationSpeed = 100;
            this.loadingCircle1.Size = new System.Drawing.Size(36, 36);
            this.loadingCircle1.SpokeCount = 12;
            this.loadingCircle1.SpokeThickness = 2;
            this.loadingCircle1.TabIndex = 0;
            this.loadingCircle1.Text = "loadingCircle1";
            // 
            // progressTextBox
            // 
            this.progressTextBox.BackColor = System.Drawing.SystemColors.ControlLight;
            this.progressTextBox.Enabled = false;
            this.progressTextBox.Location = new System.Drawing.Point(12, 96);
            this.progressTextBox.Name = "progressTextBox";
            this.progressTextBox.Size = new System.Drawing.Size(303, 20);
            this.progressTextBox.TabIndex = 1;
            // 
            // Translating
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(327, 128);
            this.Controls.Add(this.progressTextBox);
            this.Controls.Add(this.loadingCircle1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "ProgressForm";
            this.Text = "Translating...";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DT.Common.LoadingCircle loadingCircle1;
        private System.Windows.Forms.TextBox progressTextBox;
    }
}