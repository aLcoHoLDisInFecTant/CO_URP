using UnityEngine;

public enum EEncoderAxis { Horizontal, Vertical, RotationY }

public class EncodeBinding : IBinding
{
    private EncoderInputBridgeV2 bridge;
    private EEncoderAxis axis;
    private float direction; // +1 表示正方向触发，-1 表示负方向触发
    private float threshold;

    private bool lastState = false;
    private float lastValue = 0f;
    public bool IsRestricted { get; set; }

    public EncodeBinding(EncoderInputBridgeV2 bridge, EEncoderAxis axis, float direction, float threshold = 0.5f)
    {
        this.bridge = bridge;
        this.axis = axis;
        this.direction = direction;
        this.threshold = threshold;
    }

    public bool IsPressed()
    {
        float value = GetAxisValue();
        bool pressed = direction > 0 ? value > threshold : value < -threshold;
        bool lastPressed = direction > 0 ? lastValue > threshold : lastValue < -threshold;

        bool result = pressed && !lastPressed;
        lastValue = value;
        return result;
    }

    public bool IsReleased()
    {
        float value = GetAxisValue();
        bool pressed = direction > 0 ? value > threshold : value < -threshold;
        bool lastPressed = direction > 0 ? lastValue > threshold : lastValue < -threshold;

        bool result = !pressed && lastPressed;
        lastValue = value;
        return result;
    }

    private float GetAxisValue()
    {
        return axis switch
        {
            EEncoderAxis.Horizontal => bridge.GetHorizontal(),
            EEncoderAxis.Vertical => bridge.GetVertical(),
            EEncoderAxis.RotationY => bridge.GetRotationY(),
            _ => 0f
        };
    }
}
