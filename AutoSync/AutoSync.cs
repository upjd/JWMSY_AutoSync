using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using AutoSync.BaseCon;
using Infragistics.Win.UltraWinGrid;

namespace AutoSync
{
    public partial class AutoSync : Form
    {
        /// <summary>
        /// 数据库连接字符串
        /// </summary>
        private string _wmsCon;


        /// <summary>
        /// uri
        /// </summary>
        private string _BaseConUri;

        /// <summary>
        /// uri
        /// </summary>
        private string _PoStoreUri;


        /// <summary>
        /// uri
        /// </summary>
        private string _ProduceUri;


        /// <summary>
        /// uri
        /// </summary>
        private string _SaleDeliveryUri;


        /// <summary>
        /// uri
        /// </summary>
        private string _TransferDeliveryUri;


        /// <summary>
        /// uri
        /// </summary>
        private string _TransferStoreUri;


        /// <summary>
        /// uri
        /// </summary>
        private string _SyncWoUri;


        private string _OrderService;


        public AutoSync()
        {
            InitializeComponent();
        }

        private void AutoSync_Load(object sender, EventArgs e)
        {
            tstxtServer.Text = Properties.Settings.Default.cServer;
            //记录开始的时间
            dtpStartTime.Value = DateTime.Now;

            timerExec.Interval = (int)nudTimeSpan.Value * 60000;
            SetUri();
            timerExec.Enabled = true;
            ExecUpload();
        }

        private void SetUri()
        {
            var ws = new BaseConService { Url = "http://" + tstxtServer.Text + "/BaseConService.asmx" };
            _wmsCon = ws.GetWmsConstring();
            _BaseConUri = string.Format(@"http://{0}/BaseConService.asmx", tstxtServer.Text);
            _PoStoreUri = string.Format(@"http://{0}/SyncPurReceivalBill.asmx", tstxtServer.Text);
            _ProduceUri = string.Format(@"http://{0}/SyncMaterialReqBill.asmx", tstxtServer.Text);
            _SaleDeliveryUri = string.Format(@"http://{0}/SyncSaleIssueBill.asmx", tstxtServer.Text);
            _TransferDeliveryUri = string.Format(@"http://{0}/SyncMoveIssueBill.asmx", tstxtServer.Text);
            _TransferStoreUri = string.Format(@"http://{0}/SyncMoveInWarehsBill.asmx", tstxtServer.Text);
            _SyncWoUri = string.Format(@"http://{0}/SyncManufactureRecBill.asmx", tstxtServer.Text);
            _OrderService = string.Format(@"http://{0}/EasOrder.asmx", tstxtServer.Text);
        }

        private void tsbtnSave_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.cServer = tstxtServer.Text;
            Properties.Settings.Default.Save();
            MessageBox.Show(@"保存成功");
            SetUri();
        }

        private void AutoSync_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = MessageBox.Show(@"确定退出吗？
如果退出，WMS数据将断开与EAS的同步", @"是否退出？", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes;
        }

        private void tsbExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void timerSpan_Tick(object sender, EventArgs e)
        {
            var tSpan = DateTime.Now - dtpStartTime.Value;
            lblTimeSpan.Text = tSpan.Days.ToString(CultureInfo.InvariantCulture) + @"天"
                + tSpan.Hours + @"小时" + tSpan.Minutes + @"分钟" + tSpan.Seconds + @"秒";
        }

        private void timerExec_Tick(object sender, EventArgs e)
        {
            ExecUpload();

            if (DateTime.Now.Hour == 0 && DateTime.Now.Minute < 10)
            {
                SyncInventory();
            }
        }

