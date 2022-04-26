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
        string query = "", prevQty = ""; DataTable dtJob; string ORGID = "", LoginStageCode = "", Login_User="";
        string plantCode = "";string FamilyCode = ""; string NOT_VALIDATE_JOB = string.Empty;
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

        [HttpPost]
        public ActionResult Grid(SubAssemblyModel obj)
        {
            int recordsTotal = 0; string avlqury = string.Empty; string planid = string.Empty;
            planid = SubAssFun.GetPlanId(obj);
            obj.P_Search = Request.Form.GetValues("search[value]").FirstOrDefault();
            planid = SubAssFun.GetPlanId(obj);
            if (string.IsNullOrEmpty(planid))
            {
                return Json(new
                {
                    draw = obj.draw
                }, JsonRequestBehavior.AllowGet);
            }
            string keycode = SubAssFun.GetOfflineCode(Convert.ToString(obj.FAMILYCODE));
            string field = SubAssFun.getSrnoField(Convert.ToString(obj.FAMILYCODE));
            avlqury = @" (select count(*) from XXES_PRINT_SERIALS WHERE QCOK = 'Y' AND OFFLINE_KEYCODE = '" + keycode + "' " +
                " AND PLANT_CODE = '" + Convert.ToString(obj.PLANTCODE) + "' AND DCODE = a.ITEMCODE " +
                            "AND SRNO NOT IN(SELECT " + field + " FROM XXES_JOB_STATUS WHERE PLANT_CODE = '" + Convert.ToString(obj.PLANTCODE) + "' AND FAMILY_CODE = '" + Convert.ToString(obj.FAMILYCODE) + "' AND " + field + " IS NOT NULL) ) AS AVAILABLE";
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
            Convert.ToString(obj.PLANTCODE), Convert.ToString(obj.FAMILYCODE),
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
                string plantID = SubAssFun.GetPlanId(data);
                family = Convert.ToString(FAMILYCODE).Trim();
                if (string.IsNullOrEmpty(plantID) || string.IsNullOrEmpty(family))
                {
                    return Json(Item, JsonRequestBehavior.AllowGet);
                }
                //and dcode = a.itemcode
                if (optAsPerPlanning==true)
                    query = string.Format(@"SELECT  a.SEQ_NO,a.ITEMCODE,a.QTY,
                    a.DESCRIPTION || ' # ' || a.itemcode   ITEM,
                    a.ITEMCODE || '#' || a.TRAN_ID || '#' || a.AUTOID  ITEMVALUE
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
        public JsonResult FillJobsForDropdown(string FAMILYCODE,string ITEMCODE)
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

    
    }
}