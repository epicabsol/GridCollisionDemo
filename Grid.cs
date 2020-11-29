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
    }
}
