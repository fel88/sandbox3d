namespace RenderTool
{
    public class SimpleModel : SceneObject
    {

        public SimpleModel(Scene sc) : base(sc)
        {

        }
        public Model Model;

        public override void Draw(IDrawingEnvironment denv)
        {
            Model.Draw();
        }
    }
}


