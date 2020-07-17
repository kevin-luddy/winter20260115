// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.Common
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Stopwatch class to help us log start and stop of an activity, as well as time elapsed
    /// </summary>
    public class StopwatchTimer : IDisposable
    {
        /// <summary>
        /// Activity name.
        /// </summary>
        private string activity;

        /// <summary>
        /// Logger that will log the messages.
        /// </summary>
        private Logger logger;

        /// <summary>
        /// Stopwatch to keep track of things.
        /// </summary>
        private Stopwatch stopwatch;

        /// <summary>
        /// Constructor that will log the beginning of the activity.
        /// </summary>
        /// <param name="activity">Activity name.</param>
        /// <param name="logger">Logger to log the activity start and end.</param>
        public StopwatchTimer(string activity, Logger logger, [CallerMemberName] string memberName = "")
        {
            this.activity = memberName + " " + activity;
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
            else
            {
                this.logger = logger;
                this.activity = logger.Identifier + "." + this.activity;
            }

            this.stopwatch = new Stopwatch();
            this.stopwatch.Start();
            this.logger.Debug(string.Format(Constants.LOG_ACTIVITY_START, this.activity));
            this.logger.Performance("BEGIN - " + this.activity, 0);
        }

        /// <summary>
        /// Constructor that will log the beginning of the activity.
        /// </summary>
        /// <param name="activity">Activity name.</param>
        /// <param name="logger">Logger to log the activity start and end.</param>
        public StopwatchTimer(Logger logger, [CallerMemberName] string memberName = "")
            :this(string.Empty, logger, memberName)
        {
            if (string.IsNullOrEmpty(memberName))
            {
                throw new ArgumentNullException(nameof(memberName));
            }
        }

        /// <summary>
        /// Returns the current elapsed time.
        /// </summary>
        public long ElapsedMilliseconds
        {
            get
            {
                return this.stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// On dispose, we log an activity end message.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposable used by the IDispose Pattern.
        /// </summary>
        /// <param name="limitCleanupToNativeOnly">Indicates whether we need to only cleanup native resources (false), or native and managed (true). 
        ///                                         This is not used in this class, but is required by the pattern.</param>
        protected virtual void Dispose(bool limitCleanupToNativeOnly)
        {
            this.stopwatch.Stop();
            this.logger.Debug(string.Format(Constants.LOG_ACTIVITY_END, this.activity, this.stopwatch.ElapsedMilliseconds));
            this.logger.Performance(this.activity, this.stopwatch.ElapsedMilliseconds);

            this.activity = null;
            this.logger = null;
            this.stopwatch = null;
        }
    }
}
