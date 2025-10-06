using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Tone_Row_Matrix
{
    public class Note
    {
        private static readonly string[] ChromaticScale = 
        {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };

        private static readonly Dictionary<string, int> NoteToNumber = new()
        {
            {"C", 0}, {"C#", 1}, {"Db", 1},
            {"D", 2}, {"D#", 3}, {"Eb", 3},
            {"E", 4},
            {"F", 5}, {"F#", 6}, {"Gb", 6},
            {"G", 7}, {"G#", 8}, {"Ab", 8},
            {"A", 9}, {"A#", 10}, {"Bb", 10},
            {"B", 11}
        };

        public int Value { get; }
        public string Name { get; }

        public Note(string noteName)
        {
            string normalizedName = NormalizeNoteName(noteName);
            if (!NoteToNumber.ContainsKey(normalizedName))
                throw new ArgumentException($"Invalid note name: {noteName}");
            
            Value = NoteToNumber[normalizedName];
            Name = ChromaticScale[Value];
        }

        public Note(int value)
        {
            Value = ((value % 12) + 12) % 12;
            Name = ChromaticScale[Value];
        }

        private static string NormalizeNoteName(string noteName)
        {
            noteName = noteName.Replace("b", "b").Replace("#", "#");
            
            if (noteName.Length == 2)
            {
                char note = char.ToUpper(noteName[0]);
                char accidental = noteName[1];
                
                if (accidental == 'b')
                {
                    return note + "b";
                }
                else if (accidental == '#')
                {
                    return note + "#";
                }
            }
            else if (noteName.Length == 1)
            {
                return char.ToUpper(noteName[0]).ToString();
            }
            
            return noteName;
        }

        public Note Transpose(int semitones)
        {
            return new Note(Value + semitones);
        }

        public int IntervalTo(Note other)
        {
            return ((other.Value - Value) + 12) % 12;
        }

        public override string ToString() => Name;
    }

    public class ToneRowMatrix
    {
        private readonly Note[] _originalRow;
        private readonly Note[,] _matrix;

        public ToneRowMatrix(Note[] originalRow)
        {
            if (originalRow.Length != 12)
                throw new ArgumentException("Tone row must contain exactly 12 notes");

            if (originalRow.Select(n => n.Value).Distinct().Count() != 12)
                throw new ArgumentException("Tone row must contain all 12 different pitch classes");

            _originalRow = originalRow;
            _matrix = GenerateMatrix();
        }

        private Note[,] GenerateMatrix()
        {
            var matrix = new Note[12, 12];
            
            var inversion = new Note[12];
            inversion[0] = _originalRow[0];
            
            for (int i = 1; i < 12; i++)
            {
                int intervalFromFirst = _originalRow[0].IntervalTo(_originalRow[i]);
                int invertedInterval = (12 - intervalFromFirst) % 12;
                inversion[i] = _originalRow[0].Transpose(invertedInterval);
            }
            
            matrix[0, 0] = _originalRow[0];
            
            for (int j = 1; j < 12; j++)
            {
                matrix[0, j] = _originalRow[j];
            }
            
            for (int i = 1; i < 12; i++)
            {
                matrix[i, 0] = inversion[i];
            }
            
            for (int i = 1; i < 12; i++)
            {
                for (int j = 1; j < 12; j++)
                {
                    int intervalFromPrimeFirst = _originalRow[0].IntervalTo(_originalRow[j]);
                    matrix[i, j] = matrix[i, 0].Transpose(intervalFromPrimeFirst);
                }
            }

            return matrix;
        }

        public string GenerateMarkdown()
        {
            var sb = new StringBuilder();
            
            sb.AppendLine("# Twelve-Tone Matrix");
            sb.AppendLine();
            sb.AppendLine($"**Prime Row (P0):** {string.Join(" ", (IEnumerable<Note>)_originalRow)}");
            sb.AppendLine();

            sb.Append("|   |");
            for (int j = 0; j < 12; j++)
            {
                sb.Append($" I{_matrix[0, j].Value:D2} |");
            }
            sb.AppendLine(" R |");

            sb.Append("|---|");
            for (int j = 0; j < 13; j++)
            {
                sb.Append("-----|");
            }
            sb.AppendLine();

            for (int i = 0; i < 12; i++)
            {
                sb.Append($"| P{_matrix[i, 0].Value:D2} |");
                
                for (int j = 0; j < 12; j++)
                {
                    sb.Append($" {_matrix[i, j],-3} |");
                }
                
                int riLabel = _matrix[i, 11].Value;
                sb.AppendLine($" RI{riLabel:D2} |");
            }

            sb.Append("|   |");
            for (int j = 0; j < 12; j++)
            {
                int rLabel = _matrix[11, 11 - j].Value;
                sb.Append($" R{rLabel:D2} |");
            }
            sb.AppendLine("   |");

            sb.AppendLine();
            sb.AppendLine("## Legend");
            sb.AppendLine("- **P**: Prime (original row transposed)");
            sb.AppendLine("- **I**: Inversion (intervals inverted)");
            sb.AppendLine("- **R**: Retrograde (row played backwards)");
            sb.AppendLine("- **RI**: Retrograde Inversion (inversion played backwards)");

            return sb.ToString();
        }

        public void PrintMatrix()
        {
            Console.WriteLine("\nTwelve-Tone Matrix:");
            Console.WriteLine($"Prime Row (P0): {string.Join(" ", (IEnumerable<Note>)_originalRow)}");
            Console.WriteLine();

            Console.Write("    ");
            for (int j = 0; j < 12; j++)
            {
                Console.Write($"I{_matrix[0, j].Value:D2}  ");
            }
            Console.WriteLine(" R");

            for (int i = 0; i < 12; i++)
            {
                Console.Write($"P{_matrix[i, 0].Value:D2}  ");
                
                for (int j = 0; j < 12; j++)
                {
                    Console.Write($"{_matrix[i, j],-3} ");
                }
                
                int riLabel = _matrix[i, 11].Value;
                Console.WriteLine($" RI{riLabel:D2}");
            }

            Console.Write("    ");
            for (int j = 0; j < 12; j++)
            {
                int rLabel = _matrix[11, 11 - j].Value;
                Console.Write($"R{rLabel:D2}  ");
            }
            Console.WriteLine();
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Twelve-Tone Matrix Generator ===");
            Console.WriteLine();

            try
            {
                var toneRow = GetToneRowFromUser();
                var matrix = new ToneRowMatrix(toneRow);
                
                matrix.PrintMatrix();
                
                Console.WriteLine();
                Console.Write("Would you like to save this matrix to a markdown file? (y/n): ");
                string saveResponse = Console.ReadLine()?.Trim().ToLower() ?? "";
                
                if (saveResponse == "y" || saveResponse == "yes")
                {
                    SaveMatrixToFile(matrix);
                }
                
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey();
            }
        }

        static Note[] GetToneRowFromUser()
        {
            Console.WriteLine("Enter your 12-tone row. You can use:");
            Console.WriteLine("- Sharps: C# D# F# G# A# (or C#, D#, etc.)");
            Console.WriteLine("- Flats: Db Eb Gb Ab Bb (or Db, Eb, etc.)");
            Console.WriteLine("- Natural notes: C D E F G A B");
            Console.WriteLine();
            Console.WriteLine("Separate notes with spaces or commas.");
            Console.WriteLine("Example: C D# E F# G# A B C# D Eb F G");
            Console.WriteLine();
            
            while (true)
            {
                Console.Write("Enter your tone row: ");
                string input = Console.ReadLine()?.Trim() ?? "";
                
                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Please enter a tone row.");
                    continue;
                }

                try
                {
                    var noteStrings = input.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    
                    if (noteStrings.Length != 12)
                    {
                        Console.WriteLine($"Please enter exactly 12 notes. You entered {noteStrings.Length} notes.");
                        continue;
                    }

                    var notes = noteStrings.Select(ns => new Note(ns.Trim())).ToArray();
                    
                    var uniqueValues = notes.Select(n => n.Value).Distinct().Count();
                    if (uniqueValues != 12)
                    {
                        Console.WriteLine("All 12 different pitch classes must be used exactly once.");
                        continue;
                    }

                    Console.WriteLine($"\nParsed tone row: {string.Join(" ", (IEnumerable<Note>)notes)}");
                    return notes;
                }
                catch (ArgumentException ex)
                {
                    Console.WriteLine($"Error parsing notes: {ex.Message}");
                    Console.WriteLine("Please try again.");
                }
            }
        }

        static void SaveMatrixToFile(ToneRowMatrix matrix)
        {
            try
            {
                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                string fileName = $"ToneRowMatrix_{DateTime.Now:yyyyMMdd_HHmmss}.md";
                string fullPath = Path.Combine(downloadsPath, fileName);

                string markdownContent = matrix.GenerateMarkdown();
                File.WriteAllText(fullPath, markdownContent, Encoding.UTF8);

                Console.WriteLine($"\nMatrix saved successfully to: {fullPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
                
                try
                {
                    string fileName = $"ToneRowMatrix_{DateTime.Now:yyyyMMdd_HHmmss}.md";
                    string markdownContent = matrix.GenerateMarkdown();
                    File.WriteAllText(fileName, markdownContent, Encoding.UTF8);
                    Console.WriteLine($"Matrix saved to current directory: {fileName}");
                }
                catch (Exception ex2)
                {
                    Console.WriteLine($"Could not save file: {ex2.Message}");
                }
            }
        }
    }
}
 