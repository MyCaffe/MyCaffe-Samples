namespace ImageClassification
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
            this.btnSimpleClassification = new System.Windows.Forms.Button();
            this.btnExportImages = new System.Windows.Forms.Button();
            this.btnSimplerClassification = new System.Windows.Forms.Button();
            this.btnSimplestClassification = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSimpleClassification
            // 
            this.btnSimpleClassification.Location = new System.Drawing.Point(137, 12);
            this.btnSimpleClassification.Name = "btnSimpleClassification";
            this.btnSimpleClassification.Size = new System.Drawing.Size(128, 23);
            this.btnSimpleClassification.TabIndex = 0;
            this.btnSimpleClassification.Text = "Simple Classification";
            this.btnSimpleClassification.UseVisualStyleBackColor = true;
            this.btnSimpleClassification.Click += new System.EventHandler(this.btnSimpleClassification_Click);
            // 
            // btnExportImages
            // 
            this.btnExportImages.Location = new System.Drawing.Point(25, 12);
            this.btnExportImages.Name = "btnExportImages";
            this.btnExportImages.Size = new System.Drawing.Size(106, 23);
            this.btnExportImages.TabIndex = 0;
            this.btnExportImages.Text = "Export Images";
            this.btnExportImages.UseVisualStyleBackColor = true;
            this.btnExportImages.Click += new System.EventHandler(this.btnExportImages_Click);
            // 
            // btnSimplerClassification
            // 
            this.btnSimplerClassification.Location = new System.Drawing.Point(271, 12);
            this.btnSimplerClassification.Name = "btnSimplerClassification";
            this.btnSimplerClassification.Size = new System.Drawing.Size(128, 23);
            this.btnSimplerClassification.TabIndex = 1;
            this.btnSimplerClassification.Text = "Simpler Classification";
            this.btnSimplerClassification.UseVisualStyleBackColor = true;
            this.btnSimplerClassification.Click += new System.EventHandler(this.btnSimplerClassification_Click);
            // 
            // btnSimplestClassification
            // 
            this.btnSimplestClassification.Location = new System.Drawing.Point(405, 12);
            this.btnSimplestClassification.Name = "btnSimplestClassification";
            this.btnSimplestClassification.Size = new System.Drawing.Size(128, 23);
            this.btnSimplestClassification.TabIndex = 2;
            this.btnSimplestClassification.Text = "Simplest Classification";
            this.btnSimplestClassification.UseVisualStyleBackColor = true;
            this.btnSimplestClassification.Click += new System.EventHandler(this.btnSimplestClassification_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnSimplestClassification);
            this.Controls.Add(this.btnSimplerClassification);
            this.Controls.Add(this.btnExportImages);
            this.Controls.Add(this.btnSimpleClassification);
            this.Name = "MainForm";
            this.Text = "Image Classification";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSimpleClassification;
        private System.Windows.Forms.Button btnExportImages;
        private System.Windows.Forms.Button btnSimplerClassification;
        private System.Windows.Forms.Button btnSimplestClassification;
    }
}

