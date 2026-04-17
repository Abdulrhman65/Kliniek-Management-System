using kliniek.Models;

namespace kliniek.Forms
{
    public partial class PrescriptionForm : Form
    {
        private readonly Patient _patient;

        public PrescriptionForm(Patient patient)
        {
            InitializeComponent();
            _patient = patient;
            lblPatientName.Text = $"المريض: {patient.fullname}";
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtDiagnosis.Text))
            {
                MessageBox.Show("برجاء كتابة التشخيص", "تنبيه", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            btnSave.Enabled = false;
            var originalBtnText = btnSave.Text;
            btnSave.Text = "جاري الحفظ...";

            try
            {

                var docUsername = Program.SharedData.LogedInDoc?.username;
                if (string.IsNullOrEmpty(docUsername))
                {
                    MessageBox.Show("جلسة الطبيب منتهية. يرجى تسجيل الدخول مرة أخرى.", "تنبيه",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.DialogResult = DialogResult.Abort;
                    this.Close();
                    return;
                }

                var prescription = new Prescription(
                    docUsername,
                    _patient.username,
                    txtDiagnosis.Text.Trim(),
                    txtMedications.Text.Trim(),
                    txtNotes.Text.Trim()
                );


                await Program.SharedData.SavePrescription(prescription);

                Program.SharedData.prescriptions?.Add(prescription);

                MessageBox.Show("تم حفظ الروشتة بنجاح ✅", "نجاح", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء الاتصال بالخادم: {ex.Message}", "خطأ في النظام", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (!this.IsDisposed && !this.Disposing)
                {
                    btnSave.Enabled = true;
                    btnSave.Text = originalBtnText;
                }
            }
        }
    }
}
