using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalker2.Models
{
    public class Walker
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int NeighborhoodId { get; set; }
        public List<Walks> Walks { get; set; }

    }

}
