namespace MornyMorse
{
    partial class MainForm
    {
        public class NoKeyboardCheckedListBox : CheckedListBox
        {
            protected override void OnKeyPress(KeyPressEventArgs e)
            {
                // Suppress all key presses to disable keyboard search
                e.Handled = true;
                // Don't call base.OnKeyPress(e) to stop further processing
            }
        }
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
            replayButton = new Button();
            panel1 = new Panel();
            startButton = new Button();
            testSetChecks = new NoKeyboardCheckedListBox();
            averageValue = new Label();
            wpmLabel = new Label();
            label1 = new Label();
            wpm = new NumericUpDown();
            letterWpm = new NumericUpDown();
            label2 = new Label();
            currentInputText = new Label();
            ((System.ComponentModel.ISupportInitialize)wpm).BeginInit();
            ((System.ComponentModel.ISupportInitialize)letterWpm).BeginInit();
            SuspendLayout();
            // 
            // replayButton
            // 
            replayButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            replayButton.Location = new Point(1821, 67);
            replayButton.Name = "replayButton";
            replayButton.Size = new Size(173, 40);
            replayButton.TabIndex = 2;
            replayButton.Text = "Replay (Shift+A)";
            replayButton.UseVisualStyleBackColor = true;
            replayButton.Click += replay_Click;
            // 
            // panel1
            // 
            panel1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            panel1.Location = new Point(12, 21);
            panel1.Name = "panel1";
            panel1.Size = new Size(1780, 1085);
            panel1.TabIndex = 3;
            // 
            // startButton
            // 
            startButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startButton.Location = new Point(1821, 21);
            startButton.Name = "startButton";
            startButton.Size = new Size(173, 40);
            startButton.TabIndex = 4;
            startButton.Text = "Start";
            startButton.UseVisualStyleBackColor = true;
            startButton.Click += start_Click;
            // 
            // testSetChecks
            // 
            testSetChecks.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            testSetChecks.CheckOnClick = true;
            testSetChecks.FormattingEnabled = true;
            testSetChecks.Items.AddRange(new object[] { "Letters", "Numbers", "ProSigns", "Bigrams", "Trigrams", "Words" });
            testSetChecks.Location = new Point(1823, 232);
            testSetChecks.Name = "testSetChecks";
            testSetChecks.Size = new Size(173, 228);
            testSetChecks.TabIndex = 5;
            // 
            // averageValue
            // 
            averageValue.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            averageValue.AutoSize = true;
            averageValue.Location = new Point(1821, 571);
            averageValue.Name = "averageValue";
            averageValue.Size = new Size(24, 30);
            averageValue.TabIndex = 7;
            averageValue.Text = "0";
            // 
            // wpmLabel
            // 
            wpmLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            wpmLabel.AutoSize = true;
            wpmLabel.Location = new Point(1823, 126);
            wpmLabel.Name = "wpmLabel";
            wpmLabel.Size = new Size(64, 30);
            wpmLabel.TabIndex = 10;
            wpmLabel.Text = "WPM";
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(1823, 179);
            label1.Name = "label1";
            label1.Size = new Size(77, 30);
            label1.TabIndex = 11;
            label1.Text = "Spaces";
            // 
            // wpm
            // 
            wpm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            wpm.Location = new Point(1907, 123);
            wpm.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            wpm.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            wpm.Name = "wpm";
            wpm.Size = new Size(87, 35);
            wpm.TabIndex = 12;
            wpm.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // letterWpm
            // 
            letterWpm.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            letterWpm.Location = new Point(1905, 178);
            letterWpm.Maximum = new decimal(new int[] { 50, 0, 0, 0 });
            letterWpm.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            letterWpm.Name = "letterWpm";
            letterWpm.Size = new Size(91, 35);
            letterWpm.TabIndex = 13;
            letterWpm.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(1820, 479);
            label2.Name = "label2";
            label2.Size = new Size(67, 30);
            label2.TabIndex = 14;
            label2.Text = "Input:";
            label2.Click += label2_Click;
            // 
            // currentInputText
            // 
            currentInputText.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            currentInputText.AutoSize = true;
            currentInputText.BorderStyle = BorderStyle.FixedSingle;
            currentInputText.Font = new Font("Segoe UI", 11.1428576F, FontStyle.Regular, GraphicsUnit.Point, 0);
            currentInputText.Location = new Point(1823, 509);
            currentInputText.Name = "currentInputText";
            currentInputText.Size = new Size(2, 39);
            currentInputText.TabIndex = 15;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2018, 1118);
            Controls.Add(currentInputText);
            Controls.Add(label2);
            Controls.Add(letterWpm);
            Controls.Add(wpm);
            Controls.Add(label1);
            Controls.Add(wpmLabel);
            Controls.Add(averageValue);
            Controls.Add(testSetChecks);
            Controls.Add(startButton);
            Controls.Add(panel1);
            Controls.Add(replayButton);
            KeyPreview = true;
            MinimumSize = new Size(20, 20);
            Name = "MainForm";
            Text = "Morny Morse";
            ((System.ComponentModel.ISupportInitialize)wpm).EndInit();
            ((System.ComponentModel.ISupportInitialize)letterWpm).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button replayButton;
        private Panel panel1;
        private Button startButton;
        private NoKeyboardCheckedListBox testSetChecks;
        private Label averageValue;
        private Label wpmLabel;
        private Label label1;
        private NumericUpDown wpm;
        private NumericUpDown letterWpm;
        private Label label2;
        private Label currentInputText;
    }
}
