using Microsoft.Xna.Framework;


namespace Starship
{
    public abstract class Mobile
    {
        public Vector2 Position;

        public Vector2 GetPosition()
        {
            return this.Position;
        }
        public void SetPosition( Vector2 position)
        {
             this.Position = position;
        }

        public void Move(string direction, float speed)
        {
            switch (direction)
            {
                case "left":
                    this.Position.X -= speed;
                    break;
                case "right":
                    this.Position.X += speed;
                    break;
                case "up":
                    this.Position.Y -= speed;
                    break;
                case "down":
                    this.Position.Y += speed;
                    break;
                default:
                    break;
            }
        }


    }
}
