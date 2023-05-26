namespace MahjongSpriteVersion
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnPause = new System.Windows.Forms.Button();
            this.btnNew = new System.Windows.Forms.Button();
            this.lblClock = new System.Windows.Forms.Label();
            this.btnShuffle = new System.Windows.Forms.Button();
            this.pbLayoutPanel = new System.Windows.Forms.PictureBox();
            this.lblScore = new System.Windows.Forms.Label();
            this.lblSuper = new System.Windows.Forms.Label();
            this.txtSuper = new System.Windows.Forms.TextBox();
            this.txtSuperQty = new System.Windows.Forms.TextBox();
            this.lblTimeBonus = new System.Windows.Forms.Label();
            this.txtTimeBonus = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtSuperTime = new System.Windows.Forms.TextBox();
            this.txtTilesCollected = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cboLayout = new System.Windows.Forms.ComboBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.btnUndo = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbLayoutPanel)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
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
            // lblClock
            // 
            this.lblClock.AutoSize = true;
            this.lblClock.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClock.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblClock.Location = new System.Drawing.Point(18, 467);
            this.lblClock.Name = "lblClock";
            this.lblClock.Size = new System.Drawing.Size(55, 25);
            this.lblClock.TabIndex = 14;
            this.lblClock.Text = "0:00";
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
            // pbLayoutPanel
            // 
            this.pbLayoutPanel.Location = new System.Drawing.Point(217, 10);
            this.pbLayoutPanel.Name = "pbLayoutPanel";
            this.pbLayoutPanel.Size = new System.Drawing.Size(794, 768);
            this.pbLayoutPanel.TabIndex = 17;
            this.pbLayoutPanel.TabStop = false;
            this.pbLayoutPanel.Click += new System.EventHandler(this.pbLayoutPanel_Click);
            this.pbLayoutPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.pbLayoutPanel_MouseClick);
            // 
            // lblScore
            // 
            this.lblScore.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblScore.Location = new System.Drawing.Point(18, 143);
            this.lblScore.Name = "lblScore";
            this.lblScore.Size = new System.Drawing.Size(74, 23);
            this.lblScore.TabIndex = 18;
            this.lblScore.Text = "0";
            this.lblScore.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblSuper
            // 
            this.lblSuper.AutoSize = true;
            this.lblSuper.Location = new System.Drawing.Point(20, 170);
            this.lblSuper.Name = "lblSuper";
            this.lblSuper.Size = new System.Drawing.Size(79, 13);
            this.lblSuper.TabIndex = 19;
            this.lblSuper.Text = "Super Bonuses";
            // 
            // txtSuper
            // 
            this.txtSuper.Location = new System.Drawing.Point(49, 186);
            this.txtSuper.Name = "txtSuper";
            this.txtSuper.ReadOnly = true;
            this.txtSuper.Size = new System.Drawing.Size(44, 20);
            this.txtSuper.TabIndex = 21;
            this.txtSuper.Text = "0";
            // 
            // txtSuperQty
            // 
            this.txtSuperQty.Location = new System.Drawing.Point(18, 186);
            this.txtSuperQty.Name = "txtSuperQty";
            this.txtSuperQty.ReadOnly = true;
            this.txtSuperQty.Size = new System.Drawing.Size(20, 20);
            this.txtSuperQty.TabIndex = 22;
            this.txtSuperQty.Text = "0";
            // 
            // lblTimeBonus
            // 
            this.lblTimeBonus.AutoSize = true;
            this.lblTimeBonus.Location = new System.Drawing.Point(20, 266);
            this.lblTimeBonus.Name = "lblTimeBonus";
            this.lblTimeBonus.Size = new System.Drawing.Size(63, 13);
            this.lblTimeBonus.TabIndex = 23;
            this.lblTimeBonus.Text = "Time Bonus";
            // 
            // txtTimeBonus
            // 
            this.txtTimeBonus.Location = new System.Drawing.Point(18, 282);
            this.txtTimeBonus.Name = "txtTimeBonus";
            this.txtTimeBonus.ReadOnly = true;
            this.txtTimeBonus.Size = new System.Drawing.Size(65, 20);
            this.txtTimeBonus.TabIndex = 24;
            this.txtTimeBonus.Text = "0";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 219);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 25;
            this.label1.Text = "Time Left";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 241);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 26;
            this.label2.Text = "Tiles";
            // 
            // txtSuperTime
            // 
            this.txtSuperTime.Location = new System.Drawing.Point(71, 216);
            this.txtSuperTime.Name = "txtSuperTime";
            this.txtSuperTime.ReadOnly = true;
            this.txtSuperTime.Size = new System.Drawing.Size(20, 20);
            this.txtSuperTime.TabIndex = 27;
            this.txtSuperTime.Text = "0";
            // 
            // txtTilesCollected
            // 
            this.txtTilesCollected.Location = new System.Drawing.Point(72, 238);
            this.txtTilesCollected.Name = "txtTilesCollected";
            this.txtTilesCollected.ReadOnly = true;
            this.txtTilesCollected.Size = new System.Drawing.Size(20, 20);
            this.txtTilesCollected.TabIndex = 28;
            this.txtTilesCollected.Text = "0";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnUndo);
            this.panel1.Controls.Add(this.comboBox1);
            this.panel1.Controls.Add(this.cboLayout);
            this.panel1.Controls.Add(this.lblClock);
            this.panel1.Controls.Add(this.txtTilesCollected);
            this.panel1.Controls.Add(this.btnPause);
            this.panel1.Controls.Add(this.txtSuperTime);
            this.panel1.Controls.Add(this.btnShuffle);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.btnNew);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.lblScore);
            this.panel1.Controls.Add(this.txtTimeBonus);
            this.panel1.Controls.Add(this.lblSuper);
            this.panel1.Controls.Add(this.lblTimeBonus);
            this.panel1.Controls.Add(this.txtSuper);
            this.panel1.Controls.Add(this.txtSuperQty);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(200, 779);
            this.panel1.TabIndex = 29;
            // 
            // cboLayout
            // 
            this.cboLayout.FormattingEnabled = true;
            this.cboLayout.Items.AddRange(new object[] {
            "Number One",
            "The Runner Up"});
            this.cboLayout.Location = new System.Drawing.Point(18, 116);
            this.cboLayout.Name = "cboLayout";
            this.cboLayout.Size = new System.Drawing.Size(156, 21);
            this.cboLayout.TabIndex = 29;
            this.cboLayout.SelectedIndexChanged += new System.EventHandler(this.cboLayout_SelectedIndexChanged);
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(17, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 30;
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
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1011, 779);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pbLayoutPanel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "Mahjong by Karl";
            this.Load += new System.EventHandler(this.MainForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pbLayoutPanel)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPause;
        private System.Windows.Forms.Button btnNew;
        private System.Windows.Forms.Label lblClock;
        private System.Windows.Forms.Button btnShuffle;
        private System.Windows.Forms.PictureBox pbLayoutPanel;
        private System.Windows.Forms.Label lblScore;
        private System.Windows.Forms.Label lblSuper;
        private System.Windows.Forms.TextBox txtSuper;
        private System.Windows.Forms.TextBox txtSuperQty;
        private System.Windows.Forms.Label lblTimeBonus;
        private System.Windows.Forms.TextBox txtTimeBonus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtSuperTime;
        private System.Windows.Forms.TextBox txtTilesCollected;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox cboLayout;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button btnUndo;
    }
}

