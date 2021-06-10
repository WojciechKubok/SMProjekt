
namespace SMProjekt
{
    partial class Form1
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.playButton = new System.Windows.Forms.Button();
            this.recordButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.pauzePlayButton = new System.Windows.Forms.Button();
            this.timerLabel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.pauzeRecordButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(12, 14);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(502, 166);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick_1);
            // 
            // playButton
            // 
            this.playButton.Location = new System.Drawing.Point(692, 43);
            this.playButton.Name = "playButton";
            this.playButton.Size = new System.Drawing.Size(75, 23);
            this.playButton.TabIndex = 3;
            this.playButton.Text = "Odtwórz";
            this.playButton.UseVisualStyleBackColor = true;
            this.playButton.Click += new System.EventHandler(this.openToolStripMenuItem_Click_1);
            // 
            // recordButton
            // 
            this.recordButton.Location = new System.Drawing.Point(692, 231);
            this.recordButton.Name = "recordButton";
            this.recordButton.Size = new System.Drawing.Size(75, 23);
            this.recordButton.TabIndex = 4;
            this.recordButton.Text = "Nagraj";
            this.recordButton.UseVisualStyleBackColor = true;
            this.recordButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(669, 111);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(119, 23);
            this.stopButton.TabIndex = 5;
            this.stopButton.Text = "Stop odtwarzania";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // pauzePlayButton
            // 
            this.pauzePlayButton.Location = new System.Drawing.Point(669, 82);
            this.pauzePlayButton.Name = "pauzePlayButton";
            this.pauzePlayButton.Size = new System.Drawing.Size(119, 23);
            this.pauzePlayButton.TabIndex = 6;
            this.pauzePlayButton.Text = "Pauza odtwarzania";
            this.pauzePlayButton.UseVisualStyleBackColor = true;
            this.pauzePlayButton.Click += new System.EventHandler(this.pauzePlayButton_Click);
            // 
            // timerLabel
            // 
            this.timerLabel.AutoSize = true;
            this.timerLabel.Location = new System.Drawing.Point(548, 14);
            this.timerLabel.Name = "timerLabel";
            this.timerLabel.Size = new System.Drawing.Size(49, 13);
            this.timerLabel.TabIndex = 7;
            this.timerLabel.Text = "00:00:00";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(669, 301);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(119, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Stop nagrywania";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // timer2
            // 
            this.timer2.Interval = 10;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // pauzeRecordButton
            // 
            this.pauzeRecordButton.Location = new System.Drawing.Point(669, 272);
            this.pauzeRecordButton.Name = "pauzeRecordButton";
            this.pauzeRecordButton.Size = new System.Drawing.Size(119, 23);
            this.pauzeRecordButton.TabIndex = 9;
            this.pauzeRecordButton.Text = "Pauza nagrywania";
            this.pauzeRecordButton.UseVisualStyleBackColor = true;
            this.pauzeRecordButton.Click += new System.EventHandler(this.pauzeRecordButton_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.pauzeRecordButton);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.timerLabel);
            this.Controls.Add(this.pauzePlayButton);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.recordButton);
            this.Controls.Add(this.playButton);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button playButton;
        private System.Windows.Forms.Button recordButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button pauzePlayButton;
        private System.Windows.Forms.Label timerLabel;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Button pauzeRecordButton;
    }
}

