using System;
using DG.Tweening;
using Hauntscope.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hauntscope.UI.Menu
{
    // A cassette in a deck (GDD 5.35): the reels turn while the transcript types itself out, the REC light blinks, and a
    // tap anywhere on the card shows the rest of the page at once. Used for the Bureau's briefing and Vale's tapes.
    public sealed class TapePlayerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private TMP_Text _title;
        [SerializeField] private TMP_Text _body;
        [SerializeField] private Typewriter _typewriter;
        [SerializeField] private TMP_Text _page;
        [SerializeField] private Button _nextButton;
        [SerializeField] private TMP_Text _nextLabel;
        [SerializeField] private Button _skipButton;
        [SerializeField] private TMP_Text _skipLabel;
        [SerializeField] private Button _cardButton;
        [SerializeField] private RectTransform[] _reels;
        [SerializeField] private Graphic _recLight;
        [SerializeField, Min(0.1f)] private float _reelTurnTime = 1.1f;
        [SerializeField, Min(0.05f)] private float _blinkTime = 0.5f;

        public event Action NextClicked;

        public event Action SkipClicked;

        public event Action TypingFinished;

        public bool IsTyping => _typewriter.IsTyping;

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if (!visible)
                StopReels();
        }

        public void SetHeader(string header, string label, string title)
        {
            _header.text = header;
            _label.text = label;
            _title.text = title;
        }

        // A new page: the reels start turning and the transcript types out; TypingFinished comes when it is all there.
        public void ShowPage(string body, string page)
        {
            _body.text = body;
            _page.text = page;
            StartReels();
            _typewriter.Play();
        }

        public void CompleteTyping()
        {
            _typewriter.Complete();
        }

        public void SetButtons(string next, string skip)
        {
            _nextLabel.text = next;
            _skipLabel.text = skip;
            _skipButton.gameObject.SetActive(!string.IsNullOrEmpty(skip));
        }

        private void StartReels()
        {
            StopReels();
            foreach (var reel in _reels)
            {
                reel.DOLocalRotate(new Vector3(0f, 0f, -360f), _reelTurnTime, RotateMode.FastBeyond360).SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Incremental).Ui(gameObject);
            }

            _recLight.DOFade(0.15f, _blinkTime).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).Ui(gameObject);
        }

        private void StopReels()
        {
            foreach (var reel in _reels)
                reel.DOKill();

            _recLight.DOKill();
            var color = _recLight.color;
            color.a = 1f;
            _recLight.color = color;
        }

        private void OnTypingFinished()
        {
            StopReels();
            TypingFinished?.Invoke();
        }

        private void Awake()
        {
            _nextButton.onClick.AddListener(OnNextClicked);
            _skipButton.onClick.AddListener(OnSkipClicked);
            _cardButton.onClick.AddListener(OnCardClicked);
            _typewriter.Finished += OnTypingFinished;
        }

        private void OnDestroy()
        {
            _nextButton.onClick.RemoveListener(OnNextClicked);
            _skipButton.onClick.RemoveListener(OnSkipClicked);
            _cardButton.onClick.RemoveListener(OnCardClicked);
            _typewriter.Finished -= OnTypingFinished;
        }

        private void OnNextClicked()
        {
            NextClicked?.Invoke();
        }

        private void OnSkipClicked()
        {
            SkipClicked?.Invoke();
        }

        private void OnCardClicked()
        {
            _typewriter.Complete();
        }

#if UNITY_EDITOR
        private void Reset()
        {
            _header = Find<TMP_Text>("Card/Header");
            _label = Find<TMP_Text>("Card/Cassette/Label");
            _title = Find<TMP_Text>("Card/Title");
            _body = Find<TMP_Text>("Card/Body");
            _typewriter = Find<Typewriter>("Card/Body");
            _page = Find<TMP_Text>("Card/Page");
            _nextButton = Find<Button>("Card/NextButton");
            _nextLabel = Find<TMP_Text>("Card/NextButton/Label");
            _skipButton = Find<Button>("Card/SkipButton");
            _skipLabel = Find<TMP_Text>("Card/SkipButton/Label");
            _cardButton = Find<Button>("Card");
            _recLight = Find<Graphic>("Card/RecLight");
            var reels = transform.Find("Card/Cassette/Reels");
            if (reels != null)
            {
                _reels = new RectTransform[reels.childCount];
                for (var i = 0; i < reels.childCount; i++)
                    _reels[i] = (RectTransform)reels.GetChild(i);
            }
        }

        private T Find<T>(string path) where T : Component
        {
            var child = transform.Find(path);
            return child != null ? child.GetComponent<T>() : null;
        }
#endif
    }
}
