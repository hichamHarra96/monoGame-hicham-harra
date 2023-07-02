using Microsoft.Xna.Framework;


namespace Starship
{
    public  class Saucer : Mobile
    {

        public Saucer(Vector2 position, bool isActive)
        {
            this.Position = position;
            this.IsActive = isActive;
            this.IsInScreen = true;
            
        }
        public bool IsActive;
        public bool IsInScreen;
 


    }
}
