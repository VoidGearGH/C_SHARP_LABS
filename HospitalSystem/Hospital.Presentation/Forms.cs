using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Hospital.Business;
using Hospital.Services;

namespace Hospital.Presentation
{
    internal class Item<T>
    {
        public T Value { get; }
        public string Text { get; }
        public Item(T value, string text) { Value = value; Text = text; }
        public override string ToString() => Text;
    }

    internal static class Ui
    {
        public static List<Item<T>> Items<T>(IEnumerable<T> values, Func<T, string> name) =>
            values.Select(v => new Item<T>(v, name(v))).ToList();

        public static Label Caption(string text) =>
            new Label { Text = text, AutoSize = true, Margin = new Padding(3, 8, 3, 0) };

        public static ComboBox Combo(int width = 220) =>
            new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = width };

        public static void Show(OperationResult r) =>
            MessageBox.Show(r.Message, r.Success ? "Готово" : "Ошибка",
                MessageBoxButtons.OK, r.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);

        public static void Info(string text) =>
            MessageBox.Show(text, "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public class MainForm : Form
    {
        private readonly HospitalService _service = new HospitalService();

        public MainForm()
        {
            Text = "ИС Больница";
            Size = new Size(400, 300);
            StartPosition = FormStartPosition.CenterScreen;

            var btnAdmit = new Button { Text = "1. Госпитализация пациента", Dock = DockStyle.Top, Height = 50 };
            var btnDoctor = new Button { Text = "2. Кабинет врача (ЭМК)", Dock = DockStyle.Top, Height = 50 };
            var btnNurse = new Button { Text = "3. Кабинет медсестры", Dock = DockStyle.Top, Height = 50 };
            var btnDischarge = new Button { Text = "4. Выписка (Эпикриз)", Dock = DockStyle.Top, Height = 50 };

            btnAdmit.Click += (s, e) => Open(new AdmissionForm(_service));
            btnDoctor.Click += (s, e) => Open(new DoctorForm(_service));
            btnNurse.Click += (s, e) => Open(new NurseForm(_service));
            btnDischarge.Click += (s, e) => Open(new DischargeForm(_service));

            Controls.Add(btnDischarge);
            Controls.Add(btnNurse);
            Controls.Add(btnDoctor);
            Controls.Add(btnAdmit);
        }

        private void Open(Form form)
        {
            using (form) form.ShowDialog(this);
        }
    }

    public class AdmissionForm : Form
    {
        private readonly HospitalService _service;
        private readonly ComboBox cmbDuty = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly ComboBox cmbDept = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly ComboBox cmbDoctor = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly TextBox txtName = new TextBox { Dock = DockStyle.Fill };
        private readonly TextBox txtAge = new TextBox { Dock = DockStyle.Fill };
        private readonly TextBox txtDiagnosis = new TextBox { Dock = DockStyle.Fill };

        public AdmissionForm(HospitalService service)
        {
            _service = service;
            Text = "Госпитализация";
            Size = new Size(480, 370);
            StartPosition = FormStartPosition.CenterParent;

            var layout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 7,
                Padding = new Padding(10)
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
            for (int i = 0; i < 6; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));

            AddRow(layout, 0, "Дежурный врач:", cmbDuty);
            AddRow(layout, 1, "ФИО пациента:", txtName);
            AddRow(layout, 2, "Возраст:", txtAge);
            AddRow(layout, 3, "Диагноз (первичный осмотр):", txtDiagnosis);
            AddRow(layout, 4, "Отделение:", cmbDept);
            AddRow(layout, 5, "Лечащий врач:", cmbDoctor);

            var btn = new Button { Text = "Госпитализировать", Dock = DockStyle.Fill };
            btn.Click += BtnSave_Click;
            layout.Controls.Add(btn, 0, 6);
            layout.SetColumnSpan(btn, 2);
            Controls.Add(layout);
            AcceptButton = btn;

            cmbDuty.DataSource = _service.GetDutyDoctors();
            cmbDept.DataSource = _service.GetDepartments();
            cmbDept.SelectedIndexChanged += (s, e) => LoadDoctors();
            LoadDoctors();
        }

        private static void AddRow(TableLayoutPanel layout, int row, string caption, Control input)
        {
            layout.Controls.Add(new Label { Text = caption, Anchor = AnchorStyles.Left, AutoSize = true }, 0, row);
            layout.Controls.Add(input, 1, row);
        }

        private void LoadDoctors()
        {
            cmbDoctor.DataSource = cmbDept.SelectedItem is Department d
                ? _service.GetDoctors(d.Id)
                : new List<Doctor>();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!int.TryParse(txtAge.Text, out int age))
            {
                Ui.Info("Неверный возраст");
                return;
            }

            var duty = cmbDuty.SelectedItem as Doctor;
            var dept = cmbDept.SelectedItem as Department;
            var doctor = cmbDoctor.SelectedItem as Doctor;

            var result = _service.AdmitPatient(txtName.Text, age,
                duty?.Id ?? Guid.Empty, dept?.Id ?? Guid.Empty, doctor?.Id ?? Guid.Empty,
                txtDiagnosis.Text);

            Ui.Show(result);
            if (result.Success) Close();
        }
    }

    public class PrescriptionDialog : Form
    {
        private readonly HospitalService _service;
        private readonly Guid _patientId;

        private readonly ComboBox cmbKind = Ui.Combo(250);

        private readonly Panel pnlMed = new Panel { Dock = DockStyle.Top, Height = 170 };
        private readonly ComboBox cmbForm = Ui.Combo(250);
        private readonly TextBox txtDrug = new TextBox { Width = 250 };
        private readonly NumericUpDown nudQty = new NumericUpDown { Width = 100, Minimum = 0, Maximum = 1000, Value = 1 };
        private readonly NumericUpDown nudDosage = new NumericUpDown { Width = 100, Minimum = 0, Maximum = 100000, DecimalPlaces = 1, Increment = 0.5M };
        private readonly NumericUpDown nudDays = new NumericUpDown { Width = 100, Minimum = 0, Maximum = 365 };

        private readonly Panel pnlDiag = new Panel { Dock = DockStyle.Top, Height = 170 };
        private readonly ComboBox cmbDiag = Ui.Combo(250);
        private readonly TextBox txtDiagComment = new TextBox { Width = 250 };

        private readonly Panel pnlPrev = new Panel { Dock = DockStyle.Top, Height = 170 };
        private readonly ComboBox cmbPrev = Ui.Combo(250);
        private readonly TextBox txtPrevComment = new TextBox { Width = 250 };

        public PrescriptionDialog(HospitalService service, Guid patientId)
        {
            _service = service;
            _patientId = patientId;

            Text = "Новое назначение";
            Size = new Size(440, 330);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            var pnlKind = new Panel { Dock = DockStyle.Top, Height = 45 };
            Place(pnlKind, "Тип назначения:", cmbKind, 0);

            Place(pnlMed, "Форма:", cmbForm, 0);
            Place(pnlMed, "Препарат:", txtDrug, 1);
            Place(pnlMed, "Количество (инъекции):", nudQty, 2);
            Place(pnlMed, "Дозировка (таблетки):", nudDosage, 3);
            Place(pnlMed, "Дней приёма (таблетки):", nudDays, 4);

            Place(pnlDiag, "Вид диагностики:", cmbDiag, 0);
            Place(pnlDiag, "Комментарий:", txtDiagComment, 1);

            Place(pnlPrev, "Вид процедуры:", cmbPrev, 0);
            Place(pnlPrev, "Комментарий:", txtPrevComment, 1);

            var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 50 };
            var btnOk = new Button { Text = "Назначить", Location = new Point(210, 10), Width = 100 };
            var btnCancel = new Button { Text = "Отмена", Location = new Point(320, 10), Width = 90, DialogResult = DialogResult.Cancel };
            btnOk.Click += BtnOk_Click;
            pnlBottom.Controls.Add(btnOk);
            pnlBottom.Controls.Add(btnCancel);
            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Controls.Add(pnlBottom);
            Controls.Add(pnlPrev);
            Controls.Add(pnlDiag);
            Controls.Add(pnlMed);
            Controls.Add(pnlKind);

            cmbKind.DataSource = Ui.Items(
                (PrescriptionType[])Enum.GetValues(typeof(PrescriptionType)), Names.Of);
            cmbForm.DataSource = Ui.Items(
                (MedicationForm[])Enum.GetValues(typeof(MedicationForm)), Names.Of);
            cmbDiag.DataSource = Ui.Items(
                (DiagnosticType[])Enum.GetValues(typeof(DiagnosticType)), Names.Of);
            cmbPrev.DataSource = Ui.Items(
                (PreventiveType[])Enum.GetValues(typeof(PreventiveType)), Names.Of);

            cmbKind.SelectedIndexChanged += (s, e) => UpdateVisibility();
            cmbForm.SelectedIndexChanged += (s, e) => UpdateVisibility();
            UpdateVisibility();
        }

        private static void Place(Panel panel, string caption, Control input, int row)
        {
            int y = 10 + row * 30;
            panel.Controls.Add(new Label { Text = caption, AutoSize = true, Location = new Point(10, y + 3) });
            input.Location = new Point(170, y);
            panel.Controls.Add(input);
        }

        private PrescriptionType CurrentKind =>
            (cmbKind.SelectedItem as Item<PrescriptionType>)?.Value ?? PrescriptionType.Medication;

        private MedicationForm CurrentForm =>
            (cmbForm.SelectedItem as Item<MedicationForm>)?.Value ?? MedicationForm.Injection;

        private void UpdateVisibility()
        {
            pnlMed.Visible = CurrentKind == PrescriptionType.Medication;
            pnlDiag.Visible = CurrentKind == PrescriptionType.Diagnostic;
            pnlPrev.Visible = CurrentKind == PrescriptionType.Preventive;

            bool injection = CurrentForm == MedicationForm.Injection;
            nudQty.Enabled = injection;
            nudDosage.Enabled = !injection;
            nudDays.Enabled = !injection;
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            OperationResult result;
            switch (CurrentKind)
            {
                case PrescriptionType.Medication:
                    result = CurrentForm == MedicationForm.Injection
                        ? _service.AddInjection(_patientId, txtDrug.Text, (int)nudQty.Value)
                        : _service.AddPills(_patientId, txtDrug.Text, (double)nudDosage.Value, (int)nudDays.Value);
                    break;
                case PrescriptionType.Diagnostic:
                    result = _service.AddDiagnostic(_patientId,
                        ((Item<DiagnosticType>)cmbDiag.SelectedItem).Value, txtDiagComment.Text);
                    break;
                default:
                    result = _service.AddPreventive(_patientId,
                        ((Item<PreventiveType>)cmbPrev.SelectedItem).Value, txtPrevComment.Text);
                    break;
            }

            Ui.Show(result);
            if (result.Success) DialogResult = DialogResult.OK;
        }
    }

    public class DoctorForm : Form
    {
        private readonly HospitalService _service;
        private readonly ComboBox cmbDoctor = Ui.Combo();
        private readonly ComboBox cmbPatient = Ui.Combo(260);
        private readonly ListBox lstPrescriptions = new ListBox { Dock = DockStyle.Fill, HorizontalScrollbar = true };

        public DoctorForm(HospitalService service)
        {
            _service = service;
            Text = "Кабинет врача (ЭМК)";
            Size = new Size(900, 450);
            StartPosition = FormStartPosition.CenterParent;

            var btnAdd = new Button { Text = "Добавить назначение", AutoSize = true };
            var btnFulfill = new Button { Text = "Отметить выполненным", AutoSize = true };
            btnAdd.Click += BtnAdd_Click;
            btnFulfill.Click += BtnFulfill_Click;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            top.Controls.AddRange(new Control[]
            {
                Ui.Caption("Врач:"), cmbDoctor, Ui.Caption("Пациент:"), cmbPatient, btnAdd, btnFulfill
            });

            Controls.Add(lstPrescriptions);
            Controls.Add(top);

            cmbDoctor.DataSource = _service.GetDoctors();
            cmbDoctor.SelectedIndexChanged += (s, e) => LoadPatients();
            cmbPatient.SelectedIndexChanged += (s, e) => RefreshList();
            LoadPatients();
        }

        private void LoadPatients()
        {
            cmbPatient.DataSource = cmbDoctor.SelectedItem is Doctor d
                ? _service.GetActivePatientsForDoctor(d.Id)
                : new List<Patient>();
            RefreshList();
        }

        private void RefreshList()
        {
            lstPrescriptions.Items.Clear();
            if (cmbPatient.SelectedItem is Patient p)
                foreach (var presc in _service.GetMedicalRecord(p.Id).Prescriptions)
                    lstPrescriptions.Items.Add(presc);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!(cmbPatient.SelectedItem is Patient p)) { Ui.Info("Выберите пациента."); return; }
            using (var dlg = new PrescriptionDialog(_service, p.Id))
                if (dlg.ShowDialog(this) == DialogResult.OK) RefreshList();
        }

        private void BtnFulfill_Click(object sender, EventArgs e)
        {
            if (!(cmbDoctor.SelectedItem is Doctor d) || !(cmbPatient.SelectedItem is Patient p))
            {
                Ui.Info("Выберите врача и пациента.");
                return;
            }
            if (!(lstPrescriptions.SelectedItem is Prescription presc))
            {
                Ui.Info("Выберите назначение в списке.");
                return;
            }

            Ui.Show(_service.FulfillPrescription(p.Id, presc.Id, ExecutorRole.Doctor, d.Id));
            RefreshList();
        }
    }

    public class NurseForm : Form
    {
        private readonly HospitalService _service;
        private readonly ComboBox cmbNurse = Ui.Combo();
        private readonly ComboBox cmbPatient = Ui.Combo(260);
        private readonly ListBox lstPrescriptions = new ListBox { Dock = DockStyle.Fill, HorizontalScrollbar = true };

        public NurseForm(HospitalService service)
        {
            _service = service;
            Text = "Кабинет медсестры";
            Size = new Size(800, 420);
            StartPosition = FormStartPosition.CenterParent;

            var btnFulfill = new Button { Text = "Отметить выполненным", AutoSize = true };
            btnFulfill.Click += BtnFulfill_Click;

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            top.Controls.AddRange(new Control[]
            {
                Ui.Caption("Медсестра:"), cmbNurse, Ui.Caption("Пациент:"), cmbPatient, btnFulfill
            });

            Controls.Add(lstPrescriptions);
            Controls.Add(top);

            cmbNurse.DataSource = _service.GetNurses();
            cmbNurse.SelectedIndexChanged += (s, e) => LoadPatients();
            cmbPatient.SelectedIndexChanged += (s, e) => RefreshList();
            LoadPatients();
        }

        private void LoadPatients()
        {
            cmbPatient.DataSource = cmbNurse.SelectedItem is Nurse n
                ? _service.GetActivePatientsInDepartment(n.DepartmentId)
                : new List<Patient>();
            RefreshList();
        }

        private void RefreshList()
        {
            lstPrescriptions.Items.Clear();
            if (cmbPatient.SelectedItem is Patient p)
                foreach (var presc in _service.GetPendingPrescriptions(p.Id, ExecutorRole.Nurse))
                    lstPrescriptions.Items.Add(presc);
        }

        private void BtnFulfill_Click(object sender, EventArgs e)
        {
            if (!(cmbNurse.SelectedItem is Nurse n) || !(cmbPatient.SelectedItem is Patient p))
            {
                Ui.Info("Выберите медсестру и пациента.");
                return;
            }
            if (!(lstPrescriptions.SelectedItem is Prescription presc))
            {
                Ui.Info("Выберите назначение в списке.");
                return;
            }

            Ui.Show(_service.FulfillPrescription(p.Id, presc.Id, ExecutorRole.Nurse, n.Id));
            RefreshList();
        }
    }

    public class DischargeForm : Form
    {
        private readonly HospitalService _service;
        private readonly ComboBox cmbPatient = Ui.Combo(380);
        private readonly TextBox txtSummary = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical
        };

        public DischargeForm(HospitalService service)
        {
            _service = service;
            Text = "Выписка пациента";
            Size = new Size(760, 480);
            StartPosition = FormStartPosition.CenterParent;

            var btnDischarge = new Button { Text = "Выписать пациента", AutoSize = true };
            var btnSummary = new Button { Text = "Показать эпикриз", AutoSize = true };
            btnDischarge.Click += BtnDischarge_Click;
            btnSummary.Click += (s, e) => ShowSummary();

            var top = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8) };
            top.Controls.AddRange(new Control[] { Ui.Caption("Пациент:"), cmbPatient, btnDischarge, btnSummary });

            Controls.Add(txtSummary);
            Controls.Add(top);

            ReloadPatients(null);
        }

        private void ReloadPatients(Guid? keep)
        {
            var list = _service.GetAllPatients();
            cmbPatient.DataSource = list;
            if (keep != null)
            {
                int i = list.FindIndex(p => p.Id == keep.Value);
                if (i >= 0) cmbPatient.SelectedIndex = i;
            }
        }

        private void BtnDischarge_Click(object sender, EventArgs e)
        {
            if (!(cmbPatient.SelectedItem is Patient p)) return;
            if (p.DischargeDate != null)
            {
                Ui.Info("Пациент уже выписан. Нажмите «Показать эпикриз».");
                return;
            }

            int pending = _service.CountPendingPrescriptions(p.Id);
            if (pending > 0)
            {
                var answer = MessageBox.Show(
                    $"У пациента остались невыполненные назначения: {pending}.\nВсё равно выписать?",
                    "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (answer != DialogResult.Yes) return;
            }

            var result = _service.DischargePatient(p.Id);
            Ui.Show(result);
            if (!result.Success) return;

            ReloadPatients(p.Id);
            ShowSummary();
        }

        private void ShowSummary()
        {
            if (!(cmbPatient.SelectedItem is Patient p)) return;

            var summary = _service.GetDischargeSummary(p.Id);
            if (summary == null)
            {
                Ui.Info("Пациент ещё не выписан — эпикриз формируется после выписки.");
                return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("=== ВЫПИСНОЙ ЭПИКРИЗ ===").AppendLine();
            sb.AppendLine($"Пациент: {summary.Patient.FullName}, возраст: {summary.Patient.Age}");
            sb.AppendLine($"Отделение: {summary.DepartmentName}");
            sb.AppendLine($"Лечащий врач: {summary.DoctorName}");
            sb.AppendLine($"Период пребывания: {summary.AdmissionDate:dd.MM.yyyy} - {summary.DischargeDate:dd.MM.yyyy}");
            sb.AppendLine($"Диагноз при поступлении: {summary.Diagnosis}").AppendLine();
            sb.AppendLine("Выполненные назначения:");

            if (summary.FulfilledPrescriptions.Count == 0)
                sb.AppendLine("  (нет)");
            foreach (var presc in summary.FulfilledPrescriptions)
                sb.AppendLine($"  - {presc.Summary} | {presc.FulfilledDate:dd.MM.yyyy HH:mm}, {presc.FulfilledByName}");

            txtSummary.Text = sb.ToString();
        }
    }
}
