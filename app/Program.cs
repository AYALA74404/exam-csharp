using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

class Program
{
    private const int FieldCount = 6;
    private const int StudentFreeDays = 14;
    private const int StudentFineLimit = 40;
    private const int GuestFreeDays = 7;
    private const int AlumnaFreeDays = 10;
    private const int AlumnaFineLimit = 25;
    private const int TeacherFlagDays = 60;

    private sealed class InputRow
    {
        public string Original { get; }
        public string[] Fields { get; }

        public InputRow(string original)
        {
            Original = original;
            Fields = original.Split(',');
            for (int i = 0; i < Fields.Length; i++)
                Fields[i] = Fields[i].Trim();
        }
    }

    private sealed class ReaderReport
    {
        public string Id { get; }
        public string Name { get; }
        public int Fine { get; set; }
        public int PositiveFineLoans { get; set; }

        public ReaderReport(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    private sealed class ReportData
    {
        public Dictionary<string, ReaderReport> Readers { get; } = new Dictionary<string, ReaderReport>();
        public List<string[]> Flagged { get; } = new List<string[]>();
        public List<string> Errors { get; } = new List<string>();
        public Dictionary<string, int> BookCounts { get; } = new Dictionary<string, int>();
        public int TotalFine { get; set; }
    }

    static void Main(string[] args)
    {
        Console.OutputEncoding = new UTF8Encoding(false);
        string path = args.Length > 0 ? args[0] : FindSample();
        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        int reportDay = ReadReportDay(lines);
        List<InputRow> rows = ReadRows(lines);

        Console.Error.WriteLine("skeleton ok: read " + Path.GetFileName(path) +
            " - report day " + reportDay + " - " + rows.Count + " rows");

        ReportData report = BuildReport(rows, reportDay);
        PrintReport(report);
    }

    private static ReportData BuildReport(List<InputRow> rows, int reportDay)
    {
        ReportData report = new ReportData();

        foreach (InputRow inputRow in rows)
        {
            if (!TryReadLoan(inputRow, reportDay, out string type, out int borrowedDays))
            {
                report.Errors.Add(inputRow.Original);
                continue;
            }

            string[] fields = inputRow.Fields;
            int fine = CalculateFine(type, borrowedDays);
            ReaderReport reader = GetOrCreateReader(report.Readers, fields[0], fields[1]);

            reader.Fine += fine;
            if (fine > 0)
                reader.PositiveFineLoans++;

            report.TotalFine += fine;
            AddBook(report.BookCounts, fields[3]);

            if (type == "teacher" && fields[5] == "" && borrowedDays > TeacherFlagDays)
                AddFlaggedReader(report.Flagged, fields[0], fields[1]);
        }

        return report;
    }

    private static bool TryReadLoan(InputRow row, int reportDay, out string type, out int borrowedDays)
    {
        type = "";
        borrowedDays = 0;
        string[] fields = row.Fields;

        if (fields.Length != FieldCount ||
            IsEmptyRequiredField(fields) ||
            !IsKnownType(fields[2]) ||
            !int.TryParse(fields[4], out int loanDay))
            return false;

        int endDay = reportDay;
        if (fields[5] != "" && !int.TryParse(fields[5], out endDay))
            return false;

        if (loanDay < 1 || loanDay > 365 || endDay < 1 || endDay > 365 || endDay < loanDay)
            return false;

        type = fields[2];
        borrowedDays = endDay - loanDay;
        return true;
    }

    private static bool IsEmptyRequiredField(string[] fields)
    {
        for (int i = 0; i < fields.Length; i++)
        {
            if (i != 5 && fields[i] == "")
                return true;
        }
        return false;
    }

    private static bool IsKnownType(string type)
    {
        return type == "student" || type == "teacher" || type == "guest" || type == "alumna";
    }

    private static int CalculateFine(string type, int borrowedDays)
    {
        if (type == "student")
        {
            if (borrowedDays < StudentFreeDays + 2)
                return 0;

            return Math.Min(borrowedDays - StudentFreeDays, StudentFineLimit);
        }

        if (type == "guest" && borrowedDays > GuestFreeDays)
            return (int)Math.Ceiling((borrowedDays - GuestFreeDays) * 2.5);

        if (type == "alumna" && borrowedDays > AlumnaFreeDays)
        {
            int fine = (int)Math.Ceiling((borrowedDays - AlumnaFreeDays) * 1.5);
            return Math.Min(fine, AlumnaFineLimit);
        }

        return 0;
    }

    private static ReaderReport GetOrCreateReader(
        Dictionary<string, ReaderReport> readers, string id, string name)
    {
        if (!readers.TryGetValue(id, out ReaderReport? reader))
        {
            reader = new ReaderReport(id, name);
            readers.Add(id, reader);
        }
        return reader;
    }

    private static void AddBook(Dictionary<string, int> books, string book)
    {
        books[book] = books.TryGetValue(book, out int count) ? count + 1 : 1;
    }

    private static void AddFlaggedReader(List<string[]> flagged, string id, string name)
    {
        if (!flagged.Any(reader => reader[0] == id))
            flagged.Add(new[] { id, name });
    }

    private static void PrintReport(ReportData report)
    {
        Console.WriteLine("FINES");
        foreach (ReaderReport reader in report.Readers.Values)
            Console.WriteLine($"{reader.Id},{reader.Name},{reader.Fine}");

        Console.WriteLine("FLAGGED");
        foreach (string[] reader in report.Flagged)
            Console.WriteLine($"{reader[0]},{reader[1]}");

        Console.WriteLine("ERRORS");
        foreach (string error in report.Errors)
            Console.WriteLine(error);

        Console.WriteLine($"TOTAL,{report.TotalFine}");

        Console.WriteLine("TOP_BOOKS");
        foreach (var book in report.BookCounts
                     .OrderByDescending(book => book.Value)
                     .ThenBy(book => book.Key)
                     .Take(3))
            Console.WriteLine($"{book.Key},{book.Value}");

        Console.WriteLine("AT_RISK");
        foreach (ReaderReport reader in report.Readers.Values
                     .Where(reader => reader.PositiveFineLoans > 2))
            Console.WriteLine($"{reader.Id},{reader.Name}");
    }

    private static int ReadReportDay(string[] lines)
    {
        return int.Parse(lines[0].Split(':')[1].Trim());
    }

    private static List<InputRow> ReadRows(string[] lines)
    {
        List<InputRow> rows = new List<InputRow>();
        for (int i = 2; i < lines.Length; i++)
        {
            if (lines[i].Trim() != "")
                rows.Add(new InputRow(lines[i]));
        }
        return rows;
    }

    private static string FindSample()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            string candidate = Path.Combine(directory.FullName, "data", "sample.txt");
            if (File.Exists(candidate))
                return candidate;
            directory = directory.Parent;
        }
        return Path.Combine("data", "sample.txt");
    }
}
