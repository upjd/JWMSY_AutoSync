using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoSync
{
    public partial class FrmLotApprove : Form
    {
        public FrmLotApprove()
        {
            InitializeComponent();
        }

        private void tsbtnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void tsbtnApprove_Click(object sender, EventArgs e)
        {
            var strInput = txtInput.Text;
            var cOrderList = strInput.Split(',');
            for (var i = 0; i < cOrderList.Length; i++)
            {
                SyncApprove(cOrderList[i].ToUpper());
            }

            MessageBox.Show(@"完成");

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


        private void VLogError(string vRoutine, string vErrorDesc)
        {
            TextWriter tw = new StreamWriter(Application.StartupPath + @"\Log\EasTrx.log", true);
            tw.WriteLine("*********" + DateTime.Now);
            tw.WriteLine("Routine : " + vRoutine);
            tw.WriteLine("Error : " + vErrorDesc);
            tw.Close();
        }

        private void FrmLotApprove_Load(object sender, EventArgs e)
        {

        }
    }
}
