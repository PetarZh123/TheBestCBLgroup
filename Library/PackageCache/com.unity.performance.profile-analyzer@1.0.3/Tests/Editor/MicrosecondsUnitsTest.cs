﻿using NUnit.Framework;
using UnityEditor.Performance.ProfileAnalyzer;
using System.Collections.Generic;

public class MicrosecondsUnitsFixture : UnitsTestFixture
{
    [SetUp]
    public void SetupTest()
    {
        displayUnits = new DisplayUnits(Units.Microseconds);
    }
}

public class MicrosecondsUnitsTest : MicrosecondsUnitsFixture
{
    static TestData[] DecimalLimitValues = new TestData[] {
        new TestData(0.000000001f,  "0"),         
        new TestData(0.00009f,      "0"),
        new TestData(0.0004f,       "0"),
        new TestData(0.0005f,       "1"),
        new TestData(0.0112f,       "11"),
        new TestData(0.1012f,       "101"),
        new TestData(1.0012f,       "1001"),
        new TestData(10.0012f,      "10001"),
        new TestData(100.0012f,     "100001"),
        new TestData(1000.0012f,    "1000001"),
        new TestData(10000.0012f,   "10000000"),    // Only 6 sf valid
        new TestData(100000.0012f,  "100000000"),
    };

    [Test]
    public void DecimalLimit([ValueSource("DecimalLimitValues")] TestData testData)
    {
        string output = displayUnits.ToString(testData.value, showUnits: false, limitToNDigits: 0, showFullValueWhenBelowZero: false);
        Assert.AreEqual(testData.expectedOutput, output);
    }


    static TestData[] ShowFullValueWhenBelowZeroValues = new TestData[] {
        new TestData(0.000000001f,  "1E-06"),
        new TestData(0.00009f,      "0.09"),
        new TestData(0.0004f,       "0.4"),
        new TestData(0.0005f,       "1"),
        new TestData(0.0112f,       "11"),
        new TestData(0.1012f,       "101"),
        new TestData(1.0012f,       "1001"),
        new TestData(10.0012f,      "10001"),
        new TestData(100.0012f,     "100001"),
        new TestData(1000.0012f,    "1000001"),
        new TestData(10000.0012f,   "10000000"),        // Only 6 sf valid
        new TestData(100000.0012f,  "100000000"),
    };

    [Test]
    public void ShowFullValueWhenBelowZero([ValueSource("ShowFullValueWhenBelowZeroValues")] TestData testData)
    {
        string output = displayUnits.ToString(testData.value, showUnits: false, limitToNDigits: 0, showFullValueWhenBelowZero: true);
        Assert.AreEqual(testData.expectedOutput, output);
    }


    static TestData[] WithUnitsValues = new TestData[] {
        new TestData(0.000000001f,  "0us"),
        new TestData(0.00009f,      "0us"),
        new TestData(0.0004f,       "0us"),
        new TestData(0.0005f,       "1us"),
        new TestData(0.0112f,       "11us"),
        new TestData(0.1012f,       "101us"),
        new TestData(1.0012f,       "1001us"),
        new TestData(10.0012f,      "10001us"),
        new TestData(100.0012f,     "100001us"),
        new TestData(1000.0012f,    "1000001us"),
        new TestData(10000.0012f,   "10000000us"),    // Only 6 sf valid
        new TestData(100000.0012f,  "100000000us"),
    };

    [Test]
    public void WithUnits([ValueSource("WithUnitsValues")] TestData testData)
    {
        string output = displayUnits.ToString(testData.value, showUnits: true, limitToNDigits: 0, showFullValueWhenBelowZero: false);
        Assert.AreEqual(testData.expectedOutput, output);
    }


    static TestData[] LimitedTo5DigitsValues = new TestData[] {
        new TestData(0.000000001f,  "0"),
        new TestData(0.00009f,      "0"),
        new TestData(0.0004f,       "0"),
        new TestData(0.0005f,       "1"),
        new TestData(0.0112f,       "11"),
        new TestData(0.1012f,       "101"),
        new TestData(1.0012f,       "1001"),
        new TestData(10.0012f,      "10001"),
        new TestData(100.0012f,     "100ms"),
        new TestData(1000.0012f,    "1s"),
        new TestData(10000.0012f,   "10s"),
        new TestData(100000.0012f,  "100s"),
    };

    [Test]
    public void LimitedTo5Digits([ValueSource("LimitedTo5DigitsValues")] TestData testData)
    {
        string output = displayUnits.ToString(testData.value, showUnits: false, limitToNDigits: 5, showFullValueWhenBelowZero: false);
        Assert.AreEqual(testData.expectedOutput, output);
    }
}