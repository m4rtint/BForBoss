using Perigon.Analytics;
using Perigon.Utility;

namespace BForBoss
{
    public class ControllerInputSettingsViewModel: BaseInputSettingsViewModel
    {
        public override float GetHorizontal => _configuration.ControllerHorizontalSensitivity * MAPPED_SENSITIVITY_MULTIPLIER;
        public override float GetVertical => _configuration.ControllerVerticalSensitivity * MAPPED_SENSITIVITY_MULTIPLIER;
        
        public ControllerInputSettingsViewModel(IInputConfiguration configuration = null, IBForBossAnalytics analytics = null) : base(configuration, analytics)
        {
        }

        public override void ApplySettings(float horizontal, float vertical, bool isInverted)
        {
            _configuration.ControllerHorizontalSensitivity = horizontal / MAPPED_SENSITIVITY_MULTIPLIER;
            _configuration.ControllerVerticalSensitivity = vertical / MAPPED_SENSITIVITY_MULTIPLIER;
            _configuration.IsInverted = isInverted;
            SetInputSettingsAnalytics();
        }
    }
}
