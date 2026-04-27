namespace Lumi.Language;

public static class SupportedMethods
{
    public static class ListMethods
    {
        public const string Add = "add";
        public const string Remove = "remove";
        public const string Length = "length";
        public const string Get = "get";
        public const string Set = "set";

        public static bool Contains(string methodName)
        {
            return methodName switch
            {
                Add => true,
                Remove => true,
                Length => true,
                Get => true,
                Set => true,
                _ => false
            };
        }
    }
}