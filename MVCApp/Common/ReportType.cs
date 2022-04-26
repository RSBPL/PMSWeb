using System;
using System.Collections.Generic;
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
        public List<ReportCode> GetEngineReportTypes
        {
            get
            {
                List<ReportCode> al =
                    new List<ReportCode>()
                      {
                          new ReportCode(){Code="ERROR",Text="SCAN ERRORS"},
                          new ReportCode(){Code="EN_AUDIT",Text="ENGINE AUDIT"},
                          new ReportCode(){Code="TOT_ENG",Text="TOTAL ENGINES MANUFUCTURED"},
                          new ReportCode(){Code="PRDENGINES",Text="ENGINES MANUFUCTURED"},
                          new ReportCode(){Code="PART_AUDIT",Text="PARTS AUDIT"},


                     };
                return al;
            }

        }
        public List<ReportCode> GetBackendReportTypes
        {
            get
            {
                List<ReportCode> al =
                    new List<ReportCode>()
                      {
                          new ReportCode(){Code="PRDBACKEND",Text="BACKEND PRODUCTION"},


                     };
                return al;
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
        public List<ReportCode> GetHydraulicReportTypes
        {
            get
            {
                List<ReportCode> al =
                    new List<ReportCode>()
                      {
                          new ReportCode(){Code="PRDHYDRAULIC",Text="HYDRAULIC PRODUCTION"},

                     };
                return al;
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
        public List<ReportCode> GetTractorReportTypes
        {
            get
            {
                List<ReportCode> al =
                    new List<ReportCode>()
                      {
                          new ReportCode(){Code="TOT",Text="TOTAL TRACTORS MANUFUCTURED"},
                          new ReportCode(){Code="JOB",Text="JOB HISTORY"},
                          new ReportCode(){Code="PLAN_AUDIT",Text="DAILY PLAN AUDIT"},
                          new ReportCode(){Code="JOBDATA",Text="DAILY PLAN JOBS"},
                          new ReportCode(){Code="RELJOBS",Text="RELEASED JOBS"},
                          new ReportCode(){Code="ERROR",Text="SCAN ERRORS"},
                          new ReportCode(){Code="HYD",Text="HYDRAULIC PRINTED"},
                          new ReportCode(){Code="RT",Text="REARTYRE PRINTED"},
                          new ReportCode(){Code="FT",Text="FRONTTYRE PRINTED"},
                          new ReportCode(){Code="TOT_ENG",Text="TOTAL ENGINES MANUFUCTURED"},
                          new ReportCode(){Code="CHECK_JOB",Text="JOB EXISTENCE"},
                          new ReportCode(){Code="PART_AUDIT",Text="PARTS AUDIT"},
                          new ReportCode(){Code="PRD",Text="PRODUCTION REPORT"},
                          new ReportCode(){Code="BULK_STORAGE_CAP",Text="BULK STORAGE CAPACITY"},
                         new ReportCode(){Code="SUPERMKT_MKT_CAP",Text="SUPER MARKET CAPACITY"},

                         //CREATED BY SARTHAK ON 29.11.2021
                         new ReportCode(){Code="SHORT_RECIPT_QUALITY",Text="SHORT RECIPT QUALITY"},
                         new ReportCode(){Code="HOLD_ITEM_QUALITY",Text="HOLD ITEM QUALITY"},
                         //CREATED BY RAJ ON 08.12.2021
                         new ReportCode(){Code="DAILY_PART_SCANNING_EFFICIENCY",Text="DAILY PART SCANNING EFFICIENCY"},
                         //new ReportCode(){Code="ROLLOUT",Text="ROLLOUT"},
                         new ReportCode(){Code="ROLL",Text="ROLLOUT"},

                         new ReportCode(){Code="ROLL_AUDIT",Text="ROLL OUT STICKER LOG"},
                         new ReportCode(){Code="PDI_AUDIT",Text="PDI STICKER LOG"}


                     };
                return al;
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
                          new ReportCode(){Code="STORE_REPORT",Text="STORE REPORT"}
                     };
                return al;
            }

        }
        public List<ReportCode> GetCraneReportTypes
        {
            get
            {
                List<ReportCode> al =
                    new List<ReportCode>()
                      {
                          new ReportCode(){Code="DAILY_CRANE",Text="TOTAL CRANE MANUFUCTURED"},
                         new ReportCode(){Code="PART_AUDIT_CRANE",Text="PARTS AUDIT"},

                     };
                return al;
            }

        }

        public List<ReportCode> GetEKIReportTypes
        {
            get
            {
                List<ReportCode> al =
                    new List<ReportCode>()
                      {
                          //created by sarthak on 25-May-2021
                         new ReportCode(){Code="BULK_STORAGE_ITEMS",Text="BULK STORAGE ITEMS"},
                         new ReportCode(){Code="SUPER_MARKET_ITEMS",Text="SUPER MARKET ITEMS"},
                         new ReportCode(){Code="SUPERMKT_PICKED_ITEMS",Text="SUPER MARKET PICKED ITEMS"},
                         new ReportCode(){Code="BULK_STORAGE_CAP",Text="BULK STORAGE CAPACITY"},
                         new ReportCode(){Code="SUPERMKT_MKT_CAP",Text="SUPER MARKET CAPACITY"},
                         new ReportCode(){Code="SHORT_RECIPT_VERIFICATION",Text="SHORT RECIPT VERIFICATION"},
                         new ReportCode(){Code="SHORT_RECIPT_QUALITY",Text="SHORT RECIPT QUALITY"},
                         new ReportCode(){Code="HOLD_ITEM_QUALITY",Text="HOLD ITEM QUALITY"},
                         new ReportCode(){Code="BULK_STORAGE_FAULTY",Text="BULK STORAGE FAULTY ITEMS"},
                         new ReportCode(){Code="SUPER_MARKET_FAULTY",Text="SUPER MARKET FAULTY ITEMS"},
                         new ReportCode(){Code="ITEM_EXCCED_MAXINVENTORY",Text="ITEMS EXCEEDING MAX. INVENTORY"},
                         new ReportCode(){Code="ITEMS_OTHER_SNP",Text="ITEMS OTHER THAN SNP"},
                         new ReportCode(){Code="MATERIAL_SHORT_BULK",Text="MATERIAL SHORT BULK"},
                         new ReportCode(){Code="MATERIAL_SHORT_SUPERMKT",Text="MATERIAL SHORT SUPERMKT"},
                         new ReportCode(){Code="STOCK_STATUS",Text="STOCK STATUS"},
                         new ReportCode(){Code="MATERIAL_BULKTEMP",Text="STOCK BULK TEMP LOCATION"},
                         new ReportCode(){Code="ROLL",Text="ROLLOUT"},
                         new ReportCode(){Code="KIT_SCANNING",Text="KIT SCANNING REPORT"}
                     };
                return al;
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