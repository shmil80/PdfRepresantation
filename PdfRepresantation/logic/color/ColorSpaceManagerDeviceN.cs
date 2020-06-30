namespace PdfRepresantation
{
    public class ColorManagerDeviceN : ColorManagerSeparation
    {
        public static ColorManagerDeviceN ManagerDeviceN=new ColorManagerDeviceN();
        protected override ColorSpace Type => ColorSpace.DeviceN;
    }
}