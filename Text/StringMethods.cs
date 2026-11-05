namespace KGallery.Text
{
    public static class StringMethods
    {
        public static string GetEndWord(int objectsCount)
        {
            return objectsCount > 0 ? "s" : "";
        }
    }
}
