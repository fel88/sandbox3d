namespace Sandbox.Lib
{
    public class IkBone : IObjectClonable
    {
        public IkBone()
        {
            Name = "";
        }
        
        public int Id { get; set; }
        public string Name { get; set; }

        public object Tag { get; set; }
        public IkBonePool Parent;

        public virtual void SetScale(float scale) { }
        public virtual void CalcAbsolute() { }


        public virtual object Clone(CloneContext context)
        {
            if (context.Get(this) != null)
            {
                return context.Get(this);
            }
            var b = new IkBone() { Name = Name, Id = Id };
            context.CloneList.Add(new CloneItem() { Original = this, Clone = b });


            b.Parent = (IkBonePool)Parent.Clone(context);
            return b;
        }
    }

}
