namespace TeamPvPAnalyzer.Enums
{
    public class StageData
    {
        public StageData(StageName name, int minX, int minY, int maxX, int maxY)
        {
            Name = name;
            X = minX;
            Y = minY;
            MaxX = maxX;
            MaxY = maxY;

            Width = maxX - minX;
            Height = maxY - minY;
        }

        public StageName Name { get; }

        public int X { get; }

        public int Y { get; }

        public int MaxX { get; }

        public int MaxY { get; }

        public int Width { get; }

        public int Height { get; }

        public bool IsInRange(int x, int y)
        {
            return X <= x && Y <= y && x <= MaxX && y <= MaxY;
        }

        public bool IsInRange(float x, float y)
        {
            return X * 16 <= x && Y * 16 <= y && x <= MaxX * 16 && y <= MaxY * 16;
        }

        public enum StageName
        {
            Espeon,
            Hell,
            Ice,
            Forest,
        }
    }
}