        private void SyncInventory()
        {
            var easOrder = new EasOrderService.EasOrder() { Url = _OrderService };

            var dtPro = easOrder.GetPro();
            var sfun = new SyncFunction(_wmsCon);
            pbMain.Maximum = dtPro.Rows.Count - 1;
            pbMain.Value = 0;
            for (var i = 0; i < dtPro.Rows.Count; i++)
            {
                var synCmd = new SqlCommand("SyncInventory") { CommandType = CommandType.StoredProcedure };
                synCmd.Parameters.AddWithValue("@cInvCode", dtPro.Rows[i]["cInvCode"]);
                synCmd.Parameters.AddWithValue("@cInvName", dtPro.Rows[i]["cInvName"]);
                synCmd.Parameters.AddWithValue("@cInvType", "Pro");
                synCmd.Parameters.AddWithValue("@FStatus", dtPro.Rows[i]["FStatus"]);
                synCmd.Parameters.AddWithValue("@bLotMgr", dtPro.Rows[i]["FIsLotNumber"]);
                sfun.ExecSqlCmd(synCmd);
                pbMain.Value = i;
            }


            var dtSemi = easOrder.GetSemi();
            pbMain.Maximum = dtSemi.Rows.Count - 1;
            pbMain.Value = 0;
            for (var i = 0; i < dtSemi.Rows.Count; i++)
            {
                var synCmd = new SqlCommand("SyncInventory") { CommandType = CommandType.StoredProcedure };
                synCmd.Parameters.AddWithValue("@cInvCode", dtPro.Rows[i]["cInvCode"]);
                synCmd.Parameters.AddWithValue("@cInvName", dtPro.Rows[i]["cInvName"]);
                synCmd.Parameters.AddWithValue("@cInvType", "Semi");
                synCmd.Parameters.AddWithValue("@FStatus", dtPro.Rows[i]["FStatus"]);
                synCmd.Parameters.AddWithValue("@bLotMgr", dtPro.Rows[i]["FIsLotNumber"]);
                sfun.ExecSqlCmd(synCmd);
                pbMain.Value = i;
            }

            var dtRm = easOrder.GetRm();
            pbMain.Maximum = dtRm.Rows.Count - 1;
            pbMain.Value = 0;
            for (var i = 0; i < dtRm.Rows.Count; i++)
            {
                var synCmd = new SqlCommand("SyncInventory") { CommandType = CommandType.StoredProcedure };
                synCmd.Parameters.AddWithValue("@cInvCode", dtPro.Rows[i]["cInvCode"]);
                synCmd.Parameters.AddWithValue("@cInvName", dtPro.Rows[i]["cInvName"]);
                synCmd.Parameters.AddWithValue("@cInvType", "Rm");
                synCmd.Parameters.AddWithValue("@FStatus", dtPro.Rows[i]["FStatus"]);
                synCmd.Parameters.AddWithValue("@bLotMgr", dtPro.Rows[i]["FIsLotNumber"]);
                sfun.ExecSqlCmd(synCmd);
                pbMain.Value = i;
            }
        }

