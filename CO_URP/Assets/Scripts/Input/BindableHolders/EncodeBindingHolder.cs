using System.Collections.Generic;

public class EncodeBindingHolder : IBindingHolder<EncodeBinding>
{
    public Dictionary<ECommand, EncodeBinding> InputBindings { get; private set; }

    private EncoderInputBridgeV2 bridge;

    public EncodeBindingHolder(EncoderInputBridgeV2 bridge)
    {
        this.bridge = bridge;
    }

    public void Init()
    {
        InputBindings = new Dictionary<ECommand, EncodeBinding>
        {
            { ECommand.UP, new EncodeBinding(bridge, EEncoderAxis.Vertical, +1) },
            { ECommand.DOWN, new EncodeBinding(bridge, EEncoderAxis.Vertical, -1) },
            { ECommand.LEFT, new EncodeBinding(bridge, EEncoderAxis.Horizontal, -1) },
            { ECommand.RIGHT, new EncodeBinding(bridge, EEncoderAxis.Horizontal, +1) },
            { ECommand.SLIDERIGHT, new EncodeBinding(bridge, EEncoderAxis.RotationY, +1) },
            { ECommand.SLIDELEFT, new EncodeBinding(bridge, EEncoderAxis.RotationY, -1) },
        };
    }
}
