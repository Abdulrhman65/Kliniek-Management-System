using kliniek.Models;
using System.Data;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace kliniek.Forms
{
    public partial class DoctorForm : Form
    {
        public DoctorForm()
        {
            InitializeComponent();
        }

        private void DoctorForm_Load(object sender, EventArgs e)
        {

            Data.DataStore data = Program.SharedData;

            label1.Text = $"د. {data.LogedInDoc?.fullname}";
            label2.Text = data.LogedInDoc?.specialization;
            panel1.Dock = DockStyle.Left;
            label4.Text = DateTime.Now.ToString("d");

            int myPatientsCount = data.patient.Count(p =>
                data.appointments.Any(a =>
                    a.doctorusername == data.LogedInDoc?.username &&
                    a.patientusername == p.username
                )
            );


            int todayAppts = data.appointments.Count(a =>
                a.doctorusername == data.LogedInDoc?.username &&
                a.date.Date == DateTime.Today
            );

            int weekPresc = data.prescriptions.Count(p =>
                p.doctorusername == data.LogedInDoc?.username &&
                p.date >= DateTime.Today.AddDays(-7)
            );


            lblPatientsCount.Text = myPatientsCount.ToString();
            lblTodayAppts.Text = todayAppts.ToString();
            lblWeekPresc.Text = weekPresc.ToString();

            LoadPatients();
            LoadTodayAppointments();

            flowLayoutPanel1.AutoScroll = true;
            flowLayoutPanel1.FlowDirection = FlowDirection.LeftToRight;

        }

        bool logO;
        private void Button1_Click(object sender, EventArgs e)
        {
            Program.SharedData.LogedInDoc = null;
            logO = true;
            this.Close();
        }

        private void LoadTodayAppointments()
        {
            ClearPanel(flowLayoutPanel2);
            Data.DataStore data = Program.SharedData;

            var todayAppts = data.appointments
                .Where(a =>
                    a.doctorusername == Program.SharedData.LogedInDoc?.username &&
                    a.date.Date == DateTime.Today)
                .Take(5)
                .ToList();
            
            foreach (var appt in todayAppts)
            {
                var patient = data.patient
                    .FirstOrDefault(p => p.username == appt.patientusername);

                
                Panel card = new()
                {
                    Width = flowLayoutPanel2.Width - 40,
                    Margin = new Padding(20, 5, 20, 5),
                    Height = 60,
                    BackColor = Color.FromArgb(38, 48, 68)
                };

                Label lblName = new()
                {
                    Text = "👤 " + (patient?.fullname ?? "غير معروف"),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(10, 8)
                };

                Label lblTime = new()
                {
                    Text = appt.date.ToString("hh:mm tt"),
                    ForeColor = Color.FromArgb(150, 175, 210),
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    Location = new Point(10, 32)
                };

                Label lblStatus = new()
                {
                    Text = appt.status,
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9),
                    Location = new Point(card.Width - 500, 23)
                };

                Button btnCancel = new()
                {
                    Text = "إلغاء الحجز",
                    BackColor = Color.FromArgb(74, 26, 26),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Width = 100,
                    Height = 35,
                    Location = new Point(card.Width - 110, 16),
                    Font = new Font("Segoe UI", 9),
                };
                btnCancel.FlatAppearance.BorderSize = 0;
                if (appt.status == "ملغي"  || appt.status == "انتهى")
                {
                    btnCancel.Enabled = false;
                }

                btnCancel.Click += async (s, e) =>
                {
                    var result = MessageBox.Show("هل تريد إلغاء الموعد؟", "تأكيد", MessageBoxButtons.YesNo);
                    if (result == DialogResult.Yes)
                    {
                        appt.status = "ملغي";
                        await data.SaveAppointment(appt);
                        LoadTodayAppointments();
                    }
                };
                if (appt.status == "انتهى")
                {
                    lblStatus.ForeColor = Color.FromArgb(106, 191, 106);
                    lblStatus.BackColor = Color.FromArgb(26, 61, 26);
                }
                else if (appt.status == "انتظار")
                {
                    lblStatus.ForeColor = Color.FromArgb(220, 180, 50);
                    lblStatus.BackColor = Color.FromArgb(61, 49, 15);
                }
                else
                {
                    lblStatus.ForeColor = Color.FromArgb(255, 107, 107);
                    lblStatus.BackColor = Color.FromArgb(74, 26, 26);
                }

                card.Controls.AddRange([lblName, lblTime, lblStatus, btnCancel]);
                flowLayoutPanel2.Controls.Add(card);
            }
        }
       
        // زهجججججججججججججججججججججت
        private void Label6_Click(object sender, EventArgs e)
        {

        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            string search = textBox1.Text.ToLower(); // بياخد الحرف بحرفه
            Data.DataStore data = Program.SharedData;

            var filtered = data.patient.Where(p =>  //  بتبحث
                p.fullname.Contains(search, StringComparison.CurrentCultureIgnoreCase) && // عن الي يحتوي على الكلمات
                data.appointments.Any(a => // any بتشوف الأول هل فيه المريض دا عند الطبيب دا ولا لععع
                    a.doctorusername == data.LogedInDoc?.username &&
                    a.patientusername == p.username)
            ).ToList();

            LoadPatients(filtered);
        }

        private void LoadPatients(List<Patient>? list = null)
        {
            ClearPanel(flowLayoutPanel1);
            Data.DataStore data = Program.SharedData;

            // جيب المرضى الخاصين بالدكتور دعع بس
            var myPatients = list ?? [.. data.patient.Where(p =>
                data.appointments.Any(a =>
                    a.doctorusername == Program.SharedData.LogedInDoc?.username &&
                    a.patientusername == p.username
                )
            )];

            foreach (var p in myPatients)
            {
                Panel card = new()
                {
                    Width = 220,
                    Height = 130,
                    BackColor = Color.FromArgb(13, 17, 23),
                    Margin = new Padding(12)
                };

                Label lblName = new()
                {
                    Text = "👤 " + p.fullname,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(10, 12)
                };

                Label lblInfo = new()
                {
                    Text = $"السن: {p.age}   |   {p.bloodtype}",
                    ForeColor = Color.FromArgb(160, 160, 160),
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    Location = new Point(10, 40)
                };

                Button btnView = new()
                {
                    Text = "عرض",
                    Width = 80,
                    Height = 28,
                    Location = new Point(10, 85),
                    BackColor = Color.FromArgb(24, 95, 165),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 9)
                };
                btnView.FlatAppearance.BorderSize = 0;

                
                var patient = p;
               
                btnView.Click += (s, e) =>
                {
                    new PatientDetailsForm(patient).ShowDialog();
                };

                card.Controls.AddRange([lblName, lblInfo, btnView]);
                flowLayoutPanel1.Controls.Add(card);
            }
        }

        private void RadioButton2_CheckedChanged(object sender, EventArgs e){ panel4.Visible = false; panel5.Visible = false; panel10.Visible = true; panelPrescriptions.Visible = false;}

        private void RadioButton1_CheckedChanged(object sender, EventArgs e){ panel4.Visible = true; panel5.Visible = false; panel10.Visible = false; panelPrescriptions.Visible = false; }

        private void RadioButton3_CheckedChanged(object sender, EventArgs e){ panel4.Visible = false;panel5.Visible = false; panel10.Visible = false;panelPrescriptions.Visible = true; LoadPrescriptions(); }
        private void RadioButton4_CheckedChanged(object sender, EventArgs e){ panel4.Visible = false;panel5.Visible = true;panel10.Visible = false;panelPrescriptions.Visible = false; Loadappo();}
        private void LoadPrescriptions()
        {
            ClearPanel(flowLayoutPanelPrescriptions);
            Data.DataStore data = Program.SharedData;

            var myPrescriptions = data.prescriptions
                .Where(p => p.doctorusername == data.LogedInDoc?.username)
                .OrderByDescending(p => p.date)
                .ToList();

            if (myPrescriptions.Count == 0)
            {
                Label lblEmpty = new()
                {
                    Text = "لا توجد روشتات بعد",
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(20)
                };
                flowLayoutPanelPrescriptions.Controls.Add(lblEmpty);
                return;
            }

            foreach (var presc in myPrescriptions)
            {
                var patient = data.patient.FirstOrDefault(p => p.username == presc.patientusername);

                Panel card = new()
                {
                    Width = flowLayoutPanelPrescriptions.Width - 40,
                    Height = 120,
                    BackColor = Color.FromArgb(21, 32, 43),
                    Margin = new Padding(10, 5, 10, 5),
                    Padding = new Padding(18),
                    AutoScroll = true,

                };

                Label lblName = new()
                {
                    Text = "👤 " + (patient?.fullname ?? "غير معروف"),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(15, 10)
                };

                Label lblDate = new()
                {
                    Text = presc.date.ToString("dd/MM/yyyy hh:mm tt"),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    Location = new Point(15, 38)
                };

                Label lblDiag = new()
                {
                    Text = "التشخيص: " + (presc.diagnosis.Length > 60 ? presc.diagnosis[..60] + "..." : presc.diagnosis),
                    ForeColor = Color.FromArgb(200, 200, 200),
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    Location = new Point(15, 62)
                };

                Label lblMeds = new()
                {
                    Text = "الأدوية: " + (presc.medications.Length > 60 ? presc.medications[..60] + "..." : presc.medications),
                    ForeColor = Color.FromArgb(106, 191, 106),
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    Location = new Point(15, 86)
                };

                card.Controls.AddRange([lblName, lblDate, lblDiag, lblMeds]);
                flowLayoutPanelPrescriptions.Controls.Add(card);
            }
        }

        private void DoctorForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (logO) this.Hide();
            else Application.Exit();
        }

        private async void DeleteAcc_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("هل أنت متأكد من حذف الحساب؟", "تأكيد", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                if (Program.SharedData.LogedInDoc != null)
                {
                    await Program.SharedData.DeleteDoctor(Program.SharedData.LogedInDoc.username);
                    Program.SharedData.doctor.Remove(Program.SharedData.LogedInDoc);
                    logO = true;
                    new LoginForm().Show();
                    if (logO) this.Close();
                }
            }
        }

       
        private void Loadappo()
        {
            ClearPanel(flowLayoutPanel3);
            Data.DataStore data = Program.SharedData;

            var myappointments = data.appointments
                .Where(p => p.doctorusername == data.LogedInDoc?.username)
                .OrderByDescending(p => p.date)
                .ToList();

            if (myappointments.Count == 0)
            {
                Label lblEmpty = new()
                {
                    Text = "لا توجد حجوزات بعد",
                    ForeColor = Color.Gray,
                    Font = new Font("Segoe UI", 12, FontStyle.Bold),
                    AutoSize = true,
                    Margin = new Padding(20)
                };
                flowLayoutPanel3.Controls.Add(lblEmpty);
                return;
            }
            flowLayoutPanel3.AutoScroll = true;
            foreach (var presc in myappointments)
            {
                var patient = data.patient.FirstOrDefault(p => p.username == presc.patientusername);

                Panel card = new()
                {
                    Width = flowLayoutPanel3.Width - 20,
                    Height = 80,
                    BackColor = Color.FromArgb(21, 32, 43),
                    Margin = new Padding(10, 5, 10, 5),
                    Padding = new Padding(20),
                    
                };

                Label lblName = new()
                {
                    Text = "👤 " + (patient?.fullname ?? "غير معروف"),
                    ForeColor = Color.FromArgb(37, 99, 235),
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    AutoSize = true,
                    Location = new Point(15, 10)
                };

                Label lblDate = new()
                {
                    Text = presc.date.ToString("dd/MM/yyyy hh:mm tt"),
                    ForeColor = Color.FromArgb(148, 163, 184),
                    Font = new Font("Segoe UI", 9),
                    AutoSize = true,
                    Location = new Point(15, 38)
                };

                card.Controls.AddRange([lblName, lblDate]);
                flowLayoutPanel3.Controls.Add(card);
            }
        }

        private void ClearPanel(Panel panel)
        {
            foreach (Control ctrl in panel.Controls)
            {
                ctrl.Click -= null; // فصل الأحداث لو ممكن
                ctrl.Dispose();     // تحرير الموارد
            }
            panel.Controls.Clear();
        }
    }
}
