﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Helpers;

namespace TLCGen.Models.Enumerations
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum NaloopTypeEnum
    {
        [Description("Start groen")]
        StartGroen,
        [Description("Einde groen")]
        EindeGroen,
        [Description("Cyclisch verlenggroen")]
        CyclischVerlengGroen
    }
}
