﻿using System.Windows.Media;
using Metrolib;

namespace Tailviewer.Ui.Controls.SidePanel
{
	internal sealed class AnalysesSidePanel
		: AbstractSidePanelViewModel
	{
		public override Geometry Icon => Icons.ChartGantt;

		public override string Id => "Analysis";

		public override void Update()
		{
			
		}
	}
}