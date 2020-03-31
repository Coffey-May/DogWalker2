using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalker2.Models
{
    public class Owner
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Address { get; set; }
        public Dogs Dogs { get; set; }

        [Required]
        public Neighborhood Neighborhood { get; set; }
        public int NeighborhoodId { get; set; }
        public string Phone { get; set; }
        public List<Dog> DogsL { get; set; }
    }
}
