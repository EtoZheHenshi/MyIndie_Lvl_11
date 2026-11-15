using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Code.WalkingMiniGame
{
    public class WalkingGame : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private WalkingActor _player;
        [SerializeField] private WalkingActor[] _passerPrefabs;
        [SerializeField] private RectTransform _actorsRoot;
        [SerializeField] private Animator _backgroundAnimator;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private GameObject _conversationPanel;
        [SerializeField] private TMP_Text _conversationText;

        [Header("Duration and movement")]
        [SerializeField, Min(1f)] private float _walkDuration = 45f;
        [SerializeField, Min(1f)] private float _passerSpeed = 260f;
        [SerializeField, Min(1f)] private float _laneChangeSpeed = 1100f;
        [SerializeField] private float[] _laneY = { 125f, -90f, -305f };
        [SerializeField] private float _playerX = -510f;
        [SerializeField] private float _spawnX = 960f;
        [SerializeField] private float _despawnX = -1000f;

        [Header("Waves")]
        [SerializeField, Min(0f)] private float _firstWaveDelay = 1f;
        [Tooltip("Increased automatically if needed for a safe passage and conversation clearance.")]
        [SerializeField, Min(0.1f)] private float _minimumWaveInterval = 4.1f;
        [SerializeField, Min(0.1f)] private float _maximumWaveInterval = 5.1f;
        [SerializeField, Range(0f, 1f)] private float _doubleWaveChance = 0.5f;
        [SerializeField, Min(0f)] private float _reactionTime = 0.7f;

        [Header("Collisions and conversations")]
        [SerializeField, Min(0.1f)] private float _conversationDuration = 3f;
        [SerializeField, Min(0f)] private float _conversationGap = 36f;
        [SerializeField, Min(0f)] private float _followingGap = 56f;
        [SerializeField, Min(0f)] private float _collisionPadding = 4f;
        [SerializeField, Min(0f)] private float _verticalCollisionInset = 8f;

        private readonly List<WalkingActor> _passers = new List<WalkingActor>();
        private readonly List<WalkingActor> _depthOrder = new List<WalkingActor>();
        private WalkingActor _conversationPartner;
        private bool _inputEnabled;
        private int _targetLane;
        private float _distanceSinceWave;
        private float _nextWaveDistance;

        public IReadOnlyList<WalkingActor> Passers => _passers;
        public int CurrentLane => _targetLane;
        public float WalkingRemaining { get; private set; }
        public float ConversationRemaining { get; private set; }
        public float ElapsedTime { get; private set; }
        public int ConversationCount { get; private set; }
        public bool IsTalking => _conversationPartner != null;
        public bool IsFinished { get; private set; }
        public event Action GameFinished;

        // Includes space for the worst sideways collision (the passer is already
        // behind the player), and time to cross both lanes between two waves.
        public float MinimumWaveDistance
        {
            get
            {
                float widest = 0f;
                foreach (WalkingActor prefab in _passerPrefabs)
                    widest = Mathf.Max(widest, prefab.Width);
                float contact = (_player.Width + widest) * 0.5f + _collisionPadding;
                float conversationSpace = 2f * contact + _conversationGap + widest + _followingGap;
                float crossingTime = Mathf.Abs(_laneY[0] - _laneY[2]) / Mathf.Max(1f, _laneChangeSpeed);
                float dodgeSpace = 2f * contact + _passerSpeed * (_reactionTime + crossingTime);
                return Mathf.Max(conversationSpace, dodgeSpace);
            }
        }

        public void ResetGame()
        {
            if (_laneY == null || _laneY.Length != 3 ||
                _laneY[0] <= _laneY[1] || _laneY[1] <= _laneY[2])
                throw new InvalidOperationException("Walking requires three lanes ordered from top to bottom.");
            if (_player == null || _passerPrefabs == null || _passerPrefabs.Length == 0)
                throw new InvalidOperationException("Assign the walking player and passer prefabs.");
            foreach (WalkingActor prefab in _passerPrefabs)
                if (prefab == null)
                    throw new InvalidOperationException("A walking passer prefab is missing.");

            _inputEnabled = false;
            ClearPassers();
            WalkingRemaining = Mathf.Max(1f, _walkDuration);
            ConversationRemaining = 0f;
            ElapsedTime = 0f;
            ConversationCount = 0;
            IsFinished = false;
            _targetLane = 1;
            _player.Place(1, _playerX, _laneY[1]);
            _player.SetAnimationRunning(true);
            _backgroundAnimator.speed = 1f;
            _conversationPanel.SetActive(false);
            _distanceSinceWave = 0f;
            _nextWaveDistance = Mathf.Max(0f, _firstWaveDelay) * _passerSpeed;
            RefreshUI();
        }

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled && !IsFinished;
        }

        public bool MoveLane(int direction)
        {
            if (!_inputEnabled || IsFinished || IsTalking || direction == 0)
                return false;
            int next = Mathf.Clamp(_targetLane + (direction > 0 ? 1 : -1), 0, 2);
            if (next == _targetLane)
                return false;
            _targetLane = next;
            return true;
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        private void Advance(float deltaTime)
        {
            // Small movement steps and swept collision checks also handle fast
            // passers or a slow frame without allowing them to tunnel past us.
            while (_inputEnabled && !IsFinished && deltaTime > 0f)
            {
                if (IsTalking)
                {
                    float used = Mathf.Min(deltaTime, ConversationRemaining);
                    ConversationRemaining = Mathf.Max(0f, ConversationRemaining - used);
                    ElapsedTime += used;
                    deltaTime -= used;
                    if (ConversationRemaining <= 0f)
                        EndConversation();
                }
                else
                {
                    float used = Mathf.Min(deltaTime, Mathf.Min(1f / 60f, WalkingRemaining));
                    if (used <= 0f)
                    {
                        Finish();
                        break;
                    }
                    ElapsedTime += used;
                    deltaTime -= used;
                    AdvanceWalking(used);
                }
            }
            RefreshUI();
        }

        private void AdvanceWalking(float deltaTime)
        {
            WalkingRemaining = Mathf.Max(0f, WalkingRemaining - deltaTime);
            float previousY = _player.Y;
            float y = Mathf.MoveTowards(previousY, _laneY[_targetLane], _laneChangeSpeed * deltaTime);
            _player.Place(_targetLane, _playerX, y);
            float movement = _passerSpeed * deltaTime;
            foreach (WalkingActor passer in _passers)
                passer.Place(passer.Lane, passer.X - movement, passer.Y);

            _distanceSinceWave += movement;
            float requiredDistance = _passers.Count == 0 ? _nextWaveDistance :
                Mathf.Max(_nextWaveDistance, MinimumWaveDistance);
            if (_distanceSinceWave >= requiredDistance)
                SpawnWave();

            foreach (WalkingActor passer in _passers)
            {
                float contact = (_player.Width + passer.Width) * 0.5f + _collisionPadding;
                bool crossesX = passer.X <= _playerX + contact &&
                    passer.X + movement >= _playerX - contact;
                float inset = Mathf.Min(_verticalCollisionInset,
                    Mathf.Min(_player.Height, passer.Height) * 0.4f);
                bool crossesY = Mathf.Min(previousY, y) + inset <= passer.Y + passer.Height - inset &&
                    Mathf.Max(previousY, y) + _player.Height - inset >= passer.Y + inset;
                if (crossesX && crossesY)
                {
                    StartConversation(passer);
                    break;
                }
            }

            for (int i = _passers.Count - 1; i >= 0; i--)
                if (_passers[i] != _conversationPartner &&
                    _passers[i].X + _passers[i].Width * 0.5f < _despawnX)
                    RemovePasser(_passers[i]);

            RefreshDepth();
            if (WalkingRemaining <= 0f && !IsTalking)
                Finish();
        }

        private void SpawnWave()
        {
            int safeLane = UnityEngine.Random.Range(0, 3);
            int firstLane = (safeLane + UnityEngine.Random.Range(1, 3)) % 3;
            SpawnPasser(firstLane);
            if (UnityEngine.Random.value < _doubleWaveChance)
                SpawnPasser(3 - safeLane - firstLane);

            _distanceSinceWave = 0f;
            float minimum = Mathf.Max(0.1f, _minimumWaveInterval);
            float maximum = Mathf.Max(minimum, _maximumWaveInterval);
            _nextWaveDistance = Mathf.Max(MinimumWaveDistance,
                UnityEngine.Random.Range(minimum, maximum) * _passerSpeed);
        }

        private void SpawnPasser(int lane)
        {
            WalkingActor prefab = _passerPrefabs[UnityEngine.Random.Range(0, _passerPrefabs.Length)];
            WalkingActor passer = Instantiate(prefab, _actorsRoot);
            passer.Place(lane, _spawnX, _laneY[lane]);
            passer.SetAnimationRunning(true);
            _passers.Add(passer);
        }

        private void StartConversation(WalkingActor passer)
        {
            _conversationPartner = passer;
            ConversationRemaining = Mathf.Max(0.1f, _conversationDuration);
            ConversationCount++;
            _targetLane = passer.Lane;
            _player.Place(_targetLane, _playerX, _laneY[_targetLane]);
            // Use full visual bounds, not a narrow collision box: the dog,
            // leash and both figures must remain separated during the talk.
            passer.Place(passer.Lane,
                _playerX + (_player.Width + passer.Width) * 0.5f + _conversationGap,
                _laneY[passer.Lane]);
            _player.SetAnimationRunning(false);
            _backgroundAnimator.speed = 0f;
            _conversationPanel.SetActive(true);
        }

        private void EndConversation()
        {
            WalkingActor partner = _conversationPartner;
            _conversationPartner = null;
            ConversationRemaining = 0f;
            RemovePasser(partner);
            _player.SetAnimationRunning(true);
            _backgroundAnimator.speed = 1f;
            _conversationPanel.SetActive(false);
            if (WalkingRemaining <= 0f)
                Finish();
        }

        private void RemovePasser(WalkingActor passer)
        {
            _passers.Remove(passer);
            passer.gameObject.SetActive(false);
            Destroy(passer.gameObject);
        }

        private void Finish()
        {
            if (IsFinished)
                return;
            IsFinished = true;
            _inputEnabled = false;
            GameFinished?.Invoke();
        }

        private void RefreshDepth()
        {
            _depthOrder.Clear();
            _depthOrder.Add(_player);
            _depthOrder.AddRange(_passers);
            _depthOrder.Sort((a, b) => b.Y.CompareTo(a.Y));
            for (int i = 0; i < _depthOrder.Count; i++)
                if (_depthOrder[i].transform.GetSiblingIndex() != i)
                    _depthOrder[i].transform.SetSiblingIndex(i);
        }

        private void RefreshUI()
        {
            _timerText.SetText("Прогулка: {0} с", Mathf.CeilToInt(WalkingRemaining + ConversationRemaining));
            if (IsTalking)
                _conversationText.SetText("Разговор: {0:1} с", ConversationRemaining);
        }

        private void ClearPassers()
        {
            _conversationPartner = null;
            foreach (WalkingActor passer in _passers)
            {
                if (passer == null)
                    continue;
                passer.gameObject.SetActive(false);
                Destroy(passer.gameObject);
            }
            _passers.Clear();
            _depthOrder.Clear();
        }

        private void OnDestroy()
        {
            ClearPassers();
        }
    }
}
