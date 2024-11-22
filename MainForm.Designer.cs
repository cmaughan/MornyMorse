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
            SuspendLayout();
            // 
            // replayButton
            // 
            replayButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            replayButton.Location = new Point(1136, 67);
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
            panel1.Size = new Size(1095, 1085);
            panel1.TabIndex = 3;
            // 
            // startButton
            // 
            startButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            startButton.Location = new Point(1136, 21);
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
            testSetChecks.Location = new Point(1136, 127);
            testSetChecks.Name = "testSetChecks";
            testSetChecks.Size = new Size(173, 228);
            testSetChecks.TabIndex = 5;
            // 
            // averageValue
            // 
            averageValue.AutoSize = true;
            averageValue.Location = new Point(1136, 373);
            averageValue.Name = "averageValue";
            averageValue.Size = new Size(24, 30);
            averageValue.TabIndex = 7;
            averageValue.Text = "0";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(12F, 30F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1333, 1118);
            Controls.Add(averageValue);
            Controls.Add(testSetChecks);
            Controls.Add(startButton);
            Controls.Add(panel1);
            Controls.Add(replayButton);
            KeyPreview = true;
            Name = "MainForm";
            Text = "Morny Morse";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button replayButton;
        private Panel panel1;
        private Button startButton;
        private NoKeyboardCheckedListBox testSetChecks;
        private Label averageValue;
    }
}
