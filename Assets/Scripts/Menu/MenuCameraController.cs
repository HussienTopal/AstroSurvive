using UnityEngine;
using UnityEngine.SceneManagement;

namespace AstroSurvive.Menu
{
    /// <summary>
    /// Smooth camera interpolation and mount controller replicating the camera rig from The Last Snooze.
    /// Uses Vector3.Lerp and Quaternion.Slerp for buttery smooth transitions between focal points.
    /// Execution Order set to -100 so camera initializes before all other menu scripts.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class MenuCameraController : MonoBehaviour
    {
        [Header("Mount Configuration")]
        [Tooltip("The camera transform to move (defaults to this transform if null).")]
        public Transform cameraTransform;

        [Tooltip("Target mount transform for current camera position and orientation.")]
        public Transform currentMount;

        [Tooltip("Speed factor for position and rotation interpolation (matches reference project).")]
        [Range(0.5f, 10f)]
        public float speedFactor = 2.5f;

        [Header("Intro Choreography")]
        public bool playIntroOnStart = true;
        public Transform introDeepSpaceMount;
        public Transform shuttleMainMenuMount;
        [Tooltip("How long to hold at the deep space view before transitioning to shuttle.")]
        public float introHoldDuration = 3.5f;
        [Tooltip("Speed factor used during the intro zoom (slower = more cinematic).")]
        public float introSpeedFactor = 0.9f;

        public System.Action onIntroComplete;
        public bool IsIntroActive => _isIntroActive;

        private Transform _previousMount;
        private float _introTimer = 0f;
        private bool _isIntroActive = false;
        private bool _introTransitionStarted = false;
        private float _originalSpeedFactor;

        private void Awake()
        {
            // Always self-assign — no dependency on Camera.main tag timing
            if (cameraTransform == null)
                cameraTransform = transform;

            _originalSpeedFactor = speedFactor;

            if (playIntroOnStart && introDeepSpaceMount != null && shuttleMainMenuMount != null)
            {
                // Hard snap to intro position immediately in Awake so the first frame is correct
                cameraTransform.position = introDeepSpaceMount.position;
                cameraTransform.rotation = introDeepSpaceMount.rotation;
                currentMount = introDeepSpaceMount;
                speedFactor = introSpeedFactor;
                _isIntroActive = true;
                _introTransitionStarted = false;
                _introTimer = introHoldDuration;
            }
            else if (currentMount != null)
            {
                cameraTransform.position = currentMount.position;
                cameraTransform.rotation = currentMount.rotation;
            }
        }

        private void Start()
        {
            // Confirm position is set (Awake already did this, Start is here for safety)
            if (!_isIntroActive && currentMount != null)
            {
                cameraTransform.position = currentMount.position;
                cameraTransform.rotation = currentMount.rotation;
            }
        }

        private void Update()
        {
            if (currentMount == null || cameraTransform == null)
                return;

            // ── Intro hold phase ────────────────────────────────────────────
            if (_isIntroActive && !_introTransitionStarted)
            {
                _introTimer -= Time.deltaTime;
                // Keep locked at intro position during hold
                cameraTransform.position = introDeepSpaceMount.position;
                cameraTransform.rotation = introDeepSpaceMount.rotation;

                if (_introTimer <= 0f)
                {
                    // Begin flying toward the shuttle
                    _introTransitionStarted = true;
                    SetMount(shuttleMainMenuMount);
                }
                return;
            }

            // ── Lerp to current mount ───────────────────────────────────────
            float alpha = speedFactor * Time.deltaTime;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, currentMount.position, alpha);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, currentMount.rotation, alpha);

            // ── Check if intro transition has finished arriving at shuttle ──
            if (_isIntroActive && _introTransitionStarted)
            {
                float distSq = (cameraTransform.position - shuttleMainMenuMount.position).sqrMagnitude;
                if (distSq < 0.5f * 0.5f)
                {
                    // Snap cleanly
                    cameraTransform.position = shuttleMainMenuMount.position;
                    cameraTransform.rotation = shuttleMainMenuMount.rotation;
                    _isIntroActive = false;
                    speedFactor = _originalSpeedFactor;
                    onIntroComplete?.Invoke();
                }
            }
        }

        // ── Public API ──────────────────────────────────────────────────────

        /// <summary>Instantly skip the intro and teleport camera to the shuttle menu mount.</summary>
        public void SkipIntro()
        {
            if (!_isIntroActive) return;
            if (shuttleMainMenuMount == null) return;

            _isIntroActive = false;
            _introTransitionStarted = false;
            speedFactor = _originalSpeedFactor;

            // Hard snap — no lerp
            cameraTransform.position = shuttleMainMenuMount.position;
            cameraTransform.rotation = shuttleMainMenuMount.rotation;
            currentMount = shuttleMainMenuMount;

            onIntroComplete?.Invoke();
        }

        /// <summary>Smoothly move camera to a new mount transform.</summary>
        public void SetMount(Transform newMount)
        {
            if (newMount == null || newMount == currentMount) return;
            _previousMount = currentMount;
            currentMount = newMount;
        }

        /// <summary>Smoothly move to a new mount with a custom speed override.</summary>
        public void SetMountWithSpeed(Transform newMount, float customSpeed)
        {
            if (newMount == null) return;
            _previousMount = currentMount;
            currentMount = newMount;
            speedFactor = customSpeed;
        }

        /// <summary>Return to the previously active mount.</summary>
        public void Back()
        {
            if (_previousMount == null) return;
            Transform temp = currentMount;
            currentMount = _previousMount;
            _previousMount = temp;
        }

        public void LoadScene(string sceneName) => SceneManager.LoadScene(sceneName);
        public void LoadScene(int sceneIndex) => SceneManager.LoadScene(sceneIndex);

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
