namespace SpineAnalyzer.SkeletalModeling
{
    partial class frmSkeletalModelingPreferences
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmSkeletalModelingPreferences));
            this.richTXTgeometryDirs = new System.Windows.Forms.RichTextBox();
            this.lblexplanation = new System.Windows.Forms.Label();
            this.pictureBox1 = new Accord.Controls.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // richTXTgeometryDirs
            // 
            this.richTXTgeometryDirs.Location = new System.Drawing.Point(29, 96);
            this.richTXTgeometryDirs.Name = "richTXTgeometryDirs";
            this.richTXTgeometryDirs.Size = new System.Drawing.Size(627, 81);
            this.richTXTgeometryDirs.TabIndex = 0;
            this.richTXTgeometryDirs.Text = "";
            // 
            // lblexplanation
            // 
            this.lblexplanation.AutoSize = true;
            this.lblexplanation.Location = new System.Drawing.Point(41, 42);
            this.lblexplanation.Name = "lblexplanation";
            this.lblexplanation.Size = new System.Drawing.Size(395, 39);
            this.lblexplanation.TabIndex = 2;
            this.lblexplanation.Text = resources.GetString("lblexplanation.Text");
            this.lblexplanation.Click += new System.EventHandler(this.lblexplanation_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.BackgroundImage = global::SpineModeling.Properties.Resources.Settings;
            this.pictureBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.pictureBox1.Image = null;
            this.pictureBox1.Location = new System.Drawing.Point(690, 18);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(90, 78);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(26, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 14);
            this.label1.TabIndex = 4;
            this.label1.Text = "Change geometry directory";
            // 
            // frmSkeletalModelingPreferences
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblexplanation);
            this.Controls.Add(this.richTXTgeometryDirs);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "frmSkeletalModelingPreferences";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Preferences";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmSkeletalModelingPreferences_FormClosed);
            this.Load += new System.EventHandler(this.frmSkeletalModelingPreferences_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox richTXTgeometryDirs;
        private System.Windows.Forms.Label lblexplanation;
        private Accord.Controls.PictureBox pictureBox1;
        private System.Windows.Forms.Label label1;
    }
}