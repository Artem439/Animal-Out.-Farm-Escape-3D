namespace Game.Scripts.Environment.Grid.Configuration
{
    public enum FieldSize
    {
        Small = 0,
        Medium = 1,
        Large = 2,
        ExtraLarge = 3
    }

    public static class FieldSizeUtility
    {
        public static int GetGridSize(FieldSize size)
        {
            return size switch
            {
                FieldSize.Small => 8,
                FieldSize.Medium => 10,
                FieldSize.Large => 14,
                FieldSize.ExtraLarge => 16,
                _ => 8
            };
        }
    }
}
