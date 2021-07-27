using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpacyLearnData
{
   public class Trainig
    {
        public string Data;

        public List<Entity> entities;
    }

    public class Entity
    {
        public int start;
        public int end;
        public string value;
        public string Entityname;
    }
}
