﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;

public static class ScenarioTestHelpers
{
    private const string testString = "Hello";
    public static readonly TimeSpan TestTimeout = TimeSpan.FromSeconds(20);

    public static string GenerateStringValue(int length)
    {
        // There's no great reason why we use this set of characters - we just want to be able to generate a longish string
        uint firstCharacter = 0x41; // A

        StringBuilder builder = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            builder.Append((char)(firstCharacter + i % 25));
        }

        return builder.ToString();
    }

    public static bool RunBasicEchoTest(Binding binding, string address, string variation, StringBuilder errorBuilder, Action<ChannelFactory> factorySettings = null)
    {
        Logger.LogInformation("Starting basic echo test.\nTest variation:...\n{0}\nUsing address: '{1}'", variation, address);

        bool success = false;
        try
        {
            ChannelFactory<IWcfService> factory = new ChannelFactory<IWcfService>(binding, new EndpointAddress(address));

            if (factorySettings != null)
            {
                factorySettings(factory);
            }

            IWcfService serviceProxy = factory.CreateChannel();

            string result = serviceProxy.Echo(testString);
            success = string.Equals(result, testString);

            if (!success)
            {
                errorBuilder.AppendLine(String.Format("    Error: expected response from service: '{0}' Actual was: '{1}'", testString, result));
            }
        }
        catch (Exception ex)
        {
            Logger.LogInformation("    {0}", ex.Message);
            errorBuilder.AppendLine(String.Format("    Error: Unexpected exception was caught while doing the basic echo test for variation...\n'{0}'\nException: {1}", variation, ex.ToString()));
        }

        Logger.LogInformation("  Result: {0} ", success ? "PASS" : "FAIL");

        return success;
    }

    public static bool RunComplexEchoTest(Binding binding, string address, string variation, StringBuilder errorBuilder, Action<ChannelFactory> factorySettings = null)
    {
        bool success = false;
        try
        {
            ChannelFactory<IWcfService> factory = new ChannelFactory<IWcfService>(binding, new EndpointAddress(address));

            if (factorySettings != null)
            {
                factorySettings(factory);
            }

            IWcfService serviceProxy = factory.CreateChannel();

            ComplexCompositeType compositeObject = new ComplexCompositeType()
            {
                BoolValue = true,
                ByteArrayValue = new byte[] { 0x60, 0x61, 0x62 },
                CharArrayValue = new char[] { 'a', 'b', 'c' },
                CharValue = 'a',
                DateTimeValue = new DateTime(2000, 1, 1),
                DayOfWeekValue = DayOfWeek.Sunday,
                DoubleValue = 3.14159265,
                FloatValue = 2.71828183f,
                GuidValue = new Guid("EFEA21A0-F59A-4F43-B5D3-B2C667CA6FB6"),
                IntValue = int.MinValue,
                LongerStringValue = GenerateStringValue(2048),
                LongValue = long.MaxValue,
                SbyteValue = (sbyte)'a',
                ShortValue = short.MaxValue,
                StringValue = "the quick brown fox jumps over the lazy dog",
                TimeSpanValue = TimeSpan.MinValue,
                UintValue = uint.MaxValue,
                UlongValue = ulong.MaxValue,
                UshortValue = ushort.MaxValue
            };

            ComplexCompositeType result = serviceProxy.EchoComplex(compositeObject);
            success = compositeObject.Equals(result);

            if (!success)
            {
                errorBuilder.AppendLine(String.Format("    Error: expected response from service: '{0}' Actual was: '{1}'", testString, result));
            }
        }
        catch (Exception ex)
        {
            Logger.LogInformation("    {0}", ex.Message);
            errorBuilder.AppendLine(String.Format("    Error: Unexpected exception was caught while doing the basic echo test for variation...\n'{0}'\nException: {1}", variation, ex.ToString()));
        }

        return success;
    }
}


public static class Logger
{
    public static void Log(string category, string message)
    {
#if LOG_TO_CONSOLE
        Console.WriteLine(String.Format("{0}: {1}", category, message));
#endif // LOG_TO_CONSOLE
    }

    public static void LogInformation(string message)
    {
#if LOG_TO_CONSOLE
        Console.WriteLine(message);
#endif // LOG_TO_CONSOLE
    }

    public static void LogInformation(string message, params object[] args)
    {
        LogInformation(String.Format(message, args));
    }

    public static void LogWarning(string message, params object[] args)
    {
        LogWarning(String.Format(message, args));
    }

    public static void LogWarning(string message)
    {
        Log("Warning", message);
    }

    public static void LogError(string message, params object[] args)
    {
        LogError(String.Format(message, args));
    }

    public static void LogError(string message)
    {
        Log("Error", message);
    }
}
