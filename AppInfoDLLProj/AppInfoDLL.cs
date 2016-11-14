using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Patchwork.AutoPatching;
using System.IO;

namespace AppInfoDLLProj
{
	[AppInfoFactory]
	public class AppInfoDLL : AppInfoFactory
	{
		public override AppInfo CreateInfo(DirectoryInfo folderInfo)
		{
			AppInfo ai = new AppInfo();
			ai.AppName = "Tyranny";
			ai.AppVersion = "1.0.0.0008";
			ai.BaseDirectory = folderInfo;
			ai.Executable = new FileInfo("D:/Games/Tyranny/Tyranny.exe");
			ai.IconLocation = new FileInfo("D:/Games/Tyranny/goggame-1266051739.ico");

			return ai;
		}
	}
}