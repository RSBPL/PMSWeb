﻿using MVCApp.CommonFunction;
using PagedList;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class SubAssemblyModel
    {
        public DateTime PlantDate { get; set; }
        public String PLANTCODE { get; set; }
        public String FAMILYCODE { get; set; }
        public String ShiftCODE { get; set; }
        public string lblRelJob { get; set; }
        public string lblNext { get; set; }
        public string lblPrev { get; set; }
        public bool lblRelJobVisible { get; set; }
        public string lblPending { get; set; }
        public bool lblPendingVisible { get; set; }
        public string Item { get; set; }
        public string TRAN_ID { get; set; }
        public string Plant { get; set; }
        public string Family { get; set; }
        public string Orgid { get; set; }
        public string ITEMCODE { get; set; }
        public int ITEMCODEIndex { get; set; }
        public bool OptFreeSrno { get; set; }
        public string ASSIGNITEMCODE { get; set; }
        public string ASSIGNSRNNo { get; set; }
        public string SerialITEMCODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string AUTOID { get; set; }
        public string PENDING { get; set; }
        public string COMPLETED { get; set; }
        public string SerialNumber { get; set; }
        public string Series { get; set; }
        public string Stage { get; set; }
        public string Job { get; set; }
        public string ASSIGNJob { get; set; }
        public string SEQ_NO { get; set; }
        public string QTY { get; set; }
        public string ASSIGNQTY { get; set; }
        public string TranId { get; set; }
        public string SubAssembly_Id { get; set; }
        public bool PrintDesc { get; set; }
        public bool IsQuality { get; set; }
        public bool optAsPerPlanning { get; set; }
        public bool optAllDcodes { get; set; }
        public string PrintMode { get; set; }
        public string Prefix1 { get; set; }
        public int draw { get; set; }
        public string start { get; set; }
        public string STARTROWINDEX { get; set; }
        public string MAXROWS { get; set; }
        public string P_Search { get; set; }
        public int TOTALCOUNT { get; set; }
        public int length { get; set; }
        public string Description { get; set; }
        public string Itemcode { get; set; }
        public string DuplicateFlag { get; set; }

    }
    public class funsubAssembly
    {
        Function fun = new Function();
        string query = string.Empty;
        string TractorFamily = string.Empty;

      


        private int Getdays(string Plant,string family)
        {
            int days = 0;
            try
            {
                query = string.Format(@"SELECT NVL(PARAMVALUE,0) FROM xxes_sft_settings 
                        WHERE STATUS = 'PL_DAYS' and plant_code='{0}' and family_code='{1}'",
                  Convert.ToString(Plant).Trim(),
                  Convert.ToString(family).Trim());
                string d = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(d))
                    days = Convert.ToInt32(d);

            }
            catch (Exception ex)
            {

                throw;
            }
            return days;
        }
        private bool isHolidayExists(DateTime dateTime,string plant)
        {
            try
            {
                string day = dateTime.DayOfWeek.ToString().Substring(0, 3);
                query = string.Format(@"select count(*) from xxes_holidays where trunc(holi_date)='{0}'
                and plant_code='{1}'", dateTime.ToString("dd-MMM-yyyy"), Convert.ToString(plant));
                return Convert.ToBoolean(fun.CheckExits(query));
            }
            catch (Exception)
            {

                throw;
            }
        }
        private int getSecSaturDay(int month, int year)
        {
            DateTime dtSat = new DateTime(year, month, 1);
            int intSatDay = 14 - Convert.ToInt32(dtSat.DayOfWeek);
            return intSatDay;
        }
        private bool isWeeklyOff(DateTime dateTime,string plant)
        {
            try
            {
                string day = dateTime.DayOfWeek.ToString().Substring(0, 3).ToUpper();
                query = string.Format(@"select paramvalue from xxes_sft_settings where status='W_OFF'
                and plant_code='{0}'", Convert.ToString(plant));
                string woff = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(woff))
                {
                    if (woff.Trim().ToUpper() == ("Only Second Saturday and All Sunday").ToUpper())
                    {
                        if (day.Equals("SAT"))
                        {
                            if (getSecSaturDay(dateTime.Month, dateTime.Year) == 2)
                            {
                                return true;
                            }
                        }
                        else if (day.Equals("SUN"))
                            return true;
                    }
                    else if (woff.Trim().ToUpper().Equals(day))
                        return true;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }
        public string GetPlanId(SubAssemblyModel subAssemblyModel)
        {
            try
            {
                DateTime dateTime = new DateTime();
                if (Convert.ToString(HttpContext.Current.Session["Login_Level"]) != "TP")
                {
                    dateTime = subAssemblyModel.PlantDate.AddDays(Getdays(subAssemblyModel.PLANTCODE, subAssemblyModel.FAMILYCODE));
                    while (isWeeklyOff(dateTime,subAssemblyModel.PLANTCODE))
                        dateTime = dateTime.AddDays(1);
                    while (isHolidayExists(dateTime, subAssemblyModel.PLANTCODE))
                        dateTime = dateTime.AddDays(1);
                    subAssemblyModel.PlantDate = dateTime;
                    //subAssemblyModel.PlantDate = false;
                }
                else
                {
                    dateTime = subAssemblyModel.PlantDate;
                }
                setFamily(subAssemblyModel.PLANTCODE);
                return fun.get_Col_Value(
                    string.Format(
                    @"select p.PLAN_ID from XXES_DAILY_PLAN_MASTER p , XXES_SUBASEMBLY_APPROVEDPLAN s
                where p.plant_code=s.plant_code and p.family_code=s.family_code and p.plan_id=s.plan_id
                and to_char(p.PLAN_DATE,'dd-Mon-yyyy')='{0}' and p.SHIFTCODE='{1}' and  p.plant_code='{2}' 
                and p.family_code='{3}' and s.status='APPROVED'"
                , dateTime.Date.ToString("dd-MMM-yyyy"), Convert.ToString(subAssemblyModel.ShiftCODE),
                Convert.ToString(subAssemblyModel.PLANTCODE).Trim().ToUpper(),
                Convert.ToString(subAssemblyModel.FAMILYCODE).Trim().ToUpper())
                );
            }
            catch (Exception)
            {

                throw;
            }

        }

        public bool isNoalreadyExists(string srno, string plant, string family, string itemcode = null)
        {
            bool status = false;
            try
            {
                if (!string.IsNullOrEmpty(itemcode))
                {
                    query = string.Format(@"select count(*) from print_serial_number where plant_code='{0}' 
                and serial_number='{1}' and ITEM_CODE='{2}'", plant, srno, itemcode);
                    if (fun.CheckExits(query))
                        return true;

                    query = string.Format(@"select count(*) from xxes_print_serials where plant_code='{0}' 
                    and srno='{1}' and family_code='{2}' and DCODE='{3}'", plant, srno, family, itemcode);
                    if (fun.CheckExits(query))
                        return true;
                }
                else
                {
                    query = string.Format(@"select count(*) from xxes_print_serials where plant_code='{0}' 
                    and srno='{1}' and family_code='{2}' ", plant, srno, family);
                    if (fun.CheckExits(query))
                        return true;
                }
            }
            catch (Exception)
            {

                throw;
            }
            return false;
        }
        private void setFamily(string plant)
        {
            try
            {
                if (Convert.ToString(plant) == "T04")
                    TractorFamily = "TRACTOR FTD";
                else if (Convert.ToString(plant) == "T05")
                    TractorFamily = "TRACTOR TD";
            }
            catch (Exception ex)
            {


            }
        }
        public string GetOfflineCode(string family)
        {
            string code = string.Empty;
            try
            {
                family = family.Trim().ToUpper();
                if (family.Equals("REARAXLE FTD") || family.Equals("REAR AXEL FTD"))
                    code = "RAB";
                else if (family.Equals("ENGINE FTD"))
                    code = "ENF";
                else if (family.Equals("ENGINE TD"))
                    code = "ENP";
                else if (family.Equals("TRANSMISSION FTD"))
                    code = "TRB";
                else if (family.Equals("BACK END TD") || family.Equals("BACKEND TD"))
                    code = "BAB";  //BABL
                else if (family.Equals("HYDRAULIC FTD"))
                    code = "HYD";

            }
            catch (Exception)
            {

                throw;
            }
            return code;
        }
        public string getSrnoField(string family)
        {
            string field = string.Empty;
            try
            {
                if (family.Trim().ToUpper().Contains("ENGINE"))
                {
                    field = "ENGINE_SRLNO";
                }
                else if (family.Trim().ToUpper().Contains("REAR AXEL"))
                {
                    field = "REARAXEL_SRLNO";
                }
                else if (family.Trim().ToUpper().Contains("TRANSMISSION"))
                {
                    field = "TRANSMISSION_SRLNO ";
                }
                else if (family.Trim().ToUpper().Contains("HYDRAULIC"))
                {
                    field = "HYDRALUIC_SRLNO";
                }
                else if (family.Trim().ToUpper().Contains("BACK END"))
                {
                    field = "BACKEND_SRLNO";
                }
                return field;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string DisplayPrevNextSerialNo(string plant, string family)
        {
            string lblNext = string.Empty;
            string lblPrev = string.Empty;
            string stage = GetOfflineCode(family.Trim().ToUpper());
                if (stage == "") return"";
                string Current_Serial_number = "", Suffix = "";
                Current_Serial_number = fun.getSeries(plant, family, stage);
                if (Current_Serial_number.Trim().Split('#')[1].Trim() == "")
                {
                    //MessageBox.Show("Series not define for selected stage", PubFun.AppName);
                    return "";
                }
                if (family.Trim().ToUpper().Contains("TRACTOR"))
                {
                    Suffix = fun.get_Col_Value("select MY_CODE from XXES_SUFFIX_CODE where MON_YYYY='" + fun.GetServerDateTime().ToString("MMM-yyyy").ToUpper() + "' and TYPE='DOMESTIC' and plant='" + plant.Trim() + "'");
                    Current_Serial_number = Current_Serial_number.Replace("#", "").Trim() + Suffix.Trim();
                }
                if (!string.IsNullOrEmpty(Current_Serial_number))
                {
                    lblNext = "Next Serial No: " + Current_Serial_number.Trim().ToUpper().Replace("#", "");
                    //lblNext.Visible = true;
                }
                else
                    lblNext = "";
                query = string.Format(@"select * from (select srno from xxes_print_serials where plant_code='{0}'
                and family_code='{1}' and offline_keycode='{2}'  order by printdate desc) where rownum=1",
                plant, family, stage);
                Current_Serial_number = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(Current_Serial_number))
                {
                    lblPrev = "Previous Serial No: " + Current_Serial_number.Trim().ToUpper();
                    //lblPrev.Visible = true;
                }
                else
                    lblPrev  = "";
            return lblNext + "=" +lblPrev;
        }
    }
}