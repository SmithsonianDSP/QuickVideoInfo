using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YTII.ModelFactory.Properties
{

    /// <summary>
    /// This property identifies classes/members/fields/etc. that the Mono Linker should *not* remove
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.All)]
    public class PreserveAttribute : System.Attribute
    {
        public PreserveAttribute() { }

        public bool AllMembers { get; set; }
        public bool Conditional { get; set; }
    }
}
