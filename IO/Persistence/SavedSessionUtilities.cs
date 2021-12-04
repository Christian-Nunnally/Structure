using System.Collections.Generic;
using System.Linq;

namespace Structure.Code.ProgramInput
{
    public static class SavedSessionUtilities
    {
        private static PersistedList<ProgramInputData> LoadSession(int setNumber)
        {
            return new PersistedList<ProgramInputData>($"session-{setNumber}", true);
        }

        public static (PersistedList<ProgramInputData>[] SavedDataSessions, PersistedList<ProgramInputData> NextDataSession) LoadSavedDataSessions()
        {
            var sessions = new List<PersistedList<ProgramInputData>>();
            var currentSession = 0;
            PersistedList<ProgramInputData> session = LoadSession(currentSession++);
            while (session.Any())
            {
                sessions.Add(session);
                session = LoadSession(currentSession++);
            }
            return (sessions.ToArray(), session);
        }
    }
}
