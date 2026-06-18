namespace Game.Scripts.Environment.Grid.Configuration
{
    public static class FieldSizeUtility
    {
        public static int GetWidth(FieldSize size)
        {
            return size switch
            {
                FieldSize.Field8X8 => 8,
                FieldSize.Field10X16 => 10,
                FieldSize.Field14X20 => 14,
                _ => 8
            };
        }

        public static int GetLength(FieldSize size)
        {
            return size switch
            {
                FieldSize.Field8X8 => 8,
                FieldSize.Field10X16 => 16,
                FieldSize.Field14X20 => 20,
                _ => 8
            };
        }
    }
}
