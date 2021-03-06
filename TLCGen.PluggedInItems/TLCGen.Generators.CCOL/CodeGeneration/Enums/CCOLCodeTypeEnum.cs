﻿namespace TLCGen.Generators.CCOL.CodeGeneration
{
    public enum CCOLCodeTypeEnum
    {
		// REG
        RegCIncludes,
        RegCTop,
        RegCKwcApplication,
        RegCPreApplication,
        RegCKlokPerioden,
        RegCAanvragen,
        RegCVerlenggroen,
        RegCMaxgroen,
        RegCWachtgroen,
        RegCMeetkriterium,
        RegCFileVerwerking,
        RegCDetectieStoring,
        RegCMeeverlengen,
        RegCRealisatieAfhandelingModules,
        RegCRealisatieAfhandelingNaModules,
        RegCRealisatieAfhandeling,
        RegCAlternatieven,
        RegCSynchronisaties,
        RegCInitApplication,
        RegCApplication,
        RegCPostApplication,
        RegCPreSystemApplication,
        RegCSystemApplication,
        RegCPostSystemApplication,
        RegCDumpApplication,

		// OV
	    OvCIncludes,
		OvCTop,
		OvCInstellingen,
		OvCRijTijdScenario,
		OvCInUitMelden,
	    OvCPARCorrecties,
	    OvCPARCcol,
	    OvCSpecialSignals,
		OvCBottom,

		// TAB
	    TabCControlParameters,

		// HST
		HstCPostInitApplication,
		HstCPreApplication,
		HstCKlokPerioden,
		HstCAanvragen,
		HstCVerlenggroen,
		HstCMaxgroen,
		HstCWachtgroen,
		HstCMeetkriterium,
		HstCDetectieStoring,
		HstCMeeverlengen,
		HstCSynchronisaties,
		HstCAlternatief,
		HstCRealisatieAfhandeling,
		HstCPostApplication,
		HstCPreSystemApplication,
		HstCPostSystemApplication,
		HstCPostDumpApplication

	};
}
