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
        /// Sweeps the given axis-aligned box from the given starting point to the given ending point, returning whether anything
        /// was hit and the location of the box when it was hit.
        /// </summary>
        /// <param name="startX">The X coordinate of the starting position of the box.</param>
        /// <param name="startY">The Y coordinate of the starting position of the box.</param>
        /// <param name="endX">The X coordinate of the position to sweep through.</param>
        /// <param name="endY">The Y coordinate of the position to sweep through.</param>
        /// <param name="width">The width of the axis-aligned box.</param>
        /// <param name="height">The height of the axis-aligned box.</param>
        /// <param name="hitX">The X position of the box when it first collided.</param>
        /// <param name="hitY">The Y position of the box when it first collided.</param>
        /// <returns>True if the given box collided with any filled cells, and false if there was no collision.</returns>
        public bool SweepBox(float startX, float startY, float endX, float endY, float width, float height, out float hitX, out float hitY)
        {
            // Test the starting location
            // NOTE: This can be skipped if you assume that the box's starting position has already been checked
            // For example, when moving a character, we might assume that the last known position was not colliding
            for (int cellY = (int)Math.Floor(startY - height * 0.5f); cellY <= (int)Math.Floor(startY + height * 0.5f); cellY++)
            {
                if (cellY >= 0 && cellY < this.Height)
                {
                    for (int cellX = (int)Math.Floor(startX - width * 0.5f); cellX <= (int)Math.Floor(startX + width * 0.5f); cellX++)
                    {
                        if (cellX >= 0 && cellX < this.Width && this.GetCell(cellX, cellY).Filled)
                        {
                            hitX = startX;
                            hitY = startY;
                            return true;
                        }
                    }
                }
            }

            // Determine the direction of travel
            // We don't use Math.Sign here because when the difference is zero, we still need a direction
            // to choose which corner of the box to use.
            int xdir = (endX - startX >= 0) ? 1 : -1;
            int ydir = (endY - startY >= 0) ? 1 : -1;

            // Determine the location of the corner in the direction of travel
            // Then, determine which cell it will end up in and increment until we get there.
            float cornerX = startX + width * 0.5f * xdir;
            float cornerY = startY + height * 0.5f * ydir;
            int cornerCellX = (int)Math.Floor(cornerX); // Which cell the corner is currently in
            int cornerCellY = (int)Math.Floor(cornerY); // Which cell the corner is currently in
            int endCellX = (int)Math.Floor(endX + width * 0.5f * xdir); // Which cell the corner will end up in
            int endCellY = (int)Math.Floor(endY + height * 0.5f * ydir); // Which cell the corner will end up in

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
            int nextX = xdir >= 0 ? (int)Math.Floor(cornerX + 1.0f) : (int)Math.Floor(cornerX);
            float stepX = nextX - cornerX; // How far in X to the next X coordinate
            float toNextX = (float)Math.Sqrt(stepX * stepX + (stepX * dy) * (stepX * dy)); // How far along the line to the next X coordinate

            // Determine how far it will be until we hit the next cell in the Y direction
            int nextY = ydir >= 0 ? (int)Math.Floor(cornerY + 1.0f) : (int)Math.Floor(cornerY);
            float stepY = nextY - cornerY; // How far in Y to the next Y coordinate
            float toNextY = (float)Math.Sqrt(stepY * stepY + (stepY * dx) * (stepY * dx)); // How far along the line to the next Y coordinate

            // Keep stepping until the corner reaches the destination cell
            while (cornerCellX != endCellX || cornerCellY != endCellY)
            {
                // Determine whether the next X value is closer, or the next Y value
                if (toNextX < toNextY)
                {
                    // Move to the next X value
                    cornerCellX += xdir;
                    cornerX += stepX;
                    cornerY += stepX * dy;

                    // Adjust our movement plan along the other axis
                    stepY -= stepX * dy; // Convert how far we moved in X to how far we moved in Y
                    toNextY -= toNextX;

                    // Calculate for the new next X value
                    nextX = cornerCellX + xdir;
                    stepX = 1.0f * xdir;
                    toNextX = (float)Math.Sqrt(1.0f * 1.0f + dy * dy);

                    // Check the newly-occupied column of cells for collision
                    int cellCount = 1 + Math.Abs(cornerCellY - (int)Math.Floor(cornerY - height * ydir));
                    for (int i = 0; i < cellCount; i++)
                    {
                        int y = cornerCellY - i * ydir;
                        if (cornerCellX >= 0 && cornerCellX < this.Width
                            && y >= 0 && y < this.Height
                            && this.GetCell(cornerCellX, y).Filled)
                        {
                            hitX = cornerX - width * 0.5f * xdir;
                            hitY = cornerY - height * 0.5f * ydir;
                            return true;
                        }
                    }
                }
                else
                {
                    // Move to the next Y value
                    cornerCellY += ydir;
                    cornerY += stepY;
                    cornerX += stepY * dx;

                    // Adjust our movement plan along the other axis
                    stepX -= stepY * dx;
                    toNextX -= toNextY;

                    // Calculate for the next next Y value
                    nextY = cornerCellY + ydir;
                    stepY = 1.0f * ydir;
                    toNextY = (float)Math.Sqrt(dx * dx + 1.0f * 1.0f);

                    // Check the newly-occupied row of cells for collision
                    int cellCount = 1 + Math.Abs(cornerCellX - (int)Math.Floor(cornerX - width * xdir));
                    for (int i = 0; i < cellCount; i++)
                    {
                        int x = cornerCellX - i * xdir;
                        if (cornerCellY >= 0 && cornerCellY < this.Height
                            && x >= 0 && x < this.Width
                            && this.GetCell(x, cornerCellY).Filled)
                        {
                            hitX = cornerX - width * 0.5f * xdir;
                            hitY = cornerY - height * 0.5f * ydir;
                            return true;
                        }
                    }
                }
            }

            // If we reach here, no cells were hit
            hitX = endX;
            hitY = endY;
            return false;
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
            int xdir = (endX - startX >= 0) ? 1 : -1;
            int ydir = (endY - startY >= 0) ? 1 : -1;

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

            // Determine how far it will be until we hit the next cell in the Y direction
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
