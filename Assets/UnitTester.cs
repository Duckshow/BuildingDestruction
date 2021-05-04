using UnityEngine;
using System;
using System.Collections.Generic;

public class UnitTester : MonoBehaviour
{
    // TODO: would be nice with a better way of running tests

    [EasyButtons.Button]
    public void TestVoxelGrid() {
        VoxelGrid.RunTests();
    }

    [EasyButtons.Button]
    public void TestVoxelMeshFactory() {
        VoxelMeshFactory.RunTests();
    }


    [EasyButtons.Button]
    public void TestBin() {
        Bin.RunTests();
    }

    [EasyButtons.Button]
    public void TestUtils() {
        Utils.RunTests();
    }

    public static void Assert(string testName, Func<Vector3Int, Vector3Int> test, Parameter param, Vector3Int expectedResult) {
        Vector3Int result = test((Vector3Int)param.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param));
    }

    public static void Assert(string testName, Func<Bin[], bool, Vector3> test, Parameter param1, Parameter param2, Vector3 expectedResult) {
        Vector3 result = test((Bin[])param1.Value, (bool)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert(string testName, Func<byte, int, bool> test, Parameter param1, Parameter param2, bool expectedResult) {
        bool result = test((byte)param1.Value, (int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert(string testName, Func<int, Direction, byte, byte, byte, byte, bool> test, Parameter param1, Parameter param2, Parameter param3, Parameter param4, Parameter param5, Parameter param6, bool expectedResult) {
        bool result = test((int)param1.Value, (Direction)param2.Value, (byte)param3.Value, (byte)param4.Value, (byte)param5.Value, (byte)param6.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2, param3, param4, param5, param6));
    }

    public static void Assert(string testName, Func<Direction, byte, byte, byte, byte> test, Parameter param1, Parameter param2, Parameter param3, Parameter param4, byte expectedResult) {
        byte result = test((Direction)param1.Value, (byte)param2.Value, (byte)param3.Value, (byte)param4.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2, param3, param4));
    }

    public static void Assert(string testName, Func<List<VoxelCluster>, int> test, Parameter param, int expectedResult) {
        int result = test((List<VoxelCluster>)param.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param));
    }

    public static void Assert(string testName, Func<Direction, Vector3Int> test, Parameter param, Vector3Int expectedResult) {
        Vector3Int result = test((Direction)param.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param));
    }

    public delegate bool Test1(VoxelAddress address, Vector3Int dimensions, Direction direction, out VoxelAddress resultAddress);
    public static void Assert(string testName, Test1 test, Parameter param1, Parameter param2, Parameter param3, bool expectedResult1, VoxelAddress expectedResult2) {
        VoxelAddress resultAddress;
        bool result = test((VoxelAddress)param1.Value, (Vector3Int)param2.Value, (Direction)param3.Value, out resultAddress);

        Debug.Assert(result == expectedResult1, GetMessage(testName, result, expectedResult1, param1, param2, param3));
        Debug.Assert(resultAddress == expectedResult2, GetMessage(testName, result, expectedResult2, param1, param2, param3));
    }

    public delegate void Test2(Bin binRight, Bin binLeft, Bin binUp, Bin binDown, Bin binFore, Bin binBack, out byte resultsRightLeft, out byte resultsUpDown, out byte resultsForeBack);
    public static void Assert(string testName, Test2 test, Parameter param1, Parameter param2, Parameter param3, Parameter param4, Parameter param5, Parameter param6, byte expectedResultRightLeft, byte expectedResultUpDown, byte expectedResultForeBack) {
        byte resultsRightLeft;
        byte resultsUpDown;
        byte resultsForeBack;
        test((Bin)param1.Value, (Bin)param2.Value, (Bin)param3.Value, (Bin)param4.Value, (Bin)param5.Value, (Bin)param6.Value, out resultsRightLeft, out resultsUpDown, out resultsForeBack);

        Debug.Assert(resultsRightLeft   == expectedResultRightLeft, GetMessage(testName, resultsRightLeft,  expectedResultRightLeft,    param1, param2, param3, param4, param5, param6));
        Debug.Assert(resultsUpDown      == expectedResultUpDown,    GetMessage(testName, resultsUpDown,     expectedResultUpDown,       param1, param2, param3, param4, param5, param6));
        Debug.Assert(resultsForeBack    == expectedResultForeBack,  GetMessage(testName, resultsForeBack,   expectedResultForeBack,     param1, param2, param3, param4, param5, param6));
    }

    public static void Assert(string testName, Func<Vector3Int, Vector3Int, bool> test, Parameter param1, Parameter param2, bool expectedResult) {
        bool result = test((Vector3Int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert(string testName, Func<Vector3Int, Vector3Int, int> test, Parameter param1, Parameter param2, int expectedResult) {
        int result = test((Vector3Int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert(string testName, Func<int, Vector3Int, Vector3Int> test, Parameter param1, Parameter param2, Vector3Int expectedResult) {
        Vector3Int result = test((int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert(string testName, Func<int, Vector3Int, VoxelAddress> test, Parameter param1, Parameter param2, VoxelAddress expectedResult) {
        VoxelAddress result = test((int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert(string testName, Func<VoxelAddress, Vector3Int, int> test, Parameter param1, Parameter param2, int expectedResult) {
        int result = test((VoxelAddress)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert(string testName, Func<VoxelAddress, Vector3Int, Vector3Int> test, Parameter param1, Parameter param2, Vector3Int expectedResult) {
        Vector3Int result = test((VoxelAddress)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert(string testName, Func<Vector3Int, Vector3Int, VoxelAddress> test, Parameter param1, Parameter param2, VoxelAddress expectedResult) {
        VoxelAddress result = test((Vector3Int)param1.Value, (Vector3Int)param2.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert(string testName, Func<Vector3Int, Bin[], Vector3Int, bool> test, Parameter param1, Parameter param2, Parameter param3, bool expectedResult) {
        bool result = test((Vector3Int)param1.Value, (Bin[])param2.Value, (Vector3Int)param3.Value);
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, param1, param2, param3));
    }

    public static void Assert(string testName, bool result, bool expectedResult, params Parameter[] parameters) {
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, parameters));
    }

    public static void Assert(string testName, VoxelAddress result, VoxelAddress expectedResult, params Parameter[] parameters) {
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, parameters));
    }

    public static string GetMessage(string testName, object result, object expectedResult, params Parameter[] parameters) {
        string message = string.Format("============== Fail: {0} ==============\n", testName);
        message += "\n";
        message += "Parameters:\n";
        foreach(Parameter parameter in parameters) {
            message += string.Format("\t-{0}: {1}\n", parameter.Name, parameter.Value);
        }
        message += "\n";
        message += string.Format("Expected Result: {0}\n", expectedResult);
        message += string.Format("Actual Result: {0}\n", result);
        message += "============================";

        return message;
    }

    public struct Parameter {
        public string Name;
        public object Value;

        public Parameter(string name, object value) {
            Name = name;
            Value = value;
        }
    }
}
