using System.IO;
using Patchwork;
using Patchwork.AutoPatching;

[assembly: PatchAssembly]
[PatchInfo]
public class TyrannyPatchInfo : IPatchInfo
{
	public TyrannyPatchInfo()
	{
	}

	public FileInfo GetTargetFile(AppInfo app)
	{
		FileInfo info = new FileInfo(@"D:\Games\Tyranny\Tyranny_Data\Managed\Assembly-CSharp-firstpass.dll");
		return info;
	}

	public string CanPatch(AppInfo app)
	{
		return null;
	}

	public string PatchVersion { get { return "1.0.0.0008"; } }
	public string Requirements { get { return "None"; } }
	public string PatchName { get { return "TyrannyMain"; } }
}
