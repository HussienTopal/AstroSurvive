using UnityEngine;

namespace AstroSurvive.Menu
{
    /// <summary>
    /// Adds a gentle floating cruise motion (bobbing, slight pitch & roll) to the space shuttle
    /// and pulses the engine glow as it travels toward Mars.
    /// </summary>
    public class ShuttleCruiseMotion : MonoBehaviour
    {
        [Header("Floating Parameters")]
        public float bobAmplitude = 0.35f;
        public float bobFrequency = 0.8f;
        public float rollAmplitude = 1.5f;
        public float rollFrequency = 0.5f;
        public float pitchAmplitude = 1.0f;
        public float pitchFrequency = 0.6f;

        [Header("Engine Thruster Effects")]
        public Light thrusterLight;
        public float minLightIntensity = 2.0f;
        public float maxLightIntensity = 4.5f;
        public float flickerSpeed = 12f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Start()
        {
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
        }

        private void Update()
        {
            float time = Time.time;

            // Gentle space cruising float
            float yOffset = Mathf.Sin(time * bobFrequency) * bobAmplitude;
            float xOffset = Mathf.Cos(time * bobFrequency * 0.5f) * (bobAmplitude * 0.3f);
            transform.localPosition = initialPosition + new Vector3(xOffset, yOffset, 0f);

            // Subtle roll & pitch
            float roll = Mathf.Sin(time * rollFrequency) * rollAmplitude;
            float pitch = Mathf.Cos(time * pitchFrequency) * pitchAmplitude;
            transform.localRotation = initialRotation * Quaternion.Euler(pitch, 0f, roll);

            // Engine thruster flicker
            if (thrusterLight != null)
            {
                float noise = Mathf.PerlinNoise(time * flickerSpeed, 0f);
                thrusterLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, noise);
            }
        }
    }
}
