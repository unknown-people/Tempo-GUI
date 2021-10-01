using System.Linq;
using System.Collections.Specialized;

namespace Music_user_bot
{
    class Whitelist
    {
        public static StringCollection white_list { get; set; }
        public static ulong ownerID { get; set; }
        public Whitelist(params ulong[] userIDs) {
            if (IsNullOrEmpty(userIDs))
            {
                white_list = new StringCollection {};
            }
            white_list = ArrayToCollection(userIDs);
        }
        public static void AddToWL(ulong userID)
        {
            white_list.Add(userID.ToString());
        }
        public static void RemoveFromWL(ulong userID)
        {
            if (IsInsideCollection(userID, white_list))
            {
                white_list.Remove(userID.ToString());
            }
        }
        public static bool IsNullOrEmpty(ulong[] array)
        {
            if (array == null || array.Length == 0)
                return true;
            else
                return array.All(item => item == 0);
        }
        private StringCollection ArrayToCollection(ulong[] input)
        {
            StringCollection result = new StringCollection() { };
            foreach(ulong entry in input)
            {
                result.Add(entry.ToString());
            }
            return result;
        }
        private static bool IsInsideCollection(ulong userId, StringCollection collection)
        {
            foreach(string entry in collection)
            {
                if (userId.ToString() == entry)
                    return true;
            }
            return false;
        }
    }
}
