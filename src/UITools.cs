using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UITools {

    public class UIAnimator : MonoBehaviour
    {
        Coroutine fadeIn = null;
        Coroutine fadeOut = null;
        float speed;

        public CanvasGroup canvas;

        public void SetUp(float fadeSpeed)
        {
            canvas = gameObject.GetComponent<CanvasGroup>();
            speed = fadeSpeed;
        }
        public void Show(bool status)
        {
            if (status)
            {
                canvas.blocksRaycasts = true;
            
                FadeIn();
            }
            else
            {
                canvas.blocksRaycasts = false;
                FadeOut();
            }
        }
			

        #region Blink()
        Coroutine blink = null;
        public void Blink()
        {
            if (blink != null) return;
            StartCoroutine(BlinkHelper());
        }
        IEnumerator BlinkHelper()
        {
            Show(true);
            while (true)
            {
                if(canvas.alpha == 1f)
                {
                    Show(false);
                    break;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
        #endregion



        #region FadeIn()
        public void FadeIn()
        {
            if (fadeIn != null) StopCoroutine(fadeIn);
            if (fadeOut != null) StopCoroutine(fadeOut);
            fadeIn = StartCoroutine(FadeInHelper());
        }
        IEnumerator FadeInHelper()
        {
            float maxAlpha = 1f;
            while (true)
            {
                if (canvas.alpha == maxAlpha) break;
                canvas.alpha = Mathf.MoveTowards(canvas.alpha, maxAlpha, Time.deltaTime * speed);

                yield return new WaitForEndOfFrame();
            }
        }
        #endregion

        #region FadeOut()
        public void FadeOut()
        {
            if (fadeIn != null) StopCoroutine(fadeIn);
            if (fadeOut != null) StopCoroutine(fadeOut);
            fadeOut = StartCoroutine(FadeOutHelper());
        }
        IEnumerator FadeOutHelper()
        {
            float minAlpha = 0f;
            while (true)
            {
                if (canvas.alpha == minAlpha) break;
                canvas.alpha = Mathf.MoveTowards(canvas.alpha, minAlpha, Time.deltaTime * speed);

                yield return new WaitForEndOfFrame();
            }
        }
        #endregion

    }

}
