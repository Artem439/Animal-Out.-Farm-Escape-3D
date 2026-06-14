namespace Game.Scripts.Environment.Grid.Configuration
{
    public enum FieldSize
    {
        Field8x8 = 0,
        Field16x10 = 1,
        Field20x14 = 2
    }

    public static class FieldSizeUtility
    {
        public static int GetWidth(FieldSize size)
        {
            return size switch
            {
                FieldSize.Field8x8 => 8,
                FieldSize.Field16x10 => 16,
                FieldSize.Field20x14 => 20,
                _ => 8
            };
        }

        public static int GetLength(FieldSize size)
        {
            return size switch
            {
                FieldSize.Field8x8 => 8,
                FieldSize.Field16x10 => 10,
                FieldSize.Field20x14 => 14,
                _ => 8
            };
        }
    }
}
