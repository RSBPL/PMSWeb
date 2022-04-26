using MVCApp.CommonFunction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{
    public class Validation
    {
        

        public static Tuple<string, string, string, bool> ValidateNullEmpty(string ControllerName, string methodName, ItemModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty; bool result = true;

            if (ControllerName.ToUpper() == "FAMILYSERIAL" && methodName.ToUpper() == "SAVE")
            {
                if (string.IsNullOrEmpty(data.Plant))
                {
                    msg = str3;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.Family))
                {
                    msg = str4;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.Stage))
                {
                    msg = str5;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.Start_Serial))
                {
                    msg = str6;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.End_Serial))
                {
                    msg = str7;
                    mstType = str1;
                    status = str2;
                    result = false;
                }
                
            }

            else if (ControllerName.ToUpper() == "FAMILYSERIAL" && methodName.ToUpper() == "UPDATE")
            {
                if (string.IsNullOrEmpty(data.Start_Serial))
                {
                    msg = str6;
                    mstType = str1;
                    status = str2;
                    result = false;
                }
                if (string.IsNullOrEmpty(data.End_Serial))
                {
                    msg = str7;
                    mstType = str1;
                    status = str2;
                    result = false;
                }
            }
            return new Tuple<string, string, string, bool>(msg, mstType, status, result);
        }

        public static Tuple<string, string, string, bool> ValidatePlantMaster(string ControllerName, string methodName, PlantAndFamily data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty; bool result = true;

            if (ControllerName.ToUpper() == "PLANTMASTER" && methodName.ToUpper() == "SAVE")
            {
                if (string.IsNullOrEmpty(data.PlantCode))
                {
                    msg = str12;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.PlantName))
                {
                    msg = str13;
                    mstType = str1;
                    status = str2;
                    result = false;
                }
                
            }

            else if (ControllerName.ToUpper() == "PLANTMASTER" && methodName.ToUpper() == "UPDATE")
            {
                if (string.IsNullOrEmpty(data.PlantCode))
                {
                    msg = str12;
                    mstType = str1;
                    status = str2;
                    result = false;
                }
                if (string.IsNullOrEmpty(data.PlantName))
                {
                    msg = str13;
                    mstType = str1;
                    status = str2;
                    result = false;
                }
            }

            return new Tuple<string, string, string, bool>(msg, mstType, status, result);
        }

        public static Tuple<string, string, string, bool> ValidateStageMaster(string ControllerName, string methodName, StageMsterModel data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty; bool result = true;

            if (ControllerName.ToUpper() == "STAGEMASTER" && methodName.ToUpper() == "SAVE")
            {
                if (string.IsNullOrEmpty(data.PLANT_CODE))
                {
                    msg = str3;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    msg = str4;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.STAGE_ID))
                {
                    msg = str15;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.OFFLINEITEMS))
                {
                    msg = str5;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.STAGE_DESCRIPTION))
                {
                    msg = str16;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

            }

            else if (ControllerName.ToUpper() == "STAGEMASTER" && methodName.ToUpper() == "UPDATE")
            {
                if (string.IsNullOrEmpty(data.PLANT_CODE))
                {
                    msg = str3;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.FAMILY_CODE))
                {
                    msg = str4;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.STAGE_ID))
                {
                    msg = str15;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.OFFLINEITEMS))
                {
                    msg = str5;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.STAGE_DESCRIPTION))
                {
                    msg = str16;
                    mstType = str1;
                    status = str2;
                    result = false;
                }
            }
            return new Tuple<string, string, string, bool>(msg, mstType, status, result);
        }


        public static Tuple<string, string, string, bool> ValidateFamilyMaster(string ControllerName, string methodName, PlantAndFamily data)
        {
            string msg = string.Empty, mstType = string.Empty, status = string.Empty; bool result = true;

            if (ControllerName.ToUpper() == "FAMILYMASTER" && methodName.ToUpper() == "SAVE")
            {
                if (string.IsNullOrEmpty(data.FamilyCode))
                {
                    msg = str19;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.FamilyName))
                {
                    msg = str20;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.NoOfStages))
                {
                    msg = str21;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.ORGId))
                {
                    msg = str22;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

            }

            else if (ControllerName.ToUpper() == "FAMILYMASTER" && methodName.ToUpper() == "UPDATE")
            {
                if (string.IsNullOrEmpty(data.FamilyCode))
                {
                    msg = str19;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.FamilyName))
                {
                    msg = str20;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.NoOfStages))
                {
                    msg = str21;
                    mstType = str1;
                    status = str2;
                    result = false;
                }

                else if (string.IsNullOrEmpty(data.ORGId))
                {
                    msg = str22;
                    mstType = str1;
                    status = str2;
                    result = false;
                }
            }

            return new Tuple<string, string, string, bool>(msg, mstType, status, result);
        }

        //public static Tuple<string, string, string, bool> ValidateChangePassword(string ControllerName, string methodName, Login data)
        //{
        //    string msg = string.Empty, mstType = string.Empty, status = string.Empty; bool result = true;

        //    if (ControllerName.ToUpper() == "CHANGEPASSWORD" && methodName.ToUpper() == "UPDATE")
        //    {
        //        if (string.IsNullOrEmpty(data.Password))
        //        {
        //            msg = str23;
        //            mstType = str1;
        //            status = str2;
        //            result = false;
        //        }

        //        else if (string.IsNullOrEmpty(data.NewPassword))
        //        {
        //            msg = str24;
        //            mstType = str1;
        //            status = str2;
        //            result = false;
        //        }

        //        else if (string.IsNullOrEmpty(data.ConfirmNewPassword))
        //        {
        //            msg = str25;
        //            mstType = str1;
        //            status = str2;
        //            result = false;
        //        }

               
        //    }

           
        //    return new Tuple<string, string, string, bool>(msg, mstType, status, result);
        //}


        //declare read-only message
        public static readonly string stus = "success";
        public static readonly string str = "alert-success";
        public static readonly string str1 = "alert-danger";
        public static readonly string str2 = "error";
        public static readonly string str3 = "Please Select Plant ..";
        public static readonly string str4 = "Please Select Family ..";
        public static readonly string str5 = "Please Select Stage ..";
        public static readonly string str6 = "Please Fill Start Serial ..";
        public static readonly string str7 = "Please Fill End Serial ..";
        public static readonly string str8 = "Number of digits must be Same in Start and End Serial ..";
        public static readonly string str9 = "Saved successfully...";
        public static readonly string str10 = "Item already exists ..";
        public static readonly string str11 = "Update successfully...";
        public static readonly string str12 = "Please Fill Plant Code ..";
        public static readonly string str13 = "Please Fill Plant Name ..";
        public static readonly string str14 = "Plant Code already exists ..";
        public static readonly string str15 = "Please Select Stage Id ..";
        public static readonly string str16 = "Please Fill Description ..";
        public static readonly string str17 = "The selected stage or stage-Id is already exists for same plant and family ..";
        public static readonly string str18 = "The IP Address already exists for some other stage ..";
        public static readonly string str19 = "Please Fill Family Code .. ..";
        public static readonly string str20 = "Please Fill Family Name ..";
        public static readonly string str21 = "Please Fill No Of Stages ..";
        public static readonly string str22 = "Please Fill ORG ID ..";
        public static readonly string str23 = "Record Deleted successfully...";
        public static readonly string str24 = "Plant,Family,Item Code & Packing Standard are required...";
        public static readonly string str25 = "Please Fill Current Serial ..";
        public static readonly string str26 = "Plant, Family, Item Code, Location, Capicity, Safty Stock,No. of Bins are reqired..!";
        public static readonly string str28 = "Location already exists..!";
        public static readonly string str29 = "Plant, Family, Date Range are reqired..!";
        public static readonly string str30 = "Plant & Family are reqired..!";
        public static readonly string str31 = "The selected Item Code is already exists for same plant and family ..";
        public static readonly string str32 = "Plant,Family,Kit No,Item Code,Super MKT Location & Quantity are required...";
        public static readonly string str33 = "Kit No. already exists in ScanKit table it can't Delete ";
        public static readonly string str34 = "Quantity should be greater then 0";
        public static readonly string str35 = "Quantity should be Numeric only";
        public static readonly string str36 = "Safty Stock Quantity Should Not be Greater Than Capicity";
        public static readonly string str37 = "Safty Stock Quantity Should be Greater Than 0";
        public static readonly string str38 = "No.Of Bins Should be Greater Than 0";
        public static readonly string str39 = "ItemCode already exists..!";
        public static readonly string str40 = "Plant, Family, Location are reqired..!";
        public static readonly string str41 = "Invalid Item Code or Sumer MKT Location ..";
        public static readonly string str42 = "Deviation Qty Required..!";
        public static readonly string str43 = "Please Select End Date..!";
        public static readonly string str44 = "Deviation Qty Should be Greater than Zero..!";
        public static readonly string str45 = "Deviation Already Exist to Selected Vendor Name & ItemCode..!";


    }
    
}