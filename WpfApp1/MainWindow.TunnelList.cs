using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using iNKORE.UI.WPF.Modern.Controls;
using userdata;
using OPL_WpfApp.Utils;
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
            sjson.getjson();
            listBox.Items.Clear();
            iplink.Clear();
            int index = 0;
            if (sjson.config.Apps != null)
            {
                foreach (userdata.App app in sjson.config.Apps)
                {
                    if(app.Enabled == 1 ? true : false)
                        if(app.Protocol=="tcp") tcpnum++;

                    Border border = new Border
                    {
                        BorderThickness = new Thickness(1),
                        CornerRadius = new CornerRadius(5)
                    };
                    SolidColorBrush brush = new SolidColorBrush();
                    object resource = FindResource("list-Background");
                    if (resource is SolidColorBrush bb)
                    {
                        Color color = bb.Color;
                        brush.Color = color;
                    }

                    brush.Opacity = 0.2; 
                    border.Background = brush;

                    Grid grid = new Grid
                    {
                        Height = 63,
                        Width = 700
                    };

                    grid.Children.Add(new Label
                    {
                        Content = app.AppName + "隧道",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(10, 3, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "目标 UID：" + app.PeerNode,
                        Margin = new Thickness(10, 35, 0, 0)
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "协议：" + app.Protocol,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(352, 3, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });
                    
                    grid.Children.Add(new Label
                    {
                        Content = "连接：",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(370, 31, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });
                    string iplink_str = "127.0.0.1:" + app.SrcPort;
                    iplink[index]= iplink_str;
                    grid.Children.Add(new TextBox
                    {
                        Text = iplink_str,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(410, 24, 0, 0),
                        Width=125,
                        VerticalAlignment = VerticalAlignment.Top,
                        Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7FFFFFFF")),
                        IsReadOnly = true,
                        ToolTip = new ToolTip
                        {
                            Content = "这是 ip 和端口，根据具体游戏填写"
                        }
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "远程端口：" + app.DstPort,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(220, 3, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "本地端口：" + app.SrcPort,
                        Margin = new Thickness(220, 35, 351, 3)
                    });

                    grid.Children.Add(new Label
                    {
                        Content = "状态：",
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(430, 2, 0, 0)
                    });

                    CheckBox checkBox = new CheckBox
                    {
                        Content = "启用",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(540, -3, 0, 34),
                        VerticalAlignment = VerticalAlignment.Center,
                        IsChecked = app.Enabled == 1 ? true : false,
                        Tag = index

                    };
                    checkBox.Checked += CheckBox_Checked;
                    checkBox.Unchecked += UnCheckBox_Checked;
                    grid.Children.Add(checkBox);
                    var clo = Brushes.Gray;
                    tunellipse.Fill = clo;
                    if (on&&app.Enabled==1)
                    {
                        if (state[app.Protocol + ":" + app.SrcPort] == 1) clo = Brushes.Orange; 
                        if(state[app.Protocol + ":" + app.SrcPort] == 2) clo = Brushes.Green;
                        if (tunnel.getruning()) tunellipse.Fill = clo;
                    }
                    Ellipse ellipse = new Ellipse
                    {
                        Stroke = Brushes.Black,
                        Fill = clo,
                        Width = 14,
                        Height = 14,
                        Margin = new Thickness(465, 4, 214, 45),
                        ToolTip = new ToolTip 
                        { 
                            Content="灰色：未启动/未启用 橙色：连接中..  绿色：连接成功"
                        }
                    };
                    grid.Children.Add(ellipse);
                    if(!on && !state.ContainsKey(app.Protocol + ":" + app.SrcPort))
                        state[app.Protocol + ":" + app.SrcPort] = app.Enabled;
                    if (!on && state.ContainsKey(app.Protocol + ":" + app.SrcPort))
                        if(state[app.Protocol + ":" + app.SrcPort]==0)
                            state[app.Protocol + ":" + app.SrcPort] = app.Enabled;

                    string delData = "M5.629,7.5 L6.72612901,18.4738834 C6.83893748,19.6019681 7.77211147,20.4662096 8.89848718,20.4990325 L8.96496269,20.5 L15.0342282,20.5 C16.1681898,20.5 17.1211231,19.6570911 17.2655686,18.5392856 L17.2731282,18.4732196 L18.1924161,9.2527383 L18.369,7.5 L19.877,7.5 L19.6849078,9.40262938 L18.7657282,18.6220326 C18.5772847,20.512127 17.0070268,21.9581787 15.1166184,21.9991088 L15.0342282,22 L8.96496269,22 C7.06591715,22 5.47142703,20.5815579 5.24265599,18.7050136 L5.23357322,18.6231389 L4.121,7.5 L5.629,7.5 Z M10.25,11.75 C10.6642136,11.75 11,12.0857864 11,12.5 L11,18.5 C11,18.9142136 10.6642136,19.25 10.25,19.25 C9.83578644,19.25 9.5,18.9142136 9.5,18.5 L9.5,12.5 C9.5,12.0857864 9.83578644,11.75 10.25,11.75 Z M13.75,11.75 C14.1642136,11.75 14.5,12.0857864 14.5,12.5 L14.5,18.5 C14.5,18.9142136 14.1642136,19.25 13.75,19.25 C13.3357864,19.25 13,18.9142136 13,18.5 L13,12.5 C13,12.0857864 13.3357864,11.75 13.75,11.75 Z M12,1.75 C13.7692836,1.75 15.2083571,3.16379796 15.2491124,4.92328595 L15.25,5 L21,5 C21.4142136,5 21.75,5.33578644 21.75,5.75 C21.75,6.14942022 21.43777,6.47591522 21.0440682,6.49872683 L21,6.5 L14.5,6.5 C14.1005798,6.5 13.7740848,6.18777001 13.7512732,5.7940682 L13.75,5.75 L13.75,5 C13.75,4.03350169 12.9664983,3.25 12,3.25 C11.0536371,3.25 10.2827253,4.00119585 10.2510148,4.93983756 L10.25,5 L10.25,5.75 C10.25,6.14942022 9.93777001,6.47591522 9.5440682,6.49872683 L9.5,6.5 L2.75,6.5 C2.33578644,6.5 2,6.16421356 2,5.75 C2,5.35057978 2.31222999,5.02408478 2.7059318,5.00127317 L2.75,5 L8.75,5 C8.75,3.20507456 10.2050746,1.75 12,1.75 Z";
                    Button closeButton = new Button
                    {
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(645, 2, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        Tag = index,
                        Style = (Style)this.FindResource("svg-button"),
                        ToolTip = new ToolTip
                        {
                            Content = "删除隧道，无法恢复"
                        },
                        Content = new System.Windows.Shapes.Path
                        {
                            Fill = (SolidColorBrush)FindResource("svg-icon"),
                            Data = Geometry.Parse(delData)
                        }
                    };
                    closeButton.Click += Del;
                    grid.Children.Add(closeButton);

                    Button editButton = new Button
                    {
                        Content = " 编辑 ",
                        Margin = new Thickness(600,20,0,0),
                        Width = 55,
                        Tag = index
                    };
                    editButton.Click += Edit;
                    grid.Children.Add(editButton);

                    Button copyip = new Button
                    {
                        Margin = new Thickness(540, 20, 0, 0),
                        Width=55,
                        Tag = index,
                        Content="复制"
                    };
                    copyip.Click += CopyipLink;
                    grid.Children.Add(copyip);

                    Button moveUpButton = new Button
                    {
                        Content = "▲",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(668, 22, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = 20,
                        Height = 15,
                        FontSize = 8,
                        Padding = new Thickness(0),
                        Tag = index,
                        ToolTip = new ToolTip { Content = "上移" }
                    };
                    moveUpButton.Click += MoveUp_Click;
                    grid.Children.Add(moveUpButton);

                    Button moveDownButton = new Button
                    {
                        Content = "▼",
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(668, 40, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        Width = 20,
                        Height = 15,
                        FontSize = 8,
                        Padding = new Thickness(0),
                        Tag = index,
                        ToolTip = new ToolTip { Content = "下移" }
                    };
                    moveDownButton.Click += MoveDown_Click;
                    grid.Children.Add(moveDownButton);

                    border.Child = grid;
                    index++;
                    ListBoxItem item = new ListBoxItem { Content = border };
                    listBox.Items.Add(item);
                }
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作！操作无效", "警告");
                Relist();
                return;
            }
            var CheckBox = (CheckBox)sender;
            int index = (int)CheckBox.Tag;
            
            sjson.onapp(index);
            Relist();
        }

        private void UnCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作！操作无效", "警告");
                Relist();
                return;
            }
            var CheckBox = (CheckBox)sender;
            int index = (int)CheckBox.Tag;
            sjson.offapp(index);
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
                var button = (Button)sender;
                int index = (int)button.Tag;
                sjson.del(index);
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
            var button = (Button)sender;
            int index = (int)button.Tag;
            edit ed = new edit(index);
            ed.Owner = this;
            ed.Topmost = true;
            ed.ShowDialog();
            Relist();
        }

        private void MoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作！操作无效", "警告");
                Relist();
                return;
            }
            var button = (Button)sender;
            int index = (int)button.Tag;
            sjson.moveUp(index);
            Relist();
        }

        private void MoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (on)
            {
                MessageBox.Show("程序在运行，禁止操作！操作无效", "警告");
                Relist();
                return;
            }
            var button = (Button)sender;
            int index = (int)button.Tag;
            sjson.moveDown(index);
            Relist();
        }

        private void CopyipLink(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            int index = (int)button.Tag;
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
