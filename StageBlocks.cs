using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using LLHandlers;
using BepInEx.Logging;
using BepInEx.Configuration;
using LLBML.Utils;


namespace StageBlocks
{
    [BepInPlugin(PluginInfos.PLUGIN_ID, PluginInfos.PLUGIN_NAME, PluginInfos.PLUGIN_VERSION)]
    [BepInDependency(LLBML.PluginInfos.PLUGIN_ID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("no.mrgentle.plugins.llb.modmenu", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInProcess("LLBlaze.exe")]

    public class StageBlocks : BaseUnityPlugin

    {
        public static ManualLogSource Log { get; private set; } = null;
        public static StageBlocks Instance { get; private set; }
        public static ConfigEntry<bool> customStageBlocks;


        public string BlocksConfigPath => Path.Combine(ModdingFolder.GetModSubFolder(this.Info).FullName, "BlockConfig.txt");
        public Dictionary<Stage, List<StageBlock>> stageBlocks = new Dictionary<Stage, List<StageBlock>>();


        void Awake()
        {
            Log = this.Logger;
            Instance = this;



            Config.Bind("Use Custom Stage Blocks", "mm_header_qol", "Use Custom Stage Blocks", new ConfigDescription("", null, "modmenu_header"));
            customStageBlocks = Config.Bind<bool>("StageBlocksToggle", "enableCustomStageBlocks", true);


            Logger.LogDebug("liuberal poop hippie pronoun fortnite be farting pooping bacon epic, i love ivermectin flouride apartmentism");

            var harmony = new Harmony(this.Info.Metadata.GUID);
            harmony.PatchAll(typeof(WorldSetStageDimensionsPatch));
            harmony.PatchAll(typeof(NitroStuckFix));

            Logger.LogDebug($"{this.Info.Metadata.Name} is loaded");
        }

        void Start()
        {
            ModDependenciesUtils.RegisterToModMenu(this.Info);
            LoadConfig();
        }

        const string CONFIG_HEADER =
@"# The format is <stage name>: <pos x>, <pos y>, <size x>, <size y>; #<hexR><hexG><hexB><hexEmmissive>
# One block per line, '#' is a comment line.
# stage xy origin is bottom center of the stage
# block xy origin is bottom left of the block
# Refer to readme for stage names (case sensitive)
";
        const string DEFAULT_CONFIG =
@"Outskirts: -625, 205, 250, 54; #070B12FF
Outskirts: 375, 205, 250, 54; #231E30FF
Sewers: -66, 285, 132, 290 ; #7E0C8555
Desert: -65, 193, 630, 54 ; #393521FF
Elevator: -740, -4, 280, 190 ; #000000FF
Elevator: 460, -4, 280, 190 ; #0000000FF
Factory: -670, 350, 350, 200 ; #322E3655
Factory: 425, -8, 350, 200 ; #91663055
Subway: -425, -10, 250, 120 ; #B04E2B77
Subway: 135, -10, 250, 120 ; #B04E2B77
Stadium: -356, 211.5, 82, 57 ; #B4DED277
Stadium: 274, 211.5, 82, 57 ; #B4DED277
Streets: -410, 215, 300, 54 ; #C45B2477
Streets: 110, 215, 300, 54 ; #C45B2477
Pool: -310.5, 193, 621, 54 ; #4D3A1E77
Room21: 391.5, -1, 231, 186 ; #000000FF
Room21: -370, 205, 280, 54 ; #000000FF
Retro Factory: -1628, -200, 1415.75, 10000; #FF0000
Retro Factory: 203.5, -200, 10000, 10000; #FF0000
Retro Factory: -2000, -100, 10000, 250; #FF0000
Retro Factory: -2000, 566, 10000, 200; #FF0000
";

        private void CreateConfig()
        {
            using (TextWriter configFile = File.CreateText(BlocksConfigPath))
            {
                configFile.Write(CONFIG_HEADER + DEFAULT_CONFIG);
            }
        }

        public void LoadConfig()
        {
            if (!File.Exists(BlocksConfigPath))
            {
                CreateConfig();
            }
            using (TextReader config = File.OpenText(BlocksConfigPath))
            {
                ReadConfig(config, stageBlocks);
            }
        }

        private static readonly Dictionary<Stage, string> allStagesMapping = StringUtils.regularStagesNames.Union(StringUtils.retroStagesNames).ToDictionary(x => x.Key, x => x.Value);
        public void WriteConfig(TextWriter writer, Dictionary<Stage, List<StageBlock>> config)
        {
            writer.Write(CONFIG_HEADER);
            foreach (Stage stage in config.Keys)
            {
                foreach (StageBlock block in config[stage])
                {
                    writer.WriteLine(allStagesMapping[stage] + ": " + block.ToString());
                }
            }
        }

        private static readonly Dictionary<string, Stage> reverseStageMapping = allStagesMapping.ToDictionary(x => x.Value, x => x.Key);
        public void ReadConfig(TextReader reader, Dictionary<Stage, List<StageBlock>> config)
        {
            config.Clear();
            while (reader.Peek() >= 0)
            {
                string line = reader.ReadLine();
                try
                {
                    if (line[0] == '#')
                    {
                        continue;
                    }
                    string[] splits = line.Split(':');
                    if (!reverseStageMapping.ContainsKey(splits[0]))
                    {
                        this.Logger.LogWarning("Unknown stage name: " + splits[0]);
                        continue;
                    }
                    Stage stage = reverseStageMapping[splits[0]];
                    if (!config.ContainsKey(stage))
                    {
                        config.Add(stage, new List<StageBlock>());
                    }
                    try
                    {
                        config[stage].Add(StageBlock.FromString(splits[1]));
                    }
                    catch (FormatException e)
                    {
                        this.Logger.LogWarning($"Couldn't convert floats from line \"{line}\" \n" + e);
                    }
                }
                catch (Exception e)
                {
                    this.Logger.LogWarning($"Couldn't parse data from line \"{line}\" \n" + e);
                }
            }
        }

    }

}
