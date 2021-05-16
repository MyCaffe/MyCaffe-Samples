
namespace Seq2SeqChatBot
{
    partial class FormMain
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bindingNavigatorMoveFirstItem = new System.Windows.Forms.ToolStripButton();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.pbProgress = new System.Windows.Forms.ToolStripProgressBar();
            this.lblProgress = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.lblLastAccuracy = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_bw = new System.ComponentModel.BackgroundWorker();
            this.timerUI = new System.Windows.Forms.Timer(this.components);
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lblIterations = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnBrowseTargetTextFile = new System.Windows.Forms.Button();
            this.btnBrowseInputTextFile = new System.Windows.Forms.Button();
            this.edtBatch = new System.Windows.Forms.TextBox();
            this.edtIterations = new System.Windows.Forms.TextBox();
            this.edtInput = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.edtTargetTextFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.edtInputTextFile = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.pbImageLoss = new System.Windows.Forms.PictureBox();
            this.pbImageAccuracy = new System.Windows.Forms.PictureBox();
            this.lvStatus = new Seq2SeqChatBot.ListViewEx();
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnTrain = new System.Windows.Forms.ToolStripButton();
            this.btnRun = new System.Windows.Forms.ToolStripButton();
            this.btnStop = new System.Windows.Forms.ToolStripButton();
            this.btnDeleteWeights = new System.Windows.Forms.ToolStripButton();
            this.btnEnableVerboseOutput = new System.Windows.Forms.ToolStripButton();
            this.openFileDialogTxt = new System.Windows.Forms.OpenFileDialog();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.lvDiscussion = new Seq2SeqChatBot.ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbImageLoss)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbImageAccuracy)).BeginInit();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(987, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bindingNavigatorMoveFirstItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // bindingNavigatorMoveFirstItem
            // 
            this.bindingNavigatorMoveFirstItem.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bindingNavigatorMoveFirstItem.Image = ((System.Drawing.Image)(resources.GetObject("bindingNavigatorMoveFirstItem.Image")));
            this.bindingNavigatorMoveFirstItem.Name = "bindingNavigatorMoveFirstItem";
            this.bindingNavigatorMoveFirstItem.RightToLeftAutoMirrorImage = true;
            this.bindingNavigatorMoveFirstItem.Size = new System.Drawing.Size(23, 20);
            this.bindingNavigatorMoveFirstItem.Text = "Move first";
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.closeToolStripMenuItem.Text = "&Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pbProgress,
            this.lblProgress,
            this.toolStripStatusLabel1,
            this.lblLastAccuracy});
            this.statusStrip1.Location = new System.Drawing.Point(0, 818);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(987, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // pbProgress
            // 
            this.pbProgress.ForeColor = System.Drawing.Color.Lime;
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(300, 16);
            this.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // lblProgress
            // 
            this.lblProgress.AutoSize = false;
            this.lblProgress.BackColor = System.Drawing.Color.Black;
            this.lblProgress.Font = new System.Drawing.Font("Century Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProgress.ForeColor = System.Drawing.Color.Lime;
            this.lblProgress.Name = "lblProgress";
            this.lblProgress.Size = new System.Drawing.Size(53, 17);
            this.lblProgress.Text = "0.00 %";
            this.lblProgress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 17);
            // 
            // lblLastAccuracy
            // 
            this.lblLastAccuracy.Font = new System.Drawing.Font("Century Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLastAccuracy.Name = "lblLastAccuracy";
            this.lblLastAccuracy.Size = new System.Drawing.Size(118, 17);
            this.lblLastAccuracy.Text = "Last accuracy: n/a";
            // 
            // m_bw
            // 
            this.m_bw.WorkerReportsProgress = true;
            this.m_bw.WorkerSupportsCancellation = true;
            this.m_bw.DoWork += new System.ComponentModel.DoWorkEventHandler(this.m_bw_DoWork);
            this.m_bw.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.m_bw_ProgressChanged);
            this.m_bw.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.m_bw_RunWorkerCompleted);
            // 
            // timerUI
            // 
            this.timerUI.Enabled = true;
            this.timerUI.Interval = 250;
            this.timerUI.Tick += new System.EventHandler(this.timerUI_Tick);
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitContainer1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(987, 769);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 24);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(987, 794);
            this.toolStripContainer1.TabIndex = 2;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.toolStrip1);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer4);
            this.splitContainer1.Size = new System.Drawing.Size(987, 769);
            this.splitContainer1.SplitterDistance = 517;
            this.splitContainer1.TabIndex = 0;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer2.Panel1.Controls.Add(this.lblIterations);
            this.splitContainer2.Panel1.Controls.Add(this.label4);
            this.splitContainer2.Panel1.Controls.Add(this.label6);
            this.splitContainer2.Panel1.Controls.Add(this.label3);
            this.splitContainer2.Panel1.Controls.Add(this.btnBrowseTargetTextFile);
            this.splitContainer2.Panel1.Controls.Add(this.btnBrowseInputTextFile);
            this.splitContainer2.Panel1.Controls.Add(this.edtBatch);
            this.splitContainer2.Panel1.Controls.Add(this.edtIterations);
            this.splitContainer2.Panel1.Controls.Add(this.edtInput);
            this.splitContainer2.Panel1.Controls.Add(this.label5);
            this.splitContainer2.Panel1.Controls.Add(this.edtTargetTextFile);
            this.splitContainer2.Panel1.Controls.Add(this.label2);
            this.splitContainer2.Panel1.Controls.Add(this.edtInputTextFile);
            this.splitContainer2.Panel1.Controls.Add(this.label1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer2.Size = new System.Drawing.Size(987, 517);
            this.splitContainer2.SplitterDistance = 120;
            this.splitContainer2.TabIndex = 0;
            // 
            // lblIterations
            // 
            this.lblIterations.AutoSize = true;
            this.lblIterations.Location = new System.Drawing.Point(203, 66);
            this.lblIterations.Name = "lblIterations";
            this.lblIterations.Size = new System.Drawing.Size(0, 13);
            this.lblIterations.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(155, 66);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "epochs";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(319, 66);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 10;
            this.label6.Text = "Batch:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(38, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Iterations:";
            // 
            // btnBrowseTargetTextFile
            // 
            this.btnBrowseTargetTextFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseTargetTextFile.Location = new System.Drawing.Point(950, 37);
            this.btnBrowseTargetTextFile.Name = "btnBrowseTargetTextFile";
            this.btnBrowseTargetTextFile.Size = new System.Drawing.Size(25, 20);
            this.btnBrowseTargetTextFile.TabIndex = 5;
            this.btnBrowseTargetTextFile.Text = "...";
            this.btnBrowseTargetTextFile.UseVisualStyleBackColor = true;
            this.btnBrowseTargetTextFile.Click += new System.EventHandler(this.btnBrowseTargetTextFile_Click);
            // 
            // btnBrowseInputTextFile
            // 
            this.btnBrowseInputTextFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseInputTextFile.Location = new System.Drawing.Point(950, 11);
            this.btnBrowseInputTextFile.Name = "btnBrowseInputTextFile";
            this.btnBrowseInputTextFile.Size = new System.Drawing.Size(25, 20);
            this.btnBrowseInputTextFile.TabIndex = 2;
            this.btnBrowseInputTextFile.Text = "...";
            this.btnBrowseInputTextFile.UseVisualStyleBackColor = true;
            this.btnBrowseInputTextFile.Click += new System.EventHandler(this.btnBrowseInputTextFile_Click);
            // 
            // edtBatch
            // 
            this.edtBatch.Location = new System.Drawing.Point(363, 63);
            this.edtBatch.Name = "edtBatch";
            this.edtBatch.Size = new System.Drawing.Size(37, 20);
            this.edtBatch.TabIndex = 11;
            this.edtBatch.Text = "1";
            this.edtBatch.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // edtIterations
            // 
            this.edtIterations.Location = new System.Drawing.Point(97, 63);
            this.edtIterations.Name = "edtIterations";
            this.edtIterations.Size = new System.Drawing.Size(52, 20);
            this.edtIterations.TabIndex = 7;
            this.edtIterations.Text = "300";
            this.edtIterations.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // edtInput
            // 
            this.edtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.edtInput.Location = new System.Drawing.Point(97, 89);
            this.edtInput.Name = "edtInput";
            this.edtInput.Size = new System.Drawing.Size(847, 20);
            this.edtInput.TabIndex = 13;
            this.edtInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.edtInput_KeyPress);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(33, 92);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(58, 13);
            this.label5.TabIndex = 12;
            this.label5.Text = "Input Text:";
            // 
            // edtTargetTextFile
            // 
            this.edtTargetTextFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.edtTargetTextFile.Location = new System.Drawing.Point(97, 37);
            this.edtTargetTextFile.Name = "edtTargetTextFile";
            this.edtTargetTextFile.Size = new System.Drawing.Size(847, 20);
            this.edtTargetTextFile.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Target text file:";
            // 
            // edtInputTextFile
            // 
            this.edtInputTextFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.edtInputTextFile.Location = new System.Drawing.Point(97, 11);
            this.edtInputTextFile.Name = "edtInputTextFile";
            this.edtInputTextFile.Size = new System.Drawing.Size(847, 20);
            this.edtInputTextFile.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Input text file:";
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.IsSplitterFixed = true;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.pbImageLoss);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.pbImageAccuracy);
            this.splitContainer3.Size = new System.Drawing.Size(987, 393);
            this.splitContainer3.SplitterDistance = 743;
            this.splitContainer3.TabIndex = 0;
            // 
            // pbImageLoss
            // 
            this.pbImageLoss.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pbImageLoss.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbImageLoss.Location = new System.Drawing.Point(0, 0);
            this.pbImageLoss.Name = "pbImageLoss";
            this.pbImageLoss.Size = new System.Drawing.Size(743, 393);
            this.pbImageLoss.TabIndex = 0;
            this.pbImageLoss.TabStop = false;
            // 
            // pbImageAccuracy
            // 
            this.pbImageAccuracy.BackColor = System.Drawing.SystemColors.ControlLight;
            this.pbImageAccuracy.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pbImageAccuracy.Location = new System.Drawing.Point(0, 0);
            this.pbImageAccuracy.Name = "pbImageAccuracy";
            this.pbImageAccuracy.Size = new System.Drawing.Size(240, 393);
            this.pbImageAccuracy.TabIndex = 1;
            this.pbImageAccuracy.TabStop = false;
            // 
            // lvStatus
            // 
            this.lvStatus.BackColor = System.Drawing.Color.Aqua;
            this.lvStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colStatus});
            this.lvStatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvStatus.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvStatus.FullRowSelect = true;
            this.lvStatus.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvStatus.HideSelection = false;
            this.lvStatus.Location = new System.Drawing.Point(0, 0);
            this.lvStatus.Name = "lvStatus";
            this.lvStatus.RowHeight = 14;
            this.lvStatus.Size = new System.Drawing.Size(523, 248);
            this.lvStatus.TabIndex = 0;
            this.lvStatus.UseCompatibleStateImageBehavior = false;
            this.lvStatus.View = System.Windows.Forms.View.Details;
            this.lvStatus.Resize += new System.EventHandler(this.lvStatus_Resize);
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 962;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnTrain,
            this.btnRun,
            this.btnStop,
            this.btnDeleteWeights,
            this.btnEnableVerboseOutput});
            this.toolStrip1.Location = new System.Drawing.Point(3, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(127, 25);
            this.toolStrip1.TabIndex = 0;
            // 
            // btnTrain
            // 
            this.btnTrain.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnTrain.Image = ((System.Drawing.Image)(resources.GetObject("btnTrain.Image")));
            this.btnTrain.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnTrain.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTrain.Name = "btnTrain";
            this.btnTrain.Size = new System.Drawing.Size(23, 22);
            this.btnTrain.Text = "Train";
            this.btnTrain.Click += new System.EventHandler(this.btnTrain_Click);
            // 
            // btnRun
            // 
            this.btnRun.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRun.Image = ((System.Drawing.Image)(resources.GetObject("btnRun.Image")));
            this.btnRun.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnRun.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(23, 22);
            this.btnRun.Text = "Run";
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // btnStop
            // 
            this.btnStop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnStop.Image = ((System.Drawing.Image)(resources.GetObject("btnStop.Image")));
            this.btnStop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(23, 22);
            this.btnStop.Text = "btnStop";
            this.btnStop.ToolTipText = "Stop";
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnDeleteWeights
            // 
            this.btnDeleteWeights.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDeleteWeights.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteWeights.Image")));
            this.btnDeleteWeights.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.btnDeleteWeights.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDeleteWeights.Name = "btnDeleteWeights";
            this.btnDeleteWeights.Size = new System.Drawing.Size(23, 22);
            this.btnDeleteWeights.Text = "Delete Weights";
            this.btnDeleteWeights.Click += new System.EventHandler(this.btnDeleteWeights_Click);
            // 
            // btnEnableVerboseOutput
            // 
            this.btnEnableVerboseOutput.CheckOnClick = true;
            this.btnEnableVerboseOutput.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEnableVerboseOutput.Image = ((System.Drawing.Image)(resources.GetObject("btnEnableVerboseOutput.Image")));
            this.btnEnableVerboseOutput.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEnableVerboseOutput.Name = "btnEnableVerboseOutput";
            this.btnEnableVerboseOutput.Size = new System.Drawing.Size(23, 22);
            this.btnEnableVerboseOutput.Text = "Enable Verbose Output";
            // 
            // openFileDialogTxt
            // 
            this.openFileDialogTxt.DefaultExt = "txt";
            this.openFileDialogTxt.Filter = "Text Files (*.txt)|*.txt||";
            this.openFileDialogTxt.Title = "Select the Input Text File";
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.lvStatus);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.lvDiscussion);
            this.splitContainer4.Size = new System.Drawing.Size(987, 248);
            this.splitContainer4.SplitterDistance = 523;
            this.splitContainer4.TabIndex = 1;
            // 
            // lvDiscussion
            // 
            this.lvDiscussion.BackColor = System.Drawing.Color.Aquamarine;
            this.lvDiscussion.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lvDiscussion.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lvDiscussion.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvDiscussion.FullRowSelect = true;
            this.lvDiscussion.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lvDiscussion.HideSelection = false;
            this.lvDiscussion.Location = new System.Drawing.Point(0, 0);
            this.lvDiscussion.Name = "lvDiscussion";
            this.lvDiscussion.RowHeight = 14;
            this.lvDiscussion.Size = new System.Drawing.Size(460, 248);
            this.lvDiscussion.TabIndex = 1;
            this.lvDiscussion.UseCompatibleStateImageBehavior = false;
            this.lvDiscussion.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Status";
            this.columnHeader1.Width = 962;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(987, 840);
            this.Controls.Add(this.toolStripContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormMain";
            this.Text = "Seq2Seq ChatBot";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbImageLoss)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbImageAccuracy)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar pbProgress;
        private System.Windows.Forms.ToolStripStatusLabel lblProgress;
        private System.Windows.Forms.ToolStripButton bindingNavigatorMoveFirstItem;
        private System.ComponentModel.BackgroundWorker m_bw;
        private System.Windows.Forms.Timer timerUI;
        private System.Windows.Forms.ToolStripContainer toolStripContainer1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnTrain;
        private System.Windows.Forms.ToolStripButton btnRun;
        private System.Windows.Forms.ToolStripButton btnStop;
        private System.Windows.Forms.ToolStripButton btnDeleteWeights;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.PictureBox pbImageLoss;
        private System.Windows.Forms.Button btnBrowseTargetTextFile;
        private System.Windows.Forms.Button btnBrowseInputTextFile;
        private System.Windows.Forms.TextBox edtTargetTextFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox edtInputTextFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.OpenFileDialog openFileDialogTxt;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox edtIterations;
        private System.Windows.Forms.TextBox edtInput;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripStatusLabel lblLastAccuracy;
        private System.Windows.Forms.Label lblIterations;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.PictureBox pbImageAccuracy;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox edtBatch;
        private System.Windows.Forms.ColumnHeader colStatus;
        private ListViewEx lvStatus;
        private System.Windows.Forms.ToolStripButton btnEnableVerboseOutput;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private ListViewEx lvDiscussion;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}

