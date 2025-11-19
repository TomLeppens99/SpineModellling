namespace SpineAnalyzer.ModelVisualization
{
    partial class frmDebugOsimAndVTK
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
            this.btnLoadModel = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveModelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.visualsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraParallelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraOrthogonalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.makeScreenshotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allignFrontalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allignLateralToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveMarkersToFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.btnDeleteNode = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.btnTranslateZneg = new System.Windows.Forms.Button();
            this.btnTranslateYneg = new System.Windows.Forms.Button();
            this.btnTranslateXneg = new System.Windows.Forms.Button();
            this.btnRotYneg = new System.Windows.Forms.Button();
            this.btnRotXneg = new System.Windows.Forms.Button();
            this.btnRotZneg = new System.Windows.Forms.Button();
            this.btnTranslateZ = new System.Windows.Forms.Button();
            this.btnRotY = new System.Windows.Forms.Button();
            this.btnTranslateY = new System.Windows.Forms.Button();
            this.btnRotX = new System.Windows.Forms.Button();
            this.btnTranslateX = new System.Windows.Forms.Button();
            this.btnRotZ = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageProp = new System.Windows.Forms.TabPage();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageProp.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnLoadModel
            // 
            this.btnLoadModel.Location = new System.Drawing.Point(3, 552);
            this.btnLoadModel.Name = "btnLoadModel";
            this.btnLoadModel.Size = new System.Drawing.Size(79, 26);
            this.btnLoadModel.TabIndex = 0;
            this.btnLoadModel.Text = "Load Model";
            this.btnLoadModel.UseVisualStyleBackColor = true;
            this.btnLoadModel.Click += new System.EventHandler(this.btnLoadModel_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.visualsToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.windowToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip1.Size = new System.Drawing.Size(1107, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveModelToolStripMenuItem,
            this.closeToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // saveModelToolStripMenuItem
            // 
            this.saveModelToolStripMenuItem.Name = "saveModelToolStripMenuItem";
            this.saveModelToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.saveModelToolStripMenuItem.Text = "Save Model";
            this.saveModelToolStripMenuItem.Click += new System.EventHandler(this.saveModelToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(135, 22);
            this.closeToolStripMenuItem.Text = "Close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // visualsToolStripMenuItem
            // 
            this.visualsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cameraParallelToolStripMenuItem,
            this.cameraOrthogonalToolStripMenuItem,
            this.makeScreenshotToolStripMenuItem,
            this.allignFrontalToolStripMenuItem,
            this.allignLateralToolStripMenuItem});
            this.visualsToolStripMenuItem.Name = "visualsToolStripMenuItem";
            this.visualsToolStripMenuItem.Size = new System.Drawing.Size(55, 20);
            this.visualsToolStripMenuItem.Text = "Visuals";
            // 
            // cameraParallelToolStripMenuItem
            // 
            this.cameraParallelToolStripMenuItem.Name = "cameraParallelToolStripMenuItem";
            this.cameraParallelToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.cameraParallelToolStripMenuItem.Text = "Camera Parallel";
            this.cameraParallelToolStripMenuItem.Click += new System.EventHandler(this.cameraParallelToolStripMenuItem_Click);
            // 
            // cameraOrthogonalToolStripMenuItem
            // 
            this.cameraOrthogonalToolStripMenuItem.Name = "cameraOrthogonalToolStripMenuItem";
            this.cameraOrthogonalToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.cameraOrthogonalToolStripMenuItem.Text = "Camera Orthogonal";
            this.cameraOrthogonalToolStripMenuItem.Click += new System.EventHandler(this.cameraOrthogonalToolStripMenuItem_Click);
            // 
            // makeScreenshotToolStripMenuItem
            // 
            this.makeScreenshotToolStripMenuItem.Name = "makeScreenshotToolStripMenuItem";
            this.makeScreenshotToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.makeScreenshotToolStripMenuItem.Text = "Make screenshot";
            this.makeScreenshotToolStripMenuItem.Click += new System.EventHandler(this.makeScreenshotToolStripMenuItem_Click);
            // 
            // allignFrontalToolStripMenuItem
            // 
            this.allignFrontalToolStripMenuItem.Name = "allignFrontalToolStripMenuItem";
            this.allignFrontalToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.allignFrontalToolStripMenuItem.Text = "Allign Frontal";
            this.allignFrontalToolStripMenuItem.Click += new System.EventHandler(this.allignFrontalToolStripMenuItem_Click);
            // 
            // allignLateralToolStripMenuItem
            // 
            this.allignLateralToolStripMenuItem.Name = "allignLateralToolStripMenuItem";
            this.allignLateralToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.allignLateralToolStripMenuItem.Text = "Allign Lateral";
            this.allignLateralToolStripMenuItem.Click += new System.EventHandler(this.allignLateralToolStripMenuItem_Click);
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveMarkersToFileToolStripMenuItem});
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(47, 20);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // saveMarkersToFileToolStripMenuItem
            // 
            this.saveMarkersToFileToolStripMenuItem.Name = "saveMarkersToFileToolStripMenuItem";
            this.saveMarkersToFileToolStripMenuItem.Size = new System.Drawing.Size(176, 22);
            this.saveMarkersToFileToolStripMenuItem.Text = "Save markers to file";
            this.saveMarkersToFileToolStripMenuItem.Click += new System.EventHandler(this.saveMarkersToFileToolStripMenuItem_Click);
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(63, 20);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.Location = new System.Drawing.Point(2, 2);
            this.treeView1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(261, 275);
            this.treeView1.TabIndex = 2;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // btnDeleteNode
            // 
            this.btnDeleteNode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteNode.Location = new System.Drawing.Point(1043, 2);
            this.btnDeleteNode.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnDeleteNode.Name = "btnDeleteNode";
            this.btnDeleteNode.Size = new System.Drawing.Size(62, 50);
            this.btnDeleteNode.TabIndex = 4;
            this.btnDeleteNode.Text = "Test";
            this.btnDeleteNode.UseVisualStyleBackColor = true;
            this.btnDeleteNode.Click += new System.EventHandler(this.btnDeleteNode_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(753, 2);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tabControl2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.tableLayoutPanel1.SetRowSpan(this.splitContainer1, 2);
            this.splitContainer1.Size = new System.Drawing.Size(273, 545);
            this.splitContainer1.SplitterDistance = 305;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 6;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage1);
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(0, 0);
            this.tabControl2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(273, 305);
            this.tabControl2.TabIndex = 8;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.treeView1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage1.Size = new System.Drawing.Size(265, 279);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Model Navigator";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.btnTranslateZneg);
            this.tabPage3.Controls.Add(this.btnTranslateYneg);
            this.tabPage3.Controls.Add(this.btnTranslateXneg);
            this.tabPage3.Controls.Add(this.btnRotYneg);
            this.tabPage3.Controls.Add(this.btnRotXneg);
            this.tabPage3.Controls.Add(this.btnRotZneg);
            this.tabPage3.Controls.Add(this.btnTranslateZ);
            this.tabPage3.Controls.Add(this.btnRotY);
            this.tabPage3.Controls.Add(this.btnTranslateY);
            this.tabPage3.Controls.Add(this.btnRotX);
            this.tabPage3.Controls.Add(this.btnTranslateX);
            this.tabPage3.Controls.Add(this.btnRotZ);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage3.Size = new System.Drawing.Size(265, 280);
            this.tabPage3.TabIndex = 1;
            this.tabPage3.Text = "DEVELOP";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // btnTranslateZneg
            // 
            this.btnTranslateZneg.Location = new System.Drawing.Point(195, 129);
            this.btnTranslateZneg.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnTranslateZneg.Name = "btnTranslateZneg";
            this.btnTranslateZneg.Size = new System.Drawing.Size(41, 34);
            this.btnTranslateZneg.TabIndex = 12;
            this.btnTranslateZneg.Text = "Tr -Z";
            this.btnTranslateZneg.UseVisualStyleBackColor = true;
            this.btnTranslateZneg.Click += new System.EventHandler(this.btnTranslateZneg_Click);
            // 
            // btnTranslateYneg
            // 
            this.btnTranslateYneg.Location = new System.Drawing.Point(195, 92);
            this.btnTranslateYneg.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnTranslateYneg.Name = "btnTranslateYneg";
            this.btnTranslateYneg.Size = new System.Drawing.Size(41, 34);
            this.btnTranslateYneg.TabIndex = 11;
            this.btnTranslateYneg.Text = "Tr -Y";
            this.btnTranslateYneg.UseVisualStyleBackColor = true;
            this.btnTranslateYneg.Click += new System.EventHandler(this.btnTranslateYneg_Click);
            // 
            // btnTranslateXneg
            // 
            this.btnTranslateXneg.Location = new System.Drawing.Point(195, 55);
            this.btnTranslateXneg.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnTranslateXneg.Name = "btnTranslateXneg";
            this.btnTranslateXneg.Size = new System.Drawing.Size(41, 34);
            this.btnTranslateXneg.TabIndex = 10;
            this.btnTranslateXneg.Text = "Tr -X";
            this.btnTranslateXneg.UseVisualStyleBackColor = true;
            this.btnTranslateXneg.Click += new System.EventHandler(this.btnTranslateXneg_Click);
            // 
            // btnRotYneg
            // 
            this.btnRotYneg.Location = new System.Drawing.Point(70, 92);
            this.btnRotYneg.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnRotYneg.Name = "btnRotYneg";
            this.btnRotYneg.Size = new System.Drawing.Size(61, 34);
            this.btnRotYneg.TabIndex = 8;
            this.btnRotYneg.Text = "Rotate -Y";
            this.btnRotYneg.UseVisualStyleBackColor = true;
            this.btnRotYneg.Click += new System.EventHandler(this.btnRotYneg_Click);
            // 
            // btnRotXneg
            // 
            this.btnRotXneg.Location = new System.Drawing.Point(70, 55);
            this.btnRotXneg.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnRotXneg.Name = "btnRotXneg";
            this.btnRotXneg.Size = new System.Drawing.Size(61, 34);
            this.btnRotXneg.TabIndex = 7;
            this.btnRotXneg.Text = "Rotate -X";
            this.btnRotXneg.UseVisualStyleBackColor = true;
            this.btnRotXneg.Click += new System.EventHandler(this.btnRotXneg_Click);
            // 
            // btnRotZneg
            // 
            this.btnRotZneg.Location = new System.Drawing.Point(70, 129);
            this.btnRotZneg.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnRotZneg.Name = "btnRotZneg";
            this.btnRotZneg.Size = new System.Drawing.Size(61, 34);
            this.btnRotZneg.TabIndex = 9;
            this.btnRotZneg.Text = "Rotate -Z";
            this.btnRotZneg.UseVisualStyleBackColor = true;
            this.btnRotZneg.Click += new System.EventHandler(this.btnRotZneg_Click);
            // 
            // btnTranslateZ
            // 
            this.btnTranslateZ.Location = new System.Drawing.Point(155, 129);
            this.btnTranslateZ.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnTranslateZ.Name = "btnTranslateZ";
            this.btnTranslateZ.Size = new System.Drawing.Size(35, 34);
            this.btnTranslateZ.TabIndex = 5;
            this.btnTranslateZ.Text = "TrZ";
            this.btnTranslateZ.UseVisualStyleBackColor = true;
            this.btnTranslateZ.Click += new System.EventHandler(this.btnTranslateZ_Click);
            // 
            // btnRotY
            // 
            this.btnRotY.Location = new System.Drawing.Point(11, 92);
            this.btnRotY.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnRotY.Name = "btnRotY";
            this.btnRotY.Size = new System.Drawing.Size(56, 34);
            this.btnRotY.TabIndex = 1;
            this.btnRotY.Text = "RotateY";
            this.btnRotY.UseVisualStyleBackColor = true;
            this.btnRotY.Click += new System.EventHandler(this.btnRotY_Click);
            // 
            // btnTranslateY
            // 
            this.btnTranslateY.Location = new System.Drawing.Point(155, 92);
            this.btnTranslateY.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnTranslateY.Name = "btnTranslateY";
            this.btnTranslateY.Size = new System.Drawing.Size(35, 34);
            this.btnTranslateY.TabIndex = 4;
            this.btnTranslateY.Text = "TrY";
            this.btnTranslateY.UseVisualStyleBackColor = true;
            this.btnTranslateY.Click += new System.EventHandler(this.btnTranslateY_Click);
            // 
            // btnRotX
            // 
            this.btnRotX.Location = new System.Drawing.Point(11, 55);
            this.btnRotX.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnRotX.Name = "btnRotX";
            this.btnRotX.Size = new System.Drawing.Size(56, 34);
            this.btnRotX.TabIndex = 0;
            this.btnRotX.Text = "RotateX";
            this.btnRotX.UseVisualStyleBackColor = true;
            this.btnRotX.Click += new System.EventHandler(this.btnRotX_Click);
            // 
            // btnTranslateX
            // 
            this.btnTranslateX.Location = new System.Drawing.Point(155, 55);
            this.btnTranslateX.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnTranslateX.Name = "btnTranslateX";
            this.btnTranslateX.Size = new System.Drawing.Size(35, 34);
            this.btnTranslateX.TabIndex = 3;
            this.btnTranslateX.Text = "TrX";
            this.btnTranslateX.UseVisualStyleBackColor = true;
            this.btnTranslateX.Click += new System.EventHandler(this.btnTranslateX_Click);
            // 
            // btnRotZ
            // 
            this.btnRotZ.Location = new System.Drawing.Point(11, 129);
            this.btnRotZ.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.btnRotZ.Name = "btnRotZ";
            this.btnRotZ.Size = new System.Drawing.Size(56, 34);
            this.btnRotZ.TabIndex = 2;
            this.btnRotZ.Text = "RotateZ";
            this.btnRotZ.UseVisualStyleBackColor = true;
            this.btnRotZ.Click += new System.EventHandler(this.btnRotZ_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageProp);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(273, 237);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPageProp
            // 
            this.tabPageProp.Controls.Add(this.propertyGrid1);
            this.tabPageProp.Location = new System.Drawing.Point(4, 22);
            this.tabPageProp.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageProp.Name = "tabPageProp";
            this.tabPageProp.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPageProp.Size = new System.Drawing.Size(265, 211);
            this.tabPageProp.TabIndex = 0;
            this.tabPageProp.Text = "Properties";
            this.tabPageProp.UseVisualStyleBackColor = true;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(2, 2);
            this.propertyGrid1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.Categorized;
            this.propertyGrid1.Size = new System.Drawing.Size(261, 207);
            this.propertyGrid1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage2.Size = new System.Drawing.Size(265, 210);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "DEVELOP";
            this.tabPage2.UseVisualStyleBackColor = true;
            this.tabPage2.Click += new System.EventHandler(this.tabPage2_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 72.99383F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 27.00617F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 78F));
            this.tableLayoutPanel1.Controls.Add(this.btnLoadModel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnDeleteNode, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.richTextBox1, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 49);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 85.37736F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.62264F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 32F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1107, 582);
            this.tableLayoutPanel1.TabIndex = 7;
            // 
            // richTextBox1
            // 
            this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox1.Location = new System.Drawing.Point(2, 471);
            this.richTextBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(747, 76);
            this.richTextBox1.TabIndex = 7;
            this.richTextBox1.Text = "";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(75, 19);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(65, 20);
            this.toolStripStatusLabel1.Text = "In Progress";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 626);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1107, 25);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // frmDebugOsimAndVTK
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1107, 651);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmDebugOsimAndVTK";
            this.Text = "OpenSim Model Visualizer - DEBUG version 1";
            this.Load += new System.EventHandler(this.frmDebugOsimAndVTK_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageProp.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnLoadModel;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem visualsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraParallelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cameraOrthogonalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem makeScreenshotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveModelToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allignFrontalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem allignLateralToolStripMenuItem;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button btnDeleteNode;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.TabPage tabPageProp;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Button btnRotX;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem windowToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ToolStripMenuItem saveMarkersToFileToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnTranslateZ;
        private System.Windows.Forms.Button btnTranslateY;
        private System.Windows.Forms.Button btnTranslateX;
        private System.Windows.Forms.Button btnRotZ;
        private System.Windows.Forms.Button btnRotY;
        private System.Windows.Forms.Button btnRotYneg;
        private System.Windows.Forms.Button btnRotXneg;
        private System.Windows.Forms.Button btnRotZneg;
        private System.Windows.Forms.Button btnTranslateZneg;
        private System.Windows.Forms.Button btnTranslateYneg;
        private System.Windows.Forms.Button btnTranslateXneg;
        private System.Windows.Forms.StatusStrip statusStrip1;
    }
}