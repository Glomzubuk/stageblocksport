
using System.Reflection;
using StageBlocks;

#region Assembly attributes
/*
 * These attributes define various metainformation of the generated DLL.
 * In general, you don't need to touch these. Instead, edit the values in PluginInfo.
 */
[assembly: AssemblyVersion(PluginInfos.PLUGIN_VERSION)]
[assembly: AssemblyTitle(PluginInfos.PLUGIN_NAME + " (" + PluginInfos.PLUGIN_ID + ")")]
[assembly: AssemblyProduct(PluginInfos.PLUGIN_NAME)]
#endregion

namespace StageBlocks
{
    public static class PluginInfos
    {
        public const string PLUGIN_NAME = "StageBlocks";
        public const string PLUGIN_ID = "us.wallace.plugins.llb.stageblocks";
        public const string PLUGIN_VERSION = "1.0.6";
    }
}
