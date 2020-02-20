using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace QUnity.Utility
{
    /// <summary>
    /// A class for handling keyboard input
    /// </summary>
    public class QKeyboardInputHandler:MonoBehaviour
    {

        private static QKeyboardInputHandler singleton;

        private Dictionary<KeyCode, Action> keyPressEvents = new Dictionary<KeyCode, Action>();
        private Dictionary<KeyCode, Action> keyReleaseEvents = new Dictionary<KeyCode, Action>();
        private List<KeyCode> keyCodes = new List<KeyCode>();
        #region Unity Functions and Handling

        private void Start()
        {
            if(singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            singleton = this;

            foreach( KeyCode c in Enum.GetValues(typeof(KeyCode)))
            {
                if (keyPressEvents.ContainsKey(c))
                    continue;
                keyPressEvents.Add(c, null);
                keyReleaseEvents.Add(c, null);
            }

        }

        private void Update()
        {
            foreach(KeyCode c in keyCodes)
            {
                if (Input.GetKeyDown(c))
                    keyPressEvents[c]?.Invoke();
                if (Input.GetKeyUp(c))
                    keyReleaseEvents[c]?.Invoke();
            }
        }

        public static QKeyboardInputHandler Getsingleton()
        {
            return singleton;
        }

        #endregion

        #region Subscription and Unsubscription

        public static void SubscribeKeyPress(KeyCode c, Action e)
        {
            singleton.keyPressEvents[c] += e;
            if (!singleton.keyCodes.Contains(c))
                singleton.keyCodes.Add(c);
        }

        public static void SubscribeKeyRelease(KeyCode c, Action e)
        {
            singleton.keyReleaseEvents[c] += e;
            if (!singleton.keyCodes.Contains(c))
                singleton.keyCodes.Add(c);
        }

        public static void UnSubscribeKeyPress(KeyCode c, Action e)
        {
            singleton.keyPressEvents[c] -= e;
            if (singleton.keyPressEvents[c] == null && singleton.keyReleaseEvents[c] == null)
                singleton.keyCodes.Remove(c);
        }

        public static void UnSubscribeKeyRelease(KeyCode c, Action e)
        {
            singleton.keyReleaseEvents[c] -= e;
            if (singleton.keyPressEvents[c] == null && singleton.keyReleaseEvents[c] == null)
                singleton.keyCodes.Remove(c);
        }

        #endregion



    }
}
