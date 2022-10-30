namespace Sandbox.Lib
{
    public class IkLimit : IObjectClonable
    {
        public float Min { get; set; }
        public float Max { get; set; }
        public object Clone(CloneContext context)
        {
            if (context.Get(this) != null)
            {
                return context.Get(this);
            }
            var r = new IkLimit() { Max = Max, Min = Min };
            context.CloneList.Add(new CloneItem() { Original = this, Clone = r });
            return r;
        }
    }

}
