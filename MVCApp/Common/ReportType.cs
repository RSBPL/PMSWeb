using MVCApp.CommonFunction;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCApp
{
    public class OfflinePrint
    {
        public string Code { get; set; }

        public string Text { get; set; }
    }
    public class ReportCode
    {
        public string Code { get; set; }

        public string Text { get; set; }
    }
    public class ReportType
    {
        Function fun = new Function();
        //public List<ReportCode> GetEngineReportTypes
        //{
        //    get
        //    {
        //        List<ReportCode> al =
        //            new List<ReportCode>()
        //              {
        //                  new ReportCode(){Code="ERROR",Text="SCAN ERRORS"},
        //                  new ReportCode(){Code="EN_AUDIT",Text="ENGINE AUDIT"},
        //                  new ReportCode(){Code="TOT_ENG",Text="TOTAL ENGINES MANUFUCTURED"},
        //                  new ReportCode(){Code="PRDENGINES",Text="ENGINES MANUFUCTURED"},
        //                  new ReportCode(){Code="PART_AUDIT",Text="PARTS AUDIT"},


        //             };
        //        return al;
        //    }

        //} 

        public List<ReportCode> GetEngineReportTypes
        {
            get
            {
                DataTable Dt = null;
                List<ReportCode> EngineReport = new List<ReportCode>();

                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT DESCRIPTION TEXT,PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='ENGINE_REPORTS'  AND STATUS='Y'"));
                }
                else
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT xss.DESCRIPTION TEXT , rr.REPORTCODE VALUE FROM XXES_ROLE_REPORT rr JOIN XXES_SFT_SETTINGS xss 
                    ON rr.REPORTCODE = xss.PARAMVALUE WHERE rr.ROLEID = '{0}' AND rr.CATEGORY = 'ENGINE_REPORTS' AND xss.STATUS='Y'", Convert.ToString(HttpContext.Current.Session["Login_Level"])));
                }

                if (Dt.Rows.Count > 0)
                {
                    int SelectOption = 1;
                    if (SelectOption == 1)
                    {
                        EngineReport.Add(new ReportCode
                        {
                            Text = "---SELECT---",
                            Code = ""
                        });
                        SelectOption = SelectOption + 1;
                    }
                    foreach (DataRow dr in Dt.AsEnumerable())
                    {
                        EngineReport.Add(new ReportCode
                        {
                            Text = dr["TEXT"].ToString(),
                            Code = dr["VALUE"].ToString(),
                        });
                    }
                }

                return EngineReport;
            }

        }
        //public List<ReportCode> GetBackendReportTypes
        //{
        //    get
        //    {
        //        List<ReportCode> al =
        //            new List<ReportCode>()
        //              {
        //                  new ReportCode(){Code="PRDBACKEND",Text="BACKEND PRODUCTION"},


        //             };
        //        return al;
        //    }

        //}

        public List<ReportCode> GetBackendReportTypes
        {
            get
            {
                DataTable Dt = null;
                List<ReportCode> BackendReport = new List<ReportCode>();

                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT DESCRIPTION TEXT,PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='BACKEND_REPORTS'  AND STATUS='Y'"));
                }
                else
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT xss.DESCRIPTION TEXT , rr.REPORTCODE VALUE FROM XXES_ROLE_REPORT rr JOIN XXES_SFT_SETTINGS xss 
                ON rr.REPORTCODE = xss.PARAMVALUE WHERE rr.ROLEID = '{0}' AND rr.CATEGORY = 'BACKEND_REPORTS' AND xss.STATUS='Y'", Convert.ToString(HttpContext.Current.Session["Login_Level"])));
                }

                if (Dt.Rows.Count > 0)
                {
                    int SelectOption = 1;
                    if (SelectOption == 1)
                    {
                        BackendReport.Add(new ReportCode
                        {
                            Text = "---SELECT---",
                            Code = ""
                        });
                        SelectOption = SelectOption + 1;
                    }
                    foreach (DataRow dr in Dt.AsEnumerable())
                    {
                        BackendReport.Add(new ReportCode
                        {
                            Text = dr["TEXT"].ToString(),
                            Code = dr["VALUE"].ToString(),
                        });
                    }
                }

                return BackendReport;
            }

        }
        public List<ReportCode> GetTransmissionReportTypes
        {
            get
            {
                List<ReportCode> al =
                    new List<ReportCode>()
                      {
                          new ReportCode(){Code="PRDTRANSMISSION",Text="TRANSMISSION PRODUCTION"},

                     };
                return al;
            }

        }
        //public List<ReportCode> GetHydraulicReportTypes
        //{
        //    get
        //    {
        //        List<ReportCode> al =
        //            new List<ReportCode>()
        //              {
        //                  new ReportCode(){Code="PRDHYDRAULIC",Text="HYDRAULIC PRODUCTION"},

        //             };
        //        return al;
        //    }

        //}
        public List<ReportCode> GetHydraulicReportTypes
        {
            get
            {
                DataTable Dt = null;
                List<ReportCode> HydraulicReport = new List<ReportCode>();

                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT DESCRIPTION TEXT,PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='HYDRAULIC_REPORTS'  AND STATUS='Y'"));
                }
                else
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT xss.DESCRIPTION TEXT , rr.REPORTCODE VALUE FROM XXES_ROLE_REPORT rr JOIN XXES_SFT_SETTINGS xss 
                    ON rr.REPORTCODE = xss.PARAMVALUE WHERE rr.ROLEID = '{0}' AND rr.CATEGORY = 'HYDRAULIC_REPORTS' AND xss.STATUS='Y'", Convert.ToString(HttpContext.Current.Session["Login_Level"])));
                }

                if (Dt.Rows.Count > 0)
                {
                    int SelectOption = 1;
                    if (SelectOption == 1)
                    {
                        HydraulicReport.Add(new ReportCode
                        {
                            Text = "---SELECT---",
                            Code = ""
                        });
                        SelectOption = SelectOption + 1;
                    }
                    foreach (DataRow dr in Dt.AsEnumerable())
                    {
                        HydraulicReport.Add(new ReportCode
                        {
                            Text = dr["TEXT"].ToString(),
                            Code = dr["VALUE"].ToString(),
                        });
                    }
                }

                return HydraulicReport;
            }

        }
        public List<ReportCode> GetRearAxelReportTypes
        {
            get
            {
                List<ReportCode> al =
                    new List<ReportCode>()
                      {
                          new ReportCode(){Code="PRDREARAXEL",Text="REAR AXEL PRODUCTION"},

                     };
                return al;
            }

        }
        //public List<ReportCode> GetTractorReportTypes
        //{
        //    get
        //    {
        //        List<ReportCode> al =
        //            new List<ReportCode>()
        //              {
        //                  new ReportCode(){Code="TOT",Text="TOTAL TRACTORS MANUFUCTURED"},
        //                  new ReportCode(){Code="JOB",Text="JOB HISTORY"},
        //                  new ReportCode(){Code="PLAN_AUDIT",Text="DAILY PLAN AUDIT"},
        //                  new ReportCode(){Code="JOBDATA",Text="DAILY PLAN JOBS"},
        //                  new ReportCode(){Code="RELJOBS",Text="RELEASED JOBS"},
        //                  new ReportCode(){Code="ERROR",Text="SCAN ERRORS"},
        //                  new ReportCode(){Code="HYD",Text="HYDRAULIC PRINTED"},
        //                  new ReportCode(){Code="RT",Text="REARTYRE PRINTED"},
        //                  new ReportCode(){Code="FT",Text="FRONTTYRE PRINTED"},
        //                  new ReportCode(){Code="TOT_ENG",Text="TOTAL ENGINES MANUFUCTURED"},
        //                  new ReportCode(){Code="CHECK_JOB",Text="JOB EXISTENCE"},
        //                  new ReportCode(){Code="PART_AUDIT",Text="PARTS AUDIT"},
        //                  new ReportCode(){Code="PRD",Text="PRODUCTION REPORT"},
        //                  new ReportCode(){Code="BULK_STORAGE_CAP",Text="BULK STORAGE CAPACITY"},
        //                 new ReportCode(){Code="SUPERMKT_MKT_CAP",Text="SUPER MARKET CAPACITY"},

        //                 //CREATED BY SARTHAK ON 29.11.2021
        //                 new ReportCode(){Code="SHORT_RECIPT_QUALITY",Text="SHORT RECIPT QUALITY"},
        //                 new ReportCode(){Code="HOLD_ITEM_QUALITY",Text="HOLD ITEM QUALITY"},
        //                 //CREATED BY RAJ ON 08.12.2021
        //                 new ReportCode(){Code="DAILY_PART_SCANNING_EFFICIENCY",Text="DAILY PART SCANNING EFFICIENCY"},
        //                 //new ReportCode(){Code="ROLLOUT",Text="ROLLOUT"},
        //                 new ReportCode(){Code="ROLL",Text="ROLLOUT"},

        //                 new ReportCode(){Code="ROLL_AUDIT",Text="ROLL OUT STICKER LOG"},
        //                 new ReportCode(){Code="PDI_AUDIT",Text="PDI STICKER LOG"},
        //                 new ReportCode(){Code="WEEKLY_OIL_FILTRATION",Text="WEEKLY OIL FILTRATION"}


        //             };
        //        return al;
        //    }

        //} 

        public List<ReportCode> GetTractorReportTypes
        {
            get
            {
                DataTable Dt = null;
                List<ReportCode> TractorReport = new List<ReportCode>();

                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT DESCRIPTION TEXT,PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='TRACTOR_REPORTS'  AND STATUS='Y'"));
                }
                else
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT xss.DESCRIPTION TEXT , rr.REPORTCODE VALUE FROM XXES_ROLE_REPORT rr JOIN XXES_SFT_SETTINGS xss 
                        ON rr.REPORTCODE = xss.PARAMVALUE WHERE rr.ROLEID = '{0}' AND rr.CATEGORY = 'TRACTOR_REPORTS' AND xss.STATUS='Y'", Convert.ToString(HttpContext.Current.Session["Login_Level"])));
                }

                if (Dt.Rows.Count > 0)
                {
                    int SelectOption = 1;
                    if (SelectOption == 1)
                    {
                        TractorReport.Add(new ReportCode
                        {
                            Text = "---SELECT---",
                            Code = ""
                        });
                        SelectOption = SelectOption + 1;
                    }
                    foreach (DataRow dr in Dt.AsEnumerable())
                    {
                        TractorReport.Add(new ReportCode
                        {
                            Text = dr["TEXT"].ToString(),
                            Code = dr["VALUE"].ToString(),
                        });
                    }
                }

                return TractorReport;
            }

        }
        public List<ReportCode> GetGateReportTypes
        {
            get
            {
                List<ReportCode> al =
                    new List<ReportCode>()
                      {
                          new ReportCode(){Code="DAILY_VEHICLE",Text="DAILY MRN"},
                          new ReportCode(){Code="AVG_TIME",Text="AVERAGE TIME DELIVER MATERIAL"},
                          new ReportCode(){Code="MAX_MIN",Text="TURN AROUND TIME"},
                          new ReportCode(){Code="VIP",Text="VEHICLE INSIDE PREMISES"},
                          new ReportCode(){Code="NOM",Text="OPERATOR WISE MRN"},
                          new ReportCode(){Code="AVG",Text="AVERAGE TIME IN A DAY"},
                          new ReportCode(){Code="MRNITEM",Text="ITEM WISE MRN DETAIL"},
                          new ReportCode(){Code="REPRINT_LABEL",Text="REPRINT LABEL"},
                          new ReportCode(){Code="STORE_REPORT",Text="STORE REPORT"},
                          new ReportCode(){Code="CRITICIAL_VENDOR_MRN",Text="CRITICIAL VENDOR MRN REPORT"},
                          new ReportCode(){Code="VEHICLE_IN_OUT",Text="VEHICLE IN AND OUT REPORT"}
                     };
                return al;
            }

        }
        //public List<ReportCode> GetCraneReportTypes
        //{
        //    get
        //    {
        //        List<ReportCode> al =
        //            new List<ReportCode>()
        //              {
        //                  new ReportCode(){Code="DAILY_CRANE",Text="TOTAL CRANE MANUFUCTURED"},
        //                 new ReportCode(){Code="PART_AUDIT_CRANE",Text="PARTS AUDIT"},

        //             };
        //        return al;
        //    }

        //}

        public List<ReportCode> GetCraneReportTypes
        {
            get
            {
                DataTable Dt = null;
                List<ReportCode> CraneReport = new List<ReportCode>();

                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT DESCRIPTION TEXT,PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='CRANE_REPORTS'  AND STATUS='Y'"));
                }
                else
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT xss.DESCRIPTION TEXT , rr.REPORTCODE VALUE FROM XXES_ROLE_REPORT rr JOIN XXES_SFT_SETTINGS xss 
                    ON rr.REPORTCODE = xss.PARAMVALUE WHERE rr.ROLEID = '{0}' AND rr.CATEGORY = 'CRANE_REPORTS'  AND xss.STATUS='Y'", Convert.ToString(HttpContext.Current.Session["Login_Level"])));
                }

                if (Dt.Rows.Count > 0)
                {
                    int SelectOption = 1;
                    if (SelectOption == 1)
                    {
                        CraneReport.Add(new ReportCode
                        {
                            Text = "---SELECT---",
                            Code = ""
                        });
                        SelectOption = SelectOption + 1;
                    }
                    foreach (DataRow dr in Dt.AsEnumerable())
                    {
                        CraneReport.Add(new ReportCode
                        {
                            Text = dr["TEXT"].ToString(),
                            Code = dr["VALUE"].ToString(),
                        });
                    }
                }

                return CraneReport;
            }

        }

        //public List<ReportCode> GetEKIReportTypes
        //{
        //    get
        //    {
        //        List<ReportCode> al =
        //            new List<ReportCode>()
        //              {
        //                  //created by sarthak on 25-May-2021
        //                 new ReportCode(){Code="BULK_STORAGE_ITEMS",Text="BULK STORAGE ITEMS"},
        //                 new ReportCode(){Code="SUPER_MARKET_ITEMS",Text="SUPER MARKET ITEMS"},
        //                 new ReportCode(){Code="SUPERMKT_PICKED_ITEMS",Text="SUPER MARKET PICKED ITEMS"},
        //                 new ReportCode(){Code="BULK_STORAGE_CAP",Text="BULK STORAGE CAPACITY"},
        //                 new ReportCode(){Code="SUPERMKT_MKT_CAP",Text="SUPER MARKET CAPACITY"},
        //                 new ReportCode(){Code="SHORT_RECIPT_VERIFICATION",Text="SHORT RECIPT VERIFICATION"},
        //                 new ReportCode(){Code="SHORT_RECIPT_QUALITY",Text="SHORT RECIPT QUALITY"},
        //                 new ReportCode(){Code="HOLD_ITEM_QUALITY",Text="HOLD ITEM QUALITY"},
        //                 new ReportCode(){Code="BULK_STORAGE_FAULTY",Text="BULK STORAGE FAULTY ITEMS"},
        //                 new ReportCode(){Code="SUPER_MARKET_FAULTY",Text="SUPER MARKET FAULTY ITEMS"},
        //                 new ReportCode(){Code="ITEM_EXCCED_MAXINVENTORY",Text="ITEMS EXCEEDING MAX. INVENTORY"},
        //                 new ReportCode(){Code="ITEMS_OTHER_SNP",Text="ITEMS OTHER THAN SNP"},
        //                 new ReportCode(){Code="MATERIAL_SHORT_BULK",Text="MATERIAL SHORT BULK"},
        //                 new ReportCode(){Code="MATERIAL_SHORT_SUPERMKT",Text="MATERIAL SHORT SUPERMKT"},
        //                 new ReportCode(){Code="STOCK_STATUS",Text="STOCK STATUS"},
        //                 new ReportCode(){Code="MATERIAL_BULKTEMP",Text="STOCK BULK TEMP LOCATION"},
        //                 new ReportCode(){Code="ROLL",Text="ROLLOUT"},
        //                 new ReportCode(){Code="KIT_SCANNING",Text="KIT SCANNING REPORT"},
        //                 new ReportCode(){Code="SHORT METERIAL DAILY",Text="SHORT METERIAL (DAILY)"}
        //             };
        //        return al;
        //    }

        //} 

        public List<ReportCode> GetEKIReportTypes
        {
            get
            {
                DataTable Dt = null;
                List<ReportCode> EKIReport = new List<ReportCode>();

                if (Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "GU" || Convert.ToString(HttpContext.Current.Session["Login_Unit"]) == "")
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT DESCRIPTION TEXT,PARAMVALUE VALUE FROM XXES_SFT_SETTINGS WHERE PARAMETERINFO='EKI_REPORTS'  AND STATUS='Y'"));
                }
                else
                {
                    Dt = fun.returnDataTable(string.Format(@"SELECT xss.DESCRIPTION TEXT , rr.REPORTCODE VALUE FROM XXES_ROLE_REPORT rr JOIN XXES_SFT_SETTINGS xss 
                    ON rr.REPORTCODE = xss.PARAMVALUE WHERE rr.ROLEID = '{0}' AND rr.CATEGORY = 'EKI_REPORTS'  AND xss.STATUS='Y'", Convert.ToString(HttpContext.Current.Session["Login_Level"])));
                }

                if (Dt.Rows.Count > 0)
                {
                    int SelectOption = 1;
                    if (SelectOption == 1)
                    {
                        EKIReport.Add(new ReportCode
                        {
                            Text = "---SELECT---",
                            Code = ""
                        });
                        SelectOption = SelectOption + 1;
                    }
                    foreach (DataRow dr in Dt.AsEnumerable())
                    {
                        EKIReport.Add(new ReportCode
                        {
                            Text = dr["TEXT"].ToString(),
                            Code = dr["VALUE"].ToString(),
                        });
                    }
                }

                return EKIReport;
            }

        }
    }
    public class OfflineItems
    {

        public List<OfflinePrint> GetOfflineItems
        {
            get
            {
                List<OfflinePrint> al =
                    new List<OfflinePrint>()
                      {
                        new OfflinePrint(){Code="BK",Text="BuckleUp"},
                        new OfflinePrint(){Code="EN",Text="Engine/TractorSrno"},
                        new OfflinePrint(){Code="BA",Text="Backend/TractorSrno"}, 
                        //new OfflinePrint(){Code="HYD",Text="Hydraulic Lift"}, 
                        //new OfflinePrint(){Code="RT",Text= "Rear Tyre"},
                        //new OfflinePrint(){Code="FT",Text="Front Tyre"},
                     };
                return al;
            }
        }
    }












}