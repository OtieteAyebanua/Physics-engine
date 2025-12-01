using System.Numerics;
using PhysicsEngine.Physics;
using PhysicsEngine.Physics.SharedLaws;

namespace PongGame
{
    // Re-purposed form for the avoider game
    public partial class AvoiderForm : Form
    {
        private GamePanel _gamePanel;

        public AvoiderForm()
        {
            Text = "Avoider - Physics Engine";
            Size = new Size(800, 600);
            StartPosition = FormStartPosition.CenterScreen;
            DoubleBuffered = true;
            // Make window fixed size (not resizable)
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;

            _gamePanel = new GamePanel { Dock = DockStyle.Fill };
            Controls.Add(_gamePanel);
        }
    }
}
