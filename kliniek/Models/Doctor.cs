namespace kliniek.Models
{
    public class Doctor(string userName, string password, string fullName, string specialization) : Person(userName, password, fullName)
    {
        public string specialization { get; set; } = specialization;
    }
}
