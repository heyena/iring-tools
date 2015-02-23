namespace XmlToDbMigrationUtility
{
    partial class XmlToDbMigrationUtilityForm
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
            this.InputFolderLabel = new System.Windows.Forms.Label();
            this.InputFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.InputFolderTextBox = new System.Windows.Forms.TextBox();
            this.InputFolderButton = new System.Windows.Forms.Button();
            this.MigrationProgressBar = new System.Windows.Forms.ProgressBar();
            this.LogRichTextBox = new System.Windows.Forms.RichTextBox();
            this.StartMigrationButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // InputFolderLabel
            // 
            this.InputFolderLabel.AutoSize = true;
            this.InputFolderLabel.Location = new System.Drawing.Point(12, 9);
            this.InputFolderLabel.Name = "InputFolderLabel";
            this.InputFolderLabel.Size = new System.Drawing.Size(116, 13);
            this.InputFolderLabel.TabIndex = 0;
            this.InputFolderLabel.Text = "Select input folder path";
            // 
            // InputFolderBrowserDialog
            // 
            this.InputFolderBrowserDialog.ShowNewFolderButton = false;
            // 
            // InputFolderTextBox
            // 
            this.InputFolderTextBox.Location = new System.Drawing.Point(134, 6);
            this.InputFolderTextBox.Name = "InputFolderTextBox";
            this.InputFolderTextBox.Size = new System.Drawing.Size(271, 20);
            this.InputFolderTextBox.TabIndex = 1;
            // 
            // InputFolderButton
            // 
            this.InputFolderButton.Location = new System.Drawing.Point(411, 4);
            this.InputFolderButton.Name = "InputFolderButton";
            this.InputFolderButton.Size = new System.Drawing.Size(75, 23);
            this.InputFolderButton.TabIndex = 2;
            this.InputFolderButton.Text = "&Input Folder";
            this.InputFolderButton.UseVisualStyleBackColor = true;
            this.InputFolderButton.Click += new System.EventHandler(this.InputFolderButton_Click);
            // 
            // MigrationProgressBar
            // 
            this.MigrationProgressBar.Location = new System.Drawing.Point(208, 61);
            this.MigrationProgressBar.Name = "MigrationProgressBar";
            this.MigrationProgressBar.Size = new System.Drawing.Size(88, 10);
            this.MigrationProgressBar.TabIndex = 3;
            // 
            // LogRichTextBox
            // 
            this.LogRichTextBox.Location = new System.Drawing.Point(15, 77);
            this.LogRichTextBox.Name = "LogRichTextBox";
            this.LogRichTextBox.Size = new System.Drawing.Size(471, 175);
            this.LogRichTextBox.TabIndex = 5;
            this.LogRichTextBox.Text = "";
            // 
            // StartMigrationButton
            // 
            this.StartMigrationButton.Location = new System.Drawing.Point(208, 32);
            this.StartMigrationButton.Name = "StartMigrationButton";
            this.StartMigrationButton.Size = new System.Drawing.Size(88, 23);
            this.StartMigrationButton.TabIndex = 6;
            this.StartMigrationButton.Text = "&Start migration";
            this.StartMigrationButton.UseVisualStyleBackColor = true;
            this.StartMigrationButton.Click += new System.EventHandler(this.StartMigrationButton_Click);
            // 
            // XmlToDbMigrationUtilityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 261);
            this.Controls.Add(this.StartMigrationButton);
            this.Controls.Add(this.LogRichTextBox);
            this.Controls.Add(this.MigrationProgressBar);
            this.Controls.Add(this.InputFolderButton);
            this.Controls.Add(this.InputFolderTextBox);
            this.Controls.Add(this.InputFolderLabel);
            this.Name = "XmlToDbMigrationUtilityForm";
            this.Text = "Xml To DB Migration Utility";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label InputFolderLabel;
        private System.Windows.Forms.FolderBrowserDialog InputFolderBrowserDialog;
        private System.Windows.Forms.TextBox InputFolderTextBox;
        private System.Windows.Forms.Button InputFolderButton;
        private System.Windows.Forms.ProgressBar MigrationProgressBar;
        private System.Windows.Forms.RichTextBox LogRichTextBox;
        private System.Windows.Forms.Button StartMigrationButton;
    }
}

