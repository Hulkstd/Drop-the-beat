using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Slider
{
    public class ExitSlider : SlideEvent<ExitSlider>, IPointerUpHandler, IPointerDownHandler
    {
        public void ExitGame()
        {
            #if UNITY_EDITOR
            Debug.Break();
            #endif
            Application.Quit();
        }
    }
}
