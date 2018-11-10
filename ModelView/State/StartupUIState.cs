﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using ISBD.View;

namespace ISBD.ModelView.State
{
	class StartupUIState : ConnectorState<IStartupUI>
	{
		private DispatcherTimer DispatcherTimer;
		private readonly int MaxTicks = 5;
		private int CurrentTicks = 0;
		
		protected override Type DefaultType => typeof(StartupPage);

		public override void StartState()
		{
			base.StartState();
			DispatcherTimer = new System.Windows.Threading.DispatcherTimer();
			DispatcherTimer.Tick += dispatcherTimer_Tick;
			DispatcherTimer.Interval = new TimeSpan(0, 0, 1);
			DispatcherTimer.Start();
		}

		private void dispatcherTimer_Tick(object sender, EventArgs e)
		{
			Connector.SetText((++CurrentTicks).ToString());
			if (CurrentTicks == MaxTicks)
			{
				DispatcherTimer.Stop();
				StateMachine.Instance.PushState<SecondState>(null);
			}
		}
	}
}