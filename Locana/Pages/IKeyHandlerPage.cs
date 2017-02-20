using Locana.Controls;
using System.Collections.Generic;

namespace Locana.Pages
{
    public interface IKeyHandlerPage
    {
        IEnumerable<KeyAssignmentData> KeyAssignments { get; }
    }
}
