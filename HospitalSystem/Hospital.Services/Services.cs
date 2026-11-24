using System;
using System.Collections.Generic;
using System.Linq;
using Hospital.Business;
using Hospital.Data;

namespace Hospital.Services
{
    public class OperationResult
    {
        public bool Success { get; private set; }
        public string Message { get; private set; }

        public static OperationResult Ok(string message) =>
            new OperationResult { Success = true, Message = message };

        public static OperationResult Fail(string message) =>
            new OperationResult { Success = false, Message = message };
    }

    public class HospitalService
    {
        private readonly IPatientDao _patientDao;
        private readonly IMedicalRecordDao _recordDao;
        private readonly IDepartmentDao _departmentDao;
        private readonly IStaffDao _staffDao;

        public HospitalService()
            : this(new PatientDao(), new MedicalRecordDao(), new DepartmentDao(), new StaffDao()) { }

        public HospitalService(IPatientDao patientDao, IMedicalRecordDao recordDao,
                               IDepartmentDao departmentDao, IStaffDao staffDao)
        {
            _patientDao = patientDao;
            _recordDao = recordDao;
            _departmentDao = departmentDao;
            _staffDao = staffDao;
        }

        public List<Department> GetDepartments() => _departmentDao.GetAll();

        public List<Doctor> GetDutyDoctors() =>
            _staffDao.GetDoctors().Where(d => d.IsDuty).ToList();

        public List<Doctor> GetDoctors(Guid? departmentId = null) =>
            _staffDao.GetDoctors()
                .Where(d => !d.IsDuty && (departmentId == null || d.DepartmentId == departmentId))
                .ToList();

        public List<Nurse> GetNurses() => _staffDao.GetNurses();

        public List<Patient> GetAllPatients() =>
            _patientDao.GetAll().OrderBy(p => p.DischargeDate != null).ThenBy(p => p.FullName).ToList();

        public List<Patient> GetActivePatientsForDoctor(Guid doctorId) =>
            _patientDao.GetAll()
                .Where(p => p.DischargeDate == null && p.AttendingDoctorId == doctorId).ToList();

        public List<Patient> GetActivePatientsInDepartment(Guid departmentId) =>
            _patientDao.GetAll()
                .Where(p => p.DischargeDate == null && p.DepartmentId == departmentId).ToList();

        public OperationResult AdmitPatient(string fullName, int age, Guid dutyDoctorId,
            Guid departmentId, Guid attendingDoctorId, string diagnosis)
        {
            return Execute(() =>
            {
                var doctors = _staffDao.GetDoctors();
                var duty = doctors.FirstOrDefault(d => d.Id == dutyDoctorId && d.IsDuty);
                if (duty == null) return OperationResult.Fail("Выберите дежурного врача.");

                var dept = _departmentDao.GetAll().FirstOrDefault(d => d.Id == departmentId);
                if (dept == null) return OperationResult.Fail("Выберите отделение.");

                var attending = doctors.FirstOrDefault(d =>
                    d.Id == attendingDoctorId && !d.IsDuty && d.DepartmentId == departmentId);
                if (attending == null)
                    return OperationResult.Fail("Лечащий врач должен работать в выбранном отделении.");

                var patient = new Patient
                {
                    FullName = fullName,
                    Age = age,
                    AdmissionDiagnosis = diagnosis,
                    DepartmentId = departmentId,
                    AttendingDoctorId = attendingDoctorId
                };
                if (!patient.Validate()) return OperationResult.Fail(patient.ErrorText);

                _recordDao.Save(new MedicalRecord
                {
                    PatientId = patient.Id,
                    DutyDoctorId = duty.Id,
                    DoctorId = attending.Id
                });

                var patients = _patientDao.GetAll();
                patients.Add(patient);
                _patientDao.Save(patients);

                return OperationResult.Ok(
                    $"Пациент госпитализирован в отделение «{dept.Name}». Лечащий врач: {attending.FullName}. ЭМК создана.");
            });
        }

        public OperationResult AddInjection(Guid patientId, string drugName, int? quantity) =>
            AddPrescription(patientId, new MedicationPrescription
            {
                Form = MedicationForm.Injection,
                Description = drugName,
                Quantity = quantity
            });

        public OperationResult AddPills(Guid patientId, string drugName, double? dosage, int? daysCount) =>
            AddPrescription(patientId, new MedicationPrescription
            {
                Form = MedicationForm.Pill,
                Description = drugName,
                Dosage = dosage,
                DaysCount = daysCount
            });

        public OperationResult AddDiagnostic(Guid patientId, DiagnosticType type, string comment) =>
            AddPrescription(patientId, new DiagnosticPrescription { DiagType = type, Description = comment });

        public OperationResult AddPreventive(Guid patientId, PreventiveType type, string comment) =>
            AddPrescription(patientId, new PreventivePrescription { PrevType = type, Description = comment });

