using System;
using System.Collections.Generic;
using UnityEngine;

namespace QUnity.Utility
{
    class QManager : MonoBehaviour
    {

        public static QManager Singleton { get; private set; }

        #region Unity Functions

        private void Start()
        {
            if(Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            Singleton = this;
        }

        private void OnDestroy()
        {
            if(Singleton == this)
            {
                Singleton = null;
            }
        }

        #endregion

        private List<Action> actionsToRunOnUpdate = new List<Action>();

        internal void AddActionToRunOnUpdate(Action act)
        {
            actionsToRunOnUpdate.Add(act);
        }

        private void Update()
        {
            for(int i = 0; i < actionsToRunOnUpdate.Count; i++)
            {
                actionsToRunOnUpdate[i]();
            }
        }

    }
}
