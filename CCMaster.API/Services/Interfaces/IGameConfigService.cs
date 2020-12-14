using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CCMaster.API.Services
{
    public interface IGameConfigService
    {
        public DORankDefinition GetRank(int score);
    }
}
