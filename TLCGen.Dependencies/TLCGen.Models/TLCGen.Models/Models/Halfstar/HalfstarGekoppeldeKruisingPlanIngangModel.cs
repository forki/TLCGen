﻿using System;
using TLCGen.Models.Enumerations;

namespace TLCGen.Models
{
	[Serializable]
    [IOElement("in", BitmappedItemTypeEnum.Uitgang, "IOName", "IsMaster")]
	public class HalfstarGekoppeldeKruisingPlanIngangModel : IOElementModel, IComparable
	{
		public override string Naam { get; set; }
		
		public string Plan { get; set; }
		public string Kruising { get; set; }
		public HalfstarGekoppeldTypeEnum Type { get; set; }
		
		public bool IsMaster => Type == HalfstarGekoppeldTypeEnum.Master;

		public string IOName => Kruising + Plan;

		public int CompareTo(object obj)
		{
			return string.Compare(Plan, ((HalfstarGekoppeldeKruisingPlanIngangModel) obj).Plan, StringComparison.Ordinal);
		}
	}
}