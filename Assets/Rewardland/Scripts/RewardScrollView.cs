using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace USP.Rewardland
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(Mask))]
    [RequireComponent(typeof(ScrollRect))]
    public class RewardScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
    {
        [Tooltip("Set starting page index - starting from 0")]
        public int startingPage = 0;
        [Tooltip("Threshold time for fast swipe in seconds")]
        private float fastSwipeThresholdTime = 0.3f;
        [Tooltip("Threshold time for fast swipe in (unscaled) pixels")]
        [SerializeField]private int fastSwipeThresholdDistance = 100;
        [Tooltip("How fast will page lerp to target position")]
        [SerializeField]private float decelerationRate = 50f;
        //page selected event
        public static event System.Action<int> GetCurrentPage;
        // fast swipes should be fast and short. If too long, then it is not fast swipe
        private int _fastSwipeThresholdMaxLimit;
        private ScrollRect _scrollRectComponent;
        private RectTransform _scrollRectRect;
        private RectTransform _container;

        private bool _horizontal;

        // number of pages in container
        private int _pageCount;
        public int _currentPage;

        // whether lerping is in progress and target lerp position
        private bool _lerp;
        private Vector2 _lerpTo;

        // target position of every page
        private List<Vector2> _pagePositions = new List<Vector2>();

        // in draggging, when dragging started and where it started
        private bool _dragging;
        private float _timeStamp;
        private Vector2 _startPosition;
        private float _bottomPosition=2048;
        private float _topPosition=-2048;

        private void Start()
        {
            SetScroll();
        }
        public void SetScroll()
        {
            _scrollRectComponent = GetComponent<ScrollRect>();
            _scrollRectRect = GetComponent<RectTransform>();
            _container = _scrollRectComponent.content;

            // is it horizontal or vertical scrollrect
            if (_scrollRectComponent.horizontal && !_scrollRectComponent.vertical)
            {
                _horizontal = true;
            }
            else if (!_scrollRectComponent.horizontal && _scrollRectComponent.vertical)
            {
                _horizontal = false;
            }
            else
            {
                Debug.LogWarning("Confusing setting of horizontal/vertical direction. Default set to horizontal.");
                _horizontal = true;
            }

            _lerp = false;

            // init
            SetPagePositions();
            SetPage(startingPage);
        }
        //------------------------------------------------------------------------
        void Update()
        {
            // if moving to target position
            if (_lerp)
            {
                // prevent overshooting with values greater than 1
                float decelerate = Mathf.Min(decelerationRate * Time.deltaTime, 1f);
                _container.anchoredPosition = Vector2.Lerp(_container.anchoredPosition, _lerpTo, decelerate);
                // time to stop lerping?
                if (Vector2.SqrMagnitude(_container.anchoredPosition - _lerpTo) < 0.25f)
                {
                    Snap();
                }
            }
        }
        private void Snap()
        {
            // snap to target and stop lerping
            _container.anchoredPosition = _lerpTo;
            _lerp = false;
            // clear also any scrollrect move that may interfere with our lerping
            _scrollRectComponent.velocity = Vector2.zero;
        }
        //------------------------------------------------------------------------
        private void SetPagePositions()
        {
            int width = 0;
            int height = 0;
            int offsetX = 0;
            int offsetY = 0;
            int containerWidth = 0;
            int containerHeight = 0;
            _pageCount = 3;//_container.childCount;
            width = (int)_scrollRectRect.rect.width;
            height = (int)_scrollRectRect.rect.height;
            if (_horizontal)
            {
                // screen width in pixels of scrollrect window
                // center position of all pages
                offsetX = width / 2;
                // total width
                containerWidth = width * _pageCount;
                // limit fast swipe length - beyond this length it is fast swipe no more
                _fastSwipeThresholdMaxLimit = width;
            }
            else
            {
                containerWidth = width;
                offsetY = height / 2;
                containerHeight = height * 3;
                _fastSwipeThresholdMaxLimit = height;
            }

            // delete any previous settings
            _pagePositions.Clear();

            // iterate through all container childern and set their positions
            for (int i = 0; i < _pageCount; i++)
            {
                Vector2 childPosition;
                if (_horizontal)
                {
                    childPosition = new Vector2(i * width - containerWidth / 2 + offsetX, 0f);
                }
                else
                {
                    childPosition = new Vector2(0f, -(i * height - containerHeight / 2 + offsetY));
                }
                _pagePositions.Add(-childPosition);
               
            }
            _container.anchoredPosition = _pagePositions[0];
        }

        //------------------------------------------------------------------------
        private void SetPage(int aPageIndex)
        {
            aPageIndex = Mathf.Clamp(aPageIndex, 0, _pageCount - 1);
            _container.anchoredPosition = _pagePositions[aPageIndex];
            _currentPage = aPageIndex;
        }

        //------------------------------------------------------------------------
        public void LerpToPage(int aPageIndex)
        {
            aPageIndex = Mathf.Clamp(aPageIndex, 0, _pageCount - 1);
            _lerpTo =  _pagePositions[aPageIndex];
            _lerp = true;
            _currentPage = aPageIndex;
            //callback for page changed.
            GetCurrentPage?.Invoke(_currentPage);
        }

        //------------------------------------------------------------------------
        private void NextScreen()
        {
            LerpToPage(_currentPage + 1);
        }

        //------------------------------------------------------------------------
        private void PreviousScreen()
        {
            LerpToPage(_currentPage - 1);
        }

        //------------------------------------------------------------------------
        private int GetNearestPage()
        {
            // based on distance from current position, find nearest page
            Vector2 currentPosition = _container.anchoredPosition;

            float distance = float.MaxValue;
            int nearestPage = _currentPage;

            for (int i = 0; i < _pagePositions.Count; i++)
            {
                float testDist = Vector2.SqrMagnitude(currentPosition - _pagePositions[i]);
                if (testDist < distance)
                {
                    distance = testDist;
                    nearestPage = i;
                }
            }

            return nearestPage;
        }

        //------------------------------------------------------------------------
        public void OnBeginDrag(PointerEventData aEventData)
        {
            // if currently lerping, then stop it as user is draging
            _lerp = false;
            // not dragging yet
            _dragging = false;
        }
        //------------------------------------------------------------------------
        public void OnEndDrag(PointerEventData aEventData)
        {
            // how much was container's content dragged
            
            float difference;
            if (_horizontal)
            {
                difference = _startPosition.x - _container.anchoredPosition.x;
            }
            else
            {
                difference = -(_startPosition.y - _container.anchoredPosition.y);
            }

            // test for fast swipe - swipe that moves only +/-1 item
            if (Time.unscaledTime - _timeStamp < fastSwipeThresholdTime &&
                Mathf.Abs(difference) > fastSwipeThresholdDistance &&
                Mathf.Abs(difference) < _fastSwipeThresholdMaxLimit)
            {
                if (difference > 0)
                {
                    NextScreen();
                }
                else
                {
                    PreviousScreen();
                }
            }
            else
            {
                // if not fast time, look to which page we got to
                LerpToPage(GetNearestPage());
            }

            _dragging = false;
        }

        //------------------------------------------------------------------------
        public void OnDrag(PointerEventData aEventData)
        {
            if (!_dragging)
            {
                // dragging started
                _dragging = true;
                // save time - unscaled so pausing with Time.scale should not affect it
                _timeStamp = Time.unscaledTime;
                // save current position of cointainer
                _startPosition = _container.anchoredPosition;
            }
            if (_container.anchoredPosition.y > _bottomPosition || _container.anchoredPosition.y<_topPosition)
            {
                Snap();
                return;
            }
        }
    }
}