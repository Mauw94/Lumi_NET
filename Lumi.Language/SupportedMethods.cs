namespace Lumi.Language;

public static class SupportedMethods
{
    /// <summary>
    /// Supported methods for list manipulation.
    /// </summary>
    public static class ListMethods
    {
        /// <summary>
        /// Adds a new element to the end of the list.
        /// </summary>
        public const string Add = "add";

        /// <summary>
        /// Tries to remove an element from the list. Returns true if the element was found and removed, false otherwise.
        /// </summary>
        public const string Remove = "remove";

        /// <summary>
        /// Gets the number of elements in the list.
        /// </summary>
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