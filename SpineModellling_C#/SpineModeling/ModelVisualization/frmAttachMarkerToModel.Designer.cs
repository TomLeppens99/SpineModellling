namespace SpineAnalyzer.ModelVisualization
{
    partial class frmAttachMarkerToModel
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
            this.lblMarkerName = new System.Windows.Forms.Label();
            this.lblReferenceBody = new System.Windows.Forms.Label();
            this.lblFixed = new System.Windows.Forms.Label();
            this.txtMarkerName = new System.Windows.Forms.TextBox();
            this.cBbodies = new System.Windows.Forms.ComboBox();
            this.cBfixed = new System.Windows.Forms.ComboBox();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnCancel = new System.Windows.Forms.Button();
            this.cBvisible = new System.Windows.Forms.ComboBox();
            this.lblVisible = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblMarkerName
            // 
            this.lblMarkerName.AutoSize = true;
            this.lblMarkerName.Location = new System.Drawing.Point(57, 20);
            this.lblMarkerName.Name = "lblMarkerName";
            this.lblMarkerName.Size = new System.Drawing.Size(93, 16);
            this.lblMarkerName.TabIndex = 0;
            this.lblMarkerName.Text = "Marker Name:";
            // 
            // lblReferenceBody
            // 
            this.lblReferenceBody.AutoSize = true;
            this.lblReferenceBody.Location = new System.Drawing.Point(57, 66);
            this.lblReferenceBody.Name = "lblReferenceBody";
            this.lblReferenceBody.Size = new System.Drawing.Size(109, 16);
            this.lblReferenceBody.TabIndex = 1;
            this.lblReferenceBody.Text = "Reference Body:";
            // 
            // lblFixed
            // 
            this.lblFixed.AutoSize = true;
            this.lblFixed.Location = new System.Drawing.Point(57, 109);
            this.lblFixed.Name = "lblFixed";
            this.lblFixed.Size = new System.Drawing.Size(44, 16);
            this.lblFixed.TabIndex = 2;
            this.lblFixed.Text = "Fixed:";
            // 
            // txtMarkerName
            // 
            this.txtMarkerName.BackColor = System.Drawing.Color.White;
            this.txtMarkerName.Cursor = System.Windows.Forms.Cursors.Hand;
            this.txtMarkerName.Location = new System.Drawing.Point(189, 20);
            this.txtMarkerName.Name = "txtMarkerName";
            this.txtMarkerName.Size = new System.Drawing.Size(192, 22);
            this.txtMarkerName.TabIndex = 3;
            this.txtMarkerName.TextChanged += new System.EventHandler(this.txtMarkerName_TextChanged);
            this.txtMarkerName.Validating += new System.ComponentModel.CancelEventHandler(this.txtMarkerName_Validating);
            // 
            // cBbodies
            // 
            this.cBbodies.BackColor = System.Drawing.Color.White;
            this.cBbodies.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cBbodies.FormattingEnabled = true;
            this.cBbodies.Location = new System.Drawing.Point(189, 66);
            this.cBbodies.Name = "cBbodies";
            this.cBbodies.Size = new System.Drawing.Size(192, 24);
            this.cBbodies.TabIndex = 5;
            this.cBbodies.SelectedIndexChanged += new System.EventHandler(this.cBbodies_SelectedIndexChanged);
            this.cBbodies.Validating += new System.ComponentModel.CancelEventHandler(this.cBbodies_Validating);
            // 
            // cBfixed
            // 
            this.cBfixed.BackColor = System.Drawing.Color.White;
            this.cBfixed.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cBfixed.FormattingEnabled = true;
            this.cBfixed.Items.AddRange(new object[] {
            "True",
            "False"});
            this.cBfixed.Location = new System.Drawing.Point(189, 109);
            this.cBfixed.Name = "cBfixed";
            this.cBfixed.Size = new System.Drawing.Size(192, 24);
            this.cBfixed.TabIndex = 6;
            this.cBfixed.SelectedIndexChanged += new System.EventHandler(this.cBfixed_SelectedIndexChanged);
            this.cBfixed.Validating += new System.ComponentModel.CancelEventHandler(this.cBfixed_Validating);
            // 
            // btnConfirm
            // 
            this.btnConfirm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfirm.Location = new System.Drawing.Point(478, 278);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(81, 43);
            this.btnConfirm.TabIndex = 7;
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // btnCancel
            // 
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.Location = new System.Drawing.Point(396, 278);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(76, 43);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // cBvisible
            // 
            this.cBvisible.BackColor = System.Drawing.Color.White;
            this.cBvisible.Cursor = System.Windows.Forms.Cursors.Hand;
            this.cBvisible.FormattingEnabled = true;
            this.cBvisible.Items.AddRange(new object[] {
            "True",
            "False"});
            this.cBvisible.Location = new System.Drawing.Point(189, 148);
            this.cBvisible.Name = "cBvisible";
            this.cBvisible.Size = new System.Drawing.Size(192, 24);
            this.cBvisible.TabIndex = 10;
            // 
            // lblVisible
            // 
            this.lblVisible.AutoSize = true;
            this.lblVisible.Location = new System.Drawing.Point(57, 148);
            this.lblVisible.Name = "lblVisible";
            this.lblVisible.Size = new System.Drawing.Size(52, 16);
            this.lblVisible.TabIndex = 9;
            this.lblVisible.Text = "Visible:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SpineAnalyzer.Properties.Resources.motiveBodyMarkersetTemplates;
            this.pictureBox1.Location = new System.Drawing.Point(-110, 168);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(582, 171);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            // 
            // frmAttachMarkerToModel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(571, 333);
            this.Controls.Add(this.cBvisible);
            this.Controls.Add(this.lblVisible);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.cBfixed);
            this.Controls.Add(this.cBbodies);
            this.Controls.Add(this.txtMarkerName);
            this.Controls.Add(this.lblFixed);
            this.Controls.Add(this.lblReferenceBody);
            this.Controls.Add(this.lblMarkerName);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmAttachMarkerToModel";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Attach Marker";
            this.Load += new System.EventHandler(this.frmAttachMarkerToModel_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblMarkerName;
        private System.Windows.Forms.Label lblReferenceBody;
        private System.Windows.Forms.Label lblFixed;
        private System.Windows.Forms.TextBox txtMarkerName;
        private System.Windows.Forms.ComboBox cBbodies;
        private System.Windows.Forms.ComboBox cBfixed;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox cBvisible;
        private System.Windows.Forms.Label lblVisible;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}