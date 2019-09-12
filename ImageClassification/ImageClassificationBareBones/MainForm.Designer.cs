namespace ImageClassificationBareBones
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
            this.btnSimplerClassification = new System.Windows.Forms.Button();
            this.btnSimpleClassification = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSimplerClassification
            // 
            this.btnSimplerClassification.Location = new System.Drawing.Point(146, 12);
            this.btnSimplerClassification.Name = "btnSimplerClassification";
            this.btnSimplerClassification.Size = new System.Drawing.Size(128, 23);
            this.btnSimplerClassification.TabIndex = 3;
            this.btnSimplerClassification.Text = "Simpler Classification";
            this.btnSimplerClassification.UseVisualStyleBackColor = true;
            this.btnSimplerClassification.Click += new System.EventHandler(this.btnSimplerClassification_Click);
            // 
            // btnSimpleClassification
            // 
            this.btnSimpleClassification.Location = new System.Drawing.Point(12, 12);
            this.btnSimpleClassification.Name = "btnSimpleClassification";
            this.btnSimpleClassification.Size = new System.Drawing.Size(128, 23);
            this.btnSimpleClassification.TabIndex = 2;
            this.btnSimpleClassification.Text = "Simple Classification";
            this.btnSimpleClassification.UseVisualStyleBackColor = true;
            this.btnSimpleClassification.Click += new System.EventHandler(this.btnSimpleClassification_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnSimplerClassification);
            this.Controls.Add(this.btnSimpleClassification);
            this.Name = "MainForm";
            this.Text = "Image Classification - Bare Bones";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSimplerClassification;
        private System.Windows.Forms.Button btnSimpleClassification;
    }
}

