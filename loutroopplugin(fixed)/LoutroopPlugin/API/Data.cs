using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoutroopPlugin.API
{
    public static class Data
    {
		public static ReferenceHub GetSCP()
		{
			return EventHandlers.SCP181;
		}

		public static void SpawnPluginRole(ReferenceHub player)
		{
			EventHandlers.SpawnSCP181(player);
		}
	}
}
