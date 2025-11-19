namespace SpineAnalyzer
{
    partial class frmManualImportEOSimages
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
            this.vistaOpenFileDialog1 = new Ookii.Dialogs.WinForms.VistaOpenFileDialog();
            this.btnFile2 = new System.Windows.Forms.Button();
            this.txtFileName2 = new System.Windows.Forms.TextBox();
            this.txtFileName1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblFrontalImage = new System.Windows.Forms.Label();
            this.btnFile1 = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.btnReturn = new System.Windows.Forms.Button();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // vistaOpenFileDialog1
            // 
            this.vistaOpenFileDialog1.FileName = "vistaOpenFileDialog1";
            this.vistaOpenFileDialog1.Filter = null;
            // 
            // btnFile2
            // 
            this.btnFile2.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnFile2.BackgroundImage = global::SpineModeling.Properties.Resources.folder_white;
            this.btnFile2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnFile2.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFile2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFile2.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnFile2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFile2.Location = new System.Drawing.Point(1190, 170);
            this.btnFile2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnFile2.Name = "btnFile2";
            this.btnFile2.Size = new System.Drawing.Size(99, 66);
            this.btnFile2.TabIndex = 34;
            this.btnFile2.UseVisualStyleBackColor = false;
            this.btnFile2.Click += new System.EventHandler(this.btnFile2_Click);
            // 
            // txtFileName2
            // 
            this.txtFileName2.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtFileName2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFileName2.Location = new System.Drawing.Point(137, 170);
            this.txtFileName2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtFileName2.Name = "txtFileName2";
            this.txtFileName2.Size = new System.Drawing.Size(1045, 22);
            this.txtFileName2.TabIndex = 38;
            // 
            // txtFileName1
            // 
            this.txtFileName1.Cursor = System.Windows.Forms.Cursors.Default;
            this.txtFileName1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtFileName1.Location = new System.Drawing.Point(137, 96);
            this.txtFileName1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtFileName1.Name = "txtFileName1";
            this.txtFileName1.Size = new System.Drawing.Size(1045, 22);
            this.txtFileName1.TabIndex = 37;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Font = new System.Drawing.Font("Tw Cen MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.SteelBlue;
            this.label4.Location = new System.Drawing.Point(4, 166);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 74);
            this.label4.TabIndex = 36;
            this.label4.Text = "Image 2 (Lateral)";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFrontalImage
            // 
            this.lblFrontalImage.AutoSize = true;
            this.lblFrontalImage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFrontalImage.Font = new System.Drawing.Font("Tw Cen MT", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFrontalImage.ForeColor = System.Drawing.Color.SteelBlue;
            this.lblFrontalImage.Location = new System.Drawing.Point(4, 92);
            this.lblFrontalImage.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFrontalImage.Name = "lblFrontalImage";
            this.lblFrontalImage.Size = new System.Drawing.Size(125, 74);
            this.lblFrontalImage.TabIndex = 35;
            this.lblFrontalImage.Text = "Image 1 (Frontal)";
            this.lblFrontalImage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnFile1
            // 
            this.btnFile1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.btnFile1.BackgroundImage = global::SpineModeling.Properties.Resources.folder_white;
            this.btnFile1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnFile1.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFile1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnFile1.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnFile1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFile1.Location = new System.Drawing.Point(1190, 96);
            this.btnFile1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnFile1.Name = "btnFile1";
            this.btnFile1.Size = new System.Drawing.Size(99, 66);
            this.btnFile1.TabIndex = 33;
            this.btnFile1.UseVisualStyleBackColor = false;
            this.btnFile1.Click += new System.EventHandler(this.btnFile1_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 133F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 107F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 79F));
            this.tableLayoutPanel1.Controls.Add(this.btnFile1, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.txtFileName2, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnFile2, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.txtFileName1, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblFrontalImage, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.btnReturn, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.btnConfirm, 3, 4);
            this.tableLayoutPanel1.Controls.Add(this.label1, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1372, 358);
            this.tableLayoutPanel1.TabIndex = 39;
            // 
            // btnReturn
            // 
            this.btnReturn.BackgroundImage = global::SpineModeling.Properties.Resources.Return_Blue_edited;
            this.btnReturn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnReturn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReturn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnReturn.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReturn.FlatAppearance.BorderSize = 0;
            this.btnReturn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReturn.Location = new System.Drawing.Point(4, 269);
            this.btnReturn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnReturn.Name = "btnReturn";
            this.btnReturn.Size = new System.Drawing.Size(125, 85);
            this.btnReturn.TabIndex = 46;
            this.btnReturn.UseVisualStyleBackColor = true;
            // 
            // btnConfirm
            // 
            this.btnConfirm.BackgroundImage = global::SpineModeling.Properties.Resources.DoneIconGreen;
            this.btnConfirm.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnConfirm.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConfirm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnConfirm.FlatAppearance.BorderSize = 0;
            this.btnConfirm.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnConfirm.Location = new System.Drawing.Point(1297, 269);
            this.btnConfirm.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(71, 85);
            this.btnConfirm.TabIndex = 47;
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Visible = false;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Tw Cen MT", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.SteelBlue;
            this.label1.Location = new System.Drawing.Point(137, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(1045, 92);
            this.label1.TabIndex = 48;
            this.label1.Text = "Please select your EOS images";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // frmManualImportEOSimages
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(1372, 358);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "frmManualImportEOSimages";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Select EOS images";
            this.Load += new System.EventHandler(this.frmManualImportEOSimages_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Ookii.Dialogs.WinForms.VistaOpenFileDialog vistaOpenFileDialog1;
        private System.Windows.Forms.Button btnFile2;
        private System.Windows.Forms.TextBox txtFileName2;
        private System.Windows.Forms.TextBox txtFileName1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblFrontalImage;
        private System.Windows.Forms.Button btnFile1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button btnReturn;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label label1;
    }
}