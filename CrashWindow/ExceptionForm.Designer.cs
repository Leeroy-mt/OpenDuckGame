using CrashWindow;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
namespace DuckGame.src.MonoTime.Console
{
    public class ExRichTextBox : RichTextBox
    {
        public ExRichTextBox()
        {
            Selectable = false;
        }
        const int WM_SETFOCUS = 0x0007;
        const int WM_KILLFOCUS = 0x0008;

        ///<summary>
        /// Enables or disables selection highlight. 
        /// If you set `Selectable` to `false` then the selection highlight
        /// will be disabled. 
        /// It's enabled by default.
        ///</summary>
        [DefaultValue(true)]
        public bool Selectable { get; set; }
        protected override void WndProc(ref Message m)
        {
            
            //if (m.Msg == 32)
            //{
            //    m.Msg = 32;
            //    return;
            //}
            //else if (m.Msg != WM_SETFOCUS)
            //{
            //    System.Console.WriteLine(m.Msg.ToString());
            //}
            if (m.Msg == WM_SETFOCUS && !Selectable)
                m.Msg = WM_KILLFOCUS;

            base.WndProc(ref m);
        }
    }
    partial class ExceptionForm
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
            crashDescription = new RichTextBox();
            pictureBox1 = new PictureBox();
            crashSourceLabel = new Label();
            forumLink = new LinkLabel();
            modDesc = new Label();
            button1 = new Button();
            button2 = new Button();
            restartButton = new Button();
            richTextBox1 = new RichTextBox();
            checkBox1 = new CheckBox();
            richTextBox2 = new ExRichTextBox();
            ((ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // crashDescription
            // 
            crashDescription.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            crashDescription.BackColor = Color.FromArgb(240, 240, 240);
            crashDescription.Location = new Point(12, 109);
            crashDescription.Name = "crashDescription";
            crashDescription.ReadOnly = true;
            crashDescription.Size = new Size(560, 195);
            crashDescription.TabIndex = 0;
            crashDescription.Text = "";
            crashDescription.LinkClicked += crashDescription_LinkClicked;
            crashDescription.TextChanged += crashDescription_TextChanged;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.FromArgb(240, 240, 240);
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(65, 65);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // crashSourceLabel
            // 
            crashSourceLabel.AutoSize = true;
            crashSourceLabel.BackColor = Color.FromArgb(240, 240, 240);
            crashSourceLabel.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            crashSourceLabel.ForeColor = Color.Red;
            crashSourceLabel.Location = new Point(12, 80);
            crashSourceLabel.Name = "crashSourceLabel";
            crashSourceLabel.Size = new Size(91, 13);
            crashSourceLabel.TabIndex = 3;
            crashSourceLabel.Text = "Crash Source: ";
            // 
            // forumLink
            // 
            forumLink.AutoSize = true;
            forumLink.BackColor = Color.FromArgb(240, 240, 240);
            forumLink.LinkArea = new LinkArea(0, 0);
            forumLink.Location = new Point(205, 40);
            forumLink.Name = "forumLink";
            forumLink.Size = new Size(211, 21);
            forumLink.TabIndex = 4;
            forumLink.Text = "Duck Game Technical Support Forums";
            forumLink.UseCompatibleTextRendering = true;
            forumLink.LinkClicked += forumLink_LinkClicked;
            // 
            // modDesc
            // 
            modDesc.AutoSize = true;
            modDesc.BackColor = Color.FromArgb(240, 240, 240);
            modDesc.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            modDesc.ForeColor = Color.Red;
            modDesc.Location = new Point(12, 93);
            modDesc.Name = "modDesc";
            modDesc.Size = new Size(303, 13);
            modDesc.TabIndex = 5;
            modDesc.Text = "Mods apparently were not responsible for this crash.";
            // 
            // button1
            // 
            button1.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button1.BackColor = Color.FromArgb(240, 240, 240);
            button1.Location = new Point(497, 393);
            button1.Name = "button1";
            button1.Size = new Size(75, 31);
            button1.TabIndex = 6;
            button1.Text = "Close";
            button1.UseVisualStyleBackColor = false;
            button1.Click += button1_Click;
            // 
            // button2
            // 
            button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            button2.BackColor = Color.FromArgb(240, 240, 240);
            button2.Location = new Point(416, 393);
            button2.Name = "button2";
            button2.Size = new Size(75, 31);
            button2.TabIndex = 7;
            button2.Text = "Copy";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click;
            // 
            // restartButton
            // 
            restartButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            restartButton.BackColor = Color.FromArgb(240, 240, 240);
            restartButton.Location = new Point(12, 393);
            restartButton.Name = "restartButton";
            restartButton.Size = new Size(124, 31);
            restartButton.TabIndex = 8;
            restartButton.Text = "Restart Duck Game";
            restartButton.UseVisualStyleBackColor = false;
            restartButton.Click += restartButton_Click;
            // 
            // richTextBox1
            // 
            richTextBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox1.BackColor = Color.FromArgb(240, 240, 240);
            richTextBox1.Location = new Point(12, 310);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.ReadOnly = true;
            richTextBox1.Size = new Size(560, 69);
            richTextBox1.TabIndex = 10;
            richTextBox1.Text = "<Information about this crash will be automatically sent to the developer, but if you can provide any additional information please do it here!>";
            richTextBox1.TextChanged += richTextBox1_TextChanged;
            // 
            // checkBox1
            // 
            checkBox1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            checkBox1.AutoSize = true;
            checkBox1.BackColor = Color.FromArgb(240, 240, 240);
            checkBox1.Location = new Point(182, 406);
            checkBox1.Margin = new Padding(2);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new Size(135, 19);
            checkBox1.TabIndex = 11;
            checkBox1.Text = "Submit Crash Report";
            checkBox1.UseVisualStyleBackColor = false;
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
            // 
            // richTextBox2
            // 
            richTextBox2.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            richTextBox2.AutoSize = true;
            richTextBox2.BackColor = Color.FromArgb(240, 240, 240);
            richTextBox2.BorderStyle = BorderStyle.None;
            richTextBox2.Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            richTextBox2.Location = new Point(83, 9);
            richTextBox2.Margin = new Padding(3, 0, 3, 0);
            richTextBox2.Name = "richTextBox2";
            richTextBox2.ScrollBars = RichTextBoxScrollBars.None;
            richTextBox2.Selectable = false;
            richTextBox2.Size = new Size(489, 68);
            richTextBox2.TabIndex = 12;
            richTextBox2.Text = "";
            richTextBox2.TextChanged += richTextBox2_TextChanged;
            // 
            // ExceptionForm
            // 
            BackColor = Color.FromArgb(240, 240, 240);
            ClientSize = new Size(584, 436);
            Controls.Add(richTextBox2);
            Controls.Add(checkBox1);
            Controls.Add(richTextBox1);
            Controls.Add(restartButton);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(modDesc);
            Controls.Add(forumLink);
            Controls.Add(crashSourceLabel);
            Controls.Add(pictureBox1);
            Controls.Add(crashDescription);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Margin = new Padding(4, 5, 4, 5);
            Name = "ExceptionForm";
            Text = "Duck Game Crash Report";
            Load += ExceptionForm_Load;
            ((ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox crashDescription;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label crashSourceLabel;
        private System.Windows.Forms.LinkLabel forumLink;
        private Label modDesc;
        private Button button1;
        private Button button2;
        private Button restartButton;
        private RichTextBox richTextBox1;
        private CheckBox checkBox1;
        private ExRichTextBox richTextBox2;
    }
}