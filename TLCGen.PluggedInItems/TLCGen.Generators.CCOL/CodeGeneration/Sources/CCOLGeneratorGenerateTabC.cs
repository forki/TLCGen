﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TLCGen.Generators.CCOL.Extensions;
using TLCGen.Generators.CCOL.Settings;
using TLCGen.Models;

namespace TLCGen.Generators.CCOL.CodeGeneration
{
    public partial class CCOLGenerator
    {
        private string GenerateTabC(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/* REGEL INSTELLINGEN */");
            sb.AppendLine("/* ------------------ */");
            sb.AppendLine();
	        if (controller.HalfstarData.IsHalfstar)
	        {
				sb.AppendLine("/* Definieer functie tbv tijden halfstar */");
		        sb.AppendLine("void signaalplan_instellingen(void);");
				sb.AppendLine();
	        }
            sb.Append(GenerateFileHeader(controller.Data, "tab.c"));
            sb.AppendLine();
            sb.Append(GenerateVersionHeader(controller.Data));
            sb.AppendLine();
            sb.Append(GenerateTabCIncludes(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlDefaults(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParameters(controller));
            if(controller.HalfstarData.IsHalfstar && controller.HalfstarData.SignaalPlannen.Any())
            {
                sb.AppendLine();
                sb.Append(GenerateHstCSignaalPlanInstellingen(controller));
            }

            return sb.ToString();
        }

        private string GenerateTabCIncludes(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"/* include files */");
            sb.AppendLine($"/* ------------- */");
            sb.AppendLine($"{ts}#include \"{controller.Data.Naam}sys.h\"");
            sb.AppendLine($"{ts}#include \"fcvar.h\"    /* fasecycli                         */");
            sb.AppendLine($"{ts}#include \"kfvar.h\"    /* conflicten                        */");
            sb.AppendLine($"{ts}#include \"usvar.h\"    /* uitgangs elementen                */");
            sb.AppendLine($"{ts}#include \"dpvar.h\"    /* detectie elementen                */");
            if(controller.Data.GarantieOntruimingsTijden)
            {
                sb.AppendLine($"{ts}#include \"to_min.h\"   /* garantie-ontruimingstijden        */");
            }
            if (controller.Data.GarantieOntruimingsTijden)
            {
                sb.AppendLine($"{ts}#include \"to_min.h\"   /* garantie-ontruimingstijden        */");
            }
            sb.AppendLine($"{ts}#include \"trg_min.h\"  /* garantie-roodtijden               */");
            sb.AppendLine($"{ts}#include \"tgg_min.h\"  /* garantie-groentijden              */");
            sb.AppendLine($"{ts}#include \"tgl_min.h\"  /* garantie-geeltijden               */");
            sb.AppendLine($"{ts}#include \"isvar.h\"    /* ingangs elementen                 */");
            if (controller.OVData.DSI)
            {
                sb.AppendLine($"{ts}#include \"dsivar.h\"   /* selectieve detectie               */");
            }
            sb.AppendLine($"{ts}#include \"hevar.h\"    /* hulp elementen                    */");
            sb.AppendLine($"{ts}#include \"mevar.h\"    /* geheugen elementen                */");
            sb.AppendLine($"{ts}#include \"tmvar.h\"    /* tijd elementen                    */");
            sb.AppendLine($"{ts}#include \"ctvar.h\"    /* teller elementen                  */");
            sb.AppendLine($"{ts}#include \"schvar.h\"   /* software schakelaars              */");
            sb.AppendLine($"{ts}#include \"prmvar.h\"   /* parameters                        */");
            sb.AppendLine($"{ts}#include \"lwmlvar.h\"  /* langstwachtende modulen structuur */");
            sb.AppendLine($"{ts}#include \"control.h\"  /* controller interface              */");
            if (controller.Data.VLOGType != Models.Enumerations.VLOGTypeEnum.Geen)
            {
                sb.AppendLine($"{ts}#ifndef NO_VLOG");
                sb.AppendLine($"{ts}{ts}#include \"vlogvar.h\"  /* variabelen t.b.v. vlogfuncties                */");
                sb.AppendLine($"{ts}{ts}#include \"logvar.h\"   /* variabelen t.b.v. logging                     */");
                sb.AppendLine($"{ts}{ts}#include \"monvar.h\"   /* variabelen t.b.v. realtime monitoring         */");
                sb.AppendLine($"{ts}#endif");
            }

            if (controller.HalfstarData.IsHalfstar)
            {
                sb.AppendLine($"{ts}#include \"tx_synch.h\"");
                sb.AppendLine($"{ts}#include \"plevar.h\"");
                sb.AppendLine($"{ts}#include \"halfstar.h\"");
            }

            sb.AppendLine();
            sb.AppendLine($"{ts}mulv FC_type[FCMAX];");
            sb.AppendLine();

            return sb.ToString();
        }

        private string GenerateTabCControlDefaults(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("void control_defaults(void)");
            sb.AppendLine("{");
            sb.AppendLine($"{ts}TDB_defmax = NG;");
            sb.AppendLine($"{ts}TDH_defmax = NG;");
            sb.AppendLine($"{ts}TBG_defmax = NG;");
            sb.AppendLine($"{ts}TOG_defmax = NG;");
            sb.AppendLine();
            sb.AppendLine($"{ts}TRG_defmax = NG;");
            sb.AppendLine($"{ts}TGG_defmax = NG;");
            sb.AppendLine($"{ts}TGL_defmax = NG;");
            sb.AppendLine($"{ts}TFG_defmax = NG;");
            sb.AppendLine($"{ts}TVG_defmax = NG;");
            sb.AppendLine();
            sb.AppendLine($"{ts}TVG_deftype |= RO_type; /* Verlenggroentijden  read-only */");
            if (controller.Data.VLOGType != Models.Enumerations.VLOGTypeEnum.Geen)
            {
                sb.AppendLine();
                sb.AppendLine($"{ts}MON_def = 1;");
                sb.AppendLine($"{ts}LOG_def = 1;");
            }
            sb.AppendLine();
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateTabCControlParameters(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("void control_parameters(void)");
            sb.AppendLine("{");
            sb.AppendLine();

            sb.Append(GenerateTabCControlParametersFasen(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersConflicten(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersUitgangen(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersDetectors(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersIngangen(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersHulpElementen(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersGeheugenElementen(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersTijdElementen(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersCounters(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersSchakelaars(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersParameters(controller));
            sb.AppendLine();
            sb.Append(GenerateTabCControlParametersExtraData(controller));
            sb.AppendLine();
            if (controller.OVData.DSI)
            {
                sb.Append(GenerateTabCControlParametersDS(controller));
            }
            sb.Append(GenerateTabCControlParametersModulen(controller));
            sb.AppendLine();
            if (controller.Data.VLOGType != Models.Enumerations.VLOGTypeEnum.Geen)
            {
                sb.Append(GenerateTabCControlParametersVLOG(controller));
                sb.AppendLine();
            }

	        AddCodeTypeToStringBuilder(controller, sb, CCOLCodeTypeEnum.TabCControlParameters, false, true);

	        if (controller.HalfstarData.IsHalfstar)
	        {
		        sb.AppendLine($"{ts}signaalplan_instellingen();");
                sb.AppendLine();
	        }

            sb.AppendLine($"{ts}#include \"{controller.Data.Naam}tab.add\"");

            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateTabCControlParametersFasen(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* fasecycli */");
            sb.AppendLine("/* --------- */");

            foreach (FaseCyclusModel fcm in controller.Fasen)
            {
                string s = $"   FC_code[{fcm.GetDefine()}] = \"{fcm.Naam}\"; TRG_max[{fcm.GetDefine()}] = {fcm.TRG};";
                int i = s.Length;
                sb.AppendLine(s);
                sb.AppendLine($"TRG_min[{fcm.GetDefine()}] = {fcm.TRG_min};".PadLeft(i));
                sb.AppendLine($"TGG_max[{fcm.GetDefine()}] = {fcm.TGG};".PadLeft(i));
                sb.AppendLine($"TGG_min[{fcm.GetDefine()}] = {fcm.TGG_min};".PadLeft(i));
                sb.AppendLine($"TFG_max[{fcm.GetDefine()}] = {fcm.TFG};".PadLeft(i));
                sb.AppendLine($"TGL_max[{fcm.GetDefine()}] = {fcm.TGL};".PadLeft(i));
                sb.AppendLine($"TGL_min[{fcm.GetDefine()}] = {fcm.TGL_min};".PadLeft(i));
                sb.AppendLine($"TVG_max[{fcm.GetDefine()}] = NG;".PadLeft(i));
            }

            return sb.ToString();
        }

        private string GenerateTabCControlParametersConflicten(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* conflicten */");
            sb.AppendLine("/* ---------- */");
            
            if (controller.InterSignaalGroep.Conflicten?.Count > 0)
            {
                string prevfasefrom = "";
                foreach (ConflictModel conflict in controller.InterSignaalGroep.Conflicten)
                {
                    var ff = conflict.GetFaseFromDefine();
                    var ft = conflict.GetFaseToDefine();
                    if (ff == ft) continue;

                    // Cause an empty line in between signalgroups
                    if (prevfasefrom == "")
                    {
                        prevfasefrom = ff;
                    }
                    if(prevfasefrom != ff)
                    {
                        prevfasefrom = ff;
                        sb.AppendLine();
                    }

                    sb.AppendLine($"{ts}TO_max[{ff}][{ft}] = {conflict.SerializedWaarde};");
                }
            }

            if(controller.Fasen.Count > 0)
            { 
                int[,] matrix = new int[controller.Fasen.Count, controller.Fasen.Count];
                for (int i = 0; i < controller.Fasen.Count; ++i)
                {
                    for (int j = 0; j < controller.Fasen.Count; ++j)
                    {
                        matrix[i, j] = -1;
                    }
                }

                for (int i = 0; i < controller.Fasen.Count; ++i)
                {
                    for (int j = 0; j < controller.Fasen.Count; ++j)
                    {
                        foreach (var cf in controller.InterSignaalGroep.Conflicten)
                        {
                            if (cf.FaseVan == controller.Fasen[i].Naam &&
                                cf.FaseNaar == controller.Fasen[j].Naam)
                            {
                                if (cf.Waarde >= -4)
                                {
                                    switch (cf.Waarde)
                                    {
                                        case -4:
                                            matrix[i, j] = -40;
                                            break;
                                        case -3:
                                            matrix[i, j] = -30;
                                            break;
                                        case -2:
                                            matrix[i, j] = -20;
                                            break;
                                        default:
                                            matrix[i, j] = cf.Waarde;
                                            break;
                                    }
                                }
                                else
                                {
                                    matrix[i, j] = -1;
                                }
                            }
                        }
                    }
                }

                // Below: only overwrite non-user defined values

                foreach (var gs in controller.InterSignaalGroep.Gelijkstarten)
                {
                    var fc = controller.Fasen.First(x => x.Naam == gs.FaseVan);
                    var i = controller.Fasen.IndexOf(fc);
                    var fc2 = controller.Fasen.First(x => x.Naam == gs.FaseNaar);
                    var j = controller.Fasen.IndexOf(fc2);

                    for (var k = 0; k < controller.Fasen.Count; ++k)
                    {
                        if (matrix[i, k] > -1 && matrix[j, k] == -1)
                        {
                            matrix[k, j] = -2;
                            matrix[j, k] = -2;
                        }
                    }
                }
                foreach (var vs in controller.InterSignaalGroep.Voorstarten)
                {
                    var fc = controller.Fasen.First(x => x.Naam == vs.FaseVan);
                    var i = controller.Fasen.IndexOf(fc);
                    var fc2 = controller.Fasen.First(x => x.Naam == vs.FaseNaar);
                    var j = controller.Fasen.IndexOf(fc2);

                    for (var k = 0; k < controller.Fasen.Count; ++k)
                    {
                        if (matrix[i, k] > -1 && matrix[j, k] == -1)
                        {
                            matrix[k, j] = -2;
                            matrix[j, k] = -2;
                        }
                    }
                }

                foreach (var nl in controller.InterSignaalGroep.Nalopen)
                {
                    var fc = controller.Fasen.First(x => x.Naam == nl.FaseVan);
                    var i = controller.Fasen.IndexOf(fc);
                    var fc2 = controller.Fasen.First(x => x.Naam == nl.FaseNaar);
                    var j = controller.Fasen.IndexOf(fc2);

                    for (var k = 0; k < controller.Fasen.Count; ++k)
                    {
                        if (matrix[j, k] > -1 && matrix[i, k] < 0 && matrix [i, k] > -20)
                        {
                            switch (nl.Type)
                            {
                                case Models.Enumerations.NaloopTypeEnum.StartGroen:
                                    if (nl.InrijdenTijdensGroen)
                                    {
                                        if (matrix[i, k] > -2) matrix[i, k] = -2;
                                        if (matrix[k, i] > -2) matrix[k, i] = -2;
                                    }
                                    else
                                    {
                                        if (matrix[i, k] > -3) matrix[i, k] = -3;
                                        if (matrix[k, i] > -3) matrix[k, i] = -3;
                                    }
                                    break;
                                case Models.Enumerations.NaloopTypeEnum.CyclischVerlengGroen:
                                    if (matrix[i, k] > -2) matrix[i, k] = -2;
                                    if (matrix[k, i] > -2) matrix[k, i] = -2;
                                    break;
                                case Models.Enumerations.NaloopTypeEnum.EindeGroen:
                                    if (nl.InrijdenTijdensGroen)
                                    {
                                        if (matrix[i, k] > -4) matrix[i, k] = -4;
                                        if (matrix[k, i] > -2) matrix[k, i] = -2;
                                    }
                                    else
                                    {
                                        if (matrix[i, k] > -4) matrix[i, k] = -4;
                                        if (matrix[k, i] > -3) matrix[k, i] = -3;
                                    }
                                    break;
                            }
                        }
                    }
                }
                sb.AppendLine();
                for (var i = 0; i < controller.Fasen.Count; ++i)
                {
                    bool AppendEmptyLine;
                    AppendEmptyLine = false;
                    for (var j = 0; j < controller.Fasen.Count; ++j)
                    {
                        if (matrix[i, j] >= -1) continue;
                        var k = "FK";
                        if (matrix[i, j] == -3 || matrix[i, j] == -30) k = "GK";
                        if (matrix[i, j] == -4 || matrix[i, j] == -40) k = "GKL";
                        sb.AppendLine($"{ts}TO_max[{_fcpf}{controller.Fasen[i].Naam}][{_fcpf}{controller.Fasen[j].Naam}] = {k};");
                        AppendEmptyLine = true;
                    }
                    if (AppendEmptyLine) sb.AppendLine();
                }

                //if (controller.InterSignaalGroep.Gelijkstarten.Count > 0 || controller.InterSignaalGroep.Voorstarten.Count > 0)
                //{
                //    foreach (var gs in controller.InterSignaalGroep.Gelijkstarten)
                //    {
                //        foreach (ConflictModel conflict in controller.InterSignaalGroep.Conflicten)
                //        {
                //            if (gs.FaseVan == conflict.FaseVan)
                //            {
                //                bool issym = false;
                //                foreach (ConflictModel conflict2 in controller.InterSignaalGroep.Conflicten)
                //                {
                //                    if (conflict.FaseNaar == conflict2.FaseNaar &&
                //                       gs.FaseNaar == conflict2.FaseVan)
                //                    {
                //                        issym = true;
                //                        break;
                //                    }
                //                }
                //                if (!issym)
                //                {
                //                    sb.AppendLine($"{ts}TO_max[{_fcpf}{gs.FaseNaar}][{conflict.GetFaseToDefine()}] = FK;");
                //                    sb.AppendLine($"{ts}TO_max[{conflict.GetFaseToDefine()}][{_fcpf}{gs.FaseNaar}] = FK;");
                //                }
                //            }
                //            if (gs.FaseNaar == conflict.FaseVan)
                //            {
                //                bool issym = false;
                //                foreach (ConflictModel conflict2 in controller.InterSignaalGroep.Conflicten)
                //                {
                //                    if (conflict.FaseNaar == conflict2.FaseNaar &&
                //                       gs.FaseVan == conflict2.FaseVan)
                //                    {
                //                        issym = true;
                //                        break;
                //                    }
                //                }
                //                if (!issym)
                //                {
                //                    sb.AppendLine($"{ts}TO_max[{_fcpf}{gs.FaseVan}][{conflict.GetFaseToDefine()}] = FK;");
                //                    sb.AppendLine($"{ts}TO_max[{conflict.GetFaseToDefine()}][{_fcpf}{gs.FaseVan}] = FK;");
                //                }
                //            }
                //        }
                //    }
                //    foreach (var vs in controller.InterSignaalGroep.Voorstarten)
                //    {
                //        foreach (ConflictModel conflict in controller.InterSignaalGroep.Conflicten)
                //        {
                //            if (vs.FaseVan == conflict.FaseVan)
                //            {
                //                bool issym = false;
                //                foreach (ConflictModel conflict2 in controller.InterSignaalGroep.Conflicten)
                //                {
                //                    if (conflict.FaseNaar == conflict2.FaseNaar &&
                //                       vs.FaseNaar == conflict2.FaseVan)
                //                    {
                //                        issym = true;
                //                        break;
                //                    }
                //                }
                //                if (!issym)
                //                {
                //                    sb.AppendLine($"{ts}TO_max[{_fcpf}{vs.FaseNaar}][{conflict.GetFaseToDefine()}] = FK;");
                //                    sb.AppendLine($"{ts}TO_max[{conflict.GetFaseToDefine()}][{_fcpf}{vs.FaseNaar}] = FK;");
                //                }
                //            }
                //        }
                //    }
                //    foreach (var nl in controller.InterSignaalGroep.Nalopen)
                //    {
                //        foreach (ConflictModel conflict in controller.InterSignaalGroep.Conflicten)
                //        {
                //            if (nl.FaseVan == conflict.FaseVan)
                //            {
                //                bool issym = false;
                //                foreach (ConflictModel conflict2 in controller.InterSignaalGroep.Conflicten)
                //                {
                //                    if (conflict.FaseNaar == conflict2.FaseNaar &&
                //                       nl.FaseNaar == conflict2.FaseVan)
                //                    {
                //                        issym = true;
                //                        break;
                //                    }
                //                }
                //                if (!issym)
                //                {
                //                    switch (nl.SynchronisatieType)
                //                    {
                //                        case Models.Enumerations.SynchronisatieTypeEnum.FictiefConflict:
                //                            sb.AppendLine($"{ts}TO_max[{_fcpf}{nl.FaseNaar}][{conflict.GetFaseToDefine()}] = FK;");
                //                            sb.AppendLine($"{ts}TO_max[{conflict.GetFaseToDefine()}][{_fcpf}{nl.FaseNaar}] = FK;");
                //                            break;
                //                        case Models.Enumerations.SynchronisatieTypeEnum.GroenConflict:
                //                            sb.AppendLine($"{ts}TO_max[{_fcpf}{nl.FaseNaar}][{conflict.GetFaseToDefine()}] = GK;");
                //                            sb.AppendLine($"{ts}TO_max[{conflict.GetFaseToDefine()}][{_fcpf}{nl.FaseNaar}] = GK;");
                //                            break;
                //                        case Models.Enumerations.SynchronisatieTypeEnum.GroenGeelConflict:
                //                            sb.AppendLine($"{ts}TO_max[{_fcpf}{nl.FaseNaar}][{conflict.GetFaseToDefine()}] = GKL;");
                //                            sb.AppendLine($"{ts}TO_max[{conflict.GetFaseToDefine()}][{_fcpf}{nl.FaseNaar}] = GK;");
                //                            break;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}

                if (controller.Data.GarantieOntruimingsTijden)
                {
                    if (controller.InterSignaalGroep.Conflicten?.Count > 0)
                    {
                        var counters = new Dictionary<int, int>();
                        foreach (var conflict in controller.InterSignaalGroep.Conflicten)
                        {
                            if (conflict.GarantieWaarde.HasValue && !counters.ContainsKey(conflict.Waarde - conflict.GarantieWaarde.Value))
                            {
                                counters.Add(conflict.Waarde - conflict.GarantieWaarde.Value, 1);
                            }
                            else if(conflict.GarantieWaarde.HasValue)
                            {
                                counters[conflict.Waarde - conflict.GarantieWaarde.Value]++;
                            }
                        }
                        var most = 0;
                        var val = 0;
                        foreach (var i in counters)
                        {
                            if (i.Value > most)
                            {
                                most = i.Value;
                                val = i.Key;
                            }
                        }
                        sb.AppendLine($"{ts}default_to_min({val});");
                        foreach (var conflict in controller.InterSignaalGroep.Conflicten)
                        {
                            if(conflict.GarantieWaarde.HasValue && (conflict.Waarde - conflict.GarantieWaarde.Value) != val)
                                sb.AppendLine($"{ts}TO_min[{conflict.GetFaseToDefine()}][{_fcpf}{conflict.FaseNaar}] = {conflict.GarantieWaarde.Value};");
                        }
                    }
                }
            }


            return sb.ToString();
        }

        private string GenerateTabCControlParametersUitgangen(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* overige uitgangen */");
            sb.AppendLine("/* ----------------- */");

            sb.Append(GetAllElementsTabCLines(_uitgangen));

            return sb.ToString();
        }

        private string GenerateTabCControlParametersDetectors(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* detectie */");
            sb.AppendLine("/* -------- */");

            int defmax = 0;
            int namemax = 0;
            int? dbmax = 0;
            int? dhmax = 0;
            int? ogmax = 0;
            int? bgmax = 0;
            int? cflmax = 0;
            int? tflmax = 0;

            var fasendets = controller.Fasen.SelectMany(x => x.Detectoren);
            var controllerdets = controller.Detectoren;
            var ovdummydets = controller.OVData.GetAllDummyDetectors();
            var alldets = fasendets.Concat(controllerdets).Concat(ovdummydets);

            foreach (var dm in alldets.Where(x => !x.Dummy))
            {
                if (dm.GetDefine()?.Length > defmax) defmax = dm.GetDefine().Length;
                if (dm.Naam?.Length > namemax) namemax = dm.Naam.Length;
                if (dm.TDB != null && dm.TDB > dbmax) dbmax = dm.TDB;
                if (dm.TDH != null && dm.TDH > dhmax) dhmax = dm.TDH;
                if (dm.TOG != null && dm.TOG > ogmax) ogmax = dm.TOG;
                if (dm.TBG != null && dm.TBG > bgmax) bgmax = dm.TBG;
                if (dm.TFL != null && dm.TFL > tflmax) tflmax = dm.TFL;
                if (dm.CFL != null && dm.CFL > cflmax) cflmax = dm.CFL;
            }
            dbmax = dbmax.ToString().Length;
            dhmax = dhmax.ToString().Length;
            ogmax = ogmax.ToString().Length;
            bgmax = bgmax.ToString().Length;
            tflmax = tflmax.ToString().Length;
            cflmax = cflmax.ToString().Length;

            int pad1 = "D_code[] ".Length + defmax;
            int pad2 = "= \"\"; ".Length + namemax;
            int pad3 = "TDB_max[] ".Length + defmax;
            int pad4 = "= ; ".Length + Math.Max(dbmax ?? 0, bgmax ?? 0);
            int pad5 = "TDH_max[] ".Length + defmax;
            int pad6 = pad1 + pad2;

            foreach(var dm in alldets)
            {
                if (!dm.Dummy)
                {
                    AppendDetectorTabString(sb, dm, pad1, pad2, pad3, pad4, pad5, pad6);
                }
            }
            /* Dummies */
            var dummydets = alldets.Where(x => x.Dummy);
            var detectorModels = dummydets as DetectorModel[] ?? dummydets.ToArray();
            if (detectorModels.Any())
            {
                dbmax = dhmax = ogmax = bgmax = tflmax = cflmax = defmax = namemax = 0;
                foreach (var dm in detectorModels)
                {
                    if (dm.GetDefine()?.Length > defmax) defmax = dm.GetDefine().Length;
                    if (dm.Naam?.Length > namemax) namemax = dm.Naam.Length;
                    if (dm.TDB != null && dm.TDB > dbmax) dbmax = dm.TDB;
                    if (dm.TDH != null && dm.TDH > dhmax) dhmax = dm.TDH;
                    if (dm.TOG != null && dm.TOG > ogmax) ogmax = dm.TOG;
                    if (dm.TBG != null && dm.TBG > bgmax) bgmax = dm.TBG;
                    if (dm.TFL != null && dm.TFL > tflmax) tflmax = dm.TFL;
                    if (dm.CFL != null && dm.CFL > cflmax) cflmax = dm.CFL;
                }
                dbmax = dbmax.ToString().Length;
                dhmax = dhmax.ToString().Length;
                ogmax = ogmax.ToString().Length;
                bgmax = bgmax.ToString().Length;
                tflmax = tflmax.ToString().Length;
                cflmax = cflmax.ToString().Length;
                pad1 = "D_code[] ".Length + defmax;
                pad2 = "= \"\"; ".Length + namemax;
                pad3 = "TDB_max[] ".Length + defmax;
                pad4 = "= ; ".Length + Math.Max(dbmax ?? 0, bgmax ?? 0);
                pad5 = "TDH_max[] ".Length + defmax;
                pad6 = pad1 + pad2;

                sb.AppendLine("#ifndef AUTOMAAT");
                foreach(var dm in detectorModels)
                {
                    AppendDetectorTabString(sb, dm, pad1, pad2, pad3, pad4, pad5, pad6);
                }
                sb.AppendLine("#endif");
            }

            return sb.ToString();
        }

        private void AppendDetectorTabString(StringBuilder sb, DetectorModel dm, int pad1, int pad2, int pad3, int pad4, int pad5, int pad6)
        {
            sb.Append("    ");
            sb.Append($"D_code[{dm.GetDefine()}] ".PadRight(pad1));
            sb.Append($"= \"{dm.Naam}\"; ".PadRight(pad2));
            if (dm.TDB != null)
            {
                sb.Append($"TDB_max[{dm.GetDefine()}] ".PadRight(pad3));
                sb.Append($"= {dm.TDB}; ".PadRight(pad4));
            }
            if (dm.TDH != null)
            {
                sb.Append($"TDH_max[{dm.GetDefine()}] ".PadRight(pad5));
                sb.AppendLine($"= {dm.TDH};");
            }
            else
            {
                sb.AppendLine("");
            }

            if (dm.TBG != null || dm.TOG != null)
            {
                sb.Append("    ");
                sb.Append("".PadLeft(pad6));
                if (dm.TBG != null)
                {
                    sb.Append($"TBG_max[{dm.GetDefine()}] ".PadRight(pad3));
                    sb.Append($"= {dm.TBG}; ".PadRight(pad4));
                }
                if (dm.TOG != null)
                {
                    sb.Append($"TOG_max[{dm.GetDefine()}] ".PadRight(pad5));
                    sb.AppendLine($"= {dm.TOG};");
                }
                else
                {
                    sb.AppendLine("");
                }
            }

            if (dm.TFL != null || dm.CFL != null)
            {
                sb.Append("    ");
                sb.Append("".PadLeft(pad6));
                if (dm.TFL != null)
                {
                    sb.Append($"TFL_max[{dm.GetDefine()}] ".PadRight(pad3));
                    sb.Append($"= {dm.TFL}; ".PadRight(pad4));
                }
                if (dm.CFL != null)
                {
                    sb.Append($"CFL_max[{dm.GetDefine()}] ".PadRight(pad5));
                    sb.AppendLine($"= {dm.CFL};");
                }
                else
                {
                    sb.AppendLine("");
                }
            }
        }

        private string GenerateTabCControlParametersIngangen(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* overige ingangen */");
            sb.AppendLine("/* ---------------- */");

            sb.Append(GetAllElementsTabCLines(_ingangen));

            return sb.ToString();
        }

        private string GenerateTabCControlParametersHulpElementen(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* hulp elementen */");
            sb.AppendLine("/* -------------- */");

            sb.Append(GetAllElementsTabCLines(_hulpElementen));

            return sb.ToString();
        }

        private string GenerateTabCControlParametersGeheugenElementen(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* geheugen elementen */");
            sb.AppendLine("/* ------------------ */");
            
            sb.Append(GetAllElementsTabCLines(_geheugenElementen));

            return sb.ToString();
        }

        private string GenerateTabCControlParametersTijdElementen(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* tijd elementen */");
            sb.AppendLine("/* -------------- */");

            sb.Append(GetAllElementsTabCLines(_timers));

            return sb.ToString();
        }

        private string GenerateTabCControlParametersCounters(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* teller elementen */");
            sb.AppendLine("/* ---------------- */");

            sb.Append(GetAllElementsTabCLines(_counters));

            return sb.ToString();
        }

        private string GenerateTabCControlParametersSchakelaars(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* schakelaars */");
            sb.AppendLine("/* ----------- */");

            sb.Append(GetAllElementsTabCLines(_schakelaars));

            return sb.ToString();
        }


        private string GenerateTabCControlParametersParameters(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* parameters */");
            sb.AppendLine("/* ---------- */");

            sb.Append(GetAllElementsTabCLines(_parameters));

            return sb.ToString();
        }

        private string GenerateTabCControlParametersExtraData(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* extra data */");
            sb.AppendLine("/* ---------- */");

            foreach(FaseCyclusModel fc in controller.Fasen)
            {
                switch(fc.Type)
                {
                    case Models.Enumerations.FaseTypeEnum.Auto:
                        sb.AppendLine($"{ts}FC_type[{_fcpf}{fc.Naam}] = MVT_type;");
                        break;
                    case Models.Enumerations.FaseTypeEnum.Fiets:
                        sb.AppendLine($"{ts}FC_type[{_fcpf}{fc.Naam}] = FTS_type;");
                        break;
                    case Models.Enumerations.FaseTypeEnum.Voetganger:
                        sb.AppendLine($"{ts}FC_type[{_fcpf}{fc.Naam}] = VTG_type;");
                        break;
                    case Models.Enumerations.FaseTypeEnum.OV:
                        sb.AppendLine($"{ts}FC_type[{_fcpf}{fc.Naam}] = OV_type;");
                        break;
                }
            }

            return sb.ToString();
        }

        private string GenerateTabCControlParametersDS(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* Selectieve detectie */");
            sb.AppendLine("/* ------------------- */");

            if (controller.OVData.OVIngrepen.Count > 0 &&
                controller.OVData.OVIngrepen.Where(x => x.Vecom).Any())
            {
                foreach (var ov in controller.OVData.OVIngrepen.Where(x => x.Vecom))
                {
                    sb.AppendLine($"{ts}DS_code[ds{ov.FaseCyclus}_in]  = \"ds{ov.FaseCyclus}_in\";");
                    sb.AppendLine($"{ts}DS_code[ds{ov.FaseCyclus}_uit] = \"ds{ov.FaseCyclus}_uit\";");
                }
            }
            else
            {
                sb.AppendLine($"{ts}DS_code[dsdummy] = \"dsdummy\";");
            }
            sb.AppendLine();

            return sb.ToString();
        }

        private string GenerateTabCControlParametersModulen(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* modulen */");
            sb.AppendLine("/* ------- */");

            foreach (ModuleModel mm in controller.ModuleMolen.Modules)
            {
                foreach (ModuleFaseCyclusModel mfcm in mm.Fasen)
                {
                    sb.AppendLine($"{ts}PRML[{mm.Naam}][{mfcm.GetFaseCyclusDefine()}] = PRIMAIR;");
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string GenerateTabCControlParametersVLOG(ControllerModel controller)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("/* Typen ingangen */");
            sb.AppendLine("/* -------------- */");

            foreach(FaseCyclusModel fc in controller.Fasen)
            {
                foreach (DetectorModel dm in fc.Detectoren)
                {
                    sb.Append($"{ts}IS_type[{_dpf}{dm.Naam}] = ");
                    switch(dm.Type)
                    {
                        case Models.Enumerations.DetectorTypeEnum.Knop:
                        case Models.Enumerations.DetectorTypeEnum.KnopBinnen:
                        case Models.Enumerations.DetectorTypeEnum.KnopBuiten:
                            sb.AppendLine("DK_type;");
                            break;
                        case Models.Enumerations.DetectorTypeEnum.File:
                        case Models.Enumerations.DetectorTypeEnum.Verweg:
                            sb.AppendLine("DL_type | DVER_type;");
                            break;
                        case Models.Enumerations.DetectorTypeEnum.Kop:
                            sb.AppendLine("DL_type | DKOP_type;");
                            break;
                        case Models.Enumerations.DetectorTypeEnum.Lang:
                            sb.AppendLine("DL_type | DLNG_type;");
                            break;
                        case Models.Enumerations.DetectorTypeEnum.VecomIngang:
                        case Models.Enumerations.DetectorTypeEnum.OpticomIngang:
                            // TODO: it is possible to use DKOP and DVER to mark in- and uitmelding: use? how?
                            sb.AppendLine("DS_type;");
                            break;
                        case Models.Enumerations.DetectorTypeEnum.Overig:
                            sb.AppendLine("DL_type;");
                            break;
                        case Models.Enumerations.DetectorTypeEnum.Radar:
                            // TODO: what type to use here?
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("Unknown detector type while generating tab.c: " + dm.Type);
                    }
                }
            }
            foreach (DetectorModel dm in controller.Detectoren)
            {
                sb.Append($"{ts}IS_type[{_dpf}{dm.Naam}] = ");
                switch (dm.Type)
                {
                    case Models.Enumerations.DetectorTypeEnum.Knop:
                    case Models.Enumerations.DetectorTypeEnum.KnopBinnen:
                    case Models.Enumerations.DetectorTypeEnum.KnopBuiten:
                        sb.AppendLine("DK_type;");
                        break;
                    case Models.Enumerations.DetectorTypeEnum.File:
                    case Models.Enumerations.DetectorTypeEnum.Verweg:
                        sb.AppendLine("DL_type | DVER_type;");
                        break;
                    case Models.Enumerations.DetectorTypeEnum.Kop:
                        sb.AppendLine("DL_type | DKOP_type;");
                        break;
                    case Models.Enumerations.DetectorTypeEnum.Lang:
                        sb.AppendLine("DL_type | DLNG_type;");
                        break;
                    case Models.Enumerations.DetectorTypeEnum.VecomIngang:
                    case Models.Enumerations.DetectorTypeEnum.OpticomIngang:
                        // TODO: it is possible to use DKOP and DVER to mark in- and uitmelding: use? how?
                        sb.AppendLine("DS_type;");
                        break;
                    case Models.Enumerations.DetectorTypeEnum.Overig:
                        sb.AppendLine("DL_type;");
                        break;
                    case Models.Enumerations.DetectorTypeEnum.Radar:
                        // TODO: what type to use here?
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unknown detector type while generating tab.c: " + dm.Type.ToString());
                }
            }

            sb.AppendLine();
            sb.AppendLine("/* Typen uitgangen */");
            sb.AppendLine("/* --------------- */");

            foreach(FaseCyclusModel fc in controller.Fasen)
            {
                sb.Append($"{ts}US_type[{_fcpf}{fc.Naam}] = ");
                switch (fc.Type)
                {
                    case Models.Enumerations.FaseTypeEnum.Auto:
                        sb.AppendLine("MVT_type;");
                        break;
                    case Models.Enumerations.FaseTypeEnum.OV:
                        sb.AppendLine("OV_type;");
                        break;
                    case Models.Enumerations.FaseTypeEnum.Fiets:
                        sb.AppendLine("FTS_type;");
                        break;
                    case Models.Enumerations.FaseTypeEnum.Voetganger:
                        sb.AppendLine("VTG_type;");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unknown vehicle type while generating tab.c: " + fc.Type.ToString());
                }
            }

            sb.AppendLine();
            sb.AppendLine($"{ts}/*VLOG - logging */");
            sb.AppendLine($"{ts}/*-------------- */");
            sb.AppendLine($"{ts}LOGTYPE[LOGTYPE_FC] = BIT0+BIT1+BIT2+BIT3+BIT5;");
            sb.AppendLine($"{ts}LOGTYPE[LOGTYPE_US] = BIT0+BIT1;");
            sb.AppendLine($"{ts}LOGTYPE[LOGTYPE_PS] = BIT0+BIT1;");
            sb.AppendLine($"{ts}LOGTYPE[LOGTYPE_DS] = BIT0+BIT1;");
            if(controller.Data.VLOGType == Models.Enumerations.VLOGTypeEnum.Filebased)
            {
                sb.AppendLine($"{ts}LOGPRM[LOGPRM_LOGKLOKSCH] = 1;");
                sb.AppendLine($"{ts}LOGPRM[LOGPRM_VLOGMODE] = VLOGMODE_LOG_FILE_ASCII;");
            }

            sb.AppendLine();
            sb.AppendLine($"{ts}/* VLOG - monitoring */");
            sb.AppendLine($"{ts}/* ----------------- */");
            sb.AppendLine($"{ts}MONTYPE[MONTYPE_FC] = BIT0+BIT1+BIT2+BIT3+BIT5;");
            sb.AppendLine($"{ts}MONTYPE[MONTYPE_US] = BIT0+BIT1;");
            sb.AppendLine($"{ts}MONTYPE[MONTYPE_PS] = BIT0+BIT1;");
            sb.AppendLine($"{ts}MONTYPE[MONTYPE_DS] = BIT0+BIT1;");

            sb.AppendLine();
            foreach (FaseCyclusModel fc in controller.Fasen)
            {
                sb.AppendLine($"{ts}MONFC[{_fcpf}{fc.Naam}] = BIT0+BIT1+BIT2+BIT3;");
            }

            sb.AppendLine();
            foreach (var u in _uitgangen.Elements)
            {
                sb.AppendLine($"{ts}MONUS[{u.Define}]= BIT0+BIT1;");
            }

            sb.AppendLine();
            sb.AppendLine($"{ts}MONPRM[MONPRM_VLOGMODE] = VLOGMODE_MON_ASCII;");

            sb.AppendLine();
            return sb.ToString();
        }
    }
}
