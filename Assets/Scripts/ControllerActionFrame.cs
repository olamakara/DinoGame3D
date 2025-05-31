class ControllerActionFrame
{
    public long Id = 0;
    public long Time = 0;
    public ControllerAction caR = null;
    public ControllerAction caL = null;
    public ControllerAction caH = null;
    public ControllerActionFrame(){}
    public ControllerActionFrame(
        long Id, long Time,
        UnityEngine.XR.InputDevice targetDeviceRight,
        UnityEngine.XR.InputDevice targetDeviceLeft,
        UnityEngine.XR.InputDevice targetDeviceHead
        )
    {
        this.Id = Id;
        this.Time = Time;
        caR = new ControllerAction(targetDeviceRight);
        caL = new ControllerAction(targetDeviceLeft);
        caH = new ControllerAction(targetDeviceHead);
    }
}