        private OperationResult AddPrescription(Guid patientId, Prescription prescription)
        {
            return Execute(() =>
            {
                if (FindActivePatient(patientId) == null)
                    return OperationResult.Fail("Пациент не найден или уже выписан.");
                if (!prescription.Validate())
                    return OperationResult.Fail(prescription.ErrorText);

                var record = LoadRecord(patientId);
                record.Prescriptions.Add(prescription);
                _recordDao.Save(record);
                return OperationResult.Ok("Назначение добавлено в ЭМК.");
            });
        }

        public MedicalRecord GetMedicalRecord(Guid patientId) => LoadRecord(patientId);

        public List<Prescription> GetPendingPrescriptions(Guid patientId, ExecutorRole role) =>
            LoadRecord(patientId).Prescriptions
                .Where(p => !p.IsFulfilled && p.Executor == role).ToList();

        public int CountPendingPrescriptions(Guid patientId) =>
            LoadRecord(patientId).Prescriptions.Count(p => !p.IsFulfilled);

        public OperationResult FulfillPrescription(Guid patientId, Guid prescriptionId,
            ExecutorRole role, Guid executorId)
        {
            return Execute(() =>
            {
                var patient = FindActivePatient(patientId);
                if (patient == null) return OperationResult.Fail("Пациент не найден или уже выписан.");

                string executorName;
                if (role == ExecutorRole.Nurse)
                {
                    var nurse = _staffDao.GetNurses().FirstOrDefault(n => n.Id == executorId);
                    if (nurse == null) return OperationResult.Fail("Выберите медсестру.");
                    if (nurse.DepartmentId != patient.DepartmentId)
                        return OperationResult.Fail("Медсестра работает в другом отделении.");
                    executorName = nurse.FullName;
                }
                else
                {
                    var doctor = _staffDao.GetDoctors().FirstOrDefault(d => d.Id == executorId && !d.IsDuty);
                    if (doctor == null) return OperationResult.Fail("Выберите врача.");
                    if (doctor.DepartmentId != patient.DepartmentId)
                        return OperationResult.Fail("Врач работает в другом отделении.");
                    executorName = doctor.FullName;
                }

                var record = LoadRecord(patientId);
                var prescription = record.Prescriptions.FirstOrDefault(p => p.Id == prescriptionId);
                if (prescription == null) return OperationResult.Fail("Назначение не найдено.");
                if (prescription.IsFulfilled) return OperationResult.Fail("Назначение уже выполнено.");
                if (prescription.Executor != role)
                    return OperationResult.Fail(
                        $"Это назначение выполняет: {Names.Of(prescription.Executor).ToLower()}.");

                prescription.Fulfill(executorId, executorName);
                _recordDao.Save(record);
                return OperationResult.Ok("Назначение отмечено как выполненное и внесено в ЭМК.");
            });
        }

        public OperationResult DischargePatient(Guid patientId)
        {
            return Execute(() =>
            {
                var patients = _patientDao.GetAll();
                var patient = patients.FirstOrDefault(p => p.Id == patientId && p.DischargeDate == null);
                if (patient == null) return OperationResult.Fail("Пациент не найден или уже выписан.");

                patient.DischargeDate = DateTime.Now;
                _patientDao.Save(patients);
                return OperationResult.Ok("Пациент выписан.");
            });
        }

        public DischargeSummary GetDischargeSummary(Guid patientId)
        {
            var patient = _patientDao.GetAll().FirstOrDefault(p => p.Id == patientId);
            if (patient == null || patient.DischargeDate == null) return null;

            var record = LoadRecord(patientId);
            var dept = _departmentDao.GetAll().FirstOrDefault(d => d.Id == patient.DepartmentId);
            var doctor = _staffDao.GetDoctors().FirstOrDefault(d => d.Id == patient.AttendingDoctorId);

            return new DischargeSummary
            {
                Patient = patient,
                DepartmentName = dept?.Name ?? "Неизвестно",
                DoctorName = doctor?.FullName ?? "Неизвестно",
                AdmissionDate = patient.AdmissionDate,
                DischargeDate = patient.DischargeDate.Value,
                Diagnosis = patient.AdmissionDiagnosis,
                FulfilledPrescriptions = record.Prescriptions
                    .Where(p => p.IsFulfilled).OrderBy(p => p.FulfilledDate).ToList()
            };
        }

        private Patient FindActivePatient(Guid id) =>
            _patientDao.GetAll().FirstOrDefault(p => p.Id == id && p.DischargeDate == null);

        private MedicalRecord LoadRecord(Guid patientId) =>
            _recordDao.GetByPatientId(patientId) ?? new MedicalRecord { PatientId = patientId };

        private static OperationResult Execute(Func<OperationResult> action)
        {
            try { return action(); }
            catch (DataAccessException ex) { return OperationResult.Fail(ex.Message); }
        }
    }
}
