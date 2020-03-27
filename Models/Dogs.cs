using System;
using System.Collections.Generic;

namespace DogWalker2.Models
{
    public class Dogs
    {
        public static implicit operator Dogs(List<Dog> v)
        {
            throw new NotImplementedException();
        }
    }
}