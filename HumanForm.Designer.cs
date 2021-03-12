
namespace Puppy
{
    partial class HumanForm
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
            // HumanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(974, 929);
            this.ControlBox = false;
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.KeyPreview = true;
            this.Name = "HumanForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Human Diagram";
            this.TopMost = true;
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.HumanForm_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.HumanForm_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.HumanForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.HumanForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.HumanForm_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion
    }
}