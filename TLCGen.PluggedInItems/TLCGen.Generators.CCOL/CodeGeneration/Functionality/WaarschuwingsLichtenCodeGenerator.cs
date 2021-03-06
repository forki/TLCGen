﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Generators.CCOL.CodeGeneration;
using TLCGen.Generators.CCOL.Settings;
using TLCGen.Models;
using TLCGen.Models.Enumerations;

namespace TLCGen.Generators.CCOL.CodeGeneration.Functionality
{

    [CCOLCodePieceGenerator]
    public class WaarschuwingsLichtenCodeGenerator : CCOLCodePieceGeneratorBase
    {

        #region Fields

        private List<CCOLElement> _MyElements;
        private List<CCOLIOElement> _MyBitmapOutputs;
#pragma warning disable 0649
        private string _uswl;
        private string _usbel;
        private string _usbeldim;
        private string _schbelcontdim;
#pragma warning restore 0649
        private string _hperiod;
        private string _prmperbel;
        private string _prmperbeldim;

        #endregion // Fields

        public override void CollectCCOLElements(ControllerModel c)
        {
            _MyElements = new List<CCOLElement>();
            _MyBitmapOutputs = new List<CCOLIOElement>();

            if (c.Signalen.WaarschuwingsGroepen.Any(x => x.Bellen))
            {
                _MyElements.Add(
                    new CCOLElement(
                        $"{_schbelcontdim}",
                        0,
                        CCOLElementTimeTypeEnum.SCH_type, 
                        CCOLElementTypeEnum.Schakelaar));
                if (c.PeriodenData.Perioden.Any(x => x.Type == PeriodeTypeEnum.BellenDimmen))
                {
                    _MyElements.Add(
                        new CCOLElement(
                            $"{_usbeldim}",
                            CCOLElementTypeEnum.Uitgang));
                }
            }

            foreach (var wlg in c.Signalen.WaarschuwingsGroepen)
            {
                if (wlg.Bellen)
                {
                    _MyElements.Add(
                        new CCOLElement(
                            $"{_usbel}{wlg.Naam}",
                            CCOLElementTypeEnum.Uitgang));
                    _MyBitmapOutputs.Add(new CCOLIOElement(wlg.BellenBitmapData as IOElementModel, $"{_uspf}{_usbel}{wlg.Naam}"));
                }
                if (wlg.Lichten)
                {
                    _MyElements.Add(
                        new CCOLElement(
                            $"{_uswl}{wlg.Naam}",
                            CCOLElementTypeEnum.Uitgang));
                    _MyBitmapOutputs.Add(new CCOLIOElement(wlg.LichtenBitmapData as IOElementModel, $"{_uspf}{_uswl}{wlg.Naam}"));
                }
            }
        }

        public override bool HasCCOLElements()
        {
            return true;
        }

        public override IEnumerable<CCOLElement> GetCCOLElements(CCOLElementTypeEnum type)
        {
            return _MyElements.Where(x => x.Type == type);
        }

        public override bool HasCCOLBitmapOutputs()
        {
            return true;
        }

        public override IEnumerable<CCOLIOElement> GetCCOLBitmapOutputs()
        {
            return _MyBitmapOutputs;
        }

        public override int HasCode(CCOLCodeTypeEnum type)
        {
            switch (type)
            {
                case CCOLCodeTypeEnum.RegCSystemApplication:
                    return 70;
                default:
                    return 0;
            }
        }

        public override string GetCode(ControllerModel c, CCOLCodeTypeEnum type, string ts)
        {
            StringBuilder sb = new StringBuilder();

            switch (type)
            {
                case CCOLCodeTypeEnum.RegCSystemApplication:
                    if(c.Signalen.WaarschuwingsGroepen.Count == 0)
                    {
                        return "";
                    }
                    if (c.Signalen.WaarschuwingsGroepen.Any(x => x.Lichten))
                    {
                        sb.AppendLine($"{ts}/* Aansturing waarschuwingslichten */");
                        foreach (var wlg in c.Signalen.WaarschuwingsGroepen)
                        {
                            if (wlg.Lichten)
                            {
                                sb.AppendLine($"{ts}CIF_GUS[{_uspf}{_uswl}{wlg.Naam}] = R[{_fcpf}{wlg.FaseCyclusVoorAansturing}] && REG;");
                            }
                        }
                    }
                    if (c.Signalen.WaarschuwingsGroepen.Any(x => x.Bellen) && c.PeriodenData.Perioden.Any(x => x.Type == PeriodeTypeEnum.BellenActief))
                    {
                        sb.AppendLine($"{ts}/* Aansturing electronische bellen */");
                        foreach (var wlg in c.Signalen.WaarschuwingsGroepen)
                        {
                            if (wlg.Bellen)
                            {
                                sb.AppendLine($"{ts}CIF_GUS[{_uspf}{_usbel}{wlg.Naam}] = R[{_fcpf}{wlg.FaseCyclusVoorAansturing}] && H[{_hpf}{_hperiod}{_prmperbel}] && REG;");
                            }
                        }
                        if (c.PeriodenData.Perioden.Any((x => x.Type == PeriodeTypeEnum.BellenDimmen)))
                        {
                            sb.AppendLine();
                            sb.AppendLine($"{ts}/* Dimming bellen */");
                            sb.Append($"{ts}CIF_GUS[{_uspf}{_usbeldim}] = H[{_hpf}{_hperiod}{_prmperbeldim}] || SCH[{_schpf}{_schbelcontdim}];");
                        }
                    }
                    sb.AppendLine();
                    return sb.ToString();

                default:
                    return null;
            }
        }

        public override bool SetSettings(CCOLGeneratorClassWithSettingsModel settings)
        {
            _hperiod = CCOLGeneratorSettingsProvider.Default.GetElementName("hperiod");
            _prmperbel = CCOLGeneratorSettingsProvider.Default.GetElementName("prmperbel");
            _prmperbeldim = CCOLGeneratorSettingsProvider.Default.GetElementName("prmperbeldim");

            return base.SetSettings(settings);
        }
    }
}
