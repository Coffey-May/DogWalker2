using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalker2.Models
{
    public class Owner
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public Dogs Dogs { get; set; }
    
        public Neighborhood Neighborhood { get; set; }
        public int NeighborhoodId { get; set; }
        public string Phone { get; set; }
        public List<Dog> DogsL { get; set; }
    }
}
