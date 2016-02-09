﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using log4net;

namespace Tailviewer.BusinessLogic
{
	/// <summary>
	///     Responsible for merging two or more <see cref="ILogFile" /> instances into one coherent view.
	///     <see cref="LogLine" />s which have no <see cref="LogLine.Timestamp" /> set (or who's parent doesn't)
	///     are discarded from this representation.
	/// </summary>
	internal sealed class MergedLogFile
		: ILogFile
		, ILogFileListener
	{
		private static readonly ILog Log =
			LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly ILogFile[] _sources;
		private readonly Task _readTask;
		private readonly List<Index> _indices;
		private readonly LogFileListenerCollection _listeners;
		private readonly CancellationTokenSource _cancellationTokenSource;
		private readonly ConcurrentQueue<PendingModification> _pendingModifications;

		struct PendingModification
		{
			public readonly ILogFile LogFile;
			public readonly LogFileSection Section;

			public PendingModification(ILogFile logFile, LogFileSection section)
			{
				LogFile = logFile;
				Section = section;
			}
		}

		/// <summary>
		/// Represents an index in the merged data-structure.
		/// Points towards a particular <see cref="LogLine"/> of a particular
		/// <see cref="ILogFile"/>.
		/// </summary>
		struct Index
		{
			public Index(int logLineIndex, ILogFile logFile)
			{
				LogLineIndex = logLineIndex;
				LogFile = logFile;
			}

			public readonly int LogLineIndex;
			public readonly ILogFile LogFile;
		}

		private const int BatchSize = 1000;

		public MergedLogFile(params ILogFile[] sources)
		{
			if (sources == null) throw new ArgumentNullException("sources");
			if (sources.Any(x => x == null)) throw new ArgumentException("sources", "sources.Any(x => x == null)");

			_sources = sources;
			_cancellationTokenSource = new CancellationTokenSource();
			_readTask = new Task(Merge,
								 _cancellationTokenSource.Token,
								 _cancellationTokenSource.Token,
								 TaskCreationOptions.LongRunning);
			_listeners = new LogFileListenerCollection(this);
			_pendingModifications = new ConcurrentQueue<PendingModification>();
			_indices = new List<Index>();
		}

		private void Merge(object obj)
		{
			CancellationToken token = _cancellationTokenSource.Token;
			var entries = new LogLine[BatchSize];
			int currentSourceIndex = 0;
			var lastLogEntry = new List<LogLine>();

			while (!token.IsCancellationRequested)
			{
				PendingModification section;
				while (_pendingModifications.TryDequeue(out section))
				{
					
				}

				
			}
		}

		public void Start(TimeSpan maximumWaitTime)
		{
			if (_readTask.Status == TaskStatus.Created)
			{
				foreach (var logFile in _sources)
				{
					logFile.AddListener(this, maximumWaitTime, BatchSize);
				}
				_readTask.Start();
			}
		}

		public void Dispose()
		{
			_cancellationTokenSource.Cancel();
			try
			{
				_readTask.Wait();
			}
			catch (AggregateException e)
			{
				Log.DebugFormat("Caught exception while disposing: {0}", e);
			}
			_readTask.Dispose();
		}

		public void Wait()
		{
			throw new NotImplementedException();
		}

		public DateTime? StartTimestamp
		{
			get { throw new NotImplementedException(); }
		}

		public int Count
		{
			get { throw new NotImplementedException(); }
		}

		public void AddListener(ILogFileListener listener, TimeSpan maximumWaitTime, int maximumLineCount)
		{
			_listeners.AddListener(listener, maximumWaitTime, maximumLineCount);
		}

		public void Remove(ILogFileListener listener)
		{
			_listeners.RemoveListener(listener);
		}

		public void GetSection(LogFileSection section, LogLine[] dest)
		{
			for (int i = 0; i < section.Count; ++i)
			{
				// TODO: This seems rubbish - maybe I should change the interface to LogLineIndex altogether?
				dest[i] = GetLine((int)(section.Index + i));
			}
		}

		public LogLine GetLine(int index)
		{
			var source = _indices[index];
			var line = source.LogFile.GetLine(source.LogLineIndex);
			return line;
		}

		public void OnLogFileModified(ILogFile logFile, LogFileSection section)
		{
			_pendingModifications.Enqueue(new PendingModification(logFile, section));
		}
	}
}