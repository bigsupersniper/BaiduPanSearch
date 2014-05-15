namespace BaidupanSearch
{
    partial class SearchForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.cmdSearch = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.tbKey = new System.Windows.Forms.TextBox();
            this.lvResult = new System.Windows.Forms.ListView();
            this.标题 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.lbResult = new System.Windows.Forms.Label();
            this.cmdPrevious = new System.Windows.Forms.Button();
            this.cmdNext = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // cmdSearch
            // 
            this.cmdSearch.Location = new System.Drawing.Point(440, 24);
            this.cmdSearch.Name = "cmdSearch";
            this.cmdSearch.Size = new System.Drawing.Size(75, 25);
            this.cmdSearch.TabIndex = 0;
            this.cmdSearch.Text = "搜索";
            this.cmdSearch.UseVisualStyleBackColor = true;
            this.cmdSearch.Click += new System.EventHandler(this.cmdSearch_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "搜索关键字 ：";
            // 
            // tbKey
            // 
            this.tbKey.Location = new System.Drawing.Point(103, 26);
            this.tbKey.Name = "tbKey";
            this.tbKey.Size = new System.Drawing.Size(321, 21);
            this.tbKey.TabIndex = 2;
            // 
            // lvResult
            // 
            this.lvResult.Alignment = System.Windows.Forms.ListViewAlignment.Left;
            this.lvResult.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.标题});
            this.lvResult.GridLines = true;
            this.lvResult.Location = new System.Drawing.Point(12, 98);
            this.lvResult.MultiSelect = false;
            this.lvResult.Name = "lvResult";
            this.lvResult.ShowItemToolTips = true;
            this.lvResult.Size = new System.Drawing.Size(740, 387);
            this.lvResult.TabIndex = 3;
            this.lvResult.UseCompatibleStateImageBehavior = false;
            this.lvResult.View = System.Windows.Forms.View.Details;
            this.lvResult.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lvResult_MouseDoubleClick);
            // 
            // 标题
            // 
            this.标题.Text = "标题";
            this.标题.Width = 660;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "搜索结果 ：";
            // 
            // lbResult
            // 
            this.lbResult.AutoSize = true;
            this.lbResult.Location = new System.Drawing.Point(91, 65);
            this.lbResult.Name = "lbResult";
            this.lbResult.Size = new System.Drawing.Size(0, 12);
            this.lbResult.TabIndex = 5;
            // 
            // cmdPrevious
            // 
            this.cmdPrevious.Enabled = false;
            this.cmdPrevious.Location = new System.Drawing.Point(544, 24);
            this.cmdPrevious.Name = "cmdPrevious";
            this.cmdPrevious.Size = new System.Drawing.Size(75, 25);
            this.cmdPrevious.TabIndex = 6;
            this.cmdPrevious.Text = "上一页";
            this.cmdPrevious.UseVisualStyleBackColor = true;
            this.cmdPrevious.Click += new System.EventHandler(this.cmdPrevious_Click);
            // 
            // cmdNext
            // 
            this.cmdNext.Enabled = false;
            this.cmdNext.Location = new System.Drawing.Point(650, 24);
            this.cmdNext.Name = "cmdNext";
            this.cmdNext.Size = new System.Drawing.Size(75, 25);
            this.cmdNext.TabIndex = 7;
            this.cmdNext.Text = "下一页";
            this.cmdNext.UseVisualStyleBackColor = true;
            this.cmdNext.Click += new System.EventHandler(this.cmdNext_Click);
            // 
            // SearchForm
            // 
            this.AcceptButton = this.cmdSearch;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 497);
            this.Controls.Add(this.cmdNext);
            this.Controls.Add(this.cmdPrevious);
            this.Controls.Add(this.lbResult);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lvResult);
            this.Controls.Add(this.tbKey);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmdSearch);
            this.MaximizeBox = false;
            this.Name = "SearchForm";
            this.Text = "百度网盘搜索";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button cmdSearch;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbKey;
        private System.Windows.Forms.ListView lvResult;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lbResult;
        private System.Windows.Forms.ColumnHeader 标题;
        private System.Windows.Forms.Button cmdPrevious;
        private System.Windows.Forms.Button cmdNext;
    }
}

