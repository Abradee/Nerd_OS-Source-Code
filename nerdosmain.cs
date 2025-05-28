using System;
using System.IO;
using Cosmos.System;
using Cosmos.System.FileSystem;
using Cosmos.System.FileSystem.VFS;

namespace NerdOS
{
    public class Kernel : Sys.Kernel
    {
// LOGIN CONFIGURATION 
        private string currentDir = @"0:\";
        private const string username = "user";
        private const string password = "password";

        protected override void BeforeRun()
        {
            var fs = new CosmosVFS();
            VFSManager.RegisterVFS(fs);
        }
// LOGIN CONFIGURATION END

        protected override void Run()
        {
            if (!Login())
            {
                WriteLineColored("Too many failed attempts. System halted.", ConsoleColor.Red);
                CPU.Halt();
            }

            Console.Clear();
            WriteLineColored("==================================================================", ConsoleColor.Cyan);
            WriteLineColored("$$\   $$\                           $$\        $$$$$$\   $$$$$$\  ", ConsoleColor.Cyan);
            WriteLineColored("$$$\  $$ |                          $$ |      $$  __$$\ $$  __$$\ ", ConsoleColor.Cyan);
            WriteLineColored("$$$$\ $$ | $$$$$$\   $$$$$$\   $$$$$$$ |      $$ /  $$ |$$ /  \__|", ConsoleColor.Cyan);
            WriteLineColored("$$ $$\$$ |$$  __$$\ $$  __$$\ $$  __$$ |      $$ |  $$ |\$$$$$$\  ", ConsoleColor.Cyan);
            WriteLineColored("$$ \$$$$ |$$$$$$$$ |$$ |  \__|$$ /  $$ |      $$ |  $$ | \____$$\ ", ConsoleColor.Cyan);
            WriteLineColored("$$ |\$$$ |$$   ____|$$ |      $$ |  $$ |      $$ |  $$ |$$\   $$ |", ConsoleColor.Cyan);
            WriteLineColored("$$ | \$$ |\$$$$$$$\ $$ |      \$$$$$$$ |       $$$$$$  |\$$$$$$  |", ConsoleColor.Cyan);
            WriteLineColored("\__|  \__| \_______|\__|       \_______|$$$$$$\\______/  \______/ ", ConsoleColor.Cyan);
            WriteLineColored("", ConsoleColor.Cyan);
            WriteLineColored("Welcome to NerdOS v1.0!", ConsoleColor.Cyan);
            WriteLineColored("Type 'help' for a list of commands.", ConsoleColor.Yellow);
            WriteLineColored("==================================================================", ConsoleColor.Cyan);

            while (true)
            {
                WriteColored($"\n{currentDir}> ", ConsoleColor.Green);
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) continue;

                string[] parts = input.Split(' ', 3);
                string cmd = parts[0].ToLower();

                try
                {
                    switch (cmd)
                    {
                        case "add": RunMath(parts, "+"); break;
                        case "sub": RunMath(parts, "-"); break;
                        case "mul": RunMath(parts, "*"); break;
                        case "div": RunMath(parts, "/"); break;

                        case "echo":
                            Console.WriteLine(parts.Length > 1 ? input.Substring(5) : "");
                            break;

                        case "clear":
                            Console.Clear(); break;

                        case "help":
                            ShowHelp(); break;

                        case "about":
                            WriteLineColored("=======================================================", ConsoleColor.Cyan);
                            WriteLineColored("NerdOS v1.0 by abradee", ConsoleColor.Cyan);
                            WriteLineColored("", ConsoleColor.Cyan);
                            WriteLineColored("Open sourced on his Github (https://github.com/Abradee)", ConsoleColor.Cyan);
                            WriteLineColored("=======================================================", ConsoleColor.Cyan);
                            break;

                        case "shutdown":
                            Power.Shutdown(); break;

                        case "restart":
                            Power.Reboot(); break;

                        case "exit":
                            WriteLineColored("System halted.", ConsoleColor.Magenta);
                            CPU.Halt(); break;

                        case "cd":
                            if (parts.Length >= 2)
                            {
                                string targetDir = Path.Combine(currentDir, parts[1]);
                                if (Directory.Exists(targetDir))
                                {
                                    currentDir = Path.GetFullPath(targetDir);
                                    WriteLineColored("Now in: " + currentDir, ConsoleColor.Gray);
                                }
                                else Error("Directory not found.");
                            }
                            else Error("Usage: cd [folder]");
                            break;

                        case "mkdir":
                            if (parts.Length >= 2)
                            {
                                Directory.CreateDirectory(Path.Combine(currentDir, parts[1]));
                                WriteLineColored("Directory created.", ConsoleColor.Gray);
                            }
                            else Error("Usage: mkdir [folder]");
                            break;

                        case "rmdir":
                            if (parts.Length >= 2)
                            {
                                string rmDir = Path.Combine(currentDir, parts[1]);
                                if (Directory.Exists(rmDir))
                                {
                                    Directory.Delete(rmDir, true);
                                    WriteLineColored("Directory deleted.", ConsoleColor.Gray);
                                }
                                else Error("Directory not found.");
                            }
                            else Error("Usage: rmdir [folder]");
                            break;

                        case "write":
                            if (parts.Length >= 3)
                            {
                                File.WriteAllText(Path.Combine(currentDir, parts[1]), parts[2]);
                                WriteLineColored("File written.", ConsoleColor.Gray);
                            }
                            else Error("Usage: write [filename] [text]");
                            break;

                        case "read":
                            if (parts.Length >= 2)
                            {
                                string path = Path.Combine(currentDir, parts[1]);
                                if (File.Exists(path))
                                    Console.WriteLine(File.ReadAllText(path));
                                else Error("File not found.");
                            }
                            else Error("Usage: read [filename]");
                            break;

                        case "delete":
                            if (parts.Length >= 2)
                            {
                                string path = Path.Combine(currentDir, parts[1]);
                                if (File.Exists(path))
                                {
                                    File.Delete(path);
                                    WriteLineColored("File deleted.", ConsoleColor.Gray);
                                }
                                else Error("File not found.");
                            }
                            else Error("Usage: delete [filename]");
                            break;

                        case "edit":
                            if (parts.Length >= 2)
                            {
                                string filePath = Path.Combine(currentDir, parts[1]);
                                WriteLineColored("Enter text. Type '__END__' to finish.", ConsoleColor.DarkGray);
                                string content = "";
                                string line;
                                while ((line = Console.ReadLine()) != "__END__")
                                    content += line + "\n";

                                File.WriteAllText(filePath, content);
                                WriteLineColored("File saved.", ConsoleColor.Gray);
                            }
                            else Error("Usage: edit [filename]");
                            break;

                        case "ls":
                            var dirs = Directory.GetDirectories(currentDir);
                            var files = Directory.GetFiles(currentDir);

                            WriteLineColored("Directories:", ConsoleColor.Blue);
                            foreach (var d in dirs)
                                WriteLineColored("  [D] " + Path.GetFileName(d), ConsoleColor.Blue);

                            WriteLineColored("Files:", ConsoleColor.Yellow);
                            foreach (var f in files)
                                WriteLineColored("  [F] " + Path.GetFileName(f), ConsoleColor.Yellow);
                            break;

                        default:
                            Error("Unknown command. Type 'help'.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Error("Exception: " + ex.Message);
                }
            }
        }

        private bool Login()
        {
            int attempts = 0;
            while (attempts < 3)
            {
                WriteColored("Username: ", ConsoleColor.Cyan);
                string user = Console.ReadLine();
                WriteColored("Password: ", ConsoleColor.Cyan);
                string pass = Console.ReadLine();

                if (user == username && pass == password)
                {
                    WriteLineColored("Login successful!", ConsoleColor.Green);
                    return true;
                }
                else
                {
                    Error("Invalid credentials.");
                    attempts++;
                }
            }
            return false;
        }

        private void RunMath(string[] parts, string op)
        {
            if (parts.Length < 3)
            {
                Error($"Usage: {op} num1 num2");
                return;
            }

            if (int.TryParse(parts[1], out int a) && int.TryParse(parts[2], out int b))
            {
                switch (op)
                {
                    case "+": Console.WriteLine("Result: " + (a + b)); break;
                    case "-": Console.WriteLine("Result: " + (a - b)); break;
                    case "*": Console.WriteLine("Result: " + (a * b)); break;
                    case "/":
                        if (b == 0) Error("Divide by zero");
                        else Console.WriteLine("Result: " + (a / b));
                        break;
                }
            }
            else Error("Invalid numbers.");
        }

        private void ShowHelp()
        {
            WriteLineColored("Available commands:", ConsoleColor.Yellow);
            Console.WriteLine("  add/sub/mul/div [a] [b]");
            Console.WriteLine("  echo [text]     - print text");
            Console.WriteLine("  clear           - clear screen");
            Console.WriteLine("  ls              - list files/folders");
            Console.WriteLine("  mkdir/rmdir [name]");
            Console.WriteLine("  cd [folder]");
            Console.WriteLine("  write/read/delete [file]");
            Console.WriteLine("  edit [file]     - multiline text editor");
            Console.WriteLine("  about/help/exit/shutdown/restart");
        }

        private void WriteColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        private void WriteLineColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }

        private void Error(string msg)
        {
            WriteLineColored("Error: " + msg, ConsoleColor.Red);
        }
    }

    public static class Program
    {
        public static void Main()
        {
            Kernel kernel = new Kernel();
            kernel.Start();
        }
    }
}
