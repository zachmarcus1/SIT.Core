﻿using FilesChecker;
using System;

namespace SIT.Tarkov.Core
{
    /// <summary>
    /// SPT-Aki FakeFileCheckerResult
    /// </summary>
    public class FakeFileCheckerResult : ICheckResult
    {
        public TimeSpan ElapsedTime { get; private set; }
        public Exception Exception { get; private set; }

        public FakeFileCheckerResult()
        {
            ElapsedTime = new TimeSpan();
            Exception = null;
        }
    }
}
