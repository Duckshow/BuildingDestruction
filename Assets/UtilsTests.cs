public static partial class Utils
{
    public static void RunTests() {
        TestGetAndSetValueFromByte();
    }

    private static void TestGetAndSetValueFromByte() {
        static void RunTest(byte b, int index, bool expectedResult) {
            UnitTester.Assert(
               "GetValueFromByte",
               GetValueFromByte,
               new UnitTester.Parameter("Byte", b),
               new UnitTester.Parameter("Index", index),
               expectedResult
           );
        }

        RunTest(0b_0000_0000, 0, false);

        for(int i = 0; i < 8; i++) {
            RunTest((byte)(1 << i), i, true);
            RunTest((byte)(1 << 0), i, i == 0);
            RunTest((byte)(1 << 7), i, i == 7);
        }

        RunTest(0b_1111_1110, 0, false);
        RunTest(0b_1111_0111, 3, false);
        RunTest(0b_0111_1111, 7, false);
        RunTest(0b_0101_0101, 4, true);

        byte b = 0b_0000_0000;
        SetValueInByte(ref b, 0, true);
        RunTest(b, 0, true);
        RunTest(b, 1, false);
        RunTest(b, 2, false);
        RunTest(b, 3, false);
        RunTest(b, 4, false);
        RunTest(b, 5, false);
        RunTest(b, 6, false);
        RunTest(b, 7, false);

        SetValueInByte(ref b, 4, true);
        RunTest(b, 0, true);
        RunTest(b, 1, false);
        RunTest(b, 2, false);
        RunTest(b, 3, false);
        RunTest(b, 4, true);
        RunTest(b, 5, false);
        RunTest(b, 6, false);
        RunTest(b, 7, false);

        SetValueInByte(ref b, 0, false);
        RunTest(b, 0, false);
        RunTest(b, 1, false);
        RunTest(b, 2, false);
        RunTest(b, 3, false);
        RunTest(b, 4, true);
        RunTest(b, 5, false);
        RunTest(b, 6, false);
        RunTest(b, 7, false);

    }
}