        private void ExecUpload()
        {
            lblLastTime.Text = DateTime.Now.ToString(CultureInfo.CurrentCulture);
            var stw = new Stopwatch();
            stw.Start();

            var iSumSucces = 0;
            var iSumFail = 0;


            timerExec.Enabled = false;
            var dt = GetNoUpdateData();
            if (dt == null || dt.Rows.Count < 1)
            {
                stw.Stop();
                //显示执行一次用时的时间
                lblCostTime.Text = stw.Elapsed.Milliseconds.ToString(CultureInfo.InvariantCulture) + @"毫秒";
                timerExec.Enabled = true;
                return;
            }
            var sPoStore = new SyncPoStore.SyncOrder() {Url = _PoStoreUri};
            var sProduce = new SyncProduce.SyncMaterialReqBill() {Url = _ProduceUri};
            var sSaleDelivery = new SyncSaleDelivery.SyncSaleIssueBill() {Url = _SaleDeliveryUri};
            var sTransferDeliver = new SyncTransferDelivery.SyncMoveIssueBill() {Url = _TransferDeliveryUri};
            var sTransferStore = new SyncTransferStore.SyncMoveInWarehsBill() {Url = _TransferStoreUri};
            var sWo = new SyncWo.SyncManufactureRecBill() {Url = _SyncWoUri};
            
            uGridScan.DataSource = dt;
            pbMain.Value = 0;
            pbMain.Maximum = dt.Rows.Count;
            for (var i = 0; i < uGridScan.Rows.Count; i++)
            {
                pbMain.Value = i + 1;
                Application.DoEvents();
                var cType = uGridScan.Rows[i].Cells["cType"].Value.ToString();
                var cOrderNumber = uGridScan.Rows[i].Cells["cOrderNumber"].Value.ToString();
                var cEasNewOrder = uGridScan.Rows[i].Cells["cEasNewOrder"].Value.ToString();
                var cGuid = uGridScan.Rows[i].Cells["cGuid"].Value.ToString();
                switch (cType)
                {
                    /*
                    case "采购收货":
                        try
                        {
                            var cResult=sPoStore.SyncPurReceivalBill(cOrderNumber, cEasNewOrder, cGuid, 1);
                            
                            if (cResult.Equals("OK"))
                            {
                                UpdateState(cGuid);
                                iSumSucces = iSumSucces + 1; 
                            }
                            else
                            {
                                VLogError(@"采购收货", cResult);
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            VLogError(@"采购收货", ex.Message);
                            iSumFail = iSumFail + 1;
                        }
                        break;
                    case "调拨出库":
                        try
                        {
                            var cResult=sTransferDeliver.SyncOrder(cOrderNumber, cEasNewOrder, cGuid, 1);
                            
                            if (cResult.Equals("OK"))
                            {
                                UpdateState(cGuid);
                                iSumSucces = iSumSucces + 1;
                            }
                            else
                            {
                                VLogError(@"调拨出库", cResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            VLogError(@"调拨出库", ex.Message);
                            iSumFail = iSumFail + 1;
                        }
                        break;
                    case "调拨入库":
                        try
                        {
                            var cResult = sTransferStore.SyncOrder(cOrderNumber, cEasNewOrder, cGuid, 1);
                            UpdateState(cGuid);
                            if (cResult.Equals("OK"))
                            {
                                iSumSucces = iSumSucces + 1;
                            }
                            else
                            {
                                VLogError(@"调拨入库", cResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            VLogError(@"调拨入库", ex.Message);
                            iSumFail = iSumFail + 1;
                        }
                        break;

                    case "生产领料":
                        try
                        {
                            var cResult=sProduce.SyncOrder(cOrderNumber, cEasNewOrder, cGuid, 1);
                            
                            if (cResult.Equals("OK"))
                            {
                                UpdateState(cGuid);
                                iSumSucces = iSumSucces + 1;
                            }
                            else
                            {
                                VLogError(@"生产领料", cResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            VLogError(@"生产领料", ex.Message);
                            iSumFail = iSumFail + 1;
                        }
                        break;

                    case "完工入库":
                        try
                        {
                            var cResult=sWo.SyncOrder(cOrderNumber, cEasNewOrder, cGuid, 1);
                            
                            if (cResult.Equals("OK"))
                            {
                                UpdateState(cGuid);
                                iSumSucces = iSumSucces + 1;
                            }
                            else
                            {
                                VLogError(@"完工入库", cResult);
                            }
                        }
                        catch (Exception ex)
                        {
                            VLogError(@"采购收货", ex.Message);
                            iSumFail = iSumFail + 1;
                        }
                        break;
                         */
                    case "销售出库":
                        try
                        {
                            var cResult = sSaleDelivery.SyncOrder(cOrderNumber, cEasNewOrder, cGuid, 1);

                            if (cResult.Equals("OK"))
                            {
                                UpdateState(cGuid);
                                iSumSucces = iSumSucces + 1;
                            }
                            else
                            {
                                VLogError(@"销售出库" + cOrderNumber, cResult);
                            }
                            //自动审核
                            try
                            {
                                SyncApprove(cOrderNumber);
                            }
                            catch (Exception exp)
                            {
                                VLogError(@"销售出库" + cOrderNumber, exp.Message);
                            }
                        }
                        catch (Exception ex)
                        {
                            VLogError(@"销售出库" + cOrderNumber, ex.Message);
                            iSumFail = iSumFail + 1;
                        }
                        break;


                    case "销售退货":
                        try
                        {
                            var cResult = sSaleDelivery.SyncOrder(cOrderNumber, cEasNewOrder, cGuid, 2);

                            if (cResult.Equals("OK"))
                            {
                                UpdateState(cGuid);
                                iSumSucces = iSumSucces + 1;
                            }
                            else
                            {
                                VLogError(@"销售退货" + cOrderNumber, cResult);
                            }
                            
                        }
                        catch (Exception ex)
                        {
                            VLogError(@"销售退货" + cOrderNumber, ex.Message);
                            iSumFail = iSumFail + 1;
                        }
                        break;
                }
            }
            timerExec.Enabled = true;
            stw.Stop();
            //显示执行一次用时的时间
            lblCostTime.Text = stw.Elapsed.Milliseconds.ToString(CultureInfo.InvariantCulture) + @"毫秒";
            var sfun = new SyncFunction(_wmsCon);
            var lcmd = new SqlCommand("insert into BLogAction(cFunction,cDescription) Values(@cFunction,@cDescription)");
            lcmd.Parameters.AddWithValue("@cFunction", "执行导入EAS");
            lcmd.Parameters.AddWithValue("@cDescription", "此次成功导入行数:" + iSumSucces + "失败数量:" + iSumFail + "开始时间:"
                                                          + lblLastTime.Text + "用时:" + lblCostTime.Text);
            sfun.Sqlexcuate(lcmd);
        }

