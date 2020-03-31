using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DogWalker2.Models
{
    public class Dog
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public int OwnerId { get; set; }
        public Owner Owner { get; set; }
        public string Breed { get; set; }
        public string Notes { get; set; }
    }
}
