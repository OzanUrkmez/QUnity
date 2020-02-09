using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace QUnity.UI
{
    public class QUIUtility : MonoBehaviour
    {

        #region Unity and Instantiation

        private static QUIUtility singleton;

        private void Start()
        {
            if(singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Fading Processes

        enum FadeType { Destroy, DeActivate, Nothing}

        private static List<FadeRoutineInfo> FadeRoutines = new List<FadeRoutineInfo>();
        private static int activeRoutines = 0;
        struct FadeRoutineInfo
        {
            public Coroutine routine;
            public bool resetColor;
            public Color originalColor;
            public FadeType type;
            public Type objectType;
            public object theObject;
        }

        public static void FinishAllFadesImmediate()
        {
            foreach(FadeRoutineInfo info in FadeRoutines)
            {
                if (info.routine == null)
                    return;
                singleton.StopCoroutine(info.routine);
                switch (info.type)
                {
                    case FadeType.Destroy:
                        Destroy(((MonoBehaviour)info.theObject).gameObject);
                        break;
                    case FadeType.DeActivate:
                        if(info.objectType == typeof(Image))
                        {
                            Image i = (Image)info.theObject;
                            if (info.resetColor)
                                i.color = info.originalColor;
                            else
                                i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
                        }
                        else if(info.objectType == typeof(TMP_Text))
                        {
                            TMP_Text i = (TMP_Text)info.theObject;
                            if (info.resetColor)
                                i.color = info.originalColor;
                            else
                                i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
                        }
                        ((MonoBehaviour)info.theObject).gameObject.SetActive(false);
                        break;
                    case FadeType.Nothing:
                        if (info.objectType == typeof(Image))
                        {
                            Image i = (Image)info.theObject;
                            if (info.resetColor)
                                i.color = info.originalColor;
                            else
                                i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
                        }
                        else if (info.objectType == typeof(TMP_Text))
                        {
                            TMP_Text i = (TMP_Text)info.theObject;
                            if (info.resetColor)
                                i.color = info.originalColor;
                            else
                                i.color = new Color(i.color.r, i.color.g, i.color.b, 0);
                        }
                        break;
                }
            }
            activeRoutines = 0;
            FadeRoutines.Clear();
        }



        /// <summary>
        /// Slowly fades the object and destroys it in the end.
        /// </summary>
        /// <param name="UIComponent"> The UI component that will be subject to the colour fade. </param>
        /// <param name="destructionTime"> The total time in seconds it will take to destroy the object. </param>
        /// <param name="fadePoint"> The seconds after which the fading will start </param>
        public static void FadeDestroy(object UIComponent, float destructionTime, float fadePoint)
        {
            FadeRoutineInfo info = new FadeRoutineInfo();
            info.resetColor = false;
            info.type = FadeType.Destroy;
            info.theObject = UIComponent;
            if (UIComponent is Image)
            {
                //image

                info.routine = singleton.StartCoroutine(singleton.DestroyImageFadeEnumerator(UIComponent as Image, destructionTime, fadePoint));
                info.objectType = typeof(Image);
                FadeRoutines.Add(info);
            }
            else if(UIComponent is TMP_Text)
            {
                //tmp text
                info.routine = singleton.StartCoroutine(singleton.DestroyTmpTextFadeEnumerator(UIComponent as TMP_Text, destructionTime, fadePoint));
                info.objectType = typeof(Image);
                FadeRoutines.Add(info);
            }
            else
            {
                Debug.LogError("UI component type not defined!");
            }
        }

        /// <summary>
        /// Slowly fades the object and then deactivates it.
        /// </summary>
        /// <param name="image"> The UI component that will be subject to the colour fade. </param>
        /// <param name="deActivateTime"> The total time in seconds it will take to deactivate the object. </param>
        /// <param name="fadePoint"> The seconds after which the fading will start. </param>
        /// <param name="resetColor"> If set to true, the object will be reset to its original transparency after being deactivated. </param>
        public static void FadeDeActivate(object UIComponent, float deActivateTime, float fadePoint, bool resetColor)
        {
            FadeRoutineInfo info = new FadeRoutineInfo();
            info.resetColor = resetColor;
            info.type = FadeType.DeActivate;
            info.theObject = UIComponent;
            if (UIComponent is Image)
            {
                //image
                info.routine = singleton.StartCoroutine(singleton.DeActivateImageFadeEnumerator(UIComponent as Image, deActivateTime, fadePoint, resetColor));
                Color c = (UIComponent as Image).color;
                info.originalColor = new Color(c.r, c.g, c.b, c.a);
                info.objectType = typeof(Image);
                FadeRoutines.Add(info);
            }
            else if (UIComponent is TMP_Text)
            {
                //tmp text
                info.routine = singleton.StartCoroutine(singleton.DeActivateTmpTextFadeEnumerator(UIComponent as TMP_Text, deActivateTime, fadePoint, resetColor));
                Color c = (UIComponent as TMP_Text).color;
                info.originalColor = new Color(c.r, c.g, c.b, c.a);
                info.objectType = typeof(TMP_Text);
                FadeRoutines.Add(info);

            }
            else
            {
                Debug.LogError("UI component type not defined!");
            }
        }

        /// <summary>
        /// Slowly fades the object and then deactivates it.
        /// </summary>
        /// <param name="image"> The UI component that will be subject to the colour fade. </param>
        /// <param name="duration"> The total time in seconds it will take to fade the object. </param>
        public static void Fade(object UIComponent, float duration)
        {
            FadeRoutineInfo info = new FadeRoutineInfo();
            info.resetColor = false;
            info.type = FadeType.Nothing;
            info.theObject = UIComponent;
            if (UIComponent is Image)
            {
                //image
                info.routine = singleton.StartCoroutine(singleton.ImageFadeEnumerator(UIComponent as Image, duration));
                FadeRoutines.Add(info);
                info.objectType = typeof(Image);
            }
            else if (UIComponent is TMP_Text)
            {
                //tmp text
                singleton.StartCoroutine(singleton.TmpTextFadeEnumerator(UIComponent as TMP_Text, duration));
                FadeRoutines.Add(info);
                info.objectType = typeof(TMP_Text);

            }
            else
            {
                Debug.LogError("UI component type not defined!");
            }
        }

        #region Images

        private IEnumerator DestroyImageFadeEnumerator(Image image, float destructionTime, float fadePoint)
        {
            activeRoutines++;
            float fadeRate = image.color.a / (destructionTime - fadePoint);
            destructionTime -= fadePoint;
            while ( fadePoint > 0)
            {
                yield return new WaitForEndOfFrame();
                fadePoint -= Time.deltaTime;
            }

           

            while (destructionTime > 0)
            {
                yield return new WaitForEndOfFrame();
                destructionTime -= Time.deltaTime;
                image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - fadeRate * Time.deltaTime);
            }

            Destroy(image.gameObject);
            activeRoutines--;
            if (activeRoutines == 0)
            {
                FadeRoutines.Clear();
            }
        }

        

        private IEnumerator DeActivateImageFadeEnumerator(Image image, float deActivateTime, float fadePoint, bool resetColor)
        {
            activeRoutines++;
            Color c = new Color(image.color.r, image.color.g, image.color.b, image.color.a);
            float fadeRate = image.color.a / (deActivateTime - fadePoint);
            deActivateTime -= fadePoint;
            while (fadePoint > 0)
            {
                yield return new WaitForEndOfFrame();
                fadePoint -= Time.deltaTime;
            }

            while (deActivateTime > 0)
            {
                yield return new WaitForEndOfFrame();
                deActivateTime -= Time.deltaTime;
                image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - fadeRate * Time.deltaTime);
            }

            image.gameObject.SetActive(false);
            if (resetColor)
                image.color = c;
            activeRoutines--;
            if (activeRoutines == 0)
            {
                FadeRoutines.Clear();
            }
        }

        private IEnumerator ImageFadeEnumerator(Image image, float fadePoint)
        {
            activeRoutines++;
            float fadeRate = image.color.a / (fadePoint);
            while (fadePoint > 0)
            {
                yield return new WaitForEndOfFrame();
                fadePoint -= Time.deltaTime;
                image.color = new Color(image.color.r, image.color.g, image.color.b, image.color.a - fadeRate * Time.deltaTime);
            }
            activeRoutines--;
            if (activeRoutines == 0)
            {
                FadeRoutines.Clear();
            }
        }

        #endregion

        #region TMP Text

        private IEnumerator DestroyTmpTextFadeEnumerator(TMP_Text text, float destructionTime, float fadePoint)
        {
            activeRoutines++;
            float fadeRate = text.color.a / (destructionTime - fadePoint);
            destructionTime -= fadePoint;

            while (fadePoint > 0)
            {
                yield return new WaitForEndOfFrame();
                fadePoint -= Time.deltaTime;
            }

            while (destructionTime > 0)
            {
                yield return new WaitForEndOfFrame();
                destructionTime -= Time.deltaTime;
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - fadeRate * Time.deltaTime);
            }

            Destroy(text.gameObject);
            activeRoutines--;
            if (activeRoutines == 0)
            {
                FadeRoutines.Clear();
            }
        }



        private IEnumerator DeActivateTmpTextFadeEnumerator(TMP_Text text, float deActivateTime, float fadePoint, bool resetColor)
        {
            activeRoutines++;
            Color c = new Color(text.color.r, text.color.g, text.color.b, text.color.a);
            float fadeRate = text.color.a / (deActivateTime - fadePoint);
            deActivateTime -= fadePoint;
            while (fadePoint > 0)
            {
                yield return new WaitForEndOfFrame();
                fadePoint -= Time.deltaTime;
            }

            while (deActivateTime > 0)
            {
                yield return new WaitForEndOfFrame();
                deActivateTime -= Time.deltaTime;
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - fadeRate * Time.deltaTime);
            }

            text.gameObject.SetActive(false);
            if (resetColor)
                text.color = c;
            activeRoutines--;
            if (activeRoutines == 0)
            {
                FadeRoutines.Clear();
            }
        }

        private IEnumerator TmpTextFadeEnumerator(TMP_Text text, float fadePoint)
        {
            activeRoutines++;
            float fadeRate = text.color.a / (fadePoint);

            while (fadePoint > 0)
            {
                yield return new WaitForEndOfFrame();
                fadePoint -= Time.deltaTime;
                text.color = new Color(text.color.r, text.color.g, text.color.b, text.color.a - fadeRate * Time.deltaTime);
            }
            activeRoutines--;
            if(activeRoutines == 0)
            {
                FadeRoutines.Clear();
            }
        }

        #endregion

        #endregion

        #region Movement

        /// <summary>
        /// Shakes a UI element, first to max and then to min.
        /// </summary>
        /// <param name="rectTransform"> the rect transform to be shaked </param>
        /// <param name="maxDisplacement"> max displacement in all directions </param>
        /// <param name="shakeTimes"> the number of times that the shake will happen (once from max to min) </param>
        /// <param name="shakeTime"> the total time the shake will take. </param>
        public static void ShakeUI(RectTransform rectTransform, Vector2 maxDisplacement, int shakeTimes, float shakeTime)
        {
            singleton.StartCoroutine(singleton.UIShake(rectTransform, maxDisplacement, shakeTimes, shakeTime));
        }

        private IEnumerator UIShake(RectTransform rectTransform, Vector2 maxDisplacement, int shakeTimes, float shakeTime)
        {
            Vector2 totalTranslation = Vector2.zero;
            float timePerShake = shakeTime / shakeTimes;
            while (shakeTime > 0)
            {
                Vector2 translation = maxDisplacement / timePerShake * 2;
                totalTranslation = Vector2.zero;
                for (int i = 0; i < 2; i++)
                {
                    float time = timePerShake / 2;
                    if (i == 1)
                        translation = -translation;
                    while (time > 0)
                    {
                        yield return new WaitForEndOfFrame();
                        time -= Time.deltaTime;
                        shakeTime -= Time.deltaTime;
                        totalTranslation += translation * Time.deltaTime;
                        TranslateUI(rectTransform, translation * Time.deltaTime);
                    }
                }
                TranslateUI(rectTransform, -totalTranslation);
                maxDisplacement = -maxDisplacement;
            }
        }

        /// <summary>
        /// Translates the UI element by the given vector
        /// </summary>
        public static void TranslateUI(RectTransform rect, Vector2 translation)
        {
            rect.offsetMin += translation;
            rect.offsetMax += translation;
        }

        #endregion

        #region Misc Utility

        #region TMP Input Field

        /// <summary>
        /// Refreshes the input field, for example when it's input type has been changed. Especially useful for passwords.
        /// </summary>
        /// <param name="field"> The input field to be refreshed. </param>
        public static void RefreshInputField(TMP_InputField field)
        {
            string s = field.text;
            if (field.text != "1")
                field.text = "1";
            else
                field.text = "2";
            field.text = s;
        }

        #endregion

        #endregion

    }
}
