using System.Collections.Generic;
using System.Linq;

namespace Structure.Code.ProgramInput
{
    public static class SavedSessionUtilities
    {
        private static (PersistedListCollection<ProgramInputData>[] SavedDataSessions, PersistedListCollection<ProgramInputData> NextDataSession) LoadDataSessions()
        {
            var sessions = new List<PersistedListCollection<ProgramInputData>>();
            var currentSession = 0;
            PersistedListCollection<ProgramInputData> session = LoadSession(currentSession++);
            while (session.Any())
            {
                sessions.Add(session);
                session = LoadSession(currentSession++);
            }
            return (sessions.ToArray(), session);
        }

        public static PersistedListCollection<ProgramInputData> LoadNextEmptyDataSession() => LoadDataSessions().NextDataSession;

        public static PersistedListCollection<ProgramInputData>[] LoadSavedDataSessions() => LoadDataSessions().SavedDataSessions;

        private static PersistedListCollection<ProgramInputData> LoadSession(int setNumber) => new PersistedListCollection<ProgramInputData>($"session-{setNumber}");
    }
}
