using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using Material.Dialog;
using Material.Dialog.Enums;

namespace Material.Files
{
    public static class Utils
    {
        public const ulong BYTE_SIZE = 1024;
        public const ulong KIB_SIZE = BYTE_SIZE * BYTE_SIZE;
        public const ulong MIB_SIZE = BYTE_SIZE * BYTE_SIZE * BYTE_SIZE;
        public const ulong GIB_SIZE = BYTE_SIZE * BYTE_SIZE * BYTE_SIZE * BYTE_SIZE;
        public const ulong TIB_SIZE = BYTE_SIZE * BYTE_SIZE * BYTE_SIZE * BYTE_SIZE * BYTE_SIZE;
        public const ulong PIB_SIZE = BYTE_SIZE * BYTE_SIZE * BYTE_SIZE * BYTE_SIZE * BYTE_SIZE * BYTE_SIZE;

        public static void RunProcess(string execPath, string arguments)
        {
            var task = new Task(delegate
            {
                var param = new ProcessStartInfo
                {
                    FileName = execPath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(execPath),
                    Arguments = arguments
                };
                Process.Start(param);
            });
            task.ContinueWith(delegate(Task task)
            {
                if (task.IsFaulted)
                {
                    var exception = task.Exception.InnerException;
                    var builder = new StringBuilder();

                    builder.AppendLine("Cannot start process:");
                    builder.AppendLine(task.Exception.InnerException.Message);

                    Dispatcher.UIThread.Post(async delegate
                    {
                        var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
                        {
                            SupportingText = builder.ToString(),
                            DialogButtons = DialogHelper.CreateSimpleDialogButtons(DialogButtonsEnum.Ok),
                            Borderless = false,
                            ContentHeader = "Error",
                            WindowTitle = "Error",
                        });
                        await dialog.Show();
                    });
                }
            });
            task.Start();
        }
        
        public static void OpenByShell(string link)
        {
            OpenFileProcedure(delegate
            {
                var param = new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true,
                    Verb = "open",
                    WorkingDirectory = Path.GetDirectoryName(link)
                };
                Process.Start(param);
            }, $"The file \"{link}\" cannot be opened or executed: ", link);
        }
        
        /// <summary>
        /// Show "open file with" dialog. Feature is supported on Windows OS only, not sure how to implement in Unix.
        /// </summary>
        /// <param name="path">The file location.</param>
        public static void ShowOpenWithDialog(string path) {
            // Works only in Windows OS
            OpenFileProcedure(delegate
            {
                var args = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "shell32.dll");
                args += ",OpenAs_RunDLL " + path;
                Process.Start("rundll32.exe", args);
            }, $"The file \"{path}\" cannot be opened or executed: ", path);
        }

        private static void OpenFileProcedure(Action del, string errorMsg, string path)
        {
            var openFileTask = new Task(del);
            openFileTask.ContinueWith(delegate(Task task)
            {
                if (task.IsFaulted)
                {
                    var exception = task.Exception.InnerException;

                    if (exception is Win32Exception e)
                    {
                        // 1155 - No application is associated with the specified file for this operation.
                        if (e.NativeErrorCode == 1155)
                        {
                            if (path != null)
                            {
                                ShowOpenWithDialog(path);
                                return;
                            }
                        }
                    }
                    
                    var builder = new StringBuilder();

                    builder.AppendLine(errorMsg);
                    builder.AppendLine(task.Exception.InnerException.Message);

                    Dispatcher.UIThread.Post(async delegate
                    {
                        var dialog = DialogHelper.CreateAlertDialog(new AlertDialogBuilderParams()
                        {
                            SupportingText = builder.ToString(),
                            DialogButtons = DialogHelper.CreateSimpleDialogButtons(DialogButtonsEnum.Ok),
                            Borderless = false,
                            ContentHeader = "Error",
                            WindowTitle = "Error",
                        });
                        await dialog.Show();
                    });
                }
            });
            openFileTask.Start();
        }
        
        public static string FormatCurrentDirSupportingText(int filesCount, int dirCount)
        {
            if (filesCount == 0 && dirCount == 0)
                return "empty directory";
            string files = filesCount == 0 ? "no" : filesCount.ToString();
            string dirs = dirCount == 0 ? "no" : dirCount.ToString();

            return $"{files} files, {dirs} directories";
        }

        public static string ToHumanReadableSizeString(this ulong v)
        {
            if (v < BYTE_SIZE)
                return $"{v} Bytes";
            else if (v < KIB_SIZE)
                return $"{Math.Round(v / (float)BYTE_SIZE, 2)} KiB";
            else if (v < MIB_SIZE)
                return $"{Math.Round(v / (float)KIB_SIZE, 2)} MiB";
            else if (v < GIB_SIZE)
                return $"{Math.Round(v / (float)MIB_SIZE, 2)} GiB";
            else
                return $"{Math.Round(v / (float)GIB_SIZE, 2)} TiB";
        }
        
        public static string ToHumanReadableSizeString(this long v)
        {
            if (v < (long)BYTE_SIZE)
                return $"{v} Bytes";
            else if (v < (long)KIB_SIZE)
                return $"{Math.Round(v / (float)BYTE_SIZE, 2)} KiB";
            else if (v < (long)MIB_SIZE)
                return $"{Math.Round(v / (float)KIB_SIZE, 2)} MiB";
            else if (v < (long)GIB_SIZE)
                return $"{Math.Round(v / (float)MIB_SIZE, 2)} GiB";
            else
                return $"{Math.Round(v / (float)GIB_SIZE, 2)} TiB";
        }

        public static bool IsSameLocation(this string a, string b)
        {
            a = a?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            b = b?.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            return a == b;
        }
    }
}
