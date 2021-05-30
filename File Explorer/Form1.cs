using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace File_Explorer
{
    public partial class Form1 : Form
    {
        private int iFiles = 0;
        private int iDirectories = 0;
        public Form1()
        {
            InitializeComponent();
        }
        private void AddDirectories(TreeNode tnSubNode)
        {
            treeView1.BeginUpdate();
            iDirectories = 0;

            try
            {
                DirectoryInfo diRoot;

                // Nếu là ổ đĩa thì lấy thư mục từ ổ đỉa
                if (tnSubNode.SelectedImageIndex < 11)
                {
                    diRoot = new DirectoryInfo(tnSubNode.FullPath + "\\");
                }
                //  Ngược lại lấy thư mục từ thư mục
                else
                {
                    diRoot = new DirectoryInfo(tnSubNode.FullPath);
                }
                DirectoryInfo[] dirs = diRoot.GetDirectories();

                tnSubNode.Nodes.Clear();

                // Add thư mục con vào tree
                foreach (DirectoryInfo dir in dirs)
                {
                    iDirectories++;
                    TreeNode subNode = new TreeNode(dir.Name);
                    subNode.ImageIndex = 11;
                    subNode.SelectedImageIndex = 12;
                    tnSubNode.Nodes.Add(subNode);
                }

            }
            catch {; }

            treeView1.EndUpdate();
        }
        private void AddFiles(string strPath)
        {
            listView1.BeginUpdate();

            listView1.Items.Clear();
            iFiles = 0;
            DirectoryInfo di = new DirectoryInfo(strPath + "\\");
            FileInfo[] theFiles = di.GetFiles();
            string _Size = string.Empty;
            foreach (FileInfo theFile in theFiles)
            {
                iFiles++;
                if (theFile.Length >= 1024)
                    _Size = string.Format("{0:### ### ###} KB", theFile.Length / 1024);
                else _Size = string.Format("{0} Bytes", theFile.Length);

                ListViewItem lvItem = new ListViewItem(theFile.Name);
                lvItem.SubItems.Add(_Size);
                lvItem.SubItems.Add(theFile.LastWriteTime.ToString("dd/MM/yyyy"));
                lvItem.SubItems.Add(theFile.LastWriteTime.ToShortTimeString());
                listView1.Items.Add(lvItem);
            }

            listView1.EndUpdate();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] drives = Environment.GetLogicalDrives(); // liet ke danh sach o dia
            treeView1.BeginUpdate();
            foreach (string strDrive in drives)
            {
                DriveInfo di = new DriveInfo(strDrive);
                int driveImage;
                switch (di.DriveType)
                {
                    case DriveType.CDRom:
                        driveImage = 5;
                        break;

                    case DriveType.Network:
                        driveImage = 7;
                        break;

                    case DriveType.Unknown:
                        driveImage = 8;
                        break;

                    default:
                        driveImage = 6;
                        break;
                }
                TreeNode node = new TreeNode(strDrive.Substring(0, 1), driveImage, driveImage);
                node.Tag = strDrive;

                if (di.IsReady == true)
                    node.Nodes.Add("...");

                treeView1.Nodes.Add(node);
            }

            //for (int i = 0; i < drives.Length; i++) 
            //{
            //    treeView1.Nodes[0].Nodes.Add(drives[i]);
            //}
            treeView1.ExpandAll();
            //treeView1.MouseDoubleClick += new MouseEventHandler(treeView1_MouseDoubleClick);
            treeView1.EndUpdate();
        }
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            if (e.Node.Nodes.Count > 0)
            {
                if (e.Node.Nodes[0].Text == "..." && e.Node.Nodes[0].Tag == null)
                {
                    e.Node.Nodes.Clear();

                    //Liệt kê danh sách thư mục con
                    string[] dirs = Directory.GetDirectories(e.Node.Tag.ToString());

                    foreach (string dir in dirs)
                    {
                        DirectoryInfo di = new DirectoryInfo(dir);
                        TreeNode node = new TreeNode(di.Name, 0, 1);

                        try
                        {
                            //Lưu đường dẫn vào thuộc tính tag của node để sau này sử dụng
                            node.Tag = dir;

                            //Kiểm tra xem có thư mục con hay không thì thêm 3 dấu chấm
                            if (di.GetDirectories().Count() > 0)
                                node.Nodes.Add(null, "...", 0, 0);
                        }
                        catch (UnauthorizedAccessException)
                        {
                            //Hiển thị Icon thư mục đã bị khóa
                            node.ImageIndex = 1;
                            node.SelectedImageIndex = 1;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, "DirectoryLister",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        finally
                        {
                            e.Node.Nodes.Add(node);
                        }
                    }
                }
            }

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // khi chọn 1 node trên tree thì set path vào textbox(path)
            path.Text = treeView1.SelectedNode.FullPath;

            // Thêm thư mục con từ ổ đĩa vào tree
            AddDirectories(e.Node);

            // Thêm file vào listview
            AddFiles(e.Node.FullPath.ToString());
        }

        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Get the selected node
            string fileName = treeView1.SelectedNode.FullPath;
          
        }
        private void listView1_ItemActivate(object sender, System.EventArgs e)
        {
            try
            {
                string sPath = treeView1.SelectedNode.FullPath;
                string sFileName = listView1.FocusedItem.Text;

                Process.Start(sPath + "\\" + sFileName);
            }
            catch (Exception Exc) { MessageBox.Show(Exc.ToString()); }
        }
    }
}
