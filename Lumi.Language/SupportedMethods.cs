namespace Lumi.Language;

public static class SupportedMethods
{
    public static class ListMethods
    {
        public const string Add = "add";
        public const string Remove = "remove";
        public const string Length = "length";

        public static bool Contains(string methodName)
        {
            return methodName switch
            {
                Add => true,
                Remove => true,
                Length => true,
                _ => false
            };
        }
    }
}