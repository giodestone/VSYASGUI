using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using VSYASGUI;

namespace VSTAGUI_Mod
{
    public class VSYASGUI_ModModSystem : ModSystem
    {
        ICoreServerAPI _Api;
        HttpApi _HttpApi;
        Config _Config;

        // Called on server and client
        // Useful for registering block/entity classes on both sides
        public override void Start(ICoreAPI api)
        {   
            
            Mod.Logger.Notification("Hello from template mod: " + api.Side);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            Mod.Logger.Notification("Hello from template mod server side: " + Lang.Get("vstagui-mod:hello"));

            _Api = api;

            try
            {
                _Config = Config.LoadOrCreate(api);
            } 
            catch (Exception e)
            {
                api.Logger.Error($"VSYASGUI-Mod: Failed to load config. Please check the config, or delete it and restart the server to generate a new one. The GUI will not function.");
                api.Logger.LogException(EnumLogType.Error, e);
                return;
            }

            _HttpApi = new HttpApi(_Config, api);

            try
            {
                _HttpApi.Start();
            }
            catch (Exception e)
            {
                api.Logger.Error($"VSYASGUI-Mod: Failed to start HTTP API. Check excpetion for exact details. The GUI will not function.");
                api.Logger.LogException(EnumLogType.Error, e);
                return;
            }
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            Mod.Logger.Notification("Hello from template mod client side: " + Lang.Get("vstagui-mod:hello"));
        }

    }
}
