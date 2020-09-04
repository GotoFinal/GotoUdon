using System.Collections.Generic;
using UnityEditor;

namespace GotoUdon.Editor.ClientManager
{
    public static class ClientProcessesManager
    {
        private static List<ClientProcess> Processes => GotoUdonInternalState.Instance.processes;

        public static bool IsAnyRunning()
        {
            Update();
            return Processes.Count > 0;
        }
        
        public static bool IsAnyRunning(int profile)
        {
            Update();
            foreach (ClientProcess clientProcess in Processes)
            {
                if (clientProcess.profile == profile) return true;
            }

            return false;
        }
        
        public static void RegisterProcess(int pid, int profile)
        {
            ClientProcess process = new ClientProcess(pid, profile);
            if (process.Process == null) return;
            Processes.Add(process);
            Update();
            EditorUtility.SetDirty(GotoUdonInternalState.Instance);
        }

        public static void Update()
        {
            if (Processes.RemoveAll(p => p.Process == null) > 0)
            {
                EditorUtility.SetDirty(GotoUdonInternalState.Instance);
            }
        }

        public static void KillLastFromProfile(int profile)
        {
            ClientProcess last = null;
            foreach (ClientProcess clientProcess in Processes)
            {
                if (clientProcess.profile != profile) continue;
                if (last == null) last = clientProcess;
                if (clientProcess.timeOfCreation > last.timeOfCreation) last = clientProcess;
            }

            if (last == null) return;
            last.StopProcess();
            Update();
        }

        public static void KillProfile(int profile)
        {
            foreach (ClientProcess clientProcess in Processes)
            {
                if (clientProcess.profile == profile) clientProcess.StopProcess();
            }

            Update();
        }

        public static void KillAll()
        {
            foreach (ClientProcess clientProcess in Processes)
            {
                clientProcess.StopProcess();
            }

            Update();
        }

        public static List<ClientProcess> GetCopyOfProcesses()
        {
            return new List<ClientProcess>(Processes);
        }
    }
}