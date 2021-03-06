﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using TLCGen.Models.Enumerations;

namespace TLCGen.Models
{
    [Serializable]
    [RefersToSignalGroup("Naam")]
    [IOElement("", BitmappedItemTypeEnum.Fase, "Naam")]
    public class FaseCyclusModel : IOElementModel, IComparable
    {
        #region Fields

        #endregion // Fields

        #region Properties
        
        [ModelName]
        [Browsable(false)]
        public override string Naam { get; set; }
        [Browsable(false)]
        public FaseTypeEnum Type { get; set; }

        public int TFG { get; set; }
        public int TGG { get; set; }
        public int TGG_min { get; set; }
        public int TRG { get; set; }
        public int TRG_min { get; set; }
        public int TGL { get; set; }
        public int TGL_min { get; set; }
        public int Kopmax { get; set; }
        [Browsable(false)]
        public bool OVIngreep { get; set; }
        [Browsable(false)]
        public bool HDIngreep { get; set; }
        public NooitAltijdAanUitEnum VasteAanvraag { get; set; }
        public NooitAltijdAanUitEnum Wachtgroen { get; set; }
        public NooitAltijdAanUitEnum Meeverlengen { get; set; }
        public MeeVerlengenTypeEnum MeeverlengenType { get; set; }
        public int? MeeverlengenVerschil { get; set; }
        public bool UitgesteldeVasteAanvraag { get; set; }
        public int UitgesteldeVasteAanvraagTijdsduur { get; set; }
        public int? AantalRijstroken { get; set; }
        public bool HiaatKoplusBijDetectieStoring { get; set; }
        public int? VervangendHiaatKoplus { get; set; }
        public bool AanvraagBijDetectieStoring { get; set; }
        public bool PercentageGroenBijDetectieStoring { get; set; }
        public int? PercentageGroen { get; set; }
        public int VeiligheidsGroenMinMG { get; set; }
        public int VeiligheidsGroenTijdsduur { get; set; }

        [XmlArrayItem(ElementName = "Detector")]
        public List<DetectorModel> Detectoren { get; set; }

        #endregion // Properties

        #region IComparable

        public int CompareTo(object obj)
        {
            if(obj is FaseCyclusModel)
            {
                string s1 = (obj as FaseCyclusModel).Naam;
                string s2 = this.Naam;
                if (s1.Length < s2.Length) s1 = s1.PadLeft(s2.Length, '0');
                else if (s2.Length < s1.Length) s2 = s2.PadLeft(s1.Length, '0');

                return s2.CompareTo(s1);
            }
            return 0;
        }

        #endregion // IComparable

        #region Constructor

        public FaseCyclusModel() : base()
        {
            Detectoren = new List<DetectorModel>();
            AantalRijstroken = 1;
        }

        #endregion // Constructor
    }
}
