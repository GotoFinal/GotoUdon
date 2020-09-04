using System;
using System.Diagnostics;
using GotoUdon.Utils;

namespace GotoUdon
{
    [Serializable]
    public class ClientProcess
    {
        public int pid;

        public int profile;

        public long timeOfCreation;

        public ClientProcess(int pid, int profile)
        {
            this.pid = pid;
            this.profile = profile;
            timeOfCreation = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public Process Process
        {
            get
            {
                if (pid == 0) return null;
                try
                {
                    Process process = Process.GetProcessById(pid);
                    return !process.HasExited ? process : null;
                }
                catch
                {
                    return null;
                }
            }
        }

        public void StopProcess()
        {
            try
            {
                Process process = Process;
                if (process != null)
                {
                    if (!process.CloseMainWindow())
                    {
                        GotoLog.Warn($"Failed to exit profile {profile} process {pid}, application refused to close window.");
                        process.Kill();
                    }

                    if (!process.WaitForExit(10000))
                    {
                        GotoLog.Warn($"Failed to exit profile {profile} process {pid}, waited 10 seconds but its still alive.");
                        process.Kill();
                    }
                }
            }
            catch
            {
                GotoLog.Warn($"Failed to exit profile {profile} process {pid}");
            }
        }
    }
}