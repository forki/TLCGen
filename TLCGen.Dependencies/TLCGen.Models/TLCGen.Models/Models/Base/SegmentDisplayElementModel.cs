﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Models.Enumerations;

namespace TLCGen.Models
{
    [Serializable]
    public class SegmentDisplayElementModel
    {
        [IOElement("", BitmappedItemTypeEnum.Uitgang, "Naam")]
        public BitmapCoordinatenDataModel BitmapData { get; set; }
        private string _Naam;
        public string Naam
        {
            get
            {
                return _Naam;
            }
            set
            {
                _Naam = value;
                BitmapData.Naam = value;
            }
        }

        public SegmentDisplayElementModel()
        {
            BitmapData = new BitmapCoordinatenDataModel();
        }
    }
}
