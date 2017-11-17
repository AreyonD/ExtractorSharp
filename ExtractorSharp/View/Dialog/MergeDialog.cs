﻿using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System;
using ExtractorSharp.UI;
using ExtractorSharp.Command;
using ExtractorSharp.Handle;
using ExtractorSharp.Core;
using ExtractorSharp.Data;

namespace ExtractorSharp.View {
    partial class MergeDialog : EaseDialog {
        private Album Album;
        private Merger Merger;
        private Controller Controller;
        public MergeDialog() {
            InitializeComponent();
            this.Controller = Program.Controller;
            this.Merger = Program.Merger;
            sortButton.Click += (o,e) => Merger.Sort(useOtherCheck.Checked);
            list.MouseDown += ListMouseDown;
            list.DragDrop += ListDragDrop;
            list.DragOver += (o, e) => e.Effect = DragDropEffects.Move;
            deleteItem.Click += Remove;
            clearItem.Click += (o, e) => Controller.Do("clearSplice");
            spliceButton.Click += SpliceImg;
            addOutItem.Click += AddOutside;
            moveDownItem.Click += MoveDown;
            moveUpItem.Click += MoveUp;
            Merger.MergeQueueChanged += Flush;
            Merger.MergeStarted += MergeStart;
            Merger.MergeProcessing += MergeProcessing;
            Merger.MergeCompleted += MergeCompleted;
        }

        private void MergeCompleted(object sender, MergeEventArgs e) {
            prograss.Visible = false;
            spliceButton.Enabled = true;
            sortButton.Enabled = true;
            Visible = false;
            Program.Controller.Do("replaceImg", Album, e.Album);
        }

        private void MergeProcessing(object sender, MergeEventArgs e) => prograss.Value++;

        private void MergeStart(object sender, MergeEventArgs e) {
            spliceButton.Enabled = false;
            sortButton.Enabled = false;
            prograss.Value = 0;
            prograss.Visible = true;
            prograss.Maximum = e.Count;
        }


        /// <summary>
        /// 向上移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveUp(object sender, EventArgs e) {
            var i = list.SelectedIndex;
            if (i > 0) {
                Program.Merger.InterChange(i, --i);
                list.SelectedIndex = i;
            }
        }

        /// <summary>
        /// 向下移动
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MoveDown(object sender, EventArgs e) {
            var i = list.SelectedIndex;
            if (i < list.Items.Count - 1 && i > -1) {
                Merger.InterChange(i, ++i);
                list.SelectedIndex = i;
            }
        }


        private void AddOutside(object sender, EventArgs e) {
            var dialog = new OpenFileDialog();
            dialog.Filter = "NPK文件,img文件 | *.NPK; *.img";
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == DialogResult.OK) {
                var array = Tools.Load(dialog.FileNames).ToArray();
                Program.Controller.Do("addSplice", array);
            }
        }

        public void Flush(object sender,MergeQueueEventArgs e) {
            list.Items.Clear();
            list.Items.AddRange(Merger.Queues.ToArray());
        }


        public override DialogResult Show(params object[] args) {
            Album = args[0] as Album;
            Flush(null,null);
            albumList.Items.Clear();
            var array = Controller.List.ToArray();
            albumList.Items.AddRange(array);
            albumList.SelectedItem = Album;
            return ShowDialog();
        }


        public void ListMouseDown(object sender, MouseEventArgs e) {
            if (list.Items.Count == 0 || e.Button != MouseButtons.Left || list.SelectedIndex < 0 || e.Clicks == 2)
                return;
            DoDragDrop(list.SelectedItem, DragDropEffects.Move);
        }

        public void ListDragDrop(object sender, DragEventArgs e) {
            var source = list.SelectedIndex;
            var target = list.IndexFromPoint(PointToClient(new Point(e.X, e.Y))) - 1;
            Merger.InterChange(source, target);
            if (target > -1)
                list.SelectedIndex = target;
        }

        public void Remove(object sender, EventArgs e) {
            var album = list.SelectedItem as Album;
            if (album != null)
                Program.Controller.Do("removeSplice", new Album[] { album });
        }

        public void SpliceImg(object sender, EventArgs e) {
            if (list.Items.Count < 1) {//当拼合队列为空时
                Messager.ShowWarnning("EmptyMergeTips");
                return;
            }
            if (albumList.SelectedItem == null) {//没有选择Img时
                if (albumList.Text == string.Empty) {
                    Messager.ShowWarnning("NotSelectImgTips");
                    return;
                } else {
                    Album = new Album();
                    Controller.Do("newImg", Album, albumList.Text);
                }
            } else
                Album = albumList.SelectedItem as Album;   
            Controller.Do("runSplice");      

        }
    }
}