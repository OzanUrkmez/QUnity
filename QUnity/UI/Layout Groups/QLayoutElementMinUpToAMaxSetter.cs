using UnityEngine;
using UnityEngine.UI;

namespace QUnity.UI.Layout_Groups
{
    [RequireComponent(typeof(LayoutElement))]
    class QLayoutElementMinUpToAMaxSetter:MonoBehaviour
    {

        [SerializeField]
        float MinWidth, MinHeight;
        [SerializeField]
        private RectTransform MaxRectExample;
        [SerializeField]
        private bool WidthDrawEnable, HeightDrawEnable;
        [SerializeField]
        private float WidthOffset, HeightOffset;
        
        private void Awake()
        {
            LayoutElement lay = GetComponent<LayoutElement>();
            if (WidthDrawEnable)
            {
                lay.minWidth = (MaxRectExample.rect.width + WidthOffset > MinWidth ? MaxRectExample.rect.width + WidthOffset : MinWidth);
            }
            if(HeightDrawEnable)
            {
                lay.minHeight = (MaxRectExample.rect.height + HeightOffset > MinHeight ? MaxRectExample.rect.height + HeightOffset : MinHeight);
            }
        }

    }
}
