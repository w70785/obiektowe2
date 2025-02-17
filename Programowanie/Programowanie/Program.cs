using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static void Main()
    {
        EmployeeManager manager = new EmployeeManager();
        manager.LoadData();

        while (true)
        {
            Console.WriteLine("\nSystem Zarządzania Ewidencją Godzinową");
            Console.WriteLine("1. Rejestrowanie wejścia/wyjścia");
            Console.WriteLine("2. Wpisanie urlopu");
            Console.WriteLine("3. Wpisanie zwolnienia lekarskiego");
            Console.WriteLine("4. Liczba spóźnień");
            Console.WriteLine("5. Nadgodziny");
            Console.WriteLine("6. Wyjście");
            Console.Write("Wybierz opcję: ");

            string choice = Console.ReadLine();
            switch (choice)
            {
                case "1":
                    manager.RecordAttendance();
                    break;
                case "2":
                    manager.RecordLeave();
                    break;
                case "3":
                    manager.RecordSickLeave();
                    break;
                case "4":
                    manager.RecordLateEntries();
                    break;
                case "5":
                    manager.CalculateOvertime();
                    break;
                case "6":
                    manager.SaveData();
                    return;
                default:
                    Console.WriteLine("Niepoprawny wybór.");
                    break;
            }
        }
    }
}

class Employee
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<Attendance> Attendances { get; set; } = new List<Attendance>();
    public List<Leave> Leaves { get; set; } = new List<Leave>();
    public List<SickLeave> SickLeaves { get; set; } = new List<SickLeave>();
}

class Attendance
{
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
}

class Leave
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

class SickLeave : Leave { }

class EmployeeManager
{
    private List<Employee> employees = new List<Employee>();
    private const string EmployeesFile = "pracownicy.txt";
    private const string LateFile = "spoznienia.txt";
    private const string OvertimeFile = "nadgodziny.txt";
    private const string SickLeaveFile = "zwolnienia.txt";
    private const string LeaveFile = "urlopy.txt";

    public void LoadData()
    {
        if (File.Exists(EmployeesFile))
        {
            var lines = File.ReadAllLines(EmployeesFile);
            foreach (var line in lines)
            {
                var data = line.Split(';');
                if (data.Length == 2)
                {
                    employees.Add(new Employee { Id = data[0].Trim(), Name = data[1].Trim() });
                }
            }
        }
    }

    public void SaveData()
    {
        var lines = employees.Select(e => $"{e.Id};{e.Name}").ToList();
        File.WriteAllLines(EmployeesFile, lines);
    }

    private void AppendToFile(string filePath, string content)
    {
        File.AppendAllText(filePath, content + Environment.NewLine);
    }

    public void RecordAttendance()
    {
        Console.Write("Podaj ID pracownika: ");
        string id = Console.ReadLine().Trim();
        var employee = employees.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (employee != null)
        {
            Console.Write("Podaj godzinę wejścia (HH:mm): ");
            DateTime entryTime = DateTime.Parse(Console.ReadLine());

            Console.Write("Czy podać godzinę wyjścia? (T/N): ");
            string response = Console.ReadLine().Trim().ToUpper();
            DateTime? exitTime = null;

            if (response == "T")
            {
                Console.Write("Podaj godzinę wyjścia (HH:mm): ");
                exitTime = DateTime.Parse(Console.ReadLine());
            }

            employee.Attendances.Add(new Attendance { EntryTime = entryTime, ExitTime = exitTime });

            string attendanceFile = $"attendance_{id}.txt";
            AppendToFile(attendanceFile, $"{entryTime};{(exitTime.HasValue ? exitTime.Value.ToString() : "null")}");

            Console.WriteLine("Zapisano obecność.");
        }
        else
        {
            Console.WriteLine("Pracownik nie znaleziony.");
        }
    }

    public void RecordLateEntries()
    {
        Console.Write("Podaj ID pracownika: ");
        string id = Console.ReadLine().Trim();
        var employee = employees.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (employee != null)
        {
            int lateCount = employee.Attendances.Count(a => a.EntryTime.Hour > 9);
            AppendToFile(LateFile, $"{id};{lateCount}");
            Console.WriteLine($"Liczba spóźnień: {lateCount}");
        }
        else
        {
            Console.WriteLine("Pracownik nie znaleziony.");
        }
    }

    public void CalculateOvertime()
    {
        Console.Write("Podaj ID pracownika: ");
        string id = Console.ReadLine().Trim();
        var employee = employees.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (employee != null)
        {
            int overtime = employee.Attendances.Sum(a => (a.ExitTime.HasValue && a.ExitTime.Value.Hour > 17) ? (a.ExitTime.Value.Hour - 17) : 0);
            AppendToFile(OvertimeFile, $"{id};{overtime}");
            Console.WriteLine($"Łączna liczba nadgodzin: {overtime}");
        }
        else
        {
            Console.WriteLine("Pracownik nie znaleziony.");
        }
    }

    public void RecordLeave()
    {
        Console.Write("Podaj ID pracownika: ");
        string id = Console.ReadLine().Trim();
        var employee = employees.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (employee != null)
        {
            Console.Write("Podaj datę rozpoczęcia urlopu (yyyy-MM-dd): ");
            DateTime start = DateTime.Parse(Console.ReadLine());
            Console.Write("Podaj datę zakończenia urlopu (yyyy-MM-dd): ");
            DateTime end = DateTime.Parse(Console.ReadLine());
            employee.Leaves.Add(new Leave { StartDate = start, EndDate = end });
            AppendToFile(LeaveFile, $"{id};{start:yyyy-MM-dd};{end:yyyy-MM-dd}");
            Console.WriteLine("Zapisano urlop.");
        }
        else
        {
            Console.WriteLine("Pracownik nie znaleziony.");
        }
    }

    public void RecordSickLeave()
    {
        Console.Write("Podaj ID pracownika: ");
        string id = Console.ReadLine().Trim();
        var employee = employees.FirstOrDefault(e => e.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
        if (employee != null)
        {
            Console.Write("Podaj datę rozpoczęcia zwolnienia (yyyy-MM-dd): ");
            DateTime start = DateTime.Parse(Console.ReadLine());
            Console.Write("Podaj datę zakończenia zwolnienia (yyyy-MM-dd): ");
            DateTime end = DateTime.Parse(Console.ReadLine());
            employee.SickLeaves.Add(new SickLeave { StartDate = start, EndDate = end });
            AppendToFile(SickLeaveFile, $"{id};{start:yyyy-MM-dd};{end:yyyy-MM-dd}");
            Console.WriteLine("Zapisano zwolnienie lekarskie.");
        }
        else
        {
            Console.WriteLine("Pracownik nie znaleziony.");
        }
    }
}
