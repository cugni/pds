using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pds2.Shared.Messages
{
    [Serializable]
    public class ChallengeMessage : SendableObj<ChallengeMessage>
    {
        public string salt;
    }
}