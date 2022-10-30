using System.Collections.Generic;
using System.Linq;

namespace Sandbox.Lib
{
    public class CloneContext
    {
        public object Get(object o)
        {
            if (CloneList.Any(z => z.Original == o))
            {
                return CloneList.First(z => z.Original == o).Clone;
            }
            return null;
        }
        public List<CloneItem> CloneList = new List<CloneItem>();
        
        public object[] Static = new object[] { };
    }

}
