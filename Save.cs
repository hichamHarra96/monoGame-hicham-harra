
namespace Starship
{
    public interface Save
    {
        public void SaveGame();
        public GameSave LoadGame();
        public void ClearSave();
    }
        
}
