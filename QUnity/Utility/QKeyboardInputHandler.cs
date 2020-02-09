using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using QuestryGameGeneral.EventSystems;

namespace QUnity.Utility
{
    /// <summary>
    /// A class for handling keyboard input
    /// </summary>
    public class QKeyboardInputHandler:MonoBehaviour
    {

        private static QKeyboardInputHandler singleton;

        private Dictionary<KeyCode, EmptyEvent> keyPressEvents = new Dictionary<KeyCode, EmptyEvent>();
        private Dictionary<KeyCode, EmptyEvent> keyReleaseEvents = new Dictionary<KeyCode, EmptyEvent>();
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
                keyPressEvents.Add(c, new EmptyEvent());
                keyReleaseEvents.Add(c, new EmptyEvent());
                keyCodes.Add(c);
            }

        }

        private void Update()
        {
            foreach(KeyCode c in keyCodes)
            {
                if (Input.GetKeyDown(c))
                    keyPressEvents[c].Dispatch();
                if (Input.GetKeyUp(c))
                    keyReleaseEvents[c].Dispatch();
            }
        }

        public static QKeyboardInputHandler Getsingleton()
        {
            return singleton;
        }

        #endregion

        #region Subscription and Unsubscription

        public void SubscribeKeyPress(KeyCode c, EmptyEventType e)
        {
            keyPressEvents[c] += e;
        }

        public void SubscribeKeyRelease(KeyCode c, EmptyEventType e)
        {
            keyReleaseEvents[c] += e;
        }

        public void UnSubscribeKeyPress(KeyCode c, EmptyEventType e)
        {
            keyPressEvents[c] -= e;
        }

        public void UnSubscribeKeyRelease(KeyCode c, EmptyEventType e)
        {
            keyReleaseEvents[c] -= e;
        }

        #endregion



    }
}
