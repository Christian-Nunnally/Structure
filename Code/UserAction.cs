using System;

namespace Structure
{
    public class UserAction
    {
        public string Description;

        public Action Action;

        public UserAction(string description, Action action)
        {
            Description = description;
            Action = action;
        }
    }
}