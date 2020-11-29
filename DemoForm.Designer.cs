
namespace GridCollisionDemo
{
    partial class DemoForm
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
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.ClientSize = new System.Drawing.Size(955, 761);
            this.DoubleBuffered = true;
            this.Name = "DemoForm";
            this.Text = "Grid Collision Demo";
            this.Load += new System.EventHandler(this.DemoForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.DemoForm_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.DemoForm_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.DemoForm_MouseMove);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DemoForm_MouseUp);
            this.Resize += new System.EventHandler(this.DemoForm_Resize);
            this.ResumeLayout(false);

        }

        #endregion
    }
}

