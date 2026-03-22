using System;
using System.Diagnostics;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using VSYASGUI_Mod;

namespace VSTAGUI_Mod
{
    public class VSYASGUI_ModModSystem : ModSystem
    {
        ICoreServerAPI _Api;
        HttpApi _HttpApi;
        Config _Config;
        Guid _InstanceGuid;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {   
            
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("VSYASGUI_Mod: Starting server instance." + Lang.Get("vsyasgui-mod:hello"));

            _InstanceGuid = Guid.NewGuid();

            _Api = api;

            try
            {
                _Config = Config.LoadOrCreate(api);
            } 
            catch (Exception e)
            {
                api.Logger.Error($"VSYASGUI_Mod: Failed to load config. Please check the config, or delete it and restart the server to generate a new one. The GUI will not function.");
                api.Logger.LogException(EnumLogType.Error, e);
                return;
            }

            api.Logger.Log(EnumLogType.Notification, $"Setting up VSAYSGUI HTTP API at endpoint {_Config.BindURL}   Api key: {_Config.ApiKey}");

            LogCache logCache = new(api, _Config);
            CpuLoadCalc cpuLoadCalc = new(Process.GetCurrentProcess(), _Config);
            _HttpApi = new HttpApi(api, _Config, logCache, _InstanceGuid, cpuLoadCalc);

            try
            {
                _HttpApi.Start();
            }
            catch (Exception e)
            {
                api.Logger.Error($"VSYASGUI_Mod: Failed to start HTTP API. Check excpetion for exact details. The GUI will not function.");
                api.Logger.LogException(EnumLogType.Error, e);
                return;
            }

        }

        public override void StartClientSide(ICoreClientAPI api)
        {
        }

    }
}
