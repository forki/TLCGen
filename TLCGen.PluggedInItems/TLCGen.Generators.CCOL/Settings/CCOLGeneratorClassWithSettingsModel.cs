﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TLCGen.Generators.CCOL.Settings
{
    [Serializable]
    public class CCOLGeneratorClassWithSettingsModel
    {
        public string ClassName { get; set; }
        public string Description { get; set; }

        [XmlArrayItem(ElementName = "Setting")]
        public List<CCOLGeneratorCodeStringSettingModel> Settings { get; set; }
    }
}
