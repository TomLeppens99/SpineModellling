
namespace SpineModeling
{
    partial class btnmuscular
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
            this.btnSkeletal = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnSkeletal
            // 
            this.btnSkeletal.Location = new System.Drawing.Point(263, 163);
            this.btnSkeletal.Name = "btnSkeletal";
            this.btnSkeletal.Size = new System.Drawing.Size(531, 213);
            this.btnSkeletal.TabIndex = 0;
            this.btnSkeletal.Text = "Skeletal";
            this.btnSkeletal.UseVisualStyleBackColor = true;
            this.btnSkeletal.Click += new System.EventHandler(this.btnSkeletal_Click);
            // 
            // btnmuscular
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(2601, 1279);
            this.Controls.Add(this.btnSkeletal);
            this.Name = "btnmuscular";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSkeletal;
    }
}

