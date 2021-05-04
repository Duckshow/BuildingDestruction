public static partial class Utils
{
    public static bool GetValueFromByte(byte b, int index) {
        byte shiftedVariable = (byte)(b >> index);

        return (shiftedVariable & 1) == 1;
    }

    public static void SetValueInByte(ref byte b, int index, bool value) {
        byte shiftedValue = (byte)(1 << index);

        if(value) {
            b |= shiftedValue;
        }
        else {
            b &= (byte)~shiftedValue;
        }
    }
}
