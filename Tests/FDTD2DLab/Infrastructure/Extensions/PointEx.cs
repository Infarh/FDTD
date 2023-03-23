// ReSharper disable once CheckNamespace
namespace System.Windows;

public static class PointEx
{
    public static Point Add(this Point a, Point b) => new(a.X + b.X, a.Y + b.Y);
    public static Point Sub(this Point a, Point b) => new(a.X - b.X, a.Y - b.Y);
}
