using Hospital.Business;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hospital.Data
{
    public class DataAccessException : Exception
    {
        public DataAccessException(string message, Exception inner = null) : base(message, inner) { }
    }

    public interface IPatientDao
    {
        List<Patient> GetAll();
        void Save(List<Patient> patients);
    }

    public interface IMedicalRecordDao
    {
        MedicalRecord GetByPatientId(Guid patientId);
        void Save(MedicalRecord record);
    }

    public interface IDepartmentDao
    {
        List<Department> GetAll();
    }

    public interface IStaffDao
    {
        List<Doctor> GetDoctors();
        List<Nurse> GetNurses();
    }

    internal static class JsonStore
    {
        public static readonly string Root = Path.Combine(AppContext.BaseDirectory, "data");

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public static T Read<T>(string path, Func<T> fallback)
        {
            try
            {
                if (!File.Exists(path)) return fallback();
                var json = File.ReadAllText(path);
                var value = JsonSerializer.Deserialize<T>(json, Options);
                return value != null ? value : fallback();
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException
                                        || ex is JsonException || ex is NotSupportedException)
            {
                throw new DataAccessException(
                    $"Не удалось прочитать файл «{Path.GetFileName(path)}»: {ex.Message}", ex);
            }
        }

        public static void Write<T>(string path, T value)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var json = JsonSerializer.Serialize(value, Options);
                var tmp = path + ".tmp";
                File.WriteAllText(tmp, json);
                File.Move(tmp, path, true);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException
                                        || ex is JsonException || ex is NotSupportedException)
            {
                throw new DataAccessException(
                    $"Не удалось записать файл «{Path.GetFileName(path)}»: {ex.Message}", ex);
            }
        }
    }

    internal static class SeedData
    {
        public static readonly Guid Cardiology = new Guid("11111111-0000-0000-0000-000000000001");
        public static readonly Guid Surgery = new Guid("11111111-0000-0000-0000-000000000002");
        public static readonly Guid Neurology = new Guid("11111111-0000-0000-0000-000000000003");
        public static readonly Guid Therapy = new Guid("11111111-0000-0000-0000-000000000004");

        public static List<Department> Departments() => new List<Department>
        {
            new Department { Id = Cardiology, Name = "Кардиология", Type = DepartmentType.Cardiology },
            new Department { Id = Surgery, Name = "Хирургия", Type = DepartmentType.Surgery },
            new Department { Id = Neurology, Name = "Неврология", Type = DepartmentType.Neurology },
            new Department { Id = Therapy, Name = "Терапия", Type = DepartmentType.Therapy }
        };

        public static StaffData Staff() => new StaffData
        {
            Doctors = new List<Doctor>
            {
                new Doctor { Id = new Guid("22222222-0000-0000-0000-000000000001"), FullName = "Сидоров С.С.", Age = 45, Specialty = "Дежурный врач", IsDuty = true },
                new Doctor { Id = new Guid("22222222-0000-0000-0000-000000000002"), FullName = "Иванов И.И.", Age = 50, Specialty = "Кардиолог", DepartmentId = Cardiology },
                new Doctor { Id = new Guid("22222222-0000-0000-0000-000000000003"), FullName = "Петров П.П.", Age = 48, Specialty = "Хирург", DepartmentId = Surgery },
                new Doctor { Id = new Guid("22222222-0000-0000-0000-000000000004"), FullName = "Смирнова А.В.", Age = 42, Specialty = "Невролог", DepartmentId = Neurology },
                new Doctor { Id = new Guid("22222222-0000-0000-0000-000000000005"), FullName = "Кузнецов Д.А.", Age = 39, Specialty = "Терапевт", DepartmentId = Therapy }
            },
            Nurses = new List<Nurse>
            {
                new Nurse { Id = new Guid("33333333-0000-0000-0000-000000000001"), FullName = "Орлова Е.М.", Age = 30, DepartmentId = Cardiology },
                new Nurse { Id = new Guid("33333333-0000-0000-0000-000000000002"), FullName = "Волкова Н.П.", Age = 33, DepartmentId = Surgery },
                new Nurse { Id = new Guid("33333333-0000-0000-0000-000000000003"), FullName = "Зайцева О.К.", Age = 28, DepartmentId = Neurology },
                new Nurse { Id = new Guid("33333333-0000-0000-0000-000000000004"), FullName = "Белова Т.С.", Age = 35, DepartmentId = Therapy }
            }
        };
    }

    public class StaffData
    {
        public List<Doctor> Doctors { get; set; } = new List<Doctor>();
        public List<Nurse> Nurses { get; set; } = new List<Nurse>();
    }

    public class PatientDao : IPatientDao
    {
        private readonly string _filePath = Path.Combine(JsonStore.Root, "patients.json");

        public List<Patient> GetAll() =>
            JsonStore.Read(_filePath, () => new List<Patient>());

        public void Save(List<Patient> patients) =>
            JsonStore.Write(_filePath, patients);
    }

    public class MedicalRecordDao : IMedicalRecordDao
    {
        private readonly string _folder = Path.Combine(JsonStore.Root, "records");

        public MedicalRecord GetByPatientId(Guid patientId) =>
            JsonStore.Read<MedicalRecord>(PathFor(patientId), () => null);

        public void Save(MedicalRecord record) =>
            JsonStore.Write(PathFor(record.PatientId), record);

        private string PathFor(Guid patientId) => Path.Combine(_folder, $"{patientId}.json");
    }

    public class DepartmentDao : IDepartmentDao
    {
        private readonly string _filePath = Path.Combine(JsonStore.Root, "departments.json");

        public List<Department> GetAll()
        {
            if (!File.Exists(_filePath))
                JsonStore.Write(_filePath, SeedData.Departments());
            return JsonStore.Read(_filePath, SeedData.Departments);
        }
    }

    public class StaffDao : IStaffDao
    {
        private readonly string _filePath = Path.Combine(JsonStore.Root, "staff.json");

        public List<Doctor> GetDoctors() => Load().Doctors;
        public List<Nurse> GetNurses() => Load().Nurses;

        private StaffData Load()
        {
            if (!File.Exists(_filePath))
                JsonStore.Write(_filePath, SeedData.Staff());
            return JsonStore.Read(_filePath, SeedData.Staff);
        }
    }
}
