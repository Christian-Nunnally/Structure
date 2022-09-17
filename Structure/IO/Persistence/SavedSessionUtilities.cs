using Structur.IO.Input;
using System.Collections.Generic;

namespace Structur.IO.Persistence
{
    public static class SavedSessionUtilities
    {
        private static (PersistedList<ProgramInputData>[] SavedDataSessions, PersistedList<ProgramInputData> NextDataSession) LoadDataSessions()
        {
            var sessions = new List<PersistedList<ProgramInputData>>();
            var currentSession = 0;
            PersistedList<ProgramInputData> session = LoadSession(currentSession++);
            while (session.HasBeenSaved)
            {
                sessions.Add(session);
                session = LoadSession(currentSession++);
            }
            return (sessions.ToArray(), session);
        }

        public static PersistedList<ProgramInputData> LoadNextEmptyDataSession() => LoadDataSessions().NextDataSession;

        public static PersistedList<ProgramInputData>[] LoadSavedDataSessions() => LoadDataSessions().SavedDataSessions;

        private static PersistedList<ProgramInputData> LoadSession(int setNumber) => new($"session-{setNumber}");
    }
}
