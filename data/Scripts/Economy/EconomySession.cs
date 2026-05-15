using System;
using System.Collections.Generic;
using System.Security.Policy;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.Utils;
using BlendTypeEnum = VRageRender.MyBillboard.BlendTypeEnum;
using System.Linq; // required for MyTransparentGeometry/MySimpleObjectDraw to be able to set blend type.

namespace FAM.Economy
{
    // This object is always present, from the world load to world unload.
    // NOTE: all clients and server run mod scripts, keep that in mind.
    // NOTE: this and gamelogic comp's update methods run on the main game thread, don't do too much in a tick or you'll lower sim speed.
    // NOTE: also mind allocations, avoid realtime allocations, re-use collections/ref-objects (except value types like structs, integers, etc).
    //
    // The MyUpdateOrder arg determines what update overrides are actually called.
    // Remove any method that you don't need, none of them are required, they're only there to show what you can use.
    // Also remove all comments you've read to avoid the overload of comments that is this file.
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation)]
    public partial class EconomySession : MySessionComponentBase
    {   
        private int _tickCount = 0;
        private bool _failed = false;
        public MyModConfig _config;
        public static EconomySession Instance; // the only way to access session comp from other classes and the only accepted static field.

        public VendorManager vendorManager;
        public override void LoadData()
        {
            // amogst the earliest execution points, but not everything is available at this point.

            // These can be used anywhere, not just in this method/class:
            // MyAPIGateway. - main entry point for the API
            // MyDefinitionManager.Static. - reading/editing definitions
            // MyGamePruningStructure. - fast way of finding entities in an area
            // MyTransparentGeometry. and MySimpleObjectDraw. - to draw sprites (from TransparentMaterials.sbc) in world (they usually live a single tick)
            // MyVisualScriptLogicProvider. - mainly designed for VST but has its uses, use as a last resort.
            // System.Diagnostics.Stopwatch - for measuring code execution time.
            // ...and many more things, ask in #programming-modding in keen's discord for what you want to do to be pointed at the available things to use.
            _config = LoadConfig();
            
            Instance = this;
        }

        public override void BeforeStart()
        {
            // executed before the world starts updating
            if (!_failed) return;
            InitEconomy();
        }

        protected override void UnloadData()
        {
            // always catch errors here because throwing them will NOT crash the game and instead prevent other mods from unloading properly, causing all sorts of hidden issues...
            try
            {
                // executed when world is exited to unregister events and stuff.
            }
            catch(Exception e)
            {
                MyLog.Default.Error(e.ToString());
            }
            finally
            {
                Instance = null; // important for avoiding this instance and all its references to remain allocated in memory
                if (!_failed)
                {
                    MyAPIGateway.Entities.OnEntityAdd -= OnEntityAdded;
                    MyAPIGateway.Entities.OnEntityRemove -= OnEntityRemoved;
                }
            }
        }

        public override void HandleInput()
        {
            // gets called 60 times a second before all other update methods, regardless of framerate, game pause or MyUpdateOrder.
        }

        public override void UpdateBeforeSimulation()
        {
            // executed every tick, 60 times a second, before physics simulation and only if game is not paused.
        }

        public override void Simulate()
        {
            // executed every tick, 60 times a second, during physics simulation and only if game is not paused.
            // NOTE in this example this won't actually be called because of the lack of MyUpdateOrder.Simulation argument in MySessionComponentDescriptor
        }

        public override void UpdateAfterSimulation()
        {
            // executed every tick, 60 times a second, after physics simulation and only if game is not paused.

            try // example try-catch for catching errors and notifying player, use only for non-critical code!
            {
                // ...
            }
            catch(Exception e) // NOTE: never use try-catch for code flow or to ignore errors! catching has a noticeable performance impact.
            {
                MyLog.Default.WriteLineAndConsole(e.ToString());

                if(MyAPIGateway.Session?.Player != null)
                    MyAPIGateway.Utilities.ShowNotification($"[ ERROR: {GetType().FullName}: {e.Message} | Send SpaceEngineers.Log to mod author ]", 10000, MyFontEnum.Red);
            }
        }

        public override void Draw()
        {
            // gets called 60 times a second after all other update methods, regardless of framerate, game pause or MyUpdateOrder.
            // NOTE: this is the only place where the camera matrix (MyAPIGateway.Session.Camera.WorldMatrix) is accurate, everywhere else it's 1 frame behind.
        }

        public override void SaveData()
        {
            // executed AFTER world was saved
        }

        public override MyObjectBuilder_SessionComponent GetObjectBuilder()
        {
            // executed during world save, most likely before entities.

            return base.GetObjectBuilder(); // leave as-is.
        }

        public override void UpdatingStopped()
        {
            // executed when game is paused
        }

        public void InitEconomy()
        {
            try
            {
                this.vendorManager = new VendorManager(_config.VendorConfig, _config.BlockConfig, _config.ProductsConfig);
            }
            catch
            {
                
            }
            

            MyAPIGateway.Entities.OnEntityAdd += OnEntityAdded;
            MyAPIGateway.Entities.OnEntityRemove += OnEntityRemoved;

            HashSet<IMyEntity> existingEntities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(existingEntities);

            foreach (var entity in existingEntities)
            {
                IMyCubeGrid grid = entity as IMyCubeGrid;
                if (grid == null) return;
                this.vendorManager.gridManager.CheckAndAddGrid(grid);
            }
        }

        public void OnEntityAdded(IMyEntity entity)
        {
            IMyCubeGrid grid = entity as IMyCubeGrid;
            if (grid == null) return;

            Dictionary<string, Dictionary<string,string>> factionBlocks = new Dictionary<string, Dictionary<string, string>>();
            foreach (var vendor in vendorManager.Vendors.Values)
            {
                factionBlocks.Add(vendor.Tag, vendor.BlockData);
            }

            this.vendorManager.gridManager.CheckAndAddGrid(grid);
        }

        private void OnEntityRemoved(IMyEntity entity)
        {
            this.vendorManager.gridManager.RemoveGrid(entity.EntityId);
        }
    }
}