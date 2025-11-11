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
            SuspendLayout();
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 444);
            Name = "Game";
            Text = "Physics Engine";
            Load += Game_Load;
            ResumeLayout(false);
        }
    }
}
