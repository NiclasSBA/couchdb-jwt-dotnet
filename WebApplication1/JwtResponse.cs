using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication1
{
    public class JwtResponse
    {
        public string Token { get; set; }
        public long ExpiresAt { get; set; }

    }
}
