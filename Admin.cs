using System.Linq;
using System.Collections.Specialized;

namespace Music_user_bot
{
    class Admin
    {
        public static StringCollection admins { get; set; }
        public static ulong ownerID { get; set; }
        public Admin(params ulong[] userIDs) {
            if (IsNullOrEmpty(userIDs))
            {
                admins = new StringCollection {};
            }
            admins = ArrayToCollection(userIDs);
        }
        public static void AddToAl(ulong userID)
        {
            admins.Add(userID.ToString());
        }
        public static void RemoveFromAl(ulong userID)
        {
            if (IsInsideCollection(userID, admins))
            {
                admins.Remove(userID.ToString());
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
