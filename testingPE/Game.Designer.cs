using System.Drawing;
using System.Windows.Forms;

namespace PhysicsEngine
{
    partial class Game
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.SuspendLayout();
            // 
            // Game
            // 
            this.AutoScaleDimensions = new SizeF(8F, 20F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(900, 500);
            this.Name = "Game";
            this.Text = "Pong – WinForms AABB";
            this.BackColor = Color.Black;
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.ResumeLayout(false);
        }
    }
}
