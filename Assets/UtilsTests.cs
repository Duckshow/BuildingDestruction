using UnityEngine;

public static partial class Utils
{
    public static void RunTests() {
        TestGetAndSetValueFromByte();
        TestGetDirectionVector();
        TestRoundUpToPOT();
        Debug.Log("Tests done.");
    }

    private static void TestGetAndSetValueFromByte() {
        static void RunTest(byte b, int index, bool expectedResult) {
            UnitTester.Assert<byte, int, bool>(
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

    private static void TestGetDirectionVector() {
        static void RunTest(Direction dir, Vector3Int expectedResult) {
            UnitTester.Assert<Direction, Vector3Int>(
                "GetDirectionVector()",
                GetDirectionVector,
                new UnitTester.Parameter("Direction", dir),
                expectedResult
            );
        }

        RunTest(Direction.None, Vector3Int.zero);
        RunTest(Direction.Right, Vector3Int.right);
        RunTest(Direction.Left, Vector3Int.left);
        RunTest(Direction.Up, Vector3Int.up);
        RunTest(Direction.Down, Vector3Int.down);
        RunTest(Direction.Fore, Vector3Int.forward);
        RunTest(Direction.Back, Vector3Int.back);
    }

    private static void TestRoundUpToPOT() {
        static void RunTest(int value, int expectedResult) {
            UnitTester.Assert<int, int>(
                "RoundUpToPOT()",
                RoundUpToPOT,
                new UnitTester.Parameter("Value", value),
                expectedResult
            );
        }

        RunTest(0, 0);
        RunTest(1, 1);
        RunTest(2, 2);

        for(int i = 3; i <= 4; i++)         { RunTest(i, (int)Mathf.Pow(2, 2)); }
        for(int i = 5; i <= 8; i++)         { RunTest(i, (int)Mathf.Pow(2, 3)); }
        for(int i = 9; i <= 16; i++)        { RunTest(i, (int)Mathf.Pow(2, 4)); }
        for(int i = 17; i <= 32; i++)       { RunTest(i, (int)Mathf.Pow(2, 5)); }
        for(int i = 33; i <= 64; i++)       { RunTest(i, (int)Mathf.Pow(2, 6)); }
        for(int i = 65; i <= 128; i++)      { RunTest(i, (int)Mathf.Pow(2, 7)); }
        for(int i = 129; i <= 256; i++)     { RunTest(i, (int)Mathf.Pow(2, 8)); }
        for(int i = 257; i <= 512; i++)     { RunTest(i, (int)Mathf.Pow(2, 9)); }
        for(int i = 513; i <= 1024; i++)    { RunTest(i, (int)Mathf.Pow(2, 10)); }
        for(int i = 1025; i <= 2048; i++)   { RunTest(i, (int)Mathf.Pow(2, 11)); }
        for(int i = 2049; i <= 4096; i++)   { RunTest(i, (int)Mathf.Pow(2, 12)); }
        for(int i = 4097; i <= 8192; i++)   { RunTest(i, (int)Mathf.Pow(2, 13)); }
        for(int i = 8193; i <= 16384; i++)  { RunTest(i, (int)Mathf.Pow(2, 14)); }
        for(int i = 16385; i <= 32768; i++) { RunTest(i, (int)Mathf.Pow(2, 15)); }
        for(int i = 32769; i <= 65536; i++) { RunTest(i, (int)Mathf.Pow(2, 16)); }
    }
}
