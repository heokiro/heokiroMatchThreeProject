using UnityEngine;

namespace Match3.Codes
{
    public class Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void AddPoint(Point point1)
        {
            X += point1.X;
            Y += point1.Y;
        }

        public Vector2 ToVector()
        {
            return new Vector2(X, Y);
        }

        public bool IsEquals(Point otherPoint)
        {
            return (X == otherPoint.X && Y == otherPoint.Y);
        }

        public static Point MultiPoint(Point point, int scale)
        {
            return new Point(point.X * scale, point.Y * scale);
        }
    
        public static Point AddPoint(Point point1, Point point2)
        {
            return new Point(point1.X + point2.X, point1.Y +point2.Y);
        }

        public static Point ClonePoint(Point sourcePoint)
        {
            return new Point(sourcePoint.X, sourcePoint.Y);
        }
    
        public static Point Zero => new(0, 0);

        public static Point Up => new(0, 1);

        public static Point Down => new(0, -1);

        public static Point Right => new(1, 0);

        public static Point Left => new(-1, 0);
    }
}
