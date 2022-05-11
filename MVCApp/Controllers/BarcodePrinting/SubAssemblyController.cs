using MVCApp.Common;
using MVCApp.CommonFunction;
using MVCApp.Models;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MVCApp.Controllers.Masters
{
    [Authorize]
    public class SubAssemblyController : Controller
    {

        OracleCommand cmd;
        OracleDataAdapter DA;
        OracleDataReader dr;
        DataTable dt;
        Function fun = new Function();
        DataTable dtPrint = new DataTable();
        funsubAssembly SubAssFun = new funsubAssembly();
        DataTable dtQuality = new DataTable();
        PrintAssemblyBarcodes printAssemblyBarcodes = new PrintAssemblyBarcodes();
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "", LoginStageCode = "", Login_User="";
        string plantCode = "";string FamilyCode = ""; string  prefix1 = string.Empty, selectedJob = string.Empty, orgid = string.Empty, stage = string.Empty, printqty = string.Empty, NOT_VALIDATE_JOB = string.Empty;
        // GET: FamilyMaster

        [HttpGet]
        public ActionResult Index()
        {
            if (string.IsNullOrEmpty(Convert.ToString(Session["Login_User"])))
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                ViewBag.value = DateTime.Now;
                return View();
            }
        }
        public ActionResult Gridtest(string PLANT_CODE,string FAMILY_CODE, DateTime PlantDate,string ShiftCODE)
        {
            SubAssemblyModel obj = new SubAssemblyModel();
            int recordsTotal = 0; string avlqury = string.Empty; string planid = string.Empty;
            planid = SubAssFun.GetPlanId(PLANT_CODE, FAMILY_CODE, PlantDate, ShiftCODE);
            planid = SubAssFun.GetPlanId(PLANT_CODE, FAMILY_CODE, PlantDate, ShiftCODE);
            if (string.IsNullOrEmpty(planid))
            {
                return Json(new
                {
                    draw = obj.draw
                }, JsonRequestBehavior.AllowGet);
            }
            string keycode = SubAssFun.GetOfflineCode(Convert.ToString(FAMILY_CODE));
            string field = SubAssFun.getSrnoField(Convert.ToString(FAMILY_CODE));
            avlqury = @" (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = '" + keycode + "' " +
                " AND PLANT_CODE = '" + Convert.ToString(PLANT_CODE) + "' AND DCODE = a.ITEMCODE " +
                            "AND SRNO NOT IN(SELECT " + field + " FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + Convert.ToString(PLANT_CODE) + "' AND FAMILY_CODE = '" + Convert.ToString(FAMILY_CODE) + "' AND " + field + " IS NOT NULL) ) AS AVAILABLE";
            //notqry = @" (select count(*) from XXES_PRINT_SERIALS WHERE (QCOK <> 'Y' or QCOK IS NULL) AND OFFLINE_KEYCODE = '" + keycode + "' " +
            //   " AND PLANT_CODE = '" + Convert.ToString(cmbPlant.SelectedValue) + "' AND DCODE = a.ITEMCODE " +
            //               "AND SRNO NOT IN(SELECT " + field + " FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + Convert.ToString(cmbPlant.SelectedValue) + "' AND FAMILY_CODE = '" + Convert.ToString(cmbFamily.SelectedValue) + "' AND " + field + " IS NOT NULL) ) AS QCNOTOK";
            query = string.Format(@"select a.AUTOID,a.TRAN_ID, a.SEQ_NO,
                a.ITEMCODE,a.DESCRIPTION,a.QTY, {3},
                (select count(*) from xxes_print_serials where plant_code=a.plant_code
                and family_code=a.family_code and SUBASSEMBLY_ID=a.AUTOID) COMPLETED,
                a.QTY-(select count(*) from xxes_print_serials where plant_code=a.plant_code
                and family_code=a.family_code and SUBASSEMBLY_ID=a.AUTOID) PENDING
                FROM xxes_daily_plan_assembly A
                WHERE a.PLANT_CODE='{0}' AND a.FAMILY_CODE='{1}' AND plan_id='{2}'  order by a.SEQ_NO ",
            Convert.ToString(PLANT_CODE), Convert.ToString(FAMILY_CODE),
            planid, avlqury);
            List<SubAssemblyModel> subAssemblies = new List<SubAssemblyModel>();
            DataTable dataTable = fun.returnDataTable(query);
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow dr in dataTable.Rows)
                {
                    SubAssemblyModel subAssembly = new SubAssemblyModel
                    {
                        AUTOID = dr["AUTOID"].ToString(),
                        TRAN_ID = dr["TRAN_ID"].ToString(),
                        ITEMCODE = dr["ITEMCODE"].ToString(),
                        DESCRIPTION = dr["DESCRIPTION"].ToString(),
                        QTY = dr["QTY"].ToString(),
                        COMPLETED = dr["COMPLETED"].ToString(),
                        PENDING = dr["PENDING"].ToString()
                    };
                    //BindItemCode(obj, planid);
                    subAssemblies.Add(subAssembly);
                }
            }
            return Json(new { draw = obj.draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = subAssemblies }, JsonRequestBehavior.AllowGet);
       
        }
       

        [HttpGet]
        public JsonResult BindPlant()
        {
            return Json(fun.Fill_Unit_Name(), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult BindFamily(string Plant)
        {
            List<DDLTextValue> result = new List<DDLTextValue>();
            if (!string.IsNullOrEmpty(Plant))
            {
                result = fun.Fill_All_Family(Plant);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult Bindshift()
        {
            return Json(fun.FillShift(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public JsonResult BindItemCode(string PLANTCODE,bool optAsPerPlanning,string FAMILYCODE,string ShiftCODE,DateTime PlantDate)
        {
            string query = string.Empty; string plant = string.Empty; string family = string.Empty;
            plantCode = "";
            FamilyCode = "";
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                DataTable dt = new DataTable();
                SubAssemblyModel data = new SubAssemblyModel();
                data.PLANTCODE = PLANTCODE;
                data.FAMILYCODE = FAMILYCODE;
                data.ShiftCODE = ShiftCODE;
                data.PlantDate = PlantDate;
                string plantID = SubAssFun.GetPlanId(PLANTCODE, FAMILYCODE, PlantDate, ShiftCODE);
                family = Convert.ToString(FAMILYCODE).Trim();
                if (string.IsNullOrEmpty(plantID) || string.IsNullOrEmpty(family))
                {
                    return Json(Item, JsonRequestBehavior.AllowGet);
                }
                //and dcode = a.itemcode
                if (optAsPerPlanning==true)
                    query = string.Format(@"SELECT  a.SEQ_NO,a.ITEMCODE,a.QTY,
                    a.DESCRIPTION || ' # ' || a.itemcode   ITEM,
                    a.ITEMCODE || '#' || a.TRAN_ID || '#' || a.AUTOID  || '#' || a.QTY ITEMVALUE
                    FROM xxes_daily_plan_assembly A
                    WHERE a.PLANT_CODE='{0}' AND a.FAMILY_CODE='{1}' AND plan_id='{2}' and (A.status is null or A.status='APPROVED')  and
                    a.qty>(select count(*) from XXES_PRINT_SERIALS where plant_code='{0}' and family_code='{1}'
                    and SUBASSEMBLY_ID=a.AUTOID  )
                    order by a.SEQ_NO ",
                    PLANTCODE, family, plantID);
                else if (optAsPerPlanning == true)
                    query = "SELECT DISTINCT A.SEGMENT1,A.ITEM_DESCRIPTION || ' # ' ||  A.SEGMENT1 ||  DESCRIPTION FROM RELESEDJOBORDER A WHERE A.organization_id = " + fun.GetOrgId(PLANTCODE) + " AND A.FAMILY_CODE = '" + family.Trim() + "' ORDER BY SEGMENT1";
                
                //query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%' )order by segment1", data.Item.Trim().ToUpper(), data.Item.Trim().ToUpper());
                dt = fun.returnDataTable(query);


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["ITEM"].ToString(),
                            Value = dr["ITEMVALUE"].ToString(),
                        });
                    }
                    plantCode = PLANTCODE;
                    FamilyCode = FAMILYCODE;
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(Item, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult BindQualityItemCode(string PLANTCODE, string FAMILYCODE, DateTime QualityDate)
        {
            string query = string.Empty; string orgid = string.Empty; string plant = string.Empty; string family = string.Empty;
            plantCode = "";
            FamilyCode = "";
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string schema = Convert.ToString(ConfigurationSettings.AppSettings["Schema"]);
                DataTable dt = new DataTable();
                SubAssemblyModel data = new SubAssemblyModel();
                data.PLANTCODE = PLANTCODE;
                data.FAMILYCODE = FAMILYCODE;
                data.PlantDate = QualityDate;
                plant = Convert.ToString(PLANTCODE).Trim();
                family = Convert.ToString(FAMILYCODE).Trim();
                orgid = fun.GetOrgId(plant);
                query = @"select s.SERIAL_NUMBER , 
                p.FCODE || '(' || ' ITEMCODE: ' || p.DCODE ||  ')' || ' SERIAL: ' || s.SERIAL_NUMBER || ' JOB-' || p.jobid || ' DESCRIPTION=' || p.DESCRIPTION ITEM_CODE
                from " + Convert.ToString(ConfigurationSettings.AppSettings["ITEMS_USER"]) + ".mtl_serial_numbers s, xxes_print_serials p where s.serial_number=p.srno  and s.current_organization_id=" + orgid + " and s.current_status<>1 and s.current_status is not null  and QCOK is null and p.plant_code='" + plant + "' and p.family_code='" + family + "' " +
                " and trunc(completion_date)>='" + QualityDate.ToString("dd-MMM-yyyy") + "'";
                using (DataTable dataTable = fun.returnDataTable(query))
                {
                    if (dataTable.Rows.Count > 0)
                    {
                        dtQuality = new DataTable();
                        dtQuality = dataTable;
                        foreach (DataRow dr in dataTable.AsEnumerable())
                        {
                            Item.Add(new DDLTextValue
                            {
                                Text = dr["ITEM_CODE"].ToString(),
                                Value = dr["SERIAL_NUMBER"].ToString(),
                            });
                        }
                    }
                }

                

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(Item, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult FillJobsForDropdown(string FAMILYCODE,string ITEMCODE,string PLANTCODE)
        {
            string query = string.Empty;
            List<DDLTextValue> Item = new List<DDLTextValue>();
            try
            {
                string itemcode = Convert.ToString(ITEMCODE).Split('#')[0].Trim();
                string strSQL = "SELECT WIP_ENTITY_NAME JOB,START_QUANTITY QTY,NVL(CC,0) AS PRINTED_QTY,(WIP_ENTITY_NAME||'-'||START_QUANTITY||'-'||NVL(CC,0)) As Text FROM " +
                 "(SELECT DISTINCT A.WIP_ENTITY_NAME,START_QUANTITY,SEGMENT1 FROM RELESEDJOBORDER A WHERE A.FAMILY_CODE = '" + FAMILYCODE + "'  " +
                " ) X LEFT JOIN " +
                "(SELECT JOBID ,COUNT(SRNO) AS CC  FROM xxes_print_serials " +
                "GROUP BY JOBID) Y ON  X.WIP_ENTITY_NAME = Y.JOBID WHERE X.START_QUANTITY != NVL( Y.CC,0)  and SEGMENT1 ='" + ITEMCODE.Trim() + "' ORDER BY WIP_ENTITY_NAME";


                //query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%' )order by segment1", data.Item.Trim().ToUpper(), data.Item.Trim().ToUpper());
                dt = fun.returnDataTable(strSQL);


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Item.Add(new DDLTextValue
                        {
                            Text = dr["Text"].ToString(),
                            Value = dr["JOB"].ToString(),
                        });
                    }
                }
                prefix1 = SubAssFun.getEnginePrefix(Convert.ToString(PLANTCODE), Convert.ToString(FAMILYCODE)
                       , Convert.ToString(ITEMCODE));

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(Item, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult bindReprintSrno(string PLANTCODE, string FAMILYCODE)
        {
            List<DDLTextValue> Srno = new List<DDLTextValue>();
            try
            {
           
          
                query = string.Format(@"select DCODE || ' (' || SRNO || ')' || 'JOB: ' || JOBID TEXT, SRNO from XXES_PRINT_SERIALS  where plant_code='{0}'
                and family_code='{1}' and QCOK is null and rework is null ",
                Convert.ToString(PLANTCODE), Convert.ToString(FAMILYCODE));


                //query = string.Format(@"select distinct segment1 || ' # ' || description as DESCRIPTION, segment1 as ITEM_CODE from " + schema + ".mtl_system_items " +
                //"where organization_id in (" + orgid + ") and substr(segment1, 1, 1) in ('D','S') AND (SEGMENT1 LIKE '%{0}%' OR DESCRIPTION LIKE '%{1}%' )order by segment1", data.Item.Trim().ToUpper(), data.Item.Trim().ToUpper());
                dt = fun.returnDataTable(query);


                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.AsEnumerable())
                    {
                        Srno.Add(new DDLTextValue
                        {
                            Text = dr["Text"].ToString(),
                            Value = dr["SRNO"].ToString(),
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                fun.LogWrite(ex);
                throw;

            }
            return Json(Srno, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult FillJobIDs(string FAMILYCODE, string PLANTCODE, string ITEMCODE)
        {
            SubAssemblyModel subAssemblyModel = new SubAssemblyModel();
            try
            {
                //SEGMENT1,ITEM_DESCRIPTION
                string orgid = fun.GetOrgId(Convert.ToString(PLANTCODE));
                ITEMCODE = ITEMCODE.Split('#')[0].Trim();
                query = string.Format(@"select count(*) RELJOB from RELESEDJOBORDER A 
                WHERE A.FAMILY_CODE = '{0}' and SEGMENT1 ='{1}'
                and ORGANIZATION_ID='{2}'",
                Convert.ToString(FAMILYCODE), ITEMCODE.Trim(), orgid);
                string reljob = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(reljob))
                {
                    subAssemblyModel.lblRelJob = "Released Jobs: " + reljob;
                    subAssemblyModel.lblRelJobVisible = true;
                }
                else
                    subAssemblyModel.lblRelJobVisible = false;
                query = string.Format(@"select count(*) RELJOB from RELESEDJOBORDER A 
                WHERE A.FAMILY_CODE = '{0}' and SEGMENT1 ='{1}'
                and ORGANIZATION_ID='{2}' and START_QUANTITY=(select COUNT(SRNO) AS CC  FROM XXES_PRINT_SERIALS 
                where plant_code='{3}' and family_code='{0}' and jobid=A.WIP_ENTITY_NAME
                GROUP BY JOBID)", FAMILYCODE, ITEMCODE.Trim(), orgid, PLANTCODE);
                string pendingjobs = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(pendingjobs))
                {
                    subAssemblyModel.lblPending = "Printed Jobs: " + pendingjobs;
                    subAssemblyModel.lblPendingVisible = true;
                }
                else
                    subAssemblyModel.lblPendingVisible = false;
            }
            catch (Exception ex)
            {

            }
            return Json(subAssemblyModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Printdata(SubAssemblyModel data)
        {
            try
            {
                int counter = 0;
                string result = string.Empty, Itemname = string.Empty, ItemCode = string.Empty,
                Tranid = string.Empty, SubAssembly_Id = string.Empty, srno = string.Empty;
                int qty = 0, tobePrintQty = 0;
                string plant = Convert.ToString(data.PLANTCODE).Trim();
                string family = Convert.ToString(data.FAMILYCODE).Trim();
                if (string.IsNullOrEmpty(plant))
                {
                    return Json("Select plant");
                }
                else if (string.IsNullOrEmpty(family))
                {
                    return Json("Select family");
                }
                else if (string.IsNullOrEmpty(Convert.ToString(data.ITEMCODE)))
                {
                    return Json("Select ITEM");
                }
                string desc = (!string.IsNullOrEmpty(data.Item) ? data.Item.Trim().Split('#')[0].Trim() : "");
                if (!string.IsNullOrEmpty(desc))
                    desc = desc.Trim().ToUpper();

                if (data.OptFreeSrno==false && data.pnlSerialNo==false) // if directly entering srno with getting print
                {
                    query = string.Format(@"select count(*) from xxes_sft_settings where plant_code='{0}' and 
                    family_code='{1}' and parameterinfo='{2}' and status='SRNO'",
                        plant, family, Convert.ToString(data.ITEMCODE).Split('#')[0].Trim());
                    if (fun.CheckExits(query))
                    {
                        srno =data.SerialNumber;
                        if (string.IsNullOrEmpty(srno))
                        {
                            return Json("Invalid Serial No");
                        }
                        if (SubAssFun.isNoalreadyExists(srno, plant, family,
                            Convert.ToString(data.ITEMCODE).Split('#')[0].Trim()))
                        {
                            return Json("Serial No already exists");
                        }

                        data.QTY = "1";
                        //txtQty.Enabled = false;
                    }
                    else
                        srno = string.Empty;
                }
                if (data.OptFreeSrno==true && data.pnlSerialNo==true)
                {
                    if (string.IsNullOrEmpty(Convert.ToString(data.SerialITEMCODE)))
                    {
                        return Json("Select valid serial no");
                    }
                    data.QTY= "1";
                    //txtQty.Enabled = false;
                }
                if (!int.TryParse(data.QTY, out qty))
                {
                    return Json("Invalid Quantity");
                }

                string lineqty = Convert.ToString(data.ITEMCODE).Split('#')[3].Trim();
                if ((string.IsNullOrEmpty(NOT_VALIDATE_JOB) || NOT_VALIDATE_JOB == "N") && !desc.Contains("MOTOR"))
                {
                    if (string.IsNullOrEmpty(Convert.ToString(data.Job)))
                    {
                        return Json("Select Valid Job");
                    }
                    else
                    {
                        printqty = Convert.ToString(data.Job).Split('-')[1].Trim();   //Convert.ToString(ddlJobs.EditValue).Split(':')[1].Split(')')[0].Trim();
                        if (!int.TryParse(printqty, out tobePrintQty))
                        {
                            return Json("Invalid To Be Print Job Quantity");
                        }
                        if (qty > Convert.ToInt32(lineqty) || qty > tobePrintQty)
                        {
                            return Json("Quantity should not be greater than selected quantity " + lineqty + " and not more than Selected job Quantity " + tobePrintQty);
                           
                        }
                    }
                }
                else
                {
                    if (qty > Convert.ToInt32(lineqty))
                    {
                        return Json("Quantity should not be greater than selected quantity" + lineqty + "");
                    }
                }

                //int index = cmbItem.Properties.GetIndexByKeyValue(cmbItem.EditValue);
                int index = 1;
                if (index != 0)
                {
                    return Json("Itemcode can be select in sequence");
                }

                selectedJob = Convert.ToString(data.Job);// Convert.ToString(ddlJobs.EditValue).Split('(')[0].Trim();


                ItemCode = Convert.ToString(data.ITEMCODE).Split('#')[0].Trim();
                Tranid = Convert.ToString(data.ITEMCODE).Split('#')[1].Trim();
                SubAssembly_Id = Convert.ToString(data.ITEMCODE).Split('#')[2].Trim();

                orgid = fun.GetOrgId(plant);

                query = string.Format(@"select count(*) from XXES_PRINT_SERIALS where plant_code='{0}'
                and family_code='{1}' and SubAssembly_Id='{2}' and dcode='{3}'", plant, family, SubAssembly_Id, ItemCode);
                string alreayprint = fun.get_Col_Value(query);
                if (!string.IsNullOrEmpty(alreayprint))
                {
                    alreayprint = (Convert.ToInt32(alreayprint) + Convert.ToInt32(qty)).ToString();
                }
                else
                    alreayprint = "0";
                if (Convert.ToInt32(alreayprint) > Convert.ToInt32(lineqty))
                {
                    return Json("Quantity should not exceed the total quanity of selected line");
                }
                if (string.IsNullOrEmpty(NOT_VALIDATE_JOB) || NOT_VALIDATE_JOB == "N")
                {
                    if (dtPrint.Rows.Count > 0)
                    {
                        DataRow[] foundRows = dtPrint.Select("JOB='" + selectedJob + "'");
                        if (foundRows.Length > 0)
                        {
                            foreach (DataRow dr in foundRows)
                            {
                                if ((Convert.ToInt32(dr[2]) + Convert.ToInt32(data.QTY)) > Convert.ToInt32(dr[1]))
                                {
                                    return Json("Quantity should not exceed the total quanity of job");

                                }
                            }
                        }
                    }
                }

                string tmpNumber = string.Empty, stage = SubAssFun.GetOfflineCode(Convert.ToString(data.FAMILYCODE).Trim().ToUpper());
                if (string.IsNullOrEmpty(stage))
                {
                    return Json("Stage code not found");
                }
                SubAssembly subAssembly = null;
                //PB.Visible = true;
                //PB.Maximum = qty;
                //PB.Minimum = 0;
                //PB.Value = 0;
                for (int i = 0; i < qty; i++)
                {
                    if (data.OptFreeSrno ==true && data.pnlSerialNo == true)
                    {
                        tmpNumber = Convert.ToString(data.SerialNumber);
                        srno = tmpNumber;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(srno))
                            tmpNumber = srno;
                        else
                            tmpNumber = fun.getSeries(Convert.ToString(data.PLANTCODE).Trim().ToUpper(), Convert.ToString(data.FAMILYCODE).Trim().ToUpper(), stage);
                    }
                    if (!string.IsNullOrEmpty(tmpNumber))
                    {
                        if (subAssembly == null)
                            subAssembly = new SubAssembly();

                        subAssembly.Plant = plant;
                        subAssembly.Family = family;
                        subAssembly.Description = desc;
                        subAssembly.Itemcode = ItemCode;
                        subAssembly.SerialNumber = tmpNumber.Replace("#", "").Trim().ToUpper();

                        if (tmpNumber.Contains("#"))
                            subAssembly.Series = tmpNumber.Split('#')[1].Trim().ToUpper();
                        else
                            subAssembly.Series = string.Empty;
                        if (string.IsNullOrEmpty(srno))
                        {
                            if (string.IsNullOrEmpty(subAssembly.SerialNumber) ||
                                string.IsNullOrEmpty(subAssembly.Series))
                            {
                                return Json("Unable to generate serial no");
                            }
                        }
                        if (data.OptFreeSrno==true && data.pnlSerialNo==true)
                        {
                            if (SubAssFun.isNoalreadyExists(subAssembly.SerialNumber, plant, family))
                            {
                                subAssembly.DuplicateFlag = "D";
                            }
                            else
                            {
                                subAssembly.DuplicateFlag = "";
                            }
                        }
                        //else
                        //{
                        //    if (isNoalreadyExists(subAssembly.SerialNumber, plant, family))
                        //    {
                        //        pbf.DisplayMsg(pnlMsg, lblMsg, "Duplicate serial no found " + subAssembly.SerialNumber, "E");
                        //        return;
                        //    }
                        //}
                        subAssembly.Job = selectedJob;
                        subAssembly.Stage = stage;
                        subAssembly.PrintDesc = (data.PrintDesc ? true : false);
                        subAssembly.IsQuality = false;
                        subAssembly.Orgid = orgid;
                        subAssembly.PrintMode = "LOCAL";
                        subAssembly.Stage = stage;
                        subAssembly.TranId = Tranid;
                        subAssembly.SubAssembly_Id = SubAssembly_Id;
                        subAssembly.Prefix1 = prefix1.Trim();
                        if (data.OptFreeSrno==true && data.pnlSerialNo==true)
                        {
                            if (printAssemblyBarcodes.PrintFamilySerial(subAssembly, 2))
                            {
                                subAssembly.Series = "FREE_SRNO";
                                fun.UpdateFamilySerials(subAssembly);
                                counter++;
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(srno))
                            {
                                if (printAssemblyBarcodes.PrintFamilySerial(subAssembly, 2))
                                {
                                    //pbf.UpdateFamilySerials(tmpNumber.Replace("#", "").Trim().ToUpper(), tmpNumber.Split('#')[1].Trim().ToUpper(), selectedJob,
                                    //    Convert.ToString(cmbItem.EditValue).Trim(), plant.Trim(), family.Trim(), orgid, stage);
                                    fun.UpdateFamilySerials(subAssembly);
                                    counter++;
                                }
                            }
                            else
                            {
                                if (printAssemblyBarcodes.PrintFamilySerial(subAssembly, 2))
                                {
                                    subAssembly.Series = "NA";
                                    fun.UpdateFamilySerials(subAssembly);
                                    counter++;
                                }
                            }
                        }
                    }
                    //PB.Value += 1;
                    //Application.DoEvents();
                }
            }
            catch
            {

            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult PrintQuality(SubAssemblyModel data)
        {
            string result = string.Empty;
            try
            {  
                string Itemname = string.Empty, ItemCode = string.Empty, Tranid = string.Empty, desc = string.Empty;
               string plant = Convert.ToString(data.PLANTCODE).Trim();
                string family = Convert.ToString(data.FAMILYCODE).Trim();
                if (string.IsNullOrEmpty(Convert.ToString(data.QualityItemCode)))
                {
                    result= "Select Valid Serial No";
                    return Json(result,JsonRequestBehavior.AllowGet);
                }
                string tmpNumber = string.Empty;
                SubAssembly subAssembly = null;
                if (Convert.ToString(data.QualityItemCode).Equals("ALL SERIALS"))
                {
                    int i = 0;
                    foreach (DataRow dr in dtQuality.Rows)
                    {
                        if (Convert.ToString(dr["ITEM_CODE"]).Equals("ALL SERIALS"))
                        {
                            continue;
                        }
                        selectedJob = Convert.ToString(dr["ITEM_CODE"]).Split('-')[1].Trim();
                        int pos = selectedJob.IndexOf("DESCRIPTION") - 1;
                        selectedJob = selectedJob.Substring(0, pos).Trim();

                        desc = Convert.ToString(dr["ITEM_CODE"]).Split('=')[1].Trim();
                        ItemCode = Convert.ToString(dr["ITEM_CODE"]).Split(':')[1].Split(')')[0].Trim();
                        tmpNumber = Convert.ToString(dr["SERIAL_NUMBER"]).Trim();
                        orgid = fun.GetOrgId(plant);
                        stage = SubAssFun.GetOfflineCode(Convert.ToString(data.FAMILYCODE).Trim().ToUpper());
                        if (!string.IsNullOrEmpty(tmpNumber))
                        {
                            if (subAssembly == null)
                                subAssembly = new SubAssembly();
                            subAssembly.Plant = plant;
                            subAssembly.Family = family;
                            subAssembly.Description = desc;
                            subAssembly.Itemcode = ItemCode;
                            subAssembly.SerialNumber = tmpNumber.Replace("#", "").Trim().ToUpper();
                            subAssembly.Series = tmpNumber.Split('#')[1].Trim().ToUpper();
                            subAssembly.Job = selectedJob;
                            subAssembly.Stage = stage;
                            subAssembly.PrintDesc = (data.PrintDesc ? true : false);
                            subAssembly.IsQuality = true;
                            subAssembly.Orgid = orgid;
                            subAssembly.PrintMode = ("NETWORK");
                            subAssembly.Stage = stage;
                            subAssembly.TranId = Tranid;
                            subAssembly.Prefix1 = SubAssFun.getEnginePrefix(plant, family, ItemCode);
                            if (printAssemblyBarcodes.PrintFamilySerial(subAssembly, 1, "1"))
                            {
                                fun.UpdatePrintSerialNo(subAssembly);

                                result = "Printed Quantity: " + (i + 1).ToString();
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }

                        }
                        i++;
                    }
                    BindQualityItemCode(data.PLANTCODE, data.FAMILYCODE, data.QualityDate);
                }
                else
                {
                    if (subAssembly == null)
                        subAssembly = new SubAssembly();
                    selectedJob = Convert.ToString(data.QualityItemCode).Split('-')[1].Trim();
                    int pos = selectedJob.IndexOf("DESCRIPTION") - 1;
                    selectedJob = selectedJob.Substring(0, pos).Trim();
                    desc = Convert.ToString(data.QualityItem).Split('=')[1].Trim();
                    ItemCode = Convert.ToString(data.QualityItem).Split(':')[1].Split(')')[0].Trim();
                    tmpNumber = Convert.ToString(data.QualityItemCode).Trim();
                    orgid = fun.GetOrgId(plant);
                    stage = SubAssFun.GetOfflineCode(Convert.ToString(data.FAMILYCODE).Trim().ToUpper());
                    if (!string.IsNullOrEmpty(tmpNumber))
                    {
                        subAssembly.Plant = plant;
                        subAssembly.Family = family;
                        subAssembly.Description = desc;
                        subAssembly.Itemcode = ItemCode;
                        subAssembly.SerialNumber = tmpNumber.Replace("#", "").Trim().ToUpper();
                        //subAssembly.Series = tmpNumber.Split('#')[1].Trim().ToUpper();
                        subAssembly.Job = selectedJob;
                        subAssembly.Stage = stage;
                        subAssembly.PrintDesc = (data.PrintDesc ? true : false);
                        subAssembly.IsQuality = true;
                        subAssembly.Orgid = orgid;
                        subAssembly.PrintMode = ("NETWORK");
                        subAssembly.Stage = stage;
                        subAssembly.TranId = Tranid;
                        if (printAssemblyBarcodes.PrintFamilySerial(subAssembly, 1))
                        {
                            fun.UpdatePrintSerialNo(subAssembly);
                            BindQualityItemCode(data.PLANTCODE, data.FAMILYCODE, data.QualityDate);

                            result = "Printed Successfully !! ";
                            return Json(result, JsonRequestBehavior.AllowGet);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}