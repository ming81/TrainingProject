using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NWH.VehiclePhysics2.Sound.SoundComponents
{
    /// <summary>
    ///     Imitates brake hiss on vehicles with pneumatic brake systems such as trucks and buses.
    ///     Accepts multiple clips of which one will be chosen at random each time this effect is played.
    /// </summary>
    [Serializable]
    public class BrakeHissComponent : SoundComponent
    {
        /// <summary>
        ///     Minimum time between two plays.
        /// </summary>
        [Tooltip("    Minimum time between two plays.")]
        public float minInterval = 4f;

        private float _timer;


        public override void Initialize()
        {
            if (Clip != null)
            {
                Source = container.AddComponent<AudioSource>();
                vc.soundManager.SetAudioSourceDefaults(Source, false, false, baseVolume);
                AddSourcesToMixer();
            }

            vc.brakes.onBrakesDeactivate.AddListener(PlayBrakeHiss);

            base.Initialize();
        }


        public override void Update()
        {
            if (!Active)
            {
                return;
            }

            _timer += Time.deltaTime;
        }


        public override void FixedUpdate()
        {
        }


        public override void Enable()
        {
            base.Enable();
            vc.brakes.onBrakesDeactivate.AddListener(PlayBrakeHiss);
        }


        public override void Disable()
        {
            base.Disable();
            vc.brakes.onBrakesDeactivate.RemoveListener(PlayBrakeHiss);
        }


        public override void SetDefaults(VehicleController vc)
        {
            base.SetDefaults(vc);

            baseVolume = 0.1f;
            basePitch  = 1f;

            if (Clip == null)
            {
                Clip = Resources.Load(VehicleController.defaultResourcesPath + "Sound/AirBrakes") as AudioClip;
                if (Clip == null)
                {
                    Debug.LogWarning(
                        $"Audio Clip for sound component {GetType().Name} could not be loaded from resources. Source will not play.");
                }
            }
        }


        public void PlayBrakeHiss()
        {
            if (_timer < minInterval || Clip == null || !vc.powertrain.engine.IsRunning)
            {
                return;
            }

            Source.clip = RandomClip;
            SetVolume(Random.Range(0.8f, 1.2f) * baseVolume);
            SetPitch(Random.Range(0.8f,  1.2f) * basePitch);
            Play();

            _timer = 0f;
        }
    }
}