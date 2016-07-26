using System.Globalization;
using System.Reflection;
using Craxy.CitiesSkylines.ToggleTrafficLights.Game.UI.Menu;
using Craxy.CitiesSkylines.ToggleTrafficLights.Utils;
using ICities;

namespace Craxy.CitiesSkylines.ToggleTrafficLights
{
    public sealed class Mod : LoadingExtensionBase, IUserMod
    {

    
        public string Name
        {
            get { return "Toggle Traffic Lights"; }
        }

        public string Description
        {
            get
            {
                return "Remove or add traffic lights at intersections.";
            }
        }

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

        }

        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

        }

        public override void OnReleased()
        {
            base.OnReleased();

        }

        public void OnEnabled()
        {

        }

        public void OnDisabled()
        {

        }

        //// do not use option menu since options for elements are extremly limited...
        //// called (AND):
        ////  - loading main menu
        ////  - loading level: before Deserializing (custom) data
        //public void OnSettingsUI(UIHelperBase helper)
        //{
        //    //SettingsUi.Create(helper);
        //}
    }
}