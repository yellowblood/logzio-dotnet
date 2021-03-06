﻿using System;
using System.Diagnostics;
using System.Reflection;
using log4net;
using log4net.Repository.Hierarchy;
using Logzio.Community.Core.Bootstrap;
using Logzio.Community.Core.Shipping;
using Logzio.Community.IntegrationTests.Listener;
using Logzio.Community.Log4Net;
using NUnit.Framework;
using Shouldly;

namespace Logzio.Community.IntegrationTests.Log4net
{
    [TestFixture]
    public class Log4NetPerfTests
    {
        private LogzioListenerDummy _dummy;

        [SetUp]
        public void Setup()
        {
            _dummy = new LogzioListenerDummy();
            _dummy.Start();
        }

        [TearDown]
        public void TearDown()
        {
            _dummy.Stop();
        }

        [Test]
        public void MeasurePerfForLog4Net()
        {
            const int bufferSize = 100;
            var logzioAppender = SetupAppender(bufferSize);
            logzioAppender.AddDebug(true);
            var logger = LogManager.GetLogger(typeof(Log4NetSanityTests));

            var stopwatch = Stopwatch.StartNew();

            const int logsAmount = 1000;
            for (var i = 0; i < logsAmount; i++)
            {
                logger.Info("A Fish");
            }

            stopwatch.Stop();
            Console.WriteLine("Total time: " + stopwatch.Elapsed);
            stopwatch.Elapsed.ShouldBeLessThanOrEqualTo(TimeSpan.FromMilliseconds(100));

            new Bootstraper().Resolve<IShipper>().WaitForSendLogsTask();

            logzioAppender.Close();
            LogManager.Shutdown();

            _dummy.Requests.Count.ShouldBe(logsAmount / bufferSize);
        }

        private static LogzioAppender SetupAppender(int bufferSize)
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository(Assembly.GetCallingAssembly());
            var logzioAppender = new LogzioAppender();
            logzioAppender.AddToken("iWnDeXJFJtuEPPcgWRDpkCdkBksbrUAO");
            logzioAppender.AddListenerUrl(LogzioListenerDummy.DefaultUrl);
            logzioAppender.AddBufferSize(bufferSize);
            hierarchy.Root.AddAppender(logzioAppender);
            hierarchy.Configured = true;
            return logzioAppender;
        }
    }
}
