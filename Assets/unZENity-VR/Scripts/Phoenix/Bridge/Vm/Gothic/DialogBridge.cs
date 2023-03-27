using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UZVR.Phoenix.Vm.Gothic;

namespace UZVR.Phoenix.Bridge.Vm.Gothic
{
    public static class DialogBridge
    {
        private const string DLLNAME = PhoenixBridge.DLLNAME;

        private static bool dialogsInstanciated;

        [DllImport(DLLNAME)] private static extern void vmGothicNpcInitDialogs(IntPtr vm);
        [DllImport(DLLNAME)] private static extern IntPtr vmGothicNpcGetDialogs(uint npcSymbolId, out int size);
        [DllImport(DLLNAME)] private static extern void vmGothicNpcGetDialog(IntPtr dialogs, int index, StringBuilder description, out int npc, out int nr, out int important, out int condition, out int info, out int trade, out int permanent);
        [DllImport(DLLNAME)] private static extern void vmGothicNpcDisposeDialogs(IntPtr dialogs);


        public static List<BDialog> GetSortedDialogsForNpc(uint npcSymbolId)
        {
            InitDialogs();

            var dialogListPtr = vmGothicNpcGetDialogs(npcSymbolId, out int dialogCount);

            var dialogs = new List<BDialog>(dialogCount);
            for (int i=0; i<dialogCount; i++)
            {
                // FIXME Need to implement overflow check (>256 char) and throw exception within bridge.
                var description = new StringBuilder(256);

                vmGothicNpcGetDialog(dialogListPtr, i, description, out int npc, out int nr, out int important, out int condition, out int info, out int trade, out int permanent);
                dialogs.Add(new BDialog()
                {
                    description = description.ToString(),
                    npc = npc,
                    nr = nr,
                    condition = condition,
                    info = info,
                    permanent = Convert.ToBoolean(permanent),
                    important = Convert.ToBoolean(important),
                    trade = Convert.ToBoolean(trade)
                });
            }

            vmGothicNpcDisposeDialogs(dialogListPtr);
            
            return dialogs.OrderBy(i => i.nr).ToList();
        }

        private static void InitDialogs()
        {
            if (dialogsInstanciated)
                return;

            vmGothicNpcInitDialogs(PhoenixBridge.VmGothicBridge.VmPtr);
            dialogsInstanciated = true;
        }
    }
}
