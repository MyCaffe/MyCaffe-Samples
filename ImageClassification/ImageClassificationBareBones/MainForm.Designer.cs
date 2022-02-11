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
            this.btnTestTrainedWeights = new System.Windows.Forms.Button();
            this.btnSimplerClassificationWithProgrammableModels = new System.Windows.Forms.Button();
            this.btnSimplestClassification = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
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
            // btnTestTrainedWeights
            // 
            this.btnTestTrainedWeights.Location = new System.Drawing.Point(12, 41);
            this.btnTestTrainedWeights.Name = "btnTestTrainedWeights";
            this.btnTestTrainedWeights.Size = new System.Drawing.Size(128, 23);
            this.btnTestTrainedWeights.TabIndex = 4;
            this.btnTestTrainedWeights.Text = "Test Trained Weights";
            this.btnTestTrainedWeights.UseVisualStyleBackColor = true;
            this.btnTestTrainedWeights.Click += new System.EventHandler(this.btnTestTrainedWeights_Click);
            // 
            // btnSimplerClassificationWithProgrammableModels
            // 
            this.btnSimplerClassificationWithProgrammableModels.Location = new System.Drawing.Point(280, 12);
            this.btnSimplerClassificationWithProgrammableModels.Name = "btnSimplerClassificationWithProgrammableModels";
            this.btnSimplerClassificationWithProgrammableModels.Size = new System.Drawing.Size(265, 23);
            this.btnSimplerClassificationWithProgrammableModels.TabIndex = 3;
            this.btnSimplerClassificationWithProgrammableModels.Text = "Simpler Classification with Programmable Models";
            this.btnSimplerClassificationWithProgrammableModels.UseVisualStyleBackColor = true;
            this.btnSimplerClassificationWithProgrammableModels.Click += new System.EventHandler(this.btnSimplerClassificationWithProgrammableModels_Click);
            // 
            // btnSimplestClassification
            // 
            this.btnSimplestClassification.Location = new System.Drawing.Point(551, 12);
            this.btnSimplestClassification.Name = "btnSimplestClassification";
            this.btnSimplestClassification.Size = new System.Drawing.Size(151, 23);
            this.btnSimplestClassification.TabIndex = 3;
            this.btnSimplestClassification.Text = "Simplest Classification";
            this.btnSimplestClassification.UseVisualStyleBackColor = true;
            this.btnSimplestClassification.Click += new System.EventHandler(this.btnSimplestClassification_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(156, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(425, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "See Visual Studio Output window for debug output showing the status of each opera" +
    "tion.";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 96);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnTestTrainedWeights);
            this.Controls.Add(this.btnSimplerClassificationWithProgrammableModels);
            this.Controls.Add(this.btnSimplestClassification);
            this.Controls.Add(this.btnSimplerClassification);
            this.Controls.Add(this.btnSimpleClassification);
            this.Name = "MainForm";
            this.Text = "Image Classification - Bare Bones";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSimplerClassification;
        private System.Windows.Forms.Button btnSimpleClassification;
        private System.Windows.Forms.Button btnTestTrainedWeights;
        private System.Windows.Forms.Button btnSimplerClassificationWithProgrammableModels;
        private System.Windows.Forms.Button btnSimplestClassification;
        private System.Windows.Forms.Label label1;
    }
}

