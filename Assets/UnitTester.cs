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

    [EasyButtons.Button]
    public void TestVoxelClusterHandler() {
        VoxelClusterHandler.RunTests();
    }

    public static void Assert(string testName, bool result, bool expectedResult, params Parameter[] parameters) {
        Debug.Assert(result == expectedResult, GetMessage(testName, result, expectedResult, parameters));
    }

    public static void Assert<T, U>(string testName, Func<T, U> test, Parameter param, U expectedResult) where U : IEquatable<U> {
        U result = test((T)param.Value);
        Debug.Assert(result.Equals(expectedResult), GetMessage(testName, result, expectedResult, param));
    }

    public static void Assert<T, U, V>(string testName, Func<T, U, V> test, Parameter param1, Parameter param2, V expectedResult) where V : IEquatable<V> {
        V result = test((T)param1.Value, (U)param2.Value);
        Debug.Assert(result.Equals(expectedResult), GetMessage(testName, result, expectedResult, param1, param2));
    }

    public static void Assert<T, U, V, X>(string testName, Func<T, U, V, X> test, Parameter param1, Parameter param2, Parameter param3, X expectedResult) where X : IEquatable<X> {
        X result = test((T)param1.Value, (U)param2.Value, (V)param3.Value);
        Debug.Assert(result.Equals(expectedResult), GetMessage(testName, result, expectedResult, param1, param2, param3));
    }

    public static void Assert<T, U, V, X, Y>(string testName, Func<T, U, V, X, Y> test, Parameter param1, Parameter param2, Parameter param3, Parameter param4, Y expectedResult) where Y : IEquatable<Y> {
        Y result = test((T)param1.Value, (U)param2.Value, (V)param3.Value, (X)param4.Value);
        Debug.Assert(result.Equals(expectedResult), GetMessage(testName, result, expectedResult, param1, param2, param3, param4));
    }

    public static void Assert<T, U, V, X, Y, Z, A>(string testName, Func<T, U, V, X, Y, Z, A> test, Parameter param1, Parameter param2, Parameter param3, Parameter param4, Parameter param5, Parameter param6, A expectedResult) where A : IEquatable<A> {
        A result = test((T)param1.Value, (U)param2.Value, (V)param3.Value, (X)param4.Value, (Y)param5.Value, (Z)param6.Value);
        Debug.Assert(result.Equals(expectedResult), GetMessage(testName, result, expectedResult, param1, param2, param3, param4, param5, param6));
    }

    public delegate void TestDelegate<T, U>(T right, T left, T up, T down, T fore, T back, out U resultsRightLeft, out U resultsUpDown, out U resultsForeBack);
    public static void Assert<T, U>(string testName, TestDelegate<T, U> test, Parameter param1, Parameter param2, Parameter param3, Parameter param4, Parameter param5, Parameter param6, U expectedResultRightLeft, U expectedResultUpDown, U expectedResultForeBack) where U : IEquatable<U> {
        U resultsRightLeft;
        U resultsUpDown;
        U resultsForeBack;
        test((T)param1.Value, (T)param2.Value, (T)param3.Value, (T)param4.Value, (T)param5.Value, (T)param6.Value, out resultsRightLeft, out resultsUpDown, out resultsForeBack);

        Debug.Assert(resultsRightLeft.Equals(expectedResultRightLeft),  GetMessage(testName, resultsRightLeft,  expectedResultRightLeft,    param1, param2, param3, param4, param5, param6));
        Debug.Assert(resultsUpDown.Equals(expectedResultUpDown),        GetMessage(testName, resultsUpDown,     expectedResultUpDown,       param1, param2, param3, param4, param5, param6));
        Debug.Assert(resultsForeBack.Equals(expectedResultForeBack),    GetMessage(testName, resultsForeBack,   expectedResultForeBack,     param1, param2, param3, param4, param5, param6));
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

    public static Bin[] GetBinsForTesting(Vector3Int dimensions) {
        int length = dimensions.x * dimensions.y * dimensions.z;

        Bin[] bins = new Bin[length];

        for(int binIndex = 0; binIndex < length; binIndex++) {
            bins[binIndex] = new Bin(binIndex, dimensions);
            bins[binIndex].SetAllVoxelExists(true);
        }

        return bins;
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
