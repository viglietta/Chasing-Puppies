
namespace Puppy
{
    partial class TrackForm
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
                t.Dispose();
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
            // TrackForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(974, 929);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.Name = "TrackForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Track";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.TrackForm_Paint);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.TrackForm_KeyDown);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TrackForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.TrackForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TrackForm_MouseUp);
            this.Resize += new System.EventHandler(this.TrackForm_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

