namespace BuildSingleSqlScriptFile
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
            this.folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            this.btnFromFolder = new System.Windows.Forms.Button();
            this.btnToFolder = new System.Windows.Forms.Button();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.lblValidation = new System.Windows.Forms.Label();
            this.lbVersions = new System.Windows.Forms.ListBox();
            this.txtFromFolder = new System.Windows.Forms.TextBox();
            this.txtToFolder = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnFromFolder
            // 
            this.btnFromFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFromFolder.Location = new System.Drawing.Point(465, 3);
            this.btnFromFolder.Name = "btnFromFolder";
            this.btnFromFolder.Size = new System.Drawing.Size(198, 33);
            this.btnFromFolder.TabIndex = 1;
            this.btnFromFolder.Text = "Browse to SQL Script Root Folder";
            this.btnFromFolder.UseVisualStyleBackColor = true;
            this.btnFromFolder.Click += new System.EventHandler(this.btnFromFolder_Click);
            // 
            // btnToFolder
            // 
            this.btnToFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnToFolder.Location = new System.Drawing.Point(465, 51);
            this.btnToFolder.Name = "btnToFolder";
            this.btnToFolder.Size = new System.Drawing.Size(198, 33);
            this.btnToFolder.TabIndex = 3;
            this.btnToFolder.Text = "Browse to Destination Folder";
            this.btnToFolder.UseVisualStyleBackColor = true;
            this.btnToFolder.Click += new System.EventHandler(this.btnToFolder_Click);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnGenerate.Location = new System.Drawing.Point(193, 179);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(171, 33);
            this.btnGenerate.TabIndex = 4;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // lblValidation
            // 
            this.lblValidation.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblValidation.ForeColor = System.Drawing.Color.Red;
            this.lblValidation.Location = new System.Drawing.Point(12, 222);
            this.lblValidation.Name = "lblValidation";
            this.lblValidation.Size = new System.Drawing.Size(651, 185);
            this.lblValidation.TabIndex = 5;
            // 
            // lbVersions
            // 
            this.lbVersions.FormattingEnabled = true;
            this.lbVersions.Location = new System.Drawing.Point(16, 104);
            this.lbVersions.Name = "lbVersions";
            this.lbVersions.Size = new System.Drawing.Size(158, 108);
            this.lbVersions.TabIndex = 6;
            // 
            // txtFromFolder
            // 
            this.txtFromFolder.Location = new System.Drawing.Point(8, 10);
            this.txtFromFolder.Name = "txtFromFolder";
            this.txtFromFolder.Size = new System.Drawing.Size(451, 20);
            this.txtFromFolder.TabIndex = 7;
            this.txtFromFolder.TextChanged += new System.EventHandler(this.txtFromFolder_TextChanged);
            // 
            // txtToFolder
            // 
            this.txtToFolder.Location = new System.Drawing.Point(8, 58);
            this.txtToFolder.Name = "txtToFolder";
            this.txtToFolder.Size = new System.Drawing.Size(451, 20);
            this.txtToFolder.TabIndex = 8;
            this.txtToFolder.TextChanged += new System.EventHandler(this.txtToFolder_TextChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 416);
            this.Controls.Add(this.txtToFolder);
            this.Controls.Add(this.txtFromFolder);
            this.Controls.Add(this.lbVersions);
            this.Controls.Add(this.lblValidation);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.btnToFolder);
            this.Controls.Add(this.btnFromFolder);
            this.Name = "Form1";
            this.Text = "GenBOE Sql File \'Combinator\'";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowser;
        private System.Windows.Forms.Button btnFromFolder;
        private System.Windows.Forms.Button btnToFolder;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Label lblValidation;
        private System.Windows.Forms.ListBox lbVersions;
        private System.Windows.Forms.TextBox txtFromFolder;
        private System.Windows.Forms.TextBox txtToFolder;
    }
}

