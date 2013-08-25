using MediaBrowser.Controller.Entities;

namespace GameBrowser.Entities
{
    /// <summary>
    /// Class ConsoleFolder
    /// </summary>
    public class GamePlatform : Folder
    {
        public override System.Guid DisplayPreferencesId
        {
            get
            {
                return Id;
            }
        }
    }
}
