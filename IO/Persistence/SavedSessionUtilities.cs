using System.Collections.Generic;
using System.Linq;

namespace Structure.Code.ProgramInput
{
    public static class SavedSessionUtilities
    {
        public static (PersistedListCollection<ProgramInputData>[] SavedDataSessions, PersistedListCollection<ProgramInputData> NextDataSession) LoadSavedDataSessions()
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

        private static PersistedListCollection<ProgramInputData> LoadSession(int setNumber)
        {
            return new PersistedListCollection<ProgramInputData>($"session-{setNumber}", true);
        }
    }
}
