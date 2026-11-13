using UnityEngine;
using UnityEngine.EventSystems;

namespace AstroSurvive.Menu
{
    /// <summary>
    /// Interactive 3D celestial object (planet or secret black hole).
    /// Handles spin, hover scale highlight, and 3D raycast click selection.
    /// Focus points are created at WORLD SPACE (not parented to this rotating object)
    /// so they remain stable when the planet spins.
    /// </summary>
    public class PlanetInteractiveItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("Planet Identity")]
        public string planetName = "Earth";
        public bool isSecretEasterEgg = false;

        [Header("Motion & Framing")]
        public float rotationSpeed = 15f;
        public Vector3 rotationAxis = Vector3.up;
        /// <summary>Camera focus point transform — must be parented to scene root, NOT this object, to avoid rotating with the planet.</summary>
        public Transform cameraFocusPoint;
        public float cameraDistance = 8f;

        [Header("Hover Highlight")]
        public float hoverScaleMultiplier = 1.08f;
        public float scaleLerpSpeed = 10f;

        [Header("Easter Egg Hint")]
        public GameObject easterEggHintEffect;
        public float hintPulseSpeed = 2f;

        private Vector3 _originalScale;
        private Vector3 _targetScale;

        private void Start()
        {
            _originalScale = transform.localScale;
            _targetScale = _originalScale;

            // Ensure a collider exists for raycasting (don't add one if already present from prefab)
            if (GetComponent<Collider>() == null)
            {
                var sc = gameObject.AddComponent<SphereCollider>();
                sc.radius = 0.5f; // local units; generator sets correct radius for large objects
            }

            // Auto-create a camera focus point if not assigned —
            // IMPORTANT: parent to scene ROOT so it does not spin with the planet!
            if (cameraFocusPoint == null)
            {
                GameObject focusObj = new GameObject(gameObject.name + "_FocusPoint");
                // Parent to scene root (or the planet's parent's parent = scene root)
                Transform sceneRoot = null;
                if (transform.parent != null && transform.parent.parent != null)
                    sceneRoot = transform.parent.parent;

                focusObj.transform.SetParent(sceneRoot, worldPositionStays: false);

                // Position in front of and slightly above the planet in world space
                Vector3 camDir = (Camera.main != null)
                    ? (Camera.main.transform.position - transform.position).normalized
                    : new Vector3(0f, 0.2f, -1f).normalized;

                focusObj.transform.position = transform.position + camDir * cameraDistance + Vector3.up * 1.2f;
                focusObj.transform.LookAt(transform.position);
                cameraFocusPoint = focusObj.transform;
            }
        }

        private void Update()
        {
            // Axial spin
            transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);

            // Smooth hover scale
            if (transform.localScale != _targetScale)
                transform.localScale = Vector3.Lerp(transform.localScale, _targetScale, scaleLerpSpeed * Time.deltaTime);

            // Easter egg gravitational distortion pulse
            if (isSecretEasterEgg && easterEggHintEffect != null)
            {
                float pulse = 1f + 0.15f * Mathf.Sin(Time.time * hintPulseSpeed);
                easterEggHintEffect.transform.localScale = Vector3.one * pulse;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _targetScale = _originalScale * hoverScaleMultiplier;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _targetScale = _originalScale;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            TriggerSelection();
        }

        private void OnMouseDown()
        {
            // Fallback for physics raycaster path
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            TriggerSelection();
        }

        private void OnMouseEnter()
        {
            if (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject())
                _targetScale = _originalScale * hoverScaleMultiplier;
        }

        private void OnMouseExit()
        {
            _targetScale = _originalScale;
        }

        public void TriggerSelection()
        {
            if (MenuUIManager.Instance != null)
                MenuUIManager.Instance.SelectPlanet(this);
        }
    }
}
