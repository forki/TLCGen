﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Generators.CCOL.Settings;
using TLCGen.Models;

namespace TLCGen.Generators.CCOL.CodeGeneration
{
    public class CCOLElement
    {
        public string Define { get; set; }
        public string Naam { get; set; }
        public CCOLElementTimeTypeEnum TType { get; set; }
        public int? Instelling { get; set; }
        public string Commentaar { get; set; }
        public CCOLElementTypeEnum Type { get; set; }
        public bool Dummy { get; set; }

        public CCOLElement()
        {
            Dummy = false;
        }

        public CCOLElement(string naam, CCOLElementTypeEnum type, string description = null)
        {
            Dummy = false;
            Naam = naam;
            Define = CCOLGeneratorSettingsProvider.Default.GetPrefix(type) + naam;
            Type = type;
			Commentaar = description;
            switch(Type)
            {
                case CCOLElementTypeEnum.Schakelaar:
                    TType = CCOLElementTimeTypeEnum.SCH_type;
                    break;
                default:
                    TType = CCOLElementTimeTypeEnum.None;
                    break;
            }
        }

        public CCOLElement(string naam, int instelling, CCOLElementTimeTypeEnum ttype, CCOLElementTypeEnum type, string description = null)
        {
            Dummy = false;
            Naam = naam;
            Define = CCOLGeneratorSettingsProvider.Default.GetPrefix(type) + naam;
            Instelling = instelling;
            TType = ttype;
            Type = type;
			Commentaar = description;
        }
    }
}
