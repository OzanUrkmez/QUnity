using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QUnity.Utility
{
    public static class QEvents
    {

        public static void RegisterUpdateAction(Action act)
        {
            QManager.Singleton.AddActionToRunOnUpdate(act);
        }

    }
}
