using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridCollisionDemo
{
    public struct GridCell
    {
        public bool Filled;
    }

    public class Grid
    {
        public int Width { get; }
        public int Height { get; }

        private GridCell[] _cells = null;

        public Grid(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this._cells = new GridCell[width * height];
        }

        public ref GridCell GetCell(int x, int y)
        {
            if (x < 0 || x >= this.Width)
                throw new ArgumentOutOfRangeException(nameof(x));

            if (y < 0 || y >= this.Height)
                throw new ArgumentOutOfRangeException(nameof(y));

            return ref this._cells[y * this.Width + x];
        }

        /// <summary>
        /// Test the given line segment for intersections with any <see cref="GridCell"/>s that are <see cref="GridCell.Filled"/>.
        /// </summary>
        /// <param name="startX">The X coordinate of the start of the line to test.</param>
        /// <param name="startY">the Y coordinate of the start of the line to test.</param>
        /// <param name="endX">The X coordinate of the end of the line to test.</param>
        /// <param name="endY">The Y coordinate of the end of the line to test.</param>
        /// <param name="hitX">The X coordinate of the point where collision occurred.</param>
        /// <param name="hitY">The Y coordinate of the point where collision occurred.</param>
        /// <returns>True if the given line segment collided, and false if there was no collision.</returns>
        public bool TestSegment(float startX, float startY, float endX, float endY, out float hitX, out float hitY)
        {
            int cellX = (int)Math.Floor(startX);
            int cellY = (int)Math.Floor(startY);

            // Test the starting cell
            if (cellX >= 0 && cellX < this.Width
                && cellY >= 0 && cellY < this.Height
                && this.GetCell(cellX, cellY).Filled)
            {
                hitX = startX;
                hitY = startY;
                return true;
            }

            float x = startX;
            float y = startY;
            int endCellX = (int)Math.Floor(endX);
            int endCellY = (int)Math.Floor(endY);
            int xdir = Math.Sign(endX - startX);
            int ydir = Math.Sign(endY - startY);

            float dy; // The slope, or, how far in the y we move for each 1 unit of x
            if (endX != startX)
            {
                dy = (endY - startY) / (endX - startX);
            }
            else
            {
                dy = Single.PositiveInfinity;
            }

            float dx; // How far in the x we move for each 1 unit of y
            if (endY != startY)
            {
                dx = (endX - startX) / (endY - startY);
            }
            else
            {
                dx = Single.PositiveInfinity;
            }

            // Determine how far it will be until we hit the next cell in the X direction
            int nextX = xdir >= 0 ? (int)Math.Floor(x + 1.0f) : (int)Math.Floor(x);
            float stepX = nextX - x; // How far in X to the next X coordinate
            float toNextX = (float)Math.Sqrt(stepX * stepX + (stepX * dy) * (stepX * dy)); // How far along the line to the next X coordinate

            int nextY = ydir >= 0 ? (int)Math.Floor(y + 1.0f) : (int)Math.Floor(y);
            float stepY = nextY - y; // How far in Y to the next Y coordinate
            float toNextY = (float)Math.Sqrt(stepY * stepY + (stepY * dx) * (stepY * dx)); // How far along the line to the next Y coordinate

            // Keep stepping until we reach the destination cell
            while (cellX != endCellX || cellY != endCellY)
            {
                // Determine whether the next X value is closer, or the next Y value
                if (toNextX < toNextY)
                {
                    // Move to the next X value
                    cellX += xdir;
                    x += stepX;
                    y += stepX * dy;

                    // Adjust our movement plan along the other axis
                    stepY -= stepX * dy; // Convert how far we moved in X to how far we moved in Y
                    toNextY -= toNextX;

                    // Calculate for the new next X value
                    nextX = cellX + xdir;
                    stepX = 1.0f * xdir;
                    toNextX = (float)Math.Sqrt(1.0f * 1.0f + dy * dy);
                }
                else
                {
                    // Move to the next Y value
                    cellY += ydir;
                    y += stepY;
                    x += stepY * dx;

                    // Adjust our movement plan along the other axis
                    stepX -= stepY * dx;
                    toNextX -= toNextY;

                    // Calculate for the next next Y value
                    nextY = cellY + ydir;
                    stepY = 1.0f * ydir;
                    toNextY = (float)Math.Sqrt(dx * dx + 1.0f * 1.0f);
                }

                // Check whether the cell we arrived at is in the grid
                if (cellX >= 0 && cellX < this.Width
                    && cellY >= 0 && cellY < this.Height)
                {
                    // Check whether it should end this test
                    if (this.GetCell(cellX, cellY).Filled)
                    {
                        hitX = x;
                        hitY = y;
                        return true;
                    }
                }
                else if (xdir > 0 && cellX >= this.Width
                    || xdir < 0 && cellX < 0
                    || ydir > 0 && cellY >= this.Height
                    || ydir < 0 && cellY < 0)
                {
                    // The segment is off the grid and travelling away from the grid
                    // so it will never collide.
                    hitX = endX;
                    hitY = endY;
                    return false;
                }
            }

            // If we reach here, no cells were hit
            hitX = endX;
            hitY = endY;
            return false;
        }
    }
}
