namespace MahjongSpriteVersion
{
    partial class GameForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameForm));
            this.btnUndo = new System.Windows.Forms.Button();
            this.cboLayout = new System.Windows.Forms.ComboBox();
            this.lblClock = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkSound = new System.Windows.Forms.CheckBox();
            this.lblShuffleBonus = new System.Windows.Forms.Label();
            this.lblMinuteBonus = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lstHighScores = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lblBonusTotal = new System.Windows.Forms.Label();
            this.lblBonusQty = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.lblFinalBonus = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblSpeedBonusTilesCollected = new System.Windows.Forms.Label();
            this.lblSpeedBonusTimeLeft = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnRedo = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnShuffle = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.lblScore = new System.Windows.Forms.Label();
            this.pbLayoutPanel = new System.Windows.Forms.PictureBox();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLayoutPanel)).BeginInit();
            this.SuspendLayout();
            // 
            // btnUndo
            // 
            this.btnUndo.Location = new System.Drawing.Point(12, 744);
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.Size = new System.Drawing.Size(75, 23);
            this.btnUndo.TabIndex = 31;
            this.btnUndo.Text = "Undo";
            this.btnUndo.UseVisualStyleBackColor = true;
            this.btnUndo.Click += new System.EventHandler(this.btnUndo_Click);
            // 
            // cboLayout
            // 
            this.cboLayout.FormattingEnabled = true;
            this.cboLayout.Items.AddRange(new object[] {
            "Number One",
            "The Runner Up",
            "Test"});
            this.cboLayout.Location = new System.Drawing.Point(18, 22);
            this.cboLayout.Name = "cboLayout";
            this.cboLayout.Size = new System.Drawing.Size(156, 21);
            this.cboLayout.TabIndex = 29;
            this.cboLayout.SelectedIndexChanged += new System.EventHandler(this.cboLayout_SelectedIndexChanged);
            // 
            // lblClock
            // 
            this.lblClock.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClock.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblClock.Location = new System.Drawing.Point(100, 150);
            this.lblClock.Name = "lblClock";
            this.lblClock.Size = new System.Drawing.Size(90, 25);
            this.lblClock.TabIndex = 14;
            this.lblClock.Text = "0:00";
            this.lblClock.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chkSound);
            this.panel1.Controls.Add(this.lblShuffleBonus);
            this.panel1.Controls.Add(this.lblMinuteBonus);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lstHighScores);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.lblBonusTotal);
            this.panel1.Controls.Add(this.lblBonusQty);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.lblFinalBonus);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.lblSpeedBonusTilesCollected);
            this.panel1.Controls.Add(this.lblSpeedBonusTimeLeft);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnRedo);
            this.panel1.Controls.Add(this.btnUndo);
            this.panel1.Controls.Add(this.cboLayout);
            this.panel1.Controls.Add(this.lblClock);
            this.panel1.Controls.Add(this.btnPause);
            this.panel1.Controls.Add(this.btnShuffle);
            this.panel1.Controls.Add(this.btnNew);
            this.panel1.Controls.Add(this.lblScore);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(263, 1011);
            this.panel1.TabIndex = 31;
            // 
            // chkSound
            // 
            this.chkSound.AutoSize = true;
            this.chkSound.Location = new System.Drawing.Point(15, 829);
            this.chkSound.Name = "chkSound";
            this.chkSound.Size = new System.Drawing.Size(57, 17);
            this.chkSound.TabIndex = 51;
            this.chkSound.Text = "Sound";
            this.chkSound.UseVisualStyleBackColor = true;
            this.chkSound.CheckedChanged += new System.EventHandler(this.chkSound_CheckedChanged);
            // 
            // lblShuffleBonus
            // 
            this.lblShuffleBonus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblShuffleBonus.Location = new System.Drawing.Point(144, 350);
            this.lblShuffleBonus.Name = "lblShuffleBonus";
            this.lblShuffleBonus.Size = new System.Drawing.Size(44, 23);
            this.lblShuffleBonus.TabIndex = 50;
            this.lblShuffleBonus.Text = "0";
            this.lblShuffleBonus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblMinuteBonus
            // 
            this.lblMinuteBonus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMinuteBonus.Location = new System.Drawing.Point(144, 330);
            this.lblMinuteBonus.Name = "lblMinuteBonus";
            this.lblMinuteBonus.Size = new System.Drawing.Size(44, 23);
            this.lblMinuteBonus.TabIndex = 49;
            this.lblMinuteBonus.Text = "0";
            this.lblMinuteBonus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label11
            // 
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(16, 370);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(125, 23);
            this.label11.TabIndex = 48;
            this.label11.Text = "Final Bonus:";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(16, 350);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(125, 23);
            this.label2.TabIndex = 47;
            this.label2.Text = "Shuffle Bonus:";
            // 
            // lstHighScores
            // 
            this.lstHighScores.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstHighScores.FormattingEnabled = true;
            this.lstHighScores.ItemHeight = 14;
            this.lstHighScores.Location = new System.Drawing.Point(12, 440);
            this.lstHighScores.Name = "lstHighScores";
            this.lstHighScores.Size = new System.Drawing.Size(239, 298);
            this.lstHighScores.TabIndex = 32;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(9, 421);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(125, 23);
            this.label1.TabIndex = 46;
            this.label1.Text = "High Scores";
            // 
            // lblBonusTotal
            // 
            this.lblBonusTotal.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBonusTotal.Location = new System.Drawing.Point(138, 296);
            this.lblBonusTotal.Name = "lblBonusTotal";
            this.lblBonusTotal.Size = new System.Drawing.Size(50, 23);
            this.lblBonusTotal.TabIndex = 45;
            this.lblBonusTotal.Text = "00000";
            this.lblBonusTotal.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblBonusQty
            // 
            this.lblBonusQty.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBonusQty.Location = new System.Drawing.Point(144, 278);
            this.lblBonusQty.Name = "lblBonusQty";
            this.lblBonusQty.Size = new System.Drawing.Size(44, 23);
            this.lblBonusQty.TabIndex = 44;
            this.lblBonusQty.Text = "0";
            this.lblBonusQty.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(13, 296);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(128, 23);
            this.label9.TabIndex = 43;
            this.label9.Text = "Bonus Received:";
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(12, 278);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(128, 23);
            this.label8.TabIndex = 42;
            this.label8.Text = "Times Received:";
            // 
            // lblFinalBonus
            // 
            this.lblFinalBonus.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFinalBonus.Location = new System.Drawing.Point(144, 370);
            this.lblFinalBonus.Name = "lblFinalBonus";
            this.lblFinalBonus.Size = new System.Drawing.Size(44, 23);
            this.lblFinalBonus.TabIndex = 41;
            this.lblFinalBonus.Text = "0";
            this.lblFinalBonus.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(15, 330);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(125, 23);
            this.label10.TabIndex = 40;
            this.label10.Text = "Minute Bonus:";
            // 
            // lblSpeedBonusTilesCollected
            // 
            this.lblSpeedBonusTilesCollected.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeedBonusTilesCollected.Location = new System.Drawing.Point(144, 247);
            this.lblSpeedBonusTilesCollected.Name = "lblSpeedBonusTilesCollected";
            this.lblSpeedBonusTilesCollected.Size = new System.Drawing.Size(44, 23);
            this.lblSpeedBonusTilesCollected.TabIndex = 39;
            this.lblSpeedBonusTilesCollected.Text = "0";
            this.lblSpeedBonusTilesCollected.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblSpeedBonusTimeLeft
            // 
            this.lblSpeedBonusTimeLeft.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeedBonusTimeLeft.Location = new System.Drawing.Point(144, 227);
            this.lblSpeedBonusTimeLeft.Name = "lblSpeedBonusTimeLeft";
            this.lblSpeedBonusTimeLeft.Size = new System.Drawing.Size(44, 23);
            this.lblSpeedBonusTimeLeft.TabIndex = 38;
            this.lblSpeedBonusTimeLeft.Text = "0";
            this.lblSpeedBonusTimeLeft.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(13, 247);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(125, 23);
            this.label7.TabIndex = 37;
            this.label7.Text = "Tiles Collected:";
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(13, 230);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 23);
            this.label6.TabIndex = 36;
            this.label6.Text = "Time Left:";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 198);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(161, 23);
            this.label5.TabIndex = 35;
            this.label5.Text = "Speed Bonus";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(12, 150);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(80, 23);
            this.label4.TabIndex = 34;
            this.label4.Text = "Time:";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 118);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(80, 23);
            this.label3.TabIndex = 33;
            this.label3.Text = "Score:";
            // 
            // btnRedo
            // 
            this.btnRedo.Location = new System.Drawing.Point(99, 744);
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.Size = new System.Drawing.Size(75, 23);
            this.btnRedo.TabIndex = 32;
            this.btnRedo.Text = "Redo";
            this.btnRedo.UseVisualStyleBackColor = true;
            this.btnRedo.Click += new System.EventHandler(this.btnRedo_Click);
            // 
            // btnPause
            // 
            this.btnPause.Location = new System.Drawing.Point(99, 58);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(75, 23);
            this.btnPause.TabIndex = 16;
            this.btnPause.Text = "Pause";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);
            // 
            // btnShuffle
            // 
            this.btnShuffle.Location = new System.Drawing.Point(18, 87);
            this.btnShuffle.Name = "btnShuffle";
            this.btnShuffle.Size = new System.Drawing.Size(156, 23);
            this.btnShuffle.TabIndex = 9;
            this.btnShuffle.Text = "Shuffle";
            this.btnShuffle.UseVisualStyleBackColor = true;
            this.btnShuffle.Click += new System.EventHandler(this.btnShuffle_Click);
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(18, 58);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(75, 23);
            this.btnNew.TabIndex = 15;
            this.btnNew.Text = "New Game";
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // lblScore
            // 
            this.lblScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScore.Location = new System.Drawing.Point(94, 118);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(96, 23);
            this.lblScore.TabIndex = 18;
            this.lblScore.Text = "0";
            this.lblScore.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pbLayoutPanel
            // 
            this.pbLayoutPanel.Location = new System.Drawing.Point(281, 12);
            this.pbLayoutPanel.Name = "pbLayoutPanel";
            this.pbLayoutPanel.Size = new System.Drawing.Size(998, 984);
            this.pbLayoutPanel.TabIndex = 30;
            this.pbLayoutPanel.TabStop = false;
            this.pbLayoutPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pbLayoutPanel_MouseClick);
            // 
            // GameForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1305, 1011);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pbLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "GameForm";
            this.Text = "Mahjong By Karl";
            this.Load += new System.EventHandler(this.GameForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLayoutPanel)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnUndo;
        private System.Windows.Forms.ComboBox cboLayout;
        private System.Windows.Forms.Label lblClock;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnShuffle;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.PictureBox pbLayoutPanel;
        private System.Windows.Forms.Button btnRedo;
        private System.Windows.Forms.Label lblFinalBonus;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblSpeedBonusTilesCollected;
        private System.Windows.Forms.Label lblSpeedBonusTimeLeft;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblBonusTotal;
        private System.Windows.Forms.Label lblBonusQty;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ListBox lstHighScores;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblShuffleBonus;
        private System.Windows.Forms.Label lblMinuteBonus;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkSound;
    }
}