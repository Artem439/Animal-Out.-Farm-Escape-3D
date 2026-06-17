namespace Game.Scripts.Environment.Grid.Configuration
{
    public enum FieldSize
    {
        Field8x8 = 0,
        Field10x16 = 1,
        Field14x20 = 2
    }

    public static class FieldSizeUtility
    {
        public static int GetWidth(FieldSize size)
        {
            return size switch
            {
                FieldSize.Field8x8 => 8,
                FieldSize.Field10x16 => 10,
                FieldSize.Field14x20 => 14,
                _ => 8
            };
        }

        public static int GetLength(FieldSize size)
        {
            return size switch
            {
                FieldSize.Field8x8 => 8,
                FieldSize.Field10x16 => 16,
                FieldSize.Field14x20 => 20,
                _ => 8
            };
        }
    }
}
