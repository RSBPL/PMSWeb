using MVCApp.CommonFunction;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace MVCApp.Models
{

	public class MRNQC
	{
		public string PLANT_CODE { get; set; }
		public string FAMILY_CODE { get; set; }
		public string MRN_NO { get; set; }
		public string ITEMCODE { get; set; }
		public string COMMODITY { get; set; }
		public string BUYER { get; set; }
		public string INSPECTOR { get; set; }
		public string SKIPLOT_RATIO { get; set; }
		public string ISDIM { get; set; }
		public string ISMT { get; set; }
		public string DIMOK_QTY0 { get; set; }
		public string DIMREJ_QTY0 { get; set; }
		public string DIMOK_DEV0 { get; set; }
		public string DIMOK_AFTERSEG0 { get; set; }
		public string DIMOK_AFTEREWORK0 { get; set; }
		public string DIMHOLD_QTY0 { get; set; }
		public string MTOK_QTY0 { get; set; }
		public string MTREJ_QTY0 { get; set; }
		public string MTOK_DEV0 { get; set; }
		public string MTOK_AFTERSEG0 { get; set; }
		public string MTOK_AFTEREWORK0 { get; set; }
		public string MTHOLD_QTY0 { get; set; }
		public string BLANKET_STDPO { get; set; }
		public string NEW_REGULAR { get; set; }
		public string NUMTIMES_REC { get; set; }
		public string SHEET_AVAILNOT { get; set; }
		public string DEVIATION_BALQTY { get; set; }
		public string PARTHANDOVER_STATUS { get; set; }
		public string AUTOSKIPLOT_PPM { get; set; }
		public string MRNAGEING_STATUS { get; set; }
		public string DOCK_QTY { get; set; }
		public string PAINTSHOP_QTY { get; set; }
		public string LINE_QTY { get; set; }
		public string STORELOC_QTY { get; set; }
		public string DIM_STATUS { get; set; }
		public string MT_STATUS { get; set; }
		public string FITMENT_STATUS { get; set; }
		public string SQA_STATUS { get; set; }
		public string VENDOR_CODE { get; set; }
		public string CREATEDBY { get; set; }
		public string CREATEDDATE { get; set; }
		public string UPDATEDBY { get; set; }
		public string UPDATEDDATE { get; set; }

	}

	public class MRNQC_HISTORY
	{
		public string PLANT_CODE { get; set; }
		public string FAMILY_CODE { get; set; }
		public string ITEMCODE { get; set; }
		public string VENDOR_CODE { get; set; }
		public string DIMOK_QTY1 { get; set; }
		public string DIMREJ_QTY1 { get; set; }
		public string DIMOK_DEV1 { get; set; }
		public string DIMOK_AFTERSEG1 { get; set; }
		public string DIMOK_AFTEREWORK1 { get; set; }
		public string DIMHOLD_QTY1 { get; set; }
		public string DIM_DATE1 { get; set; }
		public string DIMOK_QTY2 { get; set; }
		public string DIMREJ_QTY2 { get; set; }
		public string DIMOK_DEV2 { get; set; }
		public string DIMOK_AFTERSEG2 { get; set; }
		public string DIMOK_AFTEREWORK2 { get; set; }
		public string DIMHOLD_QTY2 { get; set; }
		public string DIM_DATE2 { get; set; }
		public string DIMOK_QTY3 { get; set; }
		public string DIMREJ_QTY3 { get; set; }
		public string DIMOK_DEV3 { get; set; }
		public string DIMOK_AFTERSEG3 { get; set; }
		public string DIMOK_AFTEREWORK3 { get; set; }
		public string DIMHOLD_QTY3 { get; set; }
		public string DIM_DATE3 { get; set; }
		public string MTOK_QTY1 { get; set; }
		public string MTREJ_QTY1 { get; set; }
		public string MTOK_DEV1 { get; set; }
		public string MTOK_AFTERSEG1 { get; set; }
		public string MTOK_AFTEREWORK1 { get; set; }
		public string MTHOLD_QTY1 { get; set; }
		public string MT_DATE1 { get; set; }
		public string MTOK_QTY2 { get; set; }
		public string MTREJ_QTY2 { get; set; }
		public string MTOK_DEV2 { get; set; }
		public string MTOK_AFTERSEG2 { get; set; }
		public string MTOK_AFTEREWORK2 { get; set; }
		public string MTHOLD_QTY2 { get; set; }
		public string MT_DATE2 { get; set; }
		public string MTOK_QTY3 { get; set; }
		public string MTREJ_QTY3 { get; set; }
		public string MTOK_DEV3 { get; set; }
		public string MTOK_AFTERSEG3 { get; set; }
		public string MTOK_AFTEREWORK3 { get; set; }
		public string MTHOLD_QTY3 { get; set; }
		public string MT_DATE3 { get; set; }
		public string PPM_1MONTH { get; set; }
		public string PPM_1DATE { get; set; }
		public string PPM_2MONTH { get; set; }
		public string PPM_2DATE { get; set; }
		public string PPM_3MONTH { get; set; }
		public string PPM_3DATE { get; set; }
		public string CREATEDBY { get; set; }
		public string CREATEDDATE { get; set; }
		public string UPDATEDBY { get; set; }
		public string UPDATEDDATE { get; set; }
	}

	public class MRNQCVIEWMODEL
    {
		public string TRANSACTION_DATE { get; set; }
		public string PLANT_CODE { get; set; }
		public string FAMILY_CODE { get; set; }
		public string FROMDATE { get; set; }
		public string TODATE { get; set; }
		public string MRN_NO { get; set; }
		public string VENDOR_CODE { get; set; }
		public string VENDOR_NAME { get; set; }
		public string ITEMCODE { get; set; }
		public string DESCRIPTION { get; set; }
		public string QUANTITY { get; set; }
		public string COMMODITY { get; set; }
		public string BUYER { get; set; }
		public string INSPECTOR { get; set; }
		public string SKIPLOT_RATIO { get; set; }
		public string ISDIM { get; set; }
		public string ISMT { get; set; }
		public string DIMOK_QTY0 { get; set; }
		public string DIMREJ_QTY0 { get; set; }
		public string DIMOK_DEV0 { get; set; }
		public string DIMOK_AFTERSEG0 { get; set; }
		public string DIMOK_AFTEREWORK0 { get; set; }
		public string DIMHOLD_QTY0 { get; set; }
		public string MTOK_QTY0 { get; set; }
		public string MTREJ_QTY0 { get; set; }
		public string MTOK_DEV0 { get; set; }
		public string MTOK_AFTERSEG0 { get; set; }
		public string MTOK_AFTEREWORK0 { get; set; }
		public string MTHOLD_QTY0 { get; set; }
		public string DIMOK_QTY1 { get; set; }
		public string DIMREJ_QTY1 { get; set; }
		public string DIMOK_DEV1 { get; set; }
		public string DIMOK_AFTERSEG1 { get; set; }
		public string DIMOK_AFTEREWORK1 { get; set; }
		public string DIMHOLD_QTY1 { get; set; }
		public string DIM_DATE1 { get; set; }
		public string DIM_INSPECTIONDATE1 { get; set; }
		public string DIMOK_QTY2 { get; set; }
		public string DIMREJ_QTY2 { get; set; }
		public string DIMOK_DEV2 { get; set; }
		public string DIMOK_AFTERSEG2 { get; set; }
		public string DIMOK_AFTEREWORK2 { get; set; }
		public string DIMHOLD_QTY2 { get; set; }
		public string DIM_DATE2 { get; set; }
		public string DIM_INSPECTIONDATE2 { get; set; }
		public string DIMOK_QTY3 { get; set; }
		public string DIMREJ_QTY3 { get; set; }
		public string DIMOK_DEV3 { get; set; }
		public string DIMOK_AFTERSEG3 { get; set; }
		public string DIMOK_AFTEREWORK3 { get; set; }
		public string DIMHOLD_QTY3 { get; set; }
		public string DIM_DATE3 { get; set; }
		public string DIM_INSPECTIONDATE3 { get; set; }
		public string MTOK_QTY1 { get; set; }
		public string MTREJ_QTY1 { get; set; }
		public string MTOK_DEV1 { get; set; }
		public string MTOK_AFTERSEG1 { get; set; }
		public string MTOK_AFTEREWORK1 { get; set; }
		public string MTHOLD_QTY1 { get; set; }
		public string MT_DATE1 { get; set; }
		public string MT_INSPECTIONDATE1 { get; set; }
		public string MTOK_QTY2 { get; set; }
		public string MTREJ_QTY2 { get; set; }
		public string MTOK_DEV2 { get; set; }
		public string MTOK_AFTERSEG2 { get; set; }
		public string MTOK_AFTEREWORK2 { get; set; }
		public string MTHOLD_QTY2 { get; set; }
		public string MT_DATE2 { get; set; }
		public string MT_INSPECTIONDATE2 { get; set; }
		public string MTOK_QTY3 { get; set; }
		public string MTREJ_QTY3 { get; set; }
		public string MTOK_DEV3 { get; set; }
		public string MTOK_AFTERSEG3 { get; set; }
		public string MTOK_AFTEREWORK3 { get; set; }
		public string MTHOLD_QTY3 { get; set; }
		public string MT_DATE3 { get; set; }
		public string MT_INSPECTIONDATE3 { get; set; }
		public string BLANKET_STDPO { get; set; }
		public string NEW_REGULAR { get; set; }
		public string NUMTIMES_REC { get; set; }
		public string SHEET_AVAILNOT { get; set; }
		public string DEVIATION_BALQTY { get; set; }
		public string PARTHANDOVER_STATUS { get; set; }
		public string PPM_1MONTH { get; set; }
		public string PPM_1DATE { get; set; }
		public string PPM_2MONTH { get; set; }
		public string PPM_2DATE { get; set; }
		public string PPM_3MONTH { get; set; }
		public string PPM_3DATE { get; set; }
		public string AVG3MONTH_PPM { get; set; }
		public string MRNAGEING_STATUS { get; set; }
		public string DOCK_QTY { get; set; }
		public string PAINTSHOP_QTY { get; set; }
		public string LINE_QTY { get; set; }
		public string STORELOC_QTY { get; set; }
		public string DIM_STATUS { get; set; }
		public string MT_STATUS { get; set; }
		public string FITMENT_STATUS { get; set; }
		public string SQA_STATUS { get; set; }
        public string ITEM_REVISION { get; set; }
		public string PUNAME { get; set; }
		public string STORAGE_LOC { get; set; }
        public string BOM_REVISION { get; set; }
        public string PO_REV { get; set; }

		public int draw { get; set; }
		public int start { get; set; }
		public int length { get; set; }
        public int TOTALCOUNT { get; set; }
        
	}
	
	public class AddMRN
    {
		Function fun = new Function();
		public string GenerateQuery(MRNQCVIEWMODEL obj, string orgid)
		{
			string query = string.Format(@"select a.TOTALCOUNT,a.TRANSACTION_DATE,a.VENDOR_NAME,a.VENDOR_CODE,a.MRN_NO,a.DESCRIPTION,a.ITEMCODE, a.ITEM_REVISION,a.QUANTITY, PUNAME,COMMODITY,BUYER,INSPECTOR,slp.FREQUENCY_NUM || ':' ||slp.FREQUENCY_DENOM AS SKIPLOT_RATIO, slp.PLAN_TYPE AS ISDIM,slp.PLAN_TYPE AS ISMT,DIMOK_QTY1,DIMREJ_QTY1,DIMOK_DEV1,DIMOK_AFTERSEG1,DIMOK_AFTEREWORK1, DIMHOLD_QTY1,DIM_DATE1, DIM_INSPECTIONDATE1,DIMOK_QTY2,DIMREJ_QTY2,DIMOK_DEV2,DIMOK_AFTERSEG2,DIMOK_AFTEREWORK2, DIMHOLD_QTY2,DIM_DATE2,DIM_INSPECTIONDATE2,DIMOK_QTY3,DIMREJ_QTY3,DIMOK_DEV3,DIMOK_AFTERSEG3,DIMOK_AFTEREWORK3, DIMHOLD_QTY3,DIM_DATE3,DIM_INSPECTIONDATE3,MTOK_QTY1,MTREJ_QTY1,MTOK_DEV1,MTOK_AFTERSEG1,MTOK_AFTEREWORK1, MTHOLD_QTY1,MT_DATE1,MT_INSPECTIONDATE1,MTOK_QTY2,MTREJ_QTY2,MTOK_DEV2,MTOK_AFTERSEG2,MTOK_AFTEREWORK2, MTHOLD_QTY2,MT_DATE2,MT_INSPECTIONDATE2,MTOK_QTY3,MTREJ_QTY3,MTOK_DEV3,MTOK_AFTERSEG3,MTOK_AFTEREWORK3,MTHOLD_QTY3,MT_DATE3,MT_INSPECTIONDATE3,BLANKET_STDPO,NEW_REGULAR,NUMTIMES_REC, STORAGE_LOC, CASE WHEN SHEET_AVAILNOT < 1 THEN 'Not Available' ELSE 'Available' END AS SHEET_AVAILNOT, DEVIATION_BALQTY,PARTHANDOVER_STATUS, CASE WHEN PPM_3MONTH = -99 THEN 'N/A' ELSE TO_CHAR(PPM_3MONTH) END AS PPM_3MONTH, PPM_3DATE, CASE WHEN PPM_2MONTH = -99 THEN 'N/A' ELSE TO_CHAR(PPM_2MONTH) END AS PPM_2MONTH, PPM_2DATE, CASE WHEN PPM_1MONTH = -99 THEN 'N/A' ELSE TO_CHAR(PPM_1MONTH) END AS PPM_1MONTH, PPM_1DATE,( CASE WHEN nvl(PPM_1MONTH,-99)+nvl(PPM_2MONTH,-99)+nvl(PPM_3MONTH,-99) <0 THEN slp.FREQUENCY_NUM || ':' ||slp.FREQUENCY_DENOM WHEN nvl(PPM_1MONTH,-99)+nvl(PPM_2MONTH,-99)+nvl(PPM_3MONTH,-99) = 0 THEN '1:50' WHEN nvl(PPM_1MONTH,-99)+nvl(PPM_2MONTH,-99)+nvl(PPM_3MONTH,-99) >= 1 AND nvl(PPM_1MONTH,-99)+nvl(PPM_2MONTH,-99)+nvl(PPM_3MONTH,-99) <= 1999 THEN '1:10' WHEN nvl(PPM_1MONTH,-99)+nvl(PPM_2MONTH,-99)+nvl(PPM_3MONTH,-99) >= 2000 AND nvl(PPM_1MONTH,-99)+nvl(PPM_2MONTH,-99)+nvl(PPM_3MONTH,-99) <= 9999 THEN '1:5' ELSE '1:1' END) AVG3MONTH_PPM ,MRNAGEING_STATUS,DOCK_QTY,PAINTSHOP_QTY, LINE_QTY,STORELOC_QTY, PO_REV, BOM_REVISION,PLANT_CODE,DIM_STATUS,MT_STATUS,FITMENT_STATUS, SQA_STATUS from( SELECT ROW_NUMBER() OVER (ORDER BY ird.TRANSACTION_DATE) as ROWNMBER, '{6}' ORGID, COUNT(*) over() as TOTALCOUNT, to_char(IRD.TRANSACTION_DATE, 'DD-Mon-YYYY HH24:MI:SS') as TRANSACTION_DATE, IRD.VENDOR_NAME,IRD.VENDOR_CODE, MRI.MRN_NO,SUBSTR(mri.ITEM_DESCRIPTION,1,35) AS DESCRIPTION, MRI.ITEMCODE,MRI.ITEM_REVISION,MRI.QUANTITY,MRI.PUNAME, (SELECT CATEGORY2 || '-' || CATEGORY3 FROM APPS.XXES_ONHAND_ITEM_V WHERE ORGANIZATION_ID = '{6}' AND ITEM_CODE = mri.ITEMCODE AND ROWNUM = 1) AS COMMODITY, (SELECT BUYER_NAME FROM APPS.XXES_ONHAND_ITEM_V WHERE ORGANIZATION_ID = '{6}' AND ITEM_CODE = mri.ITEMCODE AND ROWNUM = 1) AS BUYER, MRNQC.INSPECTOR ,MHS.DIMOK_QTY1, MHS.DIMREJ_QTY1, MHS.DIMOK_DEV1, MHS.DIMOK_AFTERSEG1, MHS.DIMOK_AFTEREWORK1, MHS.DIMHOLD_QTY1, MHS.DIM_DATE1, MHS.DIM_INSPECTIONDATE1, MHS.DIMOK_QTY2, MHS.DIMREJ_QTY2, MHS.DIMOK_DEV2, MHS.DIMOK_AFTERSEG2, MHS.DIMOK_AFTEREWORK2, MHS.DIMHOLD_QTY2, MHS.DIM_DATE2, MHS.DIM_INSPECTIONDATE2, MHS.DIMOK_QTY3, MHS.DIMREJ_QTY3, MHS.DIMOK_DEV3, MHS.DIMOK_AFTERSEG3, MHS.DIMOK_AFTEREWORK3, MHS.DIMHOLD_QTY3, MHS.DIM_DATE3, MHS.DIM_INSPECTIONDATE3, MHS.MTOK_QTY1, MHS.MTREJ_QTY1, MHS.MTOK_DEV1, MHS.MTOK_AFTERSEG1, MHS.MTOK_AFTEREWORK1, MHS.MTHOLD_QTY1, MHS.MT_DATE1, MHS.MT_INSPECTIONDATE1, MHS.MTOK_QTY2, MHS.MTREJ_QTY2, MHS.MTOK_DEV2, MHS.MTOK_AFTERSEG2, MHS.MTOK_AFTEREWORK2, MHS.MTHOLD_QTY2, MHS.MT_DATE2, MHS.MT_INSPECTIONDATE2, MHS.MTOK_QTY3, MHS.MTREJ_QTY3, MHS.MTOK_DEV3, MHS.MTOK_AFTERSEG3, MHS.MTOK_AFTEREWORK3, MHS.MTHOLD_QTY3, MHS.MT_DATE3, MHS.MT_INSPECTIONDATE3, '' AS BLANKET_STDPO, '' AS NEW_REGULAR, (SELECT COUNT(*) FROM XXES_MRNINFO MRINF INNER JOIN ITEM_RECEIPT_DETIALS irdS ON MRINF.MRN_NO = irdS.MRN_NO AND MRINF.PLANT_CODE = irdS.PLANT_CODE AND MRINF.FAMILY_CODE = irdS.FAMILY_CODE WHERE to_char(irdS.TRANSACTION_DATE,'dd-Mon-yyyy HH24:MI:SS') < to_char(IRD.TRANSACTION_DATE,'dd-Mon-yyyy HH24:MI:SS') AND irdS.VENDOR_CODE = IRD.VENDOR_CODE AND MRINF.ITEMCODE = MRI.ITEMCODE AND MRINF.PLANT_CODE = MRI.PLANT_CODE AND MRINF.FAMILY_CODE = MRI.FAMILY_CODE and irds.TRANSACTION_DATE is not null) NUMTIMES_REC, qrv.character4 AS STORAGE_LOC, (SELECT COUNT(*) FROM APPS.QA_PLANS WHERE NAME LIKE '%'||mri.ITEMCODE||'%' AND ORGANIZATION_ID = '{6}' ) SHEET_AVAILNOT, FN_DEVIATON(IRD.VENDOR_CODE,MRI.ITEMCODE,MRI.PLANT_CODE,MRI.FAMILY_CODE) AS DEVIATION_BALQTY, MRNQC.PARTHANDOVER_STATUS, FN_PPM(IRD.VENDOR_CODE,MRI.ITEMCODE,TRUNC(IRD.TRANSACTION_DATE),3,MRI.PLANT_CODE,MRI.FAMILY_CODE) AS PPM_3MONTH, TO_CHAR(add_months(trunc(IRD.TRANSACTION_DATE,'mm'),-3),'MON-YYYY') AS PPM_3DATE, FN_PPM(IRD.VENDOR_CODE,MRI.ITEMCODE,TRUNC(IRD.TRANSACTION_DATE),2,MRI.PLANT_CODE,MRI.FAMILY_CODE) AS PPM_2MONTH, TO_CHAR(add_months(trunc(IRD.TRANSACTION_DATE,'mm'),-2),'MON-YYYY') AS PPM_2DATE, FN_PPM(IRD.VENDOR_CODE,MRI.ITEMCODE,TRUNC(IRD.TRANSACTION_DATE),1,MRI.PLANT_CODE,MRI.FAMILY_CODE) AS PPM_1MONTH, TO_CHAR(add_months(trunc(IRD.TRANSACTION_DATE,'mm'),-1),'MON-YYYY') AS PPM_1DATE, MRNQC.MRNAGEING_STATUS, (SELECT CASE WHEN SUM(ONHAND) IS NULL THEN 0 ELSE SUM(ONHAND) END FROM APPS.XXES_ONHAND_ITEM_V WHERE ORGANIZATION_ID = '{6}' AND SUBINVENTORY_CODE LIKE '%RTV%' AND ITEM_CODE = MRI.ITEMCODE) AS DOCK_QTY, (SELECT CASE WHEN SUM(ONHAND) IS NULL THEN 0 ELSE SUM(ONHAND) END FROM APPS.XXES_ONHAND_ITEM_V WHERE ORGANIZATION_ID = '{6}' AND SUBINVENTORY_CODE LIKE '%RTV%' AND ITEM_CODE = MRI.ITEMCODE) AS PAINTSHOP_QTY, (SELECT CASE WHEN SUM(ONHAND) IS NULL THEN 0 ELSE SUM(ONHAND) END FROM APPS.XXES_ONHAND_ITEM_V WHERE ORGANIZATION_ID = '{6}' AND SUBINVENTORY_CODE LIKE '%RTV%' AND ITEM_CODE = MRI.ITEMCODE) AS LINE_QTY, (SELECT CASE WHEN SUM(ONHAND) IS NULL THEN 0 ELSE SUM(ONHAND) END FROM APPS.XXES_ONHAND_ITEM_V WHERE ORGANIZATION_ID = '{6}' AND SUBINVENTORY_CODE LIKE '%RTV%' AND ITEM_CODE = MRI.ITEMCODE) AS STORELOC_QTY, MRI.ITEM_REVISION AS PO_REV, MRI.BOM_REVISION,MRI.PLANT_CODE ,FUN_ISIR('{6}','187125') AS DIM_STATUS , '' MT_STATUS,'' FITMENT_STATUS,'' SQA_STATUS FROM XXES_MRNINFO mri INNER JOIN ITEM_RECEIPT_DETIALS ird ON MRI.MRN_NO = IRD.MRN_NO AND MRI.PLANT_CODE = IRD.PLANT_CODE AND MRI.FAMILY_CODE = IRD.FAMILY_CODE LEFT OUTER JOIN XXES_MRNQC mrnqc ON mrnqc.PLANT_CODE = MRI.PLANT_CODE AND mrnqc.FAMILY_CODE = MRI.FAMILY_CODE AND mrnqc.ITEMCODE = MRI.ITEMCODE AND mrnqc.VENDOR_CODE = IRD.VENDOR_CODE AND MRNQC.MRN_NO = MRI.MRN_NO LEFT OUTER JOIN XXES_MRNQC_HISTORY mhs ON mhs.PLANT_CODE = MRI.PLANT_CODE AND mhs.FAMILY_CODE = MRI.FAMILY_CODE AND mhs.ITEMCODE = MRI.ITEMCODE AND mhs.VENDOR_CODE = IRD.VENDOR_CODE LEFT OUTER JOIN APPS.QA_RESULTS_V qrv ON qrv.ORGANIZATION_ID = '{6}' AND qrv.VENDOR_ID = ird.VENDOR_CODE WHERE MRI.MRN_NO NOT IN(SELECT qc.MRN_NO FROM XXES_MRNQC qc WHERE qc.PLANT_CODE = MRI.PLANT_CODE AND QC.FAMILY_CODE = mri.FAMILY_CODE) AND mri.STORE_VERIFIED IS NOT NULL AND MRI.PLANT_CODE = '{0}' AND MRI.FAMILY_CODE = '{1}' AND TO_CHAR(ird.TRANSACTION_DATE,'dd-Mon-yyyy') >= TO_DATE('{2}','dd-Mon-yyyy') AND TO_CHAR(ird.TRANSACTION_DATE,'dd-Mon-yyyy') <= TO_DATE('{3}','dd-Mon-yyyy') )a LEFT OUTER JOIN APPS.XXES_SKIP_LOT_PROCESS_V slp ON slp.ORG_CODE = a.PLANT_CODE AND slp.VENDOR_CODE = a.VENDOR_CODE AND slp.ITEM = a.ITEMCODE where rownum > {4} and rownum <= {5}", obj.PLANT_CODE, obj.FAMILY_CODE, obj.FROMDATE, obj.TODATE,
				Convert.ToInt32(obj.start), obj.length, orgid);
			return query;
		}
		public List<MRNQCVIEWMODEL> GridMRN(MRNQCVIEWMODEL obj)
		{
			DataTable dt = new DataTable();
			string query = string.Empty;
			List<MRNQCVIEWMODEL> mrnList = new List<MRNQCVIEWMODEL>();
			try
			{
				string orgid = fun.getOrgId(Convert.ToString(obj.PLANT_CODE).Trim().ToUpper(), Convert.ToString(obj.FAMILY_CODE).Trim().ToUpper());
				obj.length = Convert.ToInt32(obj.start) + Convert.ToInt32(obj.length);
                OracleDataAdapter DA = new OracleDataAdapter("USP_GETRIDATA", fun.Connection());
                DA.SelectCommand.CommandType = CommandType.StoredProcedure;
                DA.SelectCommand.Parameters.Add("pPLANT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.PLANT_CODE;
                DA.SelectCommand.Parameters.Add("pFAMILY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.FAMILY_CODE;
                DA.SelectCommand.Parameters.Add("pFROM_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.FROMDATE;
                DA.SelectCommand.Parameters.Add("pTO_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = obj.TODATE;
                DA.SelectCommand.Parameters.Add("pORG_ID", OracleDbType.NVarchar2, ParameterDirection.Input).Value = orgid;

                DA.SelectCommand.Parameters.Add("RES", OracleDbType.RefCursor, ParameterDirection.Output);
                DA.Fill(dt);

                //dt = fun.returnDataTable(GenerateQuery(obj,orgid));

				if (dt.Rows.Count > 0)
				{
					foreach (DataRow dr in dt.Rows)
					{
						MRNQCVIEWMODEL BC = new MRNQCVIEWMODEL
						{
							TOTALCOUNT = Convert.ToInt32(dr["TOTALCOUNT"]),
							TRANSACTION_DATE = Convert.ToString(dr["TRANSACTION_DATE"]),
							VENDOR_NAME = Convert.ToString(dr["VENDOR_NAME"]),
							VENDOR_CODE = Convert.ToString(dr["VENDOR_CODE"]),
							MRN_NO = Convert.ToString(dr["MRN_NO"]),
							DESCRIPTION = Convert.ToString(dr["DESCRIPTION"]),
							ITEMCODE = Convert.ToString(dr["ITEMCODE"]),
							ITEM_REVISION = Convert.ToString(dr["ITEM_REVISION"]),
							QUANTITY = Convert.ToString(dr["QUANTITY"]),
							PUNAME = Convert.ToString(dr["PUNAME"]),
							COMMODITY = Convert.ToString(dr["COMMODITY"]),
							BUYER = Convert.ToString(dr["BUYER"]),
							INSPECTOR = Convert.ToString(dr["INSPECTOR"]),
							SKIPLOT_RATIO = Convert.ToString(dr["SKIPLOT_RATIO"]),
							ISDIM = Convert.ToString(dr["ISDIM"]),
							ISMT = Convert.ToString(dr["ISMT"]),

							DIMOK_QTY1 = Convert.ToString(dr["DIMOK_QTY1"]),
							DIMREJ_QTY1 = Convert.ToString(dr["DIMREJ_QTY1"]),
							DIMOK_DEV1 = Convert.ToString(dr["DIMOK_DEV1"]),
							DIMOK_AFTERSEG1 = Convert.ToString(dr["DIMOK_AFTERSEG1"]),
							DIMOK_AFTEREWORK1 = Convert.ToString(dr["DIMOK_AFTEREWORK1"]),
							DIMHOLD_QTY1 = Convert.ToString(dr["DIMHOLD_QTY1"]),
							DIM_DATE1 = Convert.ToString(dr["DIM_DATE1"]),
							DIM_INSPECTIONDATE1 = Convert.ToString(dr["DIM_INSPECTIONDATE1"]),
							DIMOK_QTY2 = Convert.ToString(dr["DIMOK_QTY2"]),
							DIMREJ_QTY2 = Convert.ToString(dr["DIMREJ_QTY2"]),
							DIMOK_DEV2 = Convert.ToString(dr["DIMOK_DEV2"]),
							DIMOK_AFTERSEG2 = Convert.ToString(dr["DIMOK_AFTERSEG2"]),
							DIMOK_AFTEREWORK2 = Convert.ToString(dr["DIMOK_AFTEREWORK2"]),
							DIMHOLD_QTY2 = Convert.ToString(dr["DIMHOLD_QTY2"]),
							DIM_DATE2 = Convert.ToString(dr["DIM_DATE2"]),
							DIM_INSPECTIONDATE2 = Convert.ToString(dr["DIM_INSPECTIONDATE2"]),
							DIMOK_QTY3 = Convert.ToString(dr["DIMOK_QTY3"]),
							DIMREJ_QTY3 = Convert.ToString(dr["DIMREJ_QTY3"]),
							DIMOK_DEV3 = Convert.ToString(dr["DIMOK_DEV3"]),
							DIMOK_AFTERSEG3 = Convert.ToString(dr["DIMOK_AFTERSEG3"]),

							DIMOK_AFTEREWORK3 = Convert.ToString(dr["DIMOK_AFTEREWORK3"]),
							DIMHOLD_QTY3 = Convert.ToString(dr["DIMHOLD_QTY3"]),
							DIM_DATE3 = Convert.ToString(dr["DIM_DATE3"]),
							DIM_INSPECTIONDATE3 = Convert.ToString(dr["DIM_INSPECTIONDATE3"]),
							MTOK_QTY1 = Convert.ToString(dr["MTOK_QTY1"]),
							MTREJ_QTY1 = Convert.ToString(dr["MTREJ_QTY1"]),
							MTOK_DEV1 = Convert.ToString(dr["MTOK_DEV1"]),
							MTOK_AFTERSEG1 = Convert.ToString(dr["MTOK_AFTERSEG1"]),
							MTOK_AFTEREWORK1 = Convert.ToString(dr["MTOK_AFTEREWORK1"]),
							MTHOLD_QTY1 = Convert.ToString(dr["MTHOLD_QTY1"]),
							MT_DATE1 = Convert.ToString(dr["MT_DATE1"]),
							MT_INSPECTIONDATE1 = Convert.ToString(dr["MT_INSPECTIONDATE1"]),
							MTOK_QTY2 = Convert.ToString(dr["MTOK_QTY2"]),
							MTREJ_QTY2 = Convert.ToString(dr["MTREJ_QTY2"]),
							MTOK_DEV2 = Convert.ToString(dr["MTOK_DEV2"]),
							MTOK_AFTERSEG2 = Convert.ToString(dr["MTOK_AFTERSEG2"]),
							MTOK_AFTEREWORK2 = Convert.ToString(dr["MTOK_AFTEREWORK2"]),
							MTHOLD_QTY2 = Convert.ToString(dr["MTHOLD_QTY2"]),
							MT_DATE2 = Convert.ToString(dr["MT_DATE2"]),
							MT_INSPECTIONDATE2 = Convert.ToString(dr["MT_INSPECTIONDATE2"]),
							MTOK_QTY3 = Convert.ToString(dr["MTOK_QTY3"]),
							MTREJ_QTY3 = Convert.ToString(dr["MTREJ_QTY3"]),

							MTOK_DEV3 = Convert.ToString(dr["MTOK_DEV3"]),
							MTOK_AFTERSEG3 = Convert.ToString(dr["MTOK_AFTERSEG3"]),
							MTOK_AFTEREWORK3 = Convert.ToString(dr["MTOK_AFTEREWORK3"]),
							MTHOLD_QTY3 = Convert.ToString(dr["MTHOLD_QTY3"]),
							MT_DATE3 = Convert.ToString(dr["MT_DATE3"]),
							MT_INSPECTIONDATE3 = Convert.ToString(dr["MT_INSPECTIONDATE3"]),
							BLANKET_STDPO = Convert.ToString(dr["BLANKET_STDPO"]),
							NEW_REGULAR = Convert.ToString(dr["NEW_REGULAR"]),
							NUMTIMES_REC = Convert.ToString(dr["NUMTIMES_REC"]),
							STORAGE_LOC = Convert.ToString(dr["STORAGE_LOC"]),
							SHEET_AVAILNOT = Convert.ToString(dr["SHEET_AVAILNOT"]),
							DEVIATION_BALQTY = Convert.ToString(dr["DEVIATION_BALQTY"]),
							PARTHANDOVER_STATUS = Convert.ToString(dr["PARTHANDOVER_STATUS"]),
							PPM_3MONTH = Convert.ToString(dr["PPM_3MONTH"]),
							PPM_3DATE = Convert.ToString(dr["PPM_3DATE"]),
							PPM_2MONTH = Convert.ToString(dr["PPM_2MONTH"]),
							PPM_2DATE = Convert.ToString(dr["PPM_2DATE"]),
							PPM_1MONTH = Convert.ToString(dr["PPM_1MONTH"]),

							PPM_1DATE = Convert.ToString(dr["PPM_1DATE"]),
							AVG3MONTH_PPM = Convert.ToString(dr["AVG3MONTH_PPM"]),
							MRNAGEING_STATUS = Convert.ToString(dr["MRNAGEING_STATUS"]),
							DOCK_QTY = Convert.ToString(dr["DOCK_QTY"]),
							PAINTSHOP_QTY = Convert.ToString(dr["PAINTSHOP_QTY"]),
							LINE_QTY = Convert.ToString(dr["LINE_QTY"]),
							STORELOC_QTY = Convert.ToString(dr["STORELOC_QTY"]),
							PO_REV = Convert.ToString(dr["PO_REV"]),
							BOM_REVISION = Convert.ToString(dr["BOM_REVISION"]),
							PLANT_CODE = Convert.ToString(dr["PLANT_CODE"]),
							DIM_STATUS = Convert.ToString(dr["DIM_STATUS"]),
							MT_STATUS = Convert.ToString(dr["MT_STATUS"]),
							FITMENT_STATUS = Convert.ToString(dr["FITMENT_STATUS"]),
							SQA_STATUS = Convert.ToString(dr["SQA_STATUS"]),
							
						};
						mrnList.Add(BC);
					}
				}


			}
			catch (Exception ex)
			{
				fun.LogWrite(ex);
				throw;
			}

			return mrnList;
		}

		public Tuple<bool,string> InsertMRNSheet(MRNQCVIEWMODEL KM)
		{
			bool result = false; string response = string.Empty;
			try
			{

				using (OracleCommand sc = new OracleCommand("UDSP_MRNDISPLAY", fun.Connection()))
				{
					fun.ConOpen();
					sc.CommandType = CommandType.StoredProcedure;
					sc.Parameters.Add("p_PLANT_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.PLANT_CODE) == true ? null : KM.PLANT_CODE.ToUpper().Trim();
					sc.Parameters.Add("p_FAMILY_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.FAMILY_CODE) == true ? null : KM.FAMILY_CODE.ToUpper().Trim();
					sc.Parameters.Add("p_TRANSACTION_DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.TRANSACTION_DATE) == true ? null : KM.TRANSACTION_DATE.ToUpper().Trim();
					sc.Parameters.Add("p_MRN_NO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.MRN_NO) == true ? null : KM.MRN_NO.ToUpper().Trim();
					sc.Parameters.Add("p_VENDOR_CODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.VENDOR_CODE) == true ? null : KM.VENDOR_CODE.ToUpper().Trim();
					sc.Parameters.Add("p_ITEMCODE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.ITEMCODE) == true ? null : KM.ITEMCODE.ToUpper().Trim();
					sc.Parameters.Add("p_COMMODITY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.COMMODITY) == true ? null : KM.COMMODITY.ToUpper().Trim();
					sc.Parameters.Add("p_BUYER", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.BUYER) == true ? null : KM.BUYER.ToUpper().Trim();
					sc.Parameters.Add("p_INSPECTOR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.INSPECTOR) == true ? null : KM.INSPECTOR.ToUpper().Trim();

					sc.Parameters.Add("p_SKIPLOT_RATIO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.SKIPLOT_RATIO) == true ? null : KM.SKIPLOT_RATIO.ToUpper().Trim();
					sc.Parameters.Add("p_ISDIM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.ISDIM) == true ? null : KM.ISDIM.ToUpper().Trim();
					sc.Parameters.Add("p_ISMT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.ISMT) == true ? null : KM.ISMT.ToUpper().Trim();
					sc.Parameters.Add("p_DIMOK_QTY0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.DIMOK_QTY0) == true ? null : KM.DIMOK_QTY0.ToUpper().Trim();
					sc.Parameters.Add("p_DIMREJ_QTY0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.DIMREJ_QTY0) == true ? null : KM.DIMREJ_QTY0.ToUpper().Trim();
					sc.Parameters.Add("p_DIMOK_DEV0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.DIMOK_DEV0) == true ? null : KM.DIMOK_DEV0.ToUpper().Trim();
					sc.Parameters.Add("p_DIMOK_AFTERSEG0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.DIMOK_AFTERSEG0) == true ? null : KM.DIMOK_AFTERSEG0.ToUpper().Trim();
					sc.Parameters.Add("p_DIMOK_AFTEREWORK0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.DIMOK_AFTEREWORK0) == true ? null : KM.DIMOK_AFTEREWORK0.ToUpper().Trim();


					sc.Parameters.Add("p_DIMHOLD_QTY0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.DIMHOLD_QTY0) == true ? null : KM.DIMHOLD_QTY0.ToUpper().Trim();
					sc.Parameters.Add("p_MTOK_QTY0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.MTOK_QTY0) == true ? null : KM.MTOK_QTY0.ToUpper().Trim();
					sc.Parameters.Add("p_MTREJ_QTY0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.MTREJ_QTY0) == true ? null : KM.MTREJ_QTY0.ToUpper().Trim();
					sc.Parameters.Add("p_MTOK_DEV0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.MTOK_DEV0) == true ? null : KM.MTOK_DEV0.ToUpper().Trim();
					sc.Parameters.Add("p_MTOK_AFTEREWORK0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.MTOK_AFTEREWORK0) == true ? null : KM.MTOK_AFTEREWORK0.ToUpper().Trim();
					sc.Parameters.Add("p_MTOK_AFTERSEG0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.MTOK_AFTERSEG0) == true ? null : KM.MTOK_AFTERSEG0.ToUpper().Trim();
					sc.Parameters.Add("p_MTHOLD_QTY0", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.MTHOLD_QTY0) == true ? null : KM.MTHOLD_QTY0.ToUpper().Trim();
					sc.Parameters.Add("p_BLANKET_STDPO", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.BLANKET_STDPO) == true ? null : KM.BLANKET_STDPO.ToUpper().Trim();

					sc.Parameters.Add("p_NEW_REGULAR", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.NEW_REGULAR) == true ? null : KM.NEW_REGULAR.ToUpper().Trim();
					sc.Parameters.Add("p_NUMTIMES_REC", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.NUMTIMES_REC) == true ? null : KM.NUMTIMES_REC.ToUpper().Trim();
					sc.Parameters.Add("p_SHEET_AVAILNOT", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.SHEET_AVAILNOT) == true ? null : KM.SHEET_AVAILNOT.ToUpper().Trim();
					sc.Parameters.Add("p_DEVIATION_BALQTY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.DEVIATION_BALQTY) == true ? null : KM.DEVIATION_BALQTY.ToUpper().Trim();
					sc.Parameters.Add("p_PARTHANDOVER_STATUS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.PARTHANDOVER_STATUS) == true ? null : KM.PARTHANDOVER_STATUS.ToUpper().Trim();

					
					sc.Parameters.Add("p_PPM_1MONTH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.PPM_1MONTH) == true ? null : KM.PPM_1MONTH.ToUpper().Trim();
					sc.Parameters.Add("p_PPM_1DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.PPM_1DATE) == true ? null : KM.PPM_1DATE.ToUpper().Trim();
					sc.Parameters.Add("p_PPM_2MONTH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.PPM_2MONTH) == true ? null : KM.PPM_2MONTH.ToUpper().Trim();
					sc.Parameters.Add("p_PPM_2DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.PPM_2DATE) == true ? null : KM.PPM_2DATE.ToUpper().Trim();
					sc.Parameters.Add("p_PPM_3MONTH", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.PPM_3MONTH) == true ? null : KM.PPM_3MONTH.ToUpper().Trim();
					sc.Parameters.Add("p_PPM_3DATE", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.PPM_3DATE) == true ? null : KM.PPM_3DATE.ToUpper().Trim();
					sc.Parameters.Add("p_AVG3MONTH_PPM", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.AVG3MONTH_PPM) == true ? null : KM.AVG3MONTH_PPM.ToUpper().Trim();
					
					sc.Parameters.Add("p_MRNAGEING_STATUS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.MRNAGEING_STATUS) == true ? null : KM.MRNAGEING_STATUS.ToUpper().Trim();
					sc.Parameters.Add("p_DOCK_QTY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.DOCK_QTY) == true ? null : KM.DOCK_QTY.ToUpper().Trim();


					sc.Parameters.Add("p_PAINTSHOP_QTY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.PAINTSHOP_QTY) == true ? null : KM.PAINTSHOP_QTY.ToUpper().Trim();
					sc.Parameters.Add("p_LINE_QTY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.LINE_QTY) == true ? null : KM.LINE_QTY.ToUpper().Trim();
					sc.Parameters.Add("p_STORELOC_QTY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.STORELOC_QTY) == true ? null : KM.STORELOC_QTY.ToUpper().Trim();
					sc.Parameters.Add("p_DIM_STATUS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.DIM_STATUS) == true ? null : KM.DIM_STATUS.ToUpper().Trim();
					sc.Parameters.Add("p_MT_STATUS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.MT_STATUS) == true ? null : KM.MT_STATUS.ToUpper().Trim();
					sc.Parameters.Add("p_FITMENT_STATUS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.FITMENT_STATUS) == true ? null : KM.FITMENT_STATUS.ToUpper().Trim();
					sc.Parameters.Add("p_SQA_STATUS", OracleDbType.NVarchar2, ParameterDirection.Input).Value = string.IsNullOrEmpty(KM.SQA_STATUS) == true ? null : KM.SQA_STATUS.ToUpper().Trim();


					sc.Parameters.Add("p_CREATEDBY", OracleDbType.NVarchar2, ParameterDirection.Input).Value = HttpContext.Current.Session["Login_User"].ToString().ToUpper().Trim();
					sc.Parameters.Add("RETURN_MESSAGE", OracleDbType.NVarchar2, 500);
					sc.Parameters["RETURN_MESSAGE"].Direction = ParameterDirection.Output;
					sc.ExecuteNonQuery();
					response = Convert.ToString(sc.Parameters["RETURN_MESSAGE"].Value);
					result = true;
				}

			}
			catch (Exception ex)
			{
				fun.LogWrite(ex);
				response = ex.Message;
				result = false;
			}
			finally
			{
				fun.ConClose();
			}
			

			return new Tuple<bool, string>(result, response);
		}

		public bool IsNumber(string input,out double number)
        {
			var isNumeric = double.TryParse(input, out number);
			return isNumeric;
		}
	}
}