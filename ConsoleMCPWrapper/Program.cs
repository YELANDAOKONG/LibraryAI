using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.WriteLine("Usage: Wrapper.exe <TargetExecutable> [Arguments]");
            return;
        }

        string tempFileDir = Path.Combine(Path.GetTempPath(), $"LibraryAI.ConsoleMCP.Wrapper");
        if (!Directory.Exists(tempFileDir)) Directory.CreateDirectory(tempFileDir);
        string tempLogPath = Path.Combine(tempFileDir, $"{DateTime.Now:yyyyMMddHHmmss}_log.txt");

        try
        {
            using (var logFileStream = new FileStream(tempLogPath, FileMode.Create, FileAccess.Write, FileShare.Read))
            using (var logWriter = new StreamWriter(logFileStream) { AutoFlush = true })
            {
                // ProcessStartInfo notepadInfo = new ProcessStartInfo
                // {
                //     FileName = @"notepad.exe",
                //     Arguments = $"\"{tempLogPath}\"",
                //     UseShellExecute = false
                // };
                // Process.Start(notepadInfo);
                
                ProcessStartInfo targetInfo = new ProcessStartInfo
                {
                    FileName = args[0],
                    Arguments = args.Length > 1 ? string.Join(" ", args, 1, args.Length - 1) : "",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                
                var encoding = Environment.GetEnvironmentVariable("CONSOLE_ENCODING");
                if (!string.IsNullOrEmpty(encoding))
                {
                    try
                    {
                        targetInfo.StandardInputEncoding = 
                            targetInfo.StandardOutputEncoding =
                            targetInfo.StandardErrorEncoding = Encoding.GetEncoding(encoding);
                    }
                    catch
                    {
                        logWriter.WriteLine($"[ERROR] Invalid encoding: {encoding}");
                        logWriter.WriteLine($"[ERROR] Fallback to default encoding: {Console.OutputEncoding.EncodingName}");
                    }
                }

                else
                {
                    targetInfo.StandardInputEncoding = Console.InputEncoding;
                    targetInfo.StandardOutputEncoding = Console.OutputEncoding;
                    targetInfo.StandardErrorEncoding = Console.OutputEncoding;
                }
                
                

                using Process targetProcess = new Process { StartInfo = targetInfo };
                
                logWriter.WriteLine($"Start Time: {DateTime.Now}");
                logWriter.WriteLine($"Target Executable: {targetInfo.FileName}");
                logWriter.WriteLine($"Arguments: {targetInfo.Arguments}");
                logWriter.WriteLine(new string('=', 40));
                
                targetProcess.OutputDataReceived += (_, e) =>
                {
                    LogMessage(logWriter, "STDOUT", e.Data??"@[NULL]");
                    Console.WriteLine(e.Data);
                };
                targetProcess.ErrorDataReceived += (_, e) => 
                    LogMessage(logWriter, "STDERR", e.Data??"@[NULL]");

                targetProcess.Start();
                
                targetProcess.BeginOutputReadLine();
                targetProcess.BeginErrorReadLine();
                
                var inputTask = Task.Run(() =>
                {
                    while (!targetProcess.HasExited)
                    {
                        string input = Console.ReadLine();
                        if (input == null) break;
                        
                        try
                        {
                            targetProcess.StandardInput.WriteLine(input);
                            LogMessage(logWriter, "STDIN", input);
                        }
                        catch
                        {
                            break;
                        }
                    }
                });
                
                targetProcess.WaitForExit();
                LogMessage(logWriter, "INFO", $"EXIT With Code: {targetProcess.ExitCode}");
                targetProcess.Close();
                
                inputTask.Wait(1000);
            }
        }
        catch (Exception ex)
        {
            File.AppendAllText(tempLogPath, $"[ERROR] {ex.Message}\n");
        }
    }

    static void LogMessage(StreamWriter writer, string type, string message)
    {
        if (!string.IsNullOrEmpty(message))
            writer.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {type}: {message}");
    }
}
