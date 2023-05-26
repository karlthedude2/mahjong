namespace Mahjong
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.btnShuffle = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtLayoutCount = new System.Windows.Forms.TextBox();
            this.txtStyleCount = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lblClock = new System.Windows.Forms.Label();
            this.btnNew = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.layoutPanel = new Mahjong.LayoutPanel();
            this.SuspendLayout();
            // 
            // btnShuffle
            // 
            this.btnShuffle.Location = new System.Drawing.Point(24, 13);
            this.btnShuffle.Name = "btnShuffle";
            this.btnShuffle.Size = new System.Drawing.Size(75, 23);
            this.btnShuffle.TabIndex = 1;
            this.btnShuffle.Text = "Shuffle";
            this.btnShuffle.UseVisualStyleBackColor = true;
            this.btnShuffle.Click += new System.EventHandler(this.btnShuffle_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(153, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Layout Count";
            // 
            // txtLayoutCount
            // 
            this.txtLayoutCount.Location = new System.Drawing.Point(230, 15);
            this.txtLayoutCount.Name = "txtLayoutCount";
            this.txtLayoutCount.Size = new System.Drawing.Size(51, 20);
            this.txtLayoutCount.TabIndex = 3;
            // 
            // txtStyleCount
            // 
            this.txtStyleCount.Location = new System.Drawing.Point(380, 15);
            this.txtStyleCount.Name = "txtStyleCount";
            this.txtStyleCount.Size = new System.Drawing.Size(51, 20);
            this.txtStyleCount.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(303, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Style Count";
            // 
            // lblClock
            // 
            this.lblClock.AutoSize = true;
            this.lblClock.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClock.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblClock.Location = new System.Drawing.Point(551, 13);
            this.lblClock.Name = "lblClock";
            this.lblClock.Size = new System.Drawing.Size(55, 25);
            this.lblClock.TabIndex = 6;
            this.lblClock.Text = "0:00";
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(652, 13);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(75, 23);
            this.btnNew.TabIndex = 7;
            this.btnNew.Text = "New Game";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(733, 13);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 8;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // layoutPanel
            // 
            this.layoutPanel.Location = new System.Drawing.Point(116, 41);
            this.layoutPanel.Name = "layoutPanel";
            this.layoutPanel.Size = new System.Drawing.Size(722, 579);
            this.layoutPanel.TabIndex = 0;
            this.layoutPanel.TileLayout = null;
            this.layoutPanel.TileStyles = null;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(850, 621);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.lblClock);
            this.Controls.Add(this.txtStyleCount);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtLayoutCount);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnShuffle);
            this.Controls.Add(this.layoutPanel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private LayoutPanel layoutPanel;
        private System.Windows.Forms.Button btnShuffle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtLayoutCount;
        private System.Windows.Forms.TextBox txtStyleCount;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblClock;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Button btnPause;

    }
}

