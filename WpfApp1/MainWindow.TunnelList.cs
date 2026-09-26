using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using iNKORE.UI.WPF.Modern.Controls;
using userdata;
using OPL_WpfApp.Utils;
using OPL_WpfApp.Controls;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace OPL_WpfApp
{
    /// <summary>
    /// 隧道列表UI管理
    /// </summary>
    public partial class MainWindow_opl : Window
    {
        private Dictionary<string, int> state = new Dictionary<string, int>();
        private Dictionary<string, int> statelist = new Dictionary<string, int>();
        private Dictionary<int, string> iplink = new Dictionary<int, string>();

        public void Relist() // 刷新列表
        {
            tcpnum = 0;
            ListBox listBox = this.FindName("sdlist") as ListBox;
            if (listBox == null)
            {
                Logger.Log("[错误]刷新隧道列表失败：列表控件尚未初始化", "错误");
                return;
            }

            if (sjson == null)
            {
                Logger.Log("[错误]刷新隧道列表失败：配置对象尚未初始化", "错误");
                return;
            }

            sjson.getjson();
            if (sjson.config == null)
            {
                Logger.Log("[错误]刷新隧道列表失败：配置文件为空或无法读取", "错误");
                return;
            }

            listBox.Items.Clear();
            iplink.Clear();
            int index = 0;
            if (sjson.config.Apps != null)
            {
                foreach (userdata.App app in sjson.config.Apps)
                {
                    if (app == null)
                    {
                        Logger.Log("[警告]配置中存在空隧道项，已跳过", "警告");
                        continue;
                    }

                    if(app.Enabled == 1 ? true : false)
                        if(app.Protocol=="tcp") tcpnum++;

                    string iplink_str = "127.0.0.1:" + app.SrcPort;
                    iplink[index]= iplink_str;
                    var clo = Brushes.Gray;
                    string statusText = "未启动";
                    if (tunellipse != null)
                        tunellipse.Fill = clo;
                    if (on&&app.Enabled==1)
                    {
                        string stateKey = app.Protocol + ":" + app.SrcPort;
                        int appState = 0;
                        state.TryGetValue(stateKey, out appState);
                        if (appState == 1) { clo = Brushes.Orange; statusText = "连接中"; }
                        if (appState == 2) { clo = Brushes.Green; statusText = "已连接"; }
                        if (tunnel != null && tunnel.getruning() && tunellipse != null)
                            tunellipse.Fill = clo;
                    }
                    if(!on && !state.ContainsKey(app.Protocol + ":" + app.SrcPort))
                        state[app.Protocol + ":" + app.SrcPort] = app.Enabled;
                    if (!on && state.ContainsKey(app.Protocol + ":" + app.SrcPort))
                        if(state[app.Protocol + ":" + app.SrcPort]==0)
                            state[app.Protocol + ":" + app.SrcPort] = app.Enabled;

                    var item = new TunnelListItem();
                    item.Bind(index, app.AppName, app.PeerNode, app.Protocol, app.DstPort, app.SrcPort,
                        iplink_str, app.Enabled == 1, clo, statusText);
                    item.SetEditingEnabled(!on);
                    item.EnabledChanged += TunnelItem_EnabledChanged;
                    item.CopyRequested += CopyipLink;
                    item.EditRequested += Edit;
                    item.DeleteRequested += Del;
                    item.MoveRequested += MoveTunnel;
                    index++;
                    listBox.Items.Add(item);
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作!", "警告");
                return;
            }
            Add Add = new Add();
            Add.Owner = this;
            Add.Topmost = true;
            Add.ShowDialog();
            Relist();
        }

        private void TunnelItem_EnabledChanged(object sender, EventArgs e)
        {
            var item = (TunnelListItem)sender;
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作！操作无效", "警告");
                Relist();
                return;
            }

            if (item.IsTunnelEnabled) sjson.onapp(item.Index);
            else sjson.offapp(item.Index);
            Relist();
        }

        private void Del(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作！操作无效", "警告");
                Relist();
                return;
            }
            MessageBoxResult result = MessageBox.Show(
                "你确定要删除隧道吗，这是不可逆的!",
                "警告",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                sjson.del(((TunnelListItem)sender).Index);
            }
            else
            {
                return;
            }
            
            Relist();
        }

        private void Edit(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作！操作无效", "警告");
                Relist();
                return;
            }
            int index = ((TunnelListItem)sender).Index;
            edit ed = new edit(index);
            ed.Owner = this;
            ed.Topmost = true;
            ed.ShowDialog();
            Relist();
        }

        private void MoveTunnel(int sourceIndex, int targetIndex)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作！操作无效", "警告");
                Relist();
                return;
            }
            while (sourceIndex < targetIndex) sjson.moveDown(sourceIndex++);
            while (sourceIndex > targetIndex) sjson.moveUp(sourceIndex--);
            Relist();
        }

        private void CopyipLink(object sender, RoutedEventArgs e)
        {
            int index = ((TunnelListItem)sender).Index;
            if (Copy_text(iplink[index]))
            {
                MessageBox.Show("复制成功，可在游戏中使用 ctrl+v 粘贴", "提示");
            }
        }

        private void ResetUID_Button_Click(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作!", "警告");
                return;
            }
            MessageBoxResult result = MessageBox.Show(
                "你确定要重置吗？会导致失去所有已有隧道配置!",
                "警告",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                userData.ResetUID();
                TextBox UIDTextBox = (TextBox)this.FindName("UID");
                UIDTextBox.Text = userData.UID;
                sjson.newjson(userData);
                MessageBox.Show("已重置 UID，新的 UID 为：" + userData.UID, "提示");
                Relist();
            }
            else if (result == MessageBoxResult.Cancel)
            {
                return;
            }
        }

        private void CopyUID_Button_Click(object sender, RoutedEventArgs e)
        {
            if(Copy_text(UID.Text))
                MessageBox.Show("复制成功", "提示");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Relist();
        }

        private void CloseAll(object sender, RoutedEventArgs e)
        {
            if (set.settings.ConfirmBeforeCloseAll)
            {
                MessageBoxResult result = MessageBox.Show(
                    "确定要关闭全部隧道吗？关闭后所有连接将断开。",
                    "确认",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Question);
                if (result != MessageBoxResult.OK) return;
            }
            sjson.Alloff();
            Relist();
        }

        private void Outlist(object sender, RoutedEventArgs e)
        {
            string output = "";
            if (sjson.config.Apps != null)
            {
                foreach (userdata.App app in sjson.config.Apps)
                {
                    if (app.Enabled == 1)
                    {
                        if(output!="")output += ";";
                        output += app.Protocol == "tcp" ? 1 : 2 ;
                        output +=  ":" + app.PeerNode + ":" + app.DstPort + ":" + app.SrcPort;
                    }
                }
                if (output == "")
                {
                    MessageBox.Show("你目前没有启用的隧道，无法导出，请将需要导出的隧道启用", "提示");
                    return;
                }
            }
            else
            {
                MessageBox.Show("你目前没有隧道，无法导出", "提示");
                return;
            }
            if(Copy_text(output))
                MessageBox.Show("已经将启用的隧道导出为连接码，并已复制，可粘贴保存，复制连接码点击添加左边加号可添加", "提示");
        }

        private void Quick_Add(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作！操作无效", "警告");
                Relist();
                return;
            }
            string pastedText = Clipboard.GetText();
            pastedText = pastedText.Replace("\r", "");
            pastedText = pastedText.Replace("\n", "");
            pastedText = pastedText.Replace(" ", "");
            pastedText = pastedText.Replace("：", ":");
            pastedText = pastedText.Replace("；", ";");
            try
            {
                if(pastedText=="") throw new ArgumentException("无效码");
                var connections = ConnectionParser.ParseConnections(pastedText);
                sjson.getjson();
                sjson.Alloff();
                sjson.clearoindex();
                foreach (var conn in connections)
                {
                    string type = conn.Protocol;
                    if (type == "1") type = "tcp";
                    if (type == "2") type = "udp";
                    string uid = conn.UID;
                    int port = conn.Port;
                    int cport = conn.CPort;
                    sjson.Add1link(type,uid,port,cport);
                }
                Relist();
                MessageBox.Show("已将列表状态同步连接码", "提示");
            }
            catch (Exception ex)
            {
                Logger.Log($"无法识别的连接码：{pastedText} - {ex.Message} - {ex.Source} - {ex.StackTrace}");
                MessageBox.Show($"无法识别的连接码：{pastedText} \n请复制连接码后点击\r该功能为一键添加/编辑隧道为连接码隧道，房主可直接编辑发送连接码供连接方使用。 \r\r连接码用法： \r用法 1：\r uid:端口 --> tcp 协议连接码 \r示例：qwertyuioop:25565 \r\r 用法 2：\r<1/2>:uid:端口[:本地端口] --> 1 为 tcp，2 为 udp 本地端口可省略\r示例：1:qwertyuiop:25565:25575 \r多个连接可以用;间隔同时输入\r复制后直接点击该按钮即可完成添加，后直接启动即可  \r如果确认你复制的符合格式，可尝试点击右边按钮自行添加隧道\r\r {ex.Message}", "错误");
            }
        }
    }
}
