namespace kliniek.Models
{
    public class Appointment(string doctorUserName, string patientUserName, DateTime date, string status)
    {
        public string DoctorUserName { get; set; }
        public string PatientUserName { get; set; }
        public DateTime Date_ { get; set; }
        public string Status { get; set; }

        public Appointment(string doctorUserName, string patientUserName, DateTime date) 
        {

            DoctorUserName = doctorUserName;
            PatientUserName = patientUserName;
            Date_ = date;
            
        }
    }
    
}
