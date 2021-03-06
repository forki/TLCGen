﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Models.Enumerations;

namespace TLCGen.Models
{
    [Serializable]
    [RefersToSignalGroup("FaseCyclus")]
    [RefersToDetector("VanDetector", "NaarDetector")]
    public class RichtingGevoeligVerlengModel
    {
        #region Properties

        [HasDefault(false)]
        public string FaseCyclus { get; set; }
        [HasDefault(false)]
        public string VanDetector { get; set; }
        [HasDefault(false)]
        public string NaarDetector { get; set; }
        public int MaxTijdsVerschil { get; set; }
        public int VerlengTijd { get; set; }
        public RichtingGevoeligVerlengenTypeEnum TypeVerlengen { get; set; }

        #endregion // Properties
    }
}
