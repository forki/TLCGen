﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace TLCGen.Models
{
    [Serializable]
    [RefersToSignalGroup("FaseCyclus")]
    public class ModuleFaseCyclusModel
    {
        #region Properties

        public string FaseCyclus { get; set; }

        [XmlArrayItem(ElementName = "Fase")]
        public List<ModuleFaseCyclusAlternatiefModel> Alternatieven { get; set; }

        #endregion // Properties

        #region Constructor

        public ModuleFaseCyclusModel()
        {
            Alternatieven = new List<ModuleFaseCyclusAlternatiefModel>();
        }
        
        #endregion // Constructor

    }
}
