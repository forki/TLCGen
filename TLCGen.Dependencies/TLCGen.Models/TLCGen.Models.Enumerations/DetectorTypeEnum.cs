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
    public enum DetectorTypeEnum
    {
        [Description("Koplus")]
        Kop,
        [Description("Lange lus")]
        Lang,
        [Description("Verweg lus")]
        Verweg,
        [Description("File lus")]
        File,
        [Description("Drukknop")]
        Knop,
        [Description("Drukknop binnen")]
        KnopBinnen,
        [Description("Drukknop buiten")]
        KnopBuiten,
        [Description("Radar")]
        Radar,
        [Description("Vecom ingang")]
        VecomIngang,
        [Description("Opticom ingang")]
        OpticomIngang,
        [Description("Overig")]
        Overig
    }
}
