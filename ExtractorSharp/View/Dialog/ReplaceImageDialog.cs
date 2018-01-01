﻿using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ExtractorSharp.Command;
using ExtractorSharp.Component;
using ExtractorSharp.Core;
using ExtractorSharp.Data;
using ExtractorSharp.Core.Control;
using ExtractorSharp.Config;

namespace ExtractorSharp.View {
    public partial class ReplaceImageDialog : ESDialog {

        private Controller Controller { get; }
        public ReplaceImageDialog(IConnector Data) : base(Data) {
            Controller = Program.Controller;
            InitializeComponent();
            yesButton.Click += Replace;
            cancelButton.Click += Cancel;
        }

        public void Replace(object sender, EventArgs e) {
            var array = seletImageRadio.Checked ? Connector.CheckedImages : Connector.ImageArray;
            var indexes = new int[array.Length];
            for (var i = 0; i < array.Length; i++) {
                indexes[i] = array[i].Index;
            }
            var type = ColorBits.ARGB_1555;
            if (_4444_Radio.Checked)
                type = ColorBits.ARGB_4444;
            else if (_8888_Radio.Checked)
                type = ColorBits.ARGB_8888;
            var path = string.Empty;
            int mode = 0;
            if (array.Length == 1) {
                var dialog = new OpenFileDialog();
                dialog.Filter = "图片|*.jpg;*.png;*.bmp";
                if (dialog.ShowDialog() == DialogResult.OK) {
                    path = dialog.FileName;
                }
            } else if (fromGifBox.Checked) {
                var dialog = new OpenFileDialog();
                dialog.Filter = "gif动态图片|*.gif";
                if (dialog.ShowDialog() == DialogResult.OK) {
                    path = dialog.FileName;
                }
                mode = 1;
            } else {
                var dialog = new FolderBrowserDialog();
                if (dialog.ShowDialog() == DialogResult.OK) {
                    path = dialog.SelectedPath;
                }
                mode = 2;
            }
            if (!string.IsNullOrEmpty(path)) {
                Controller.Do("replaceImage", type, adjustPostionBox.Checked, mode, path, Connector.SelectedFile, indexes);
                DialogResult = DialogResult.OK;
            }
        }

        public void Cancel(object sender, EventArgs e) {
            DialogResult = DialogResult.OK;
        }

    }


}
