using System;
using System.Collections.Generic;
using System.Text;

namespace SlothBot.Bridge.Process.Arguments
{
    public class UserSpecifiedProcessArgument : InMemoryProcessArgument
    {
        public string Description { get; }

        public UserSpecifiedProcessArgument(string description, string name) : base(name, null)
        {
            Description = description;
        }
    }
}
