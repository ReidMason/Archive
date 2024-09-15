using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using TMPro;

namespace Davin.GUIs
{
    public class ScrollingText : MonoBehaviour
    {
        [Tooltip("Label scroll speed")]
        [SerializeField]
        [Range(0, 1000)]
        protected float scrollingSpeed = 100;
        public float ScrollingSpeed { get { return scrollingSpeed; } set { scrollingSpeed = value; } }

        [Tooltip("Number of times to repeat the text scrolling (set to -1 for infinite)")]
        [SerializeField]
        protected int numLoops = 1;
        public int NumLoops { get { return numLoops; } set { numLoops = value; } }

        [Tooltip("Events to fire when the label starts scrolling")]
        public UnityEvent textScrollBegun;

        [Tooltip("Events to fire when the label stops scrolling")]
        public UnityEvent textScrollEnded;

        protected Rect canvasRect;
        public Rect CanvasRect { get { return canvasRect; } set { canvasRect = value; } }

        protected Vector2 canvasExtents;
        public Vector2 CanvasExtents { get { return canvasExtents; } set { canvasExtents = value; } }

        protected TextMeshProUGUI textMeshProGUI;
        protected RectTransform rectTrans;
        protected Image bkgImage;

        protected bool scrollerRunning;
        protected Coroutine scroller;
        protected float width;
        protected float scrollPos, prevScrollPos;
        protected int loopCounter, prevLoopCounter;
        protected bool prevScrollerState;

        protected bool initialised;
        
        public virtual void init()
        {
            RectTransform canvasTransform = transform.Find("Canvas").GetComponent<RectTransform>();
            CanvasRect = canvasTransform.rect;
            CanvasExtents = new Vector2(CanvasRect.width / 2.0f, CanvasRect.height / 2.0f);

            Transform textMeshProTrans = transform.Find("Canvas/Bkg Mask/Text (TMP)");
            textMeshProGUI = textMeshProTrans.GetComponent<TextMeshProUGUI>();
            rectTrans = textMeshProTrans.GetComponent<RectTransform>();

            bkgImage = textMeshProTrans.parent.GetComponent<Image>();

            initialised = true;
        }

        public virtual void OnEnable()
        {
            if (initialised == false) init();

            if (prevScrollerState == true || textMeshProGUI.text != "") begin();
        }

        public virtual void OnDisable()
        {
            if (scrollerRunning == true) storeScrollerState();

            end();
        }

        public virtual void setText(string text, bool append = false)
        {
            if (append == false || scrollerRunning == false)
            {
                end();
                textMeshProGUI.text = text;
                begin();
            }
            else
            {
                textMeshProGUI.text += ("\t" + text);
                width = textMeshProGUI.preferredWidth - scrollPos;
            }
        }

        public virtual void begin()
        {
            scroller = StartCoroutine(scroll());
        }

        public virtual void end()
        {
            if (scroller != null)
            {
                StopCoroutine(scroller);
                scroller = null;
                scrollerRunning = false;
            }
        }

        protected virtual void storeScrollerState()
        {
            prevLoopCounter = loopCounter;
            prevScrollPos = scrollPos;

            prevScrollerState = true;
        }

        protected virtual void resetScroller()
        {
            // set scrollPos to the right edge of the mask/canvas
            scrollPos = textMeshProGUI.transform.parent.GetComponent<RectTransform>().rect.width;

            textScrollBegun.Invoke();
        }

        protected virtual IEnumerator scroll()
        {
            scrollerRunning = true;

            bkgImage.enabled = true;

            width = textMeshProGUI.preferredWidth;

            if (prevScrollerState == true)
            {
                scrollPos = prevScrollPos;
                loopCounter = prevLoopCounter;

                prevScrollerState = false;
            }
            else
            {
                loopCounter = NumLoops;
                resetScroller();
            }

            while (true)
            {
                rectTrans.localPosition = new Vector3(scrollPos, rectTrans.localPosition.y, rectTrans.localPosition.z);

                scrollPos -= ScrollingSpeed * Time.deltaTime;

                // have we displayed the full text?
                if (rectTrans.localPosition.x < -width)
                {
                    textScrollEnded.Invoke();

                    prevScrollPos = 0;

                    if (NumLoops == -1)
                    {
                        // indefinite repeat
                        resetScroller();
                    }
                    else if (loopCounter > 1)
                    {
                        loopCounter--;

                        resetScroller();
                    }
                    else
                    {
                        break;
                    }
                }

                yield return null;
            }

            textMeshProGUI.text = "";

            bkgImage.enabled = false;

            scrollerRunning = false;
        }
    }
}