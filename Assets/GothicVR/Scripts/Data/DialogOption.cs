using System.Collections.Generic;
using ZenKit.Daedalus;

namespace GVR.Data
{
    /// <summary>
    /// Gothic handles Dialogs with two parts:
    /// 1. C_Info elements which are selectable by default (e.g. when you talk to a Buddler)
    /// 2. When you select an option and go to a sub-element of (e.g.) a quest item, then you get more dialog options added.
    /// </summary>
    public class DialogOption
    {
        public string Text;
        public int Function;
    }
}
