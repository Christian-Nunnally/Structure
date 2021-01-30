using System;
using System.IO;

namespace Structure
{
    public class Backup : Module
    {
        private string _backupDirectory = "Backups";

        public override void Enable()
        {
            Program.RegularActions.Add(TryBackup);
        }

        private void TryBackup()
        {
            if (!IsBackedUp())
            {
                DoBackup();
            }
        }

        private void DoBackup()
        {
            var backupPath = Path.Combine(FileIO.SavePath, _backupDirectory, DateTime.Now.Date.ToString("yyyy-dd-MM"));
            Directory.CreateDirectory(backupPath);
            Directory.GetFiles(FileIO.SavePath)
                .All(x => File.Copy(x, Path.Combine(backupPath, Path.GetFileName(x))));
            IO.News("Backup complete.");
        }

        private bool IsBackedUp()
        {
            var today = DateTime.Now.Date;
            return Directory.Exists(Path.Combine(FileIO.SavePath, _backupDirectory, today.ToString("yyyy-dd-MM")));
        }
    }
}