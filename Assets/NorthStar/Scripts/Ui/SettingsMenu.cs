// Copyright (c) Meta Platforms, Inc. and affiliates.
using TMPro;
using UnityEngine;

namespace NorthStar
{
    /// <summary>
    /// Controller for the settings menu UI
    /// </summary>
    public class SettingsMenu : UiMenu
    {
        [SerializeField] private ToggleButton m_seatedToggle;
        [SerializeField] private OptionButton m_comfortButton;
        [SerializeField] private ToggleButton m_musicToggle, m_voiceToggle, m_captionToggle, m_reorientToggle;
        [SerializeField] private TextMeshProUGUI m_heightLabel;

        /// <summary>
        /// The minimum calibrated height in cm (from using the calibration menu)
        /// </summary>
        private const float MINIMUM_CALIBRATED_HEIGHT = 120;

        /// <summary>
        /// The maximum calibrated height in cm (from using the calibration menu)
        /// </summary>
        private const float MAXIMUM_CALIBRATED_HEIGHT = 240;

        /// <summary>
        /// The presumed ratio between seated and standing height for automatic calculations
        /// </summary>
        private const float SEATED_HEIGHT_RATIO = 0.5f;

        /// <summary>
        /// The threshold where height calibration will be estimated for the opposing setting
        /// i.e. if seated height is set too close to standing height, use the default height ratio to recalculate it
        /// </summary>
        private const float CORRECTIVE_HEIGHT_RATIO_THRESHOLD = 0.75f;

        protected override void OnStart()
        {
            Setup();
        }

        protected override void OnOpen() { Setup(); }

        private void Setup()
        {
            m_seatedToggle.State = GlobalSettings.PlayerSettings.Seated;
            m_comfortButton.Index = (int)GlobalSettings.PlayerSettings.ComfortLevel;

            if (m_musicToggle is not null)
            {
                m_musicToggle.State = GlobalSettings.PlayerSettings.MusicMuted;
            }
            if (m_voiceToggle is not null)
            {
                m_voiceToggle.State = GlobalSettings.PlayerSettings.VoiceMuted;
            }
            if (m_captionToggle is not null)
            {
                m_captionToggle.State = GlobalSettings.PlayerSettings.DisableCaptions;
            }
            if (m_reorientToggle is not null)
            {
                m_reorientToggle.State = GlobalSettings.PlayerSettings.ReorientStrength == 1f;
            }

            UpdateHeightLabel();
        }

        private const double METERS_TO_FEET = 0.3048;
        private const double FEET_TO_INCHES = 0.08333;
        private const double EYES_TO_HEAD_CM = 10;

        private void UpdateHeightLabel()
        {
            if (m_heightLabel)
            {
                var heightCm = GlobalSettings.PlayerSettings.Height + EYES_TO_HEAD_CM;
                var feet = heightCm / 100 / METERS_TO_FEET;
                var wholeFeet = (int)feet;
                var inches = (feet - System.Math.Truncate(feet)) / FEET_TO_INCHES;

                m_heightLabel.text = $"{wholeFeet}'{(int)inches}\" / {(int)heightCm} cm";
            }
        }

        public void CalibrateHeight()
        {
            var height = Camera.main.transform.localPosition.y * 100;

            if (GlobalSettings.PlayerSettings.Seated)
            {
                height = Mathf.Clamp(height, MINIMUM_CALIBRATED_HEIGHT * SEATED_HEIGHT_RATIO, MAXIMUM_CALIBRATED_HEIGHT * SEATED_HEIGHT_RATIO);
                GlobalSettings.PlayerSettings.SeatedHeight = height;
            }
            else
            {
                height = Mathf.Clamp(height, MINIMUM_CALIBRATED_HEIGHT, MAXIMUM_CALIBRATED_HEIGHT);
                GlobalSettings.PlayerSettings.Height = height;
            }

            GlobalSettings.PlayerSettings.PlayerCalibrationChanged();
            GlobalSettings.Save();

            UpdateHeightLabel();
        }

        public void SetSeated(bool seated)
        {
            GlobalSettings.PlayerSettings.Seated = seated;
            GlobalSettings.PlayerSettings.PlayerCalibrationChanged();
            GlobalSettings.Save();

            UpdateHeightLabel();
        }

        public void SetComfort(int comfort)
        {
            GlobalSettings.PlayerSettings.ComfortLevel = (GlobalSettings.ComfortLevel)comfort;
            GlobalSettings.PlayerSettings.ComfortLevelChanged();
            GlobalSettings.Save();
        }

        public void SetMusic(bool mute)
        {
            GlobalSettings.PlayerSettings.MusicMuted = mute;
            GlobalSettings.Save();
        }

        public void SetVoice(bool mute)
        {
            GlobalSettings.PlayerSettings.VoiceMuted = mute;
            GlobalSettings.Save();
        }

        public void SetCaptions(bool enabled)
        {
            GlobalSettings.PlayerSettings.DisableCaptions = enabled;
            GlobalSettings.Save();
        }

        public void SetReorient(bool reorient)
        {
            GlobalSettings.PlayerSettings.ReorientStrength = reorient ? 1 : 0;
            GlobalSettings.Save();
        }

        public void OpenCredits()
        {
            GameFlowController.Instance.GoToCredits();
        }
    }
}
