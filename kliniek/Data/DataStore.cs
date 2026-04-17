using kliniek.Models;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace kliniek.Data
{
    public class DataStore
    {
        // بيانات الجلسة الحالية
        public Doctor? LogedInDoc = null;
        public Patient? LoggedInPatient = null;

        public const string SecretCode = "#3107ML";

        // القوائم الرئيسية
        public List<Patient> patient = [];
        public List<Doctor> doctor = [];
        public List<Appointment> appointments = [];
        public List<Prescription> prescriptions = [];
        public List<string> errs = [];

        // بيانات ثابتة للـ ComboBoxes
        public List<string> bloodtybe = ["اختر فصيلة الدم...", "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-"];

        public List<string> specializations = [
            "اختر التخصص...", "طب الباطنة", "طب الأطفال", "أمراض القلب", "الجراحة العامة",
            "طب العظام", "الجلدية والتجميل", "النساء والتوليد", "طب العيون",
            "أنف وأذن وحنجرة", "المخ والأعصاب", "الطب النفسي", "المسالك البولية"
        ];

        public List<string> time = ["اختر الوقت ...", "09:00 AM", "11:00 AM", "01:00 PM", "03:00 PM", "05:00 PM"];

        // إعدادات الـ HttpClient
        private static readonly HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(30) // مهلة زمنية للاتصال
        };

        public DataStore()
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("apikey", SupabaseConfig.Key);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {SupabaseConfig.Key}");
        }

        // ==================
        // دالة تسجيل الأخطاء (Helper)
        // ==================
        private void LogException(string source, Exception ex)
        {
            try
            {
                string errorMessage = $"Error in {source}: {ex.Message} at {DateTime.Now}";
                errs.Add(errorMessage);
                string filePath = Path.Combine(Application.StartupPath, "error_log.txt");
                File.AppendAllLines(filePath, [errorMessage]);
            }
            catch { /* تجنب تعليق البرنامج إذا فشل تسجيل الخطأ */ }
        }

        // ==================
        // التحميل (بشكل متوازي لتسريع العملية)
        // ==================
        public async Task Load()
        {
            try
            {
                var t1 = client.GetStringAsync($"{SupabaseConfig.Url}/rest/v1/patients?select=*");
                var t2 = client.GetStringAsync($"{SupabaseConfig.Url}/rest/v1/doctors?select=*");
                var t3 = client.GetStringAsync($"{SupabaseConfig.Url}/rest/v1/appointments?select=*");
                var t4 = client.GetStringAsync($"{SupabaseConfig.Url}/rest/v1/prescriptions?select=*");

                await Task.WhenAll(t1, t2, t3, t4);

                patient = JsonConvert.DeserializeObject<List<Patient>>(await t1) ?? [];
                doctor = JsonConvert.DeserializeObject<List<Doctor>>(await t2) ?? [];
                appointments = JsonConvert.DeserializeObject<List<Appointment>>(await t3) ?? [];
                prescriptions = JsonConvert.DeserializeObject<List<Prescription>>(await t4) ?? [];
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ في الاتصال بالسيرفر، يرجى التحقق من الإنترنت.", "خطأ", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogException("Loading Data", ex);
            }
        }

        // ==================
        // عمليات الحفظ (Insert/Upsert)
        // ==================
        public async Task SavePatient(Patient p)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{SupabaseConfig.Url}/rest/v1/patients");
            request.Headers.Add("Prefer", "resolution=merge-duplicates");
            request.Content = new StringContent(JsonConvert.SerializeObject(new List<Patient> { p }), Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task SaveDoctor(Doctor d)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{SupabaseConfig.Url}/rest/v1/doctors");
            request.Headers.Add("Prefer", "resolution=merge-duplicates");
            request.Content = new StringContent(JsonConvert.SerializeObject(new List<Doctor> { d }), Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task SaveAppointment(Appointment a)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{SupabaseConfig.Url}/rest/v1/appointments");
            request.Headers.Add("Prefer", "resolution=merge-duplicates,return=representation");
            request.Content = new StringContent(JsonConvert.SerializeObject(a), Encoding.UTF8, "application/json");
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task SavePrescription(Prescription p)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"{SupabaseConfig.Url}/rest/v1/prescriptions");
            request.Headers.Add("Prefer", "return=minimal");

            // نرسل مصفوفة تحتوي على الحقول المطلوبة فقط (بدون الـ id)
            var dataToSave = new[]
            {
        new {
            doctorusername = p.doctorusername,
            patientusername = p.patientusername,
            diagnosis = p.diagnosis,
            medications = p.medications,
            notes = p.notes,
            date = p.date.ToString("yyyy-MM-ddTHH:mm:ssZ")
        }
    };

            request.Content = new StringContent(
                JsonConvert.SerializeObject(dataToSave),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string errorDetails = await response.Content.ReadAsStringAsync();
                throw new Exception($"خطأ من السيرفر: {errorDetails}");
            }
        }

        // ==================
        // عمليات الحذف (Delete)
        // ==================
        public async Task DeleteDoctor(string username)
        {
            try
            {
                await client.DeleteAsync($"{SupabaseConfig.Url}/rest/v1/appointments?doctorusername=eq.{username}");
                await client.DeleteAsync($"{SupabaseConfig.Url}/rest/v1/prescriptions?doctorusername=eq.{username}");
                var res = await client.DeleteAsync($"{SupabaseConfig.Url}/rest/v1/doctors?username=eq.{username}");
                res.EnsureSuccessStatusCode();
            }
            catch (Exception ex) { LogException("DeleteDoctor", ex); MessageBox.Show("خطأ في حذف الطبيب"); }
        }

        public async Task DeletePatient(string username)
        {
            try
            {
                await client.DeleteAsync($"{SupabaseConfig.Url}/rest/v1/appointments?patientusername=eq.{username}");
                await client.DeleteAsync($"{SupabaseConfig.Url}/rest/v1/prescriptions?patientusername=eq.{username}");
                var res = await client.DeleteAsync($"{SupabaseConfig.Url}/rest/v1/patients?username=eq.{username}");
                res.EnsureSuccessStatusCode();
            }
            catch (Exception ex) { LogException("DeletePatient", ex); MessageBox.Show("خطأ في حذف المريض"); }
        }

        public async Task DeleteApp(int id)
        {
            try
            {
                var res = await client.DeleteAsync($"{SupabaseConfig.Url}/rest/v1/appointments?id=eq.{id}");
                res.EnsureSuccessStatusCode();
            }
            catch (Exception ex) { LogException("DeleteApp", ex); MessageBox.Show("خطأ في حذف الموعد"); }
        }

        public async Task DeletePrescription(int id)
        {
            try
            {
                var res = await client.DeleteAsync($"{SupabaseConfig.Url}/rest/v1/prescriptions?id=eq.{id}");
                res.EnsureSuccessStatusCode();
            }
            catch (Exception ex) { LogException("DeletePrescription", ex); MessageBox.Show("خطأ في حذف الروشتة"); }
        }
    }

    public class SaveData
    {
        public List<Patient>? Patients { get; set; }
        public List<Doctor>? Doctors { get; set; }
        public List<Appointment>? Appointments { get; set; }
        public List<Prescription>? Prescriptions { get; set; }
    }
}