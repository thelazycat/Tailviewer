﻿using System;
using Tailviewer.BusinessLogic.Analysis.Analysers;

namespace Tailviewer.BusinessLogic.Analysis
{
	public sealed class NoAnalyser
		: IDataSourceAnalyser
	{
		private readonly Guid _id;

		public NoAnalyser()
		{
			_id = Guid.NewGuid();
		}

		public Guid Id => _id;
		public ILogAnalysisResult Result => null;

		public bool IsFrozen => false;

		public ILogAnalyserConfiguration Configuration { get; set; }
	}
}