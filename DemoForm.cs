using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GridCollisionDemo
{
    public partial class DemoForm : Form
    {
        private const float ViewScale = 30.0f;

        private SolidBrush EmptyBrush = new SolidBrush(Color.FromArgb(240, 240, 240));
        private SolidBrush FilledBrush = new SolidBrush(Color.FromArgb(50, 50, 50));

        private Grid Grid;

        public DemoForm()
        {
            Grid = new Grid(20, 20);

            InitializeComponent();
        }

        private void DemoForm_Load(object sender, EventArgs e)
        {
            Grid.GetCell(4, 2).Filled = true;
        }

        private void DemoForm_Paint(object sender, PaintEventArgs e)
        {
            for (int y = 0; y < Grid.Height; y++)
            {
                for (int x = 0; x < Grid.Width; x++)
                {
                    e.Graphics.FillRectangle(Grid.GetCell(x, y).Filled ? FilledBrush : EmptyBrush, x * ViewScale, y * ViewScale, ViewScale, ViewScale);
                }
            }
        }

        private void DemoForm_Resize(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
