﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Metrolib;

namespace Tailviewer.BusinessLogic.LogFiles
{
	public abstract class AbstractLogFile
		: ILogFile
	{
		private readonly ITaskScheduler _scheduler;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly LogFileListenerCollection _listeners;
		private IPeriodicTask _readTask;
		private bool _isDisposed;
		private volatile bool _endOfSourceReached;

		protected AbstractLogFile(ITaskScheduler scheduler)
		{
			if (scheduler == null)
				throw new ArgumentNullException("scheduler");

			_scheduler = scheduler;
			_cancellationTokenSource = new CancellationTokenSource();
			_listeners = new LogFileListenerCollection(this);
		}

		protected LogFileListenerCollection Listeners
		{
			get { return _listeners; }
		}

		public bool IsDisposed
		{
			get { return _isDisposed; }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void RemoveListener(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public virtual void Dispose()
		{
			_cancellationTokenSource.Cancel();
			_isDisposed = true;
			_scheduler.StopPeriodic(_readTask);
		}

		public abstract int MaxCharactersPerLine { get; }

		public abstract bool Exists { get; }

		public virtual bool EndOfSourceReached
		{
			get { return _endOfSourceReached; }
		}

		public abstract DateTime? StartTimestamp { get; }
		public abstract DateTime LastModified { get; }
		public abstract Size FileSize { get; }
		public abstract int Count { get; }

		public abstract void GetSection(LogFileSection section, LogLine[] dest);
		public abstract LogLine GetLine(int index);

		protected abstract void RunOnce(CancellationToken token);

		private void Run()
		{
			CancellationToken token = _cancellationTokenSource.Token;
			RunOnce(token);
		}

		protected void SetEndOfSourceReached()
		{
			// Now this line is very important:
			// Most tests expect that listeners have been notified
			// of all pending changes when the source enters the
			// "EndOfSourceReached" state. This would be true, if not
			// for listeners specifying a timespan that should ellapse between
			// calls to OnLogFileModified. The listener collection has
			// been notified, but the individual listeners may not be, because
			// neither the maximum line count, nor the maximum timespan has ellapsed.
			// Therefore we flush the collection to ensure that ALL listeners have been notified
			// of ALL changes (even if they didn't want them yet) before we enter the
			// EndOfSourceReached state.
			_listeners.Flush();
			_endOfSourceReached = true;
		}

		protected void ResetEndOfSourceReached()
		{
			_endOfSourceReached = false;
		}

		protected void StartTask()
		{
			_readTask = _scheduler.StartPeriodic(Run, TimeSpan.FromMilliseconds(1), ToString());
		}
	}
}