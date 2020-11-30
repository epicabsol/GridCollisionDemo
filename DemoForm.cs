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
        private enum DragMode
        {
            None,
            StartPosition,
            DestinationPosition,
        }

        private const float ViewScale = 30.0f;
        private const float PointSize = 5.0f;

        private SolidBrush EmptyBrush = new SolidBrush(Color.FromArgb(50, 50, 50));
        private SolidBrush FilledBrush = new SolidBrush(Color.FromArgb(90, 90, 90));
        private SolidBrush HoverBrush = new SolidBrush(Color.FromArgb(70, 70, 70));
        private Pen StartPen = new Pen(Color.FromArgb(50, 100, 255));
        private Pen EndPen = new Pen(Color.FromArgb(50, 250, 50));
        private Pen DestinationPen = new Pen(Color.FromArgb(250, 50, 50));

        private PointF StartPosition = new PointF(2.0f, 2.0f);
        private PointF EndPosition = new PointF(10.0f, 10.0f);
        private PointF DestinationPosition = new PointF(18.0f, 18.0f);
        private SizeF ObjectSize = new SizeF(3.5f, 1.5f);

        private Grid Grid;
        private DragMode CurrentDragMode = DragMode.None;
        private Point LastCursorPosition;
        private bool UseBox = true;

        public DemoForm()
        {
            // Create an empty grid
            this.Grid = new Grid(20, 20);

            this.InitializeComponent();
        }

        private bool HitTestPoint(PointF point, Point cursor)
        {
            // Transform the point into screen coords
            point.X *= ViewScale;
            point.Y *= ViewScale;

            float dx = cursor.X - point.X;
            float dy = cursor.Y - point.Y;

            return dx * dx + dy * dy <= PointSize * PointSize;
        }

        private void UpdateResults()
        {
            float hitX;
            float hitY;
            bool hit = UseBox ? this.Grid.SweepBox(this.StartPosition.X, this.StartPosition.Y, this.DestinationPosition.X, this.DestinationPosition.Y, ObjectSize.Width, ObjectSize.Height, out hitX, out hitY) : this.Grid.TestSegment(this.StartPosition.X, this.StartPosition.Y, this.DestinationPosition.X, this.DestinationPosition.Y, out hitX, out hitY);
            if (hit)
            {
                this.EndPosition.X = hitX;
                this.EndPosition.Y = hitY;
            }
            else
            {
                this.EndPosition = this.DestinationPosition;
            }

            this.Invalidate();
        }

        private void DemoForm_Load(object sender, EventArgs e)
        {
            // Differentiate the style of the destination pen to avoid potential colorblindness issues
            DestinationPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;

            // Fill in a couple of cells so we have something interesting to look at
            this.Grid.GetCell(4, 2).Filled = true;

            this.Grid.GetCell(5, 6).Filled = true;

            this.UpdateResults();
        }

        private void DemoForm_Paint(object sender, PaintEventArgs e)
        {
            // Enable antialiasing
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw grid cells
            for (int y = 0; y < this.Grid.Height; y++)
            {
                for (int x = 0; x < this.Grid.Width; x++)
                {
                    e.Graphics.FillRectangle(Grid.GetCell(x, y).Filled ? FilledBrush : EmptyBrush, x * ViewScale, y * ViewScale, ViewScale, ViewScale);
                }
            }

            // Fill the hovered cell, if on grid and hovered
            int hoverX = (int)Math.Floor(LastCursorPosition.X / ViewScale);
            int hoverY = (int)Math.Floor(LastCursorPosition.Y / ViewScale);
            if (hoverX >= 0 && hoverX < this.Grid.Width
                && hoverY >= 0 && hoverY < this.Grid.Height
                && !HitTestPoint(this.StartPosition, LastCursorPosition)
                && !HitTestPoint(this.DestinationPosition, LastCursorPosition))
            {
                e.Graphics.FillRectangle(HoverBrush, hoverX * ViewScale, hoverY * ViewScale, ViewScale, ViewScale);
            }

            // Draw lines between the points
            e.Graphics.DrawLine(EndPen, StartPosition.X * ViewScale, StartPosition.Y * ViewScale, EndPosition.X * ViewScale, EndPosition.Y * ViewScale);
            e.Graphics.DrawLine(DestinationPen, EndPosition.X * ViewScale, EndPosition.Y * ViewScale, DestinationPosition.X * ViewScale, DestinationPosition.Y * ViewScale);

            // Draw start, end, and destination points
            e.Graphics.DrawEllipse(StartPen, StartPosition.X * ViewScale - PointSize, StartPosition.Y * ViewScale - PointSize, PointSize * 2.0f, PointSize * 2.0f);
            e.Graphics.DrawEllipse(DestinationPen, DestinationPosition.X * ViewScale - PointSize, DestinationPosition.Y * ViewScale - PointSize, PointSize * 2.0f, PointSize * 2.0f);
            e.Graphics.DrawEllipse(EndPen, EndPosition.X * ViewScale - PointSize, EndPosition.Y * ViewScale - PointSize, PointSize * 2.0f, PointSize * 2.0f);

            if (this.UseBox)
            {
                e.Graphics.DrawRectangle(StartPen, (StartPosition.X - ObjectSize.Width * 0.5f) * ViewScale, (StartPosition.Y - ObjectSize.Height * 0.5f) * ViewScale, ObjectSize.Width * ViewScale, ObjectSize.Height * ViewScale);
                e.Graphics.DrawRectangle(EndPen, (EndPosition.X - ObjectSize.Width * 0.5f) * ViewScale, (EndPosition.Y - ObjectSize.Height * 0.5f) * ViewScale, ObjectSize.Width * ViewScale, ObjectSize.Height * ViewScale);
            }
        }

        private void DemoForm_Resize(object sender, EventArgs e)
        {
            this.Invalidate();
        }

        private void DemoForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (HitTestPoint(this.StartPosition, e.Location))
            {
                this.CurrentDragMode = DragMode.StartPosition;
            }
            else if (HitTestPoint(this.DestinationPosition, e.Location))
            {
                this.CurrentDragMode = DragMode.DestinationPosition;
            }
            else
            {
                // Determine which grid cell is being clicked
                int x = (int)Math.Floor(e.X / ViewScale);
                int y = (int)Math.Floor(e.Y / ViewScale);

                if (x >= 0 && x < Grid.Width
                    && y >= 0 && y < Grid.Height)
                {
                    // Toggle whether that grid cell is filled
                    ref GridCell cell = ref Grid.GetCell(x, y);
                    cell.Filled = !cell.Filled;
                    this.UpdateResults();
                }
            }

            LastCursorPosition = e.Location;
        }

        private void DemoForm_MouseMove(object sender, MouseEventArgs e)
        {
            PointF delta = new PointF((e.X - this.LastCursorPosition.X) / ViewScale, (e.Y - this.LastCursorPosition.Y) / ViewScale);
            if (CurrentDragMode == DragMode.StartPosition)
            {
                this.Cursor = Cursors.SizeAll;

                this.StartPosition.X += delta.X;
                this.StartPosition.Y += delta.Y;
                this.UpdateResults();
            }
            else if (CurrentDragMode == DragMode.DestinationPosition)
            {
                this.Cursor = Cursors.SizeAll;

                this.DestinationPosition.X += delta.X;
                this.DestinationPosition.Y += delta.Y;
                this.UpdateResults();
            }
            else if (CurrentDragMode == DragMode.None)
            {
                // Check what's under the mouse
                if (this.HitTestPoint(this.StartPosition, e.Location) || this.HitTestPoint(this.DestinationPosition, e.Location))
                {
                    this.Cursor = Cursors.SizeAll;
                }
                else if (e.X >= 0 && e.X < Grid.Width * ViewScale
                    && e.Y >= 0 && e.Y < Grid.Height * ViewScale)
                {
                    this.Cursor = Cursors.Hand;
                }
                else
                {
                    this.Cursor = Cursors.Arrow;
                }
            }

            LastCursorPosition = e.Location;

            this.Invalidate();
        }

        private void DemoForm_MouseUp(object sender, MouseEventArgs e)
        {
            this.CurrentDragMode = DragMode.None;

            LastCursorPosition = e.Location;
        }

        private void DemoForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Space)
            {
                this.UseBox = !this.UseBox;
                this.UpdateResults();
            }
        }
    }
}
