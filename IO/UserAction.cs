using System;

namespace Structure
{
    public class UserAction
    {
        public string Description { get; set; }

        public Action Action { get; set; }

        public UserAction(string description, Action action)
        {
            Description = description;
            Action = action;
        }
    }
}