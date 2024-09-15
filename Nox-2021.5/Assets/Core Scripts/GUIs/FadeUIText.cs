using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoxCore.GUIs
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeUIText : MonoBehaviour
    {
        public bool fadeTextIn;
        public float lifeTimeSeconds = 2;
        public float fadeTimeSeconds = 5;

        float startAlpha, endAlpha;
        
        float changeRate = 0;
        float timeSoFar = 0;
        bool fading = false;
        CanvasGroup canvasGroup;

        private void OnEnable()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                D.warn("GUI: {0}", "Must have canvas group attached!");
                enabled = false;
                return;
            }

            changeRate = 0;
            timeSoFar = 0;
//            fading = false;

            if (fadeTextIn == true) fadeIn();
            else fadeOut();
        }

        public void fadeIn()
        {
            startAlpha = 0;
            endAlpha = 1;
            timeSoFar = 0;
            fading = true;
            StartCoroutine(FadeCoroutine());
        }

        public void fadeOut()
        {
            startAlpha = 1;
            endAlpha = 0;
            timeSoFar = 0;
            fading = true;
            StartCoroutine(FadeCoroutine());
        }

        IEnumerator FadeCoroutine()
        {
            SetAlpha(startAlpha);

            yield return new WaitForSeconds(lifeTimeSeconds);

            changeRate = (endAlpha - startAlpha) / fadeTimeSeconds;
            
            while (fading)
            {
                timeSoFar += Time.deltaTime;

                if (timeSoFar > fadeTimeSeconds)
                {
                    fading = false;
                    SetAlpha(endAlpha);
                    gameObject.Recycle();
                    yield break;
                }
                else
                {
                    SetAlpha(canvasGroup.alpha + (changeRate * Time.deltaTime));
                }

                yield return null;
            }
        }

        public void SetAlpha(float alpha)
        {
            canvasGroup.alpha = Mathf.Clamp(alpha, 0, 1);
        }
    }
}