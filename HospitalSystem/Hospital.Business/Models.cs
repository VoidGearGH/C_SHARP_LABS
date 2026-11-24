using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Hospital.Business
{
    public class BusinessRule
    {
        public string Property { get; }
        public string Message { get; }

        public BusinessRule(string property, string message)
        {
            Property = property;
            Message = message;
        }
    }

    public abstract class BusinessObject
    {
        private readonly List<BusinessRule> _brokenRules = new List<BusinessRule>();

        [JsonIgnore]
        public IReadOnlyList<BusinessRule> BrokenRules => _brokenRules;

        [JsonIgnore]
        public string ErrorText => string.Join(Environment.NewLine, _brokenRules.Select(r => r.Message));

        public bool Validate()
        {
            _brokenRules.Clear();
            ValidateRules();
            return _brokenRules.Count == 0;
        }

        protected void AddRule(string property, string message) =>
            _brokenRules.Add(new BusinessRule(property, message));

        protected abstract void ValidateRules();
    }

    public enum DepartmentType { Cardiology, Surgery, Neurology, Therapy }
    public enum PrescriptionType { Medication, Diagnostic, Preventive }
    public enum MedicationForm { Injection, Pill }
    public enum DiagnosticType { BloodTest, Ultrasound, CT, Fibrogastroscopy }
    public enum PreventiveType { Massage, Physiotherapy }
    public enum ExecutorRole { Doctor, Nurse }

    public static class Names
    {
        public static string Of(PrescriptionType v) => v switch
        {
            PrescriptionType.Medication => "Медикаментозное",
            PrescriptionType.Diagnostic => "Диагностическое",
            PrescriptionType.Preventive => "Профилактическое",
            _ => v.ToString()
        };

        public static string Of(MedicationForm v) => v switch
        {
            MedicationForm.Injection => "Инъекция",
            MedicationForm.Pill => "Таблетки",
            _ => v.ToString()
        };

        public static string Of(DiagnosticType v) => v switch
        {
            DiagnosticType.BloodTest => "Анализ крови",
            DiagnosticType.Ultrasound => "УЗИ",
            DiagnosticType.CT => "Компьютерная томография",
            DiagnosticType.Fibrogastroscopy => "Фиброгастроскопия",
            _ => v.ToString()
        };

        public static string Of(PreventiveType v) => v switch
        {
            PreventiveType.Massage => "Массаж",
            PreventiveType.Physiotherapy => "Лечебная физкультура",
            _ => v.ToString()
        };

        public static string Of(ExecutorRole v) => v switch
        {
            ExecutorRole.Doctor => "Врач",
            ExecutorRole.Nurse => "Медсестра",
            _ => v.ToString()
        };
    }

    public class Department
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DepartmentType Type { get; set; }

        public override string ToString() => Name;
    }

    public class Person : BusinessObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; }
        public int Age { get; set; }

        protected override void ValidateRules()
        {
            if (string.IsNullOrWhiteSpace(FullName))
                AddRule(nameof(FullName), "ФИО не может быть пустым.");
            if (Age < 0 || Age > 120)
                AddRule(nameof(Age), "Возраст должен быть от 0 до 120.");
        }

        public override string ToString() => FullName;
    }

    public class Patient : Person
    {
        public DateTime AdmissionDate { get; set; } = DateTime.Now;
        public DateTime? DischargeDate { get; set; }
        public string AdmissionDiagnosis { get; set; }
        public Guid DepartmentId { get; set; }
        public Guid AttendingDoctorId { get; set; }

        protected override void ValidateRules()
        {
            base.ValidateRules();
            if (string.IsNullOrWhiteSpace(AdmissionDiagnosis))
                AddRule(nameof(AdmissionDiagnosis), "Укажите диагноз при поступлении.");
            if (DepartmentId == Guid.Empty)
                AddRule(nameof(DepartmentId), "Выберите отделение.");
            if (AttendingDoctorId == Guid.Empty)
                AddRule(nameof(AttendingDoctorId), "Выберите лечащего врача.");
            if (DischargeDate != null && DischargeDate < AdmissionDate)
                AddRule(nameof(DischargeDate), "Дата выписки не может быть раньше даты поступления.");
        }

        public override string ToString() =>
            $"{FullName}, {Age} л. — {(DischargeDate == null ? "на лечении" : "выписан")}";
    }

    public class Doctor : Person
    {
        public string Specialty { get; set; }
        public bool IsDuty { get; set; }
        public Guid? DepartmentId { get; set; }

        protected override void ValidateRules()
        {
            base.ValidateRules();
            if (string.IsNullOrWhiteSpace(Specialty))
                AddRule(nameof(Specialty), "Укажите специальность врача.");
            if (!IsDuty && DepartmentId == null)
                AddRule(nameof(DepartmentId), "Врач отделения должен быть привязан к отделению.");
        }

        public override string ToString() => $"{FullName} ({Specialty})";
    }

    public class Nurse : Person
    {
        public Guid DepartmentId { get; set; }

        protected override void ValidateRules()
        {
            base.ValidateRules();
            if (DepartmentId == Guid.Empty)
                AddRule(nameof(DepartmentId), "Медсестра должна быть привязана к отделению.");
        }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
    [JsonDerivedType(typeof(MedicationPrescription), "medication")]
    [JsonDerivedType(typeof(DiagnosticPrescription), "diagnostic")]
    [JsonDerivedType(typeof(PreventivePrescription), "preventive")]
    public abstract class Prescription : BusinessObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsFulfilled { get; set; }
        public DateTime? FulfilledDate { get; set; }
        public Guid? FulfilledById { get; set; }
        public string FulfilledByName { get; set; }

        public abstract PrescriptionType Type { get; }
        public abstract ExecutorRole Executor { get; }
        public abstract string Summary { get; }

        public void Fulfill(Guid executorId, string executorName)
        {
            IsFulfilled = true;
            FulfilledDate = DateTime.Now;
            FulfilledById = executorId;
            FulfilledByName = executorName;
        }

        protected override void ValidateRules() => ValidatePrescription();
        protected abstract void ValidatePrescription();

        protected string CommentSuffix =>
            string.IsNullOrWhiteSpace(Description) ? "" : $" ({Description})";

        public override string ToString()
        {
            string status = IsFulfilled
                ? $"выполнено {FulfilledDate:dd.MM.yyyy HH:mm}, {FulfilledByName}"
                : $"ожидает ({Names.Of(Executor)})";
            return $"{Summary} — {status}";
        }
    }

    public class MedicationPrescription : Prescription
    {
        public MedicationForm Form { get; set; }
        public int? Quantity { get; set; }
        public double? Dosage { get; set; }
        public int? DaysCount { get; set; }

        public override PrescriptionType Type => PrescriptionType.Medication;
        public override ExecutorRole Executor => ExecutorRole.Nurse;

        public override string Summary => Form == MedicationForm.Injection
            ? $"Инъекция: {Description}, количество: {Quantity}"
            : $"Таблетки: {Description}, дозировка: {Dosage}, дней приёма: {DaysCount}";

        protected override void ValidatePrescription()
        {
            if (string.IsNullOrWhiteSpace(Description))
                AddRule(nameof(Description), "Укажите название препарата.");

            if (Form == MedicationForm.Injection)
            {
                if (Quantity == null || Quantity <= 0)
                    AddRule(nameof(Quantity), "Для инъекций укажите количество (больше 0).");
            }
            else
            {
                if (Dosage == null || Dosage <= 0)
                    AddRule(nameof(Dosage), "Для таблеток укажите дозировку (больше 0).");
                if (DaysCount == null || DaysCount <= 0)
                    AddRule(nameof(DaysCount), "Для таблеток укажите количество дней приёма (больше 0).");
            }
        }
    }

    public class DiagnosticPrescription : Prescription
    {
        public DiagnosticType DiagType { get; set; }

        public override PrescriptionType Type => PrescriptionType.Diagnostic;
        public override ExecutorRole Executor => ExecutorRole.Doctor;
        public override string Summary => $"Диагностика: {Names.Of(DiagType)}{CommentSuffix}";

        protected override void ValidatePrescription()
        {
            if (!Enum.IsDefined(typeof(DiagnosticType), DiagType))
                AddRule(nameof(DiagType), "Неизвестный вид диагностики.");
        }
    }

    public class PreventivePrescription : Prescription
    {
        public PreventiveType PrevType { get; set; }

        public override PrescriptionType Type => PrescriptionType.Preventive;
        public override ExecutorRole Executor => ExecutorRole.Doctor;
        public override string Summary => $"Профилактика: {Names.Of(PrevType)}{CommentSuffix}";

        protected override void ValidatePrescription()
        {
            if (!Enum.IsDefined(typeof(PreventiveType), PrevType))
                AddRule(nameof(PrevType), "Неизвестный вид профилактики.");
        }
    }

    public class MedicalRecord
    {
        public Guid PatientId { get; set; }
        public Guid DutyDoctorId { get; set; }
        public Guid DoctorId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public List<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }

    public class DischargeSummary
    {
        public Patient Patient { get; set; }
        public string DepartmentName { get; set; }
        public string DoctorName { get; set; }
        public DateTime AdmissionDate { get; set; }
        public DateTime DischargeDate { get; set; }
        public string Diagnosis { get; set; }
        public List<Prescription> FulfilledPrescriptions { get; set; }
    }
}
