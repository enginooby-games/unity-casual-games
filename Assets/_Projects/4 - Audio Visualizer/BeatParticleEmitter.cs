// ========== AudioVisualizer/Particles/BeatParticleEmitter.cs ==========
/// <summary>
/// Emits particle effects synchronized with detected beats.
/// Creates visual feedback for rhythm and beats in the audio.
/// </summary>
using UnityEngine;
using AudioVisualizer.Core;

namespace AudioVisualizer.Particles
{
    public class BeatParticleEmitter : MonoBehaviour
    {
        /// <summary>Reference to the audio processor</summary>
        [SerializeField] private AudioProcessor audioProcessor;

        /// <summary>ParticleSystem component for emitting particles</summary>
        [SerializeField] private ParticleSystem particleSystem;

        /// <summary>Number of particles to emit per beat detection</summary>
        [SerializeField] private int particlesPerBeat = 50;

        /// <summary>Radius of particle emission sphere</summary>
        [SerializeField] private float emissionRadius = 2f;

        /// <summary>Lifetime of each particle in seconds</summary>
        private const float PARTICLE_LIFETIME = 2f;

        /// <summary>Initial size of each particle</summary>
        private const float PARTICLE_START_SIZE = 0.1f;

        private void Start()
        {
            InitializeParticleSystem();
            SubscribeToAudioEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromAudioEvents();
        }

        /// <summary>
        /// Initializes or finds the ParticleSystem component.
        /// Creates a new ParticleSystem if none exists.
        /// </summary>
        private void InitializeParticleSystem()
        {
            if (particleSystem == null)
            {
                particleSystem = GetComponent<ParticleSystem>();
                if (particleSystem == null)
                {
                    GameObject psObject = new GameObject("ParticleSystem");
                    psObject.transform.SetParent(transform);
                    psObject.transform.localPosition = Vector3.zero;
                    particleSystem = psObject.AddComponent<ParticleSystem>();
                }
            }

            ConfigureParticleSystem();
            Debug.Log("[BeatParticleEmitter] Particle system initialized");
        }

        /// <summary>
        /// Configures ParticleSystem settings for beat visualization.
        /// Disables continuous emission and sets particle properties.
        /// </summary>
        private void ConfigureParticleSystem()
        {
            // Main settings
            var main = particleSystem.main;
            main.startLifetime = PARTICLE_LIFETIME;
            main.startSize = PARTICLE_START_SIZE;
            main.startColor = Color.white;
            main.maxParticles = particlesPerBeat * 10;

            // Emission settings
            var emission = particleSystem.emission;
            emission.enabled = false; // Manual emission only

            // Shape settings
            var shape = particleSystem.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = emissionRadius;

            // Velocity settings
            var velocity = particleSystem.velocityOverLifetime;
            velocity.enabled = true;
            velocity.x = new ParticleSystem.MinMaxCurve(-1f, 1f);
            velocity.y = new ParticleSystem.MinMaxCurve(-1f, 1f);
            velocity.z = new ParticleSystem.MinMaxCurve(-1f, 1f);
        }

        /// <summary>
        /// Subscribes to audio processor beat detection event.
        /// Called during initialization.
        /// </summary>
        private void SubscribeToAudioEvents()
        {
            if (audioProcessor != null)
            {
                audioProcessor.OnBeatDetected += OnBeatDetected;
                Debug.Log("[BeatParticleEmitter] Subscribed to beat detection");
            }
            else
            {
                Debug.LogError("[BeatParticleEmitter] Audio processor not assigned");
            }
        }

        /// <summary>
        /// Unsubscribes from audio processor beat detection event.
        /// Called during destruction to prevent memory leaks.
        /// </summary>
        private void UnsubscribeFromAudioEvents()
        {
            if (audioProcessor != null)
            {
                audioProcessor.OnBeatDetected -= OnBeatDetected;
            }
        }

        /// <summary>
        /// Called when a beat is detected by the audio processor.
        /// Emits particles based on beat strength.
        /// </summary>
        /// <param name="beatStrength">Normalized beat strength (0-1)</param>
        private void OnBeatDetected(float beatStrength)
        {
            if (particleSystem == null)
                return;

            int emissionCount = (int)(particlesPerBeat * (0.5f + beatStrength * 0.5f));
            particleSystem.Emit(emissionCount);
        }
    }
}