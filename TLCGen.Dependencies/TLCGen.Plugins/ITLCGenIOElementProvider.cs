﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Models;

namespace TLCGen.Plugins
{
    public interface ITLCGenElementProvider : ITLCGenPlugin
    {
        List<IOElementModel> GetOutputItems();
        List<IOElementModel> GetInputItems();
        List<object> GetAllItems();

        bool IsElementNameUnique(string name);
    }
}
