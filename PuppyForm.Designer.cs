
namespace Puppy
{
    partial class PuppyForm
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
            this.SuspendLayout();
            // 
            // PuppyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(974, 929);
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "PuppyForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Puppy Diagram";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.Black;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.PuppyForm_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PuppyForm_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PuppyForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PuppyForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PuppyForm_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}