        private void SyncApprove(string cOrderNumber)
        {


            VLogError(@"销售出库" + cOrderNumber, "开始调用easWebservices" + DateTime.Now);
            var cName = Properties.Settings.Default.EasUserName;
            var cPwd = Properties.Settings.Default.EasUserPwd;
            var easDataCenter = Properties.Settings.Default.EasDataCenter;
            var easproxy = new EASLoginProxyService();
            //proxy.Url = Global.oaUrl + "/ormrpc/services/EASLogin?wsdl";
            //WSContext ctx = easproxy.login(name, pwd, "eas", "a", "L2", 2, "BaseDB");
            var ctx = easproxy.login(cName, cPwd, "eas", easDataCenter, "L2", 2, "BaseDB");
            if (ctx.sessionId != null)
            {
                //正确登录
            }
            else
            {
                VLogError(@"销售出库", cOrderNumber + "::用户名或密码错误!!");
            }

            var proxy = new WSWSYofotoFacadeSrvProxyService();
            var msg = proxy.auditSaleIssueBill("S.01", cOrderNumber);
            VLogError(@"销售出库" + cOrderNumber, "调用easWebservices结束" + DateTime.Now);
            VLogError(@"销售出库", cOrderNumber + "::" + msg);
        }

        private DataTable GetNoUpdateData()
        {
            //更新cLotNo
            //var supdate=new SqlCommand("Update a set a.cLotNo=b.cLotNo From SS_Detail a,View_WmsSSDetail b where a.cSerialNumber=b.cSerialNumber and a.cBoxNumber=b.cBoxNumber and a.cOrderNumber=b.cOrderNumber and a.cInvCode=b.cInvCode and isnull(a.cGuid,'')=isnull(b.cGuid,'') and isnull(a.cLotNo,'')='' and isnull(a.cGuid,'')<>'' " );
            //var sfup = new SyncFunction(_wmsCon);
            //sfup.ExecSqlCmd(supdate);
            //var cmd = new SqlCommand("select distinct uid,cType,cOrderNumber,cEasNewOrder,cGuid,cState,bEnable,CONVERT(char(10),daddtime,120) as dAddtime,dUpdate,cMemo from ( "
            //    +" select top 200 cGuid as uid,'销售出库单' as cType,cOrderNumber,cOrderNumber as cEasNewOrder,cGuid,'未导入' as cstate,bEnable,dDate as dAddtime,dUpdate as dUpdate,'' as cmemo from ss_detail where isnull(bEnable,0)=0 and ISNULL(cguid,'')<>'' order by autoid ) tbl");
            var cmd = new SqlCommand("select * from wms_m_eas where (ctype='销售出库' or cType='销售退货')and isnull(bEnable,0)=0 and ISNULL(cguid,'')<>'' ");
            var sfun = new SyncFunction(_wmsCon);
            return sfun.GetSqlTable(cmd);
        }

        private void UpdateState(string cGuid)
        {
            var cmd = new SqlCommand("update wms_m_eas set bEnable=1,cState='已导入',dUpdate=Getdate() where cGuid=@cGuid");
            cmd.Parameters.AddWithValue("@cGuid", cGuid);
            var sfun = new SyncFunction(_wmsCon);
            sfun.ExecSqlCmd(cmd);
        }


        private void VLogError(string vRoutine, string vErrorDesc)
        {
            TextWriter tw = new StreamWriter(Application.StartupPath + @"\Log\EasTrx.log", true);
            tw.WriteLine("*********" + DateTime.Now);
            tw.WriteLine("Routine : " + vRoutine);
            tw.WriteLine("Error : " + vErrorDesc);
            tw.Close();
        }

        private void btnSaveTime_Click(object sender, EventArgs e)
        {
            timerExec.Interval = (int)nudTimeSpan.Value * 60000;
        }

        private void AutoSync_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮 
            if (WindowState == FormWindowState.Minimized)
            {
                
                //隐藏任务栏区图标 
                this.ShowInTaskbar = false;
                //图标显示在托盘区 
                nfiMain.Visible = true;
            }

        }

        private void nfiMain_DoubleClick(object sender, EventArgs e)
        {
            //判断是否已经最小化于托盘 
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示 
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点 
                this.Activate();
                //任务栏区显示图标 
                this.ShowInTaskbar = true;
                //托盘区图标隐藏 
                nfiMain.Visible = false;
            }

        }

        private void tsbtnSyncInventory_Click(object sender, EventArgs e)
        {
            SyncInventory();
        }

        private void tsbtnApprove_Click(object sender, EventArgs e)
        {
            var fla=new FrmLotApprove();
            fla.Show();
        }
    }
}
