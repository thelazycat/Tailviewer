﻿using System.Windows.Media;
using Metrolib;

namespace Tailviewer.Ui.Controls.MainPanel
{
	public sealed class LogViewMainPanelEntry
		: IMainPanelEntry
	{
		public string Title => "Raw";

		public string Id => "log";

		public Geometry Icon => Icons.ViewHeadline;
	}
}