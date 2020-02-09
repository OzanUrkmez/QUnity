using UnityEngine;

namespace QUnity.Utility
{
    class QGeneralUtility : MonoBehaviour
    {

        private static QGeneralUtility singleton;

        #region Unity Functions

        private void Start()
        {
            if(singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            singleton = this;
        }

        #endregion

    }
}
