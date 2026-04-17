namespace kliniek.Models
{
    public class Person(string userName, string password, string fullName)
    {
        public string username { get; set; } = userName;
        public string password { get; set; } = password;
        public string fullname { get; set; } = fullName;
    }
